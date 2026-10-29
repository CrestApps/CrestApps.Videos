#!/usr/bin/env python3
"""
Fills a demo CrestApps.OrchardCore site (SQLite) with realistic FAKE data so the Contact Center and Omnichannel
reports show real numbers.

    python seed-reports-data.py <path to App_Data/Sites/Default/OrchardCore.db> [total interactions, default 450]

* Run it on a STOPPED site that was started from the blank "Contoso" snapshot (CRM starter recipe already run,
  users jamie.chen, maria.lopez, sam.patel, alex.kim and priya.shah exist). It is not idempotent: run it once.
* Everything is relative to the real current UTC time (the last ~45 days, a bit more in the last 7 days) and is
  deterministic for a given moment (fixed random seed).
* Names are made up; phone numbers are in the fictional 555-01xx range (+1212555xxxx / +1646555xxxx).
* The documents are written the way YesSql stores them: a row in the collection's Document table (Id, Type,
  Content JSON, Version) plus the rows of every index table that maps the document type.

What it creates
  1. Contact Center configuration: queue group "Customer care", queues Sales / Support / Billing, five agent
     profiles (entitled to the queues and campaigns).
  2. Omnichannel configuration: campaign group "Customer lifecycle", campaigns "Autumn renewals" and "New customer
     welcome", dispositions.
  3. 40 contacts and 60 leads (content items, same shape as the Content Import creates).
  4. ~250 Omnichannel activities (phone and SMS, all statuses, some overdue) assigned to the five users.
  5. ~450 Contact Center interactions (inbound and outbound voice, SMS) with queue history, transfers,
     recordings, abandoned / voicemail / callback / failed calls, plus the CallQueued / CallAbandoned /
     CallSentToVoicemail / CallbackRequested events the outcome classifier reads.
  6. Agent state-change events (sign in, available, busy, wrap-up, break, lunch, sign out) for the Workforce
     and Payroll reports, derived from the same calls so the numbers agree.
"""
import datetime as dt
import json
import math
import random
import sqlite3
import sys

SEED = 20261029
rng = random.Random(SEED)
NOW = dt.datetime.now(dt.timezone.utc).replace(tzinfo=None, microsecond=0)
DAYS = 45

CC = "ContactCenter_Document"
OM = "Omnichannel_Document"
DOC = "Document"

ASM_CC = "CrestApps.OrchardCore.ContactCenter.Core"
ASM_OM = "CrestApps.OrchardCore.Omnichannel.Core"


def tname(ns_type, asm):
    return f"{ns_type}, {asm}"


T_QUEUE_GROUP = tname("CrestApps.OrchardCore.ContactCenter.Core.Models.ActivityQueueGroup", ASM_CC)
T_QUEUE = tname("CrestApps.OrchardCore.ContactCenter.Core.Models.ActivityQueue", ASM_CC)
T_AGENT = tname("CrestApps.OrchardCore.ContactCenter.Core.Models.AgentProfile", ASM_CC)
T_INTERACTION = tname("CrestApps.OrchardCore.ContactCenter.Core.Models.Interaction", ASM_CC)
T_EVENT = tname("CrestApps.OrchardCore.ContactCenter.Core.Models.InteractionEvent", ASM_CC)
T_ACTIVITY = tname("CrestApps.OrchardCore.Omnichannel.Core.Models.OmnichannelActivity", ASM_OM)
T_CONTENT_ITEM = "OrchardCore.ContentManagement.ContentItem, OrchardCore.ContentManagement.Abstractions"


def dict_doc_type(model):
    return ("CrestApps.OrchardCore.Models.DictionaryDocument`1[[CrestApps.OrchardCore.Omnichannel.Core.Models."
            f"{model}, {ASM_OM}, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null]], "
            "CrestApps.OrchardCore.Abstractions")


# --------------------------------------------------------------------------------------------------------------------
# Small helpers: ids, dates, JSON
# --------------------------------------------------------------------------------------------------------------------
ALPHABET = "0123456789abcdefghijklmnopqrstuvwxyz"


def new_id():
    """A 26 character lower-case id that looks like the ones Orchard Core generates."""
    return "4" + "".join(rng.choice(ALPHABET) for _ in range(25))


def jdump(value):
    return json.dumps(value, ensure_ascii=False, separators=(",", ":"))


def iso(value):
    """JSON date: the precise UTC format the models write (7 fractional digits)."""
    if value is None:
        return None
    return value.strftime("%Y-%m-%dT%H:%M:%S.") + f"{value.microsecond * 10:07d}Z"


def sql_dt(value):
    """How the SQLite provider stores a DateTime column (fraction without trailing zeros)."""
    if value is None:
        return None
    text = value.strftime("%Y-%m-%d %H:%M:%S")
    if value.microsecond:
        text += "." + f"{value.microsecond * 10:07d}".rstrip("0")
    return text


def strip_nulls(value):
    return {k: v for k, v in value.items() if v is not None}


def nth_weekday(year, month, weekday, n):
    first = dt.date(year, month, 1)
    offset = (weekday - first.weekday()) % 7
    return first + dt.timedelta(days=offset + 7 * (n - 1))


def et_offset(date):
    """America/New_York offset in hours for a calendar date (US daylight saving rules)."""
    start = nth_weekday(date.year, 3, 6, 2)   # second Sunday of March
    end = nth_weekday(date.year, 11, 6, 1)    # first Sunday of November
    return -4 if start <= date < end else -5


def local_to_utc(date, hour):
    """Eastern wall clock (date + fractional hour) to a naive UTC datetime."""
    return dt.datetime.combine(date, dt.time()) + dt.timedelta(hours=hour - et_offset(date))


def utc_to_et_date(value):
    return (value + dt.timedelta(hours=et_offset(value.date()))).date()


def wchoice(options):
    """options: list of (value, weight)."""
    values, weights = zip(*options)
    return rng.choices(values, weights=weights, k=1)[0]


def clamp(value, low, high):
    return max(low, min(high, value))


# --------------------------------------------------------------------------------------------------------------------
# Database access (YesSql layout)
# --------------------------------------------------------------------------------------------------------------------
class Store:
    def __init__(self, path):
        self.conn = sqlite3.connect(path)
        self.next_id = {}
        self.counts = {}

    def alloc(self, table):
        # The id generator of this site keeps one counter per document table, seeded from MAX(Id) at startup.
        if table not in self.next_id:
            self.next_id[table] = self.conn.execute(f"select coalesce(max(Id),0) from [{table}]").fetchone()[0]
        self.next_id[table] += 1
        return self.next_id[table]

    def doc(self, table, type_name, content):
        doc_id = self.alloc(table)
        self.conn.execute(f"insert into [{table}] (Id, Type, Content, Version) values (?,?,?,1)",
                          (doc_id, type_name, jdump(content)))
        self.counts[type_name.split(",")[0].split(".")[-1]] = self.counts.get(type_name.split(",")[0].split(".")[-1], 0) + 1
        return doc_id

    def index(self, table, **columns):
        names = ",".join(f"[{c}]" for c in columns)
        marks = ",".join("?" for _ in columns)
        self.conn.execute(f"insert into [{table}] ({names}) values ({marks})", [self._val(v) for v in columns.values()])

    @staticmethod
    def _val(value):
        if isinstance(value, dt.datetime):
            return sql_dt(value)
        if isinstance(value, bool):
            return 1 if value else 0
        return value


# --------------------------------------------------------------------------------------------------------------------
# Fake people
# --------------------------------------------------------------------------------------------------------------------
FIRST = ["Olivia", "Noah", "Liam", "Emma", "Ava", "Elijah", "Sophia", "Lucas", "Mia", "Mason", "Isabella", "Ethan",
         "Amelia", "Logan", "Harper", "Henry", "Evelyn", "Jack", "Abigail", "Owen", "Emily", "Caleb", "Ella",
         "Wyatt", "Scarlett", "Gavin", "Grace", "Isaac", "Chloe", "Julian", "Lily", "Dylan", "Nora", "Leo",
         "Hannah", "Nathan", "Zoe", "Aaron", "Layla", "Adrian", "Stella", "Miles", "Violet", "Eli", "Hazel",
         "Colin", "Naomi", "Felix", "Paige", "Theo", "Ruby", "Hugo", "Clara", "Declan", "Maya", "Jonah", "Tessa",
         "Silas", "Vera", "Reid", "Iris", "Gideon", "Ivy", "Rowan", "Sadie", "Brent", "Alma", "Dorian", "Wren",
         "Marcus", "Joy", "Kellan", "Esme", "Tobias", "Lena", "Anders", "Cora", "Vince", "Gwen", "Roman", "Pearl",
         "Heath", "Opal", "Quinn", "Faye", "Lowell", "Mabel", "Warren", "Nell", "Spencer", "Ada", "Trent", "Bree",
         "Dexter", "Lucy", "Grant", "Rhea", "Amos", "Tilly", "Perry", "Gia", "Cyrus", "Maeve"]
LAST = ["Bennett", "Carter", "Foster", "Reed", "Brooks", "Hayes", "Sullivan", "Price", "Coleman", "Barnes", "Watson",
        "Russell", "Griffin", "Hughes", "Fisher", "Dixon", "Hunt", "Palmer", "Webb", "Holland", "Fowler", "Boyd",
        "Gibson", "Lambert", "Marsh", "Norris", "Oliver", "Pearson", "Quinn", "Rhodes", "Spencer", "Tucker",
        "Underwood", "Vaughn", "Walsh", "Yates", "Zimmerman", "Abbott", "Burke", "Chandler", "Dawson", "Ellis",
        "Franklin", "Grady", "Hartley", "Ingram", "Joyce", "Kerr", "Lowe", "Mercer", "Nash", "Osborne", "Pruitt",
        "Quigley", "Rowe", "Stone", "Thatcher", "Upton", "Vance", "Whitaker", "Yoder", "Ashby", "Blake", "Crane",
        "Doyle", "Emery", "Finch", "Garner", "Holt", "Irwin", "Jarvis", "Knox", "Lowry", "Moody", "Nolan", "Orr",
        "Pike", "Rudd", "Shaw", "Tate", "Vega", "Wade", "York", "Zane", "Archer", "Bishop", "Cole", "Drake",
        "Fenn", "Gray", "Hale", "Ives", "Jude", "Kemp", "Lyle", "Moss", "Nye", "Pratt", "Rush", "Sims", "Troy"]
TITLES = ["Office Manager", "Owner", "Operations Lead", "Accounts Payable", "Practice Manager", "Store Manager",
          "Purchasing Agent", "Director", "Administrator", "Facilities Coordinator", "Controller", "Partner"]
TRADES = ["Hardware", "Dental", "Realty", "Plumbing", "Bakery", "Logistics", "Insurance", "Landscaping", "Optical",
          "Accounting", "Auto Care", "Print Shop", "Florist", "Fitness", "Pharmacy", "Studio", "Electric", "Roofing"]
# Staff users already on the site: user name -> display name.
STAFF = [("jamie.chen", "Jamie Chen"), ("maria.lopez", "Maria Lopez"), ("sam.patel", "Sam Patel"),
         ("alex.kim", "Alex Kim"), ("priya.shah", "Priya Shah")]


def main(db_path, target_total=450):
    s = Store(db_path)
    c = s.conn

    # ----------------------------------------------------------------------------------------------------------------
    # 0. What the snapshot already holds
    # ----------------------------------------------------------------------------------------------------------------
    users = {}
    for _id, content in c.execute("select Id, Content from Document where Type like 'OrchardCore.Users.Models.User,%'"):
        user = json.loads(content)
        users[user["UserName"]] = user["UserId"]
    for name, _ in STAFF + [("admin", "admin")]:
        if name not in users:
            raise SystemExit(f"User '{name}' is missing from the site; start from the snapshot that has the demo users.")
    admin_id = users["admin"]

    reason_codes = {}
    for _id, content in c.execute(f"select Id, Content from {CC} where Type like '%AgentStateReasonCode,%'"):
        reason = json.loads(content)
        reason_codes[reason["Name"]] = reason["ItemId"]

    lead_status = {}
    row = c.execute("select Content from Document where Type like ? limit 1", (dict_doc_type("LeadStatus"),)).fetchone()
    if row:
        for rec in json.loads(row[0])["Records"].values():
            lead_status[rec["Name"]] = rec["ItemId"]
    lead_sources = [r[0] for r in c.execute("select ContentItemId from ContentItemIndex where ContentType='LeadSource' order by Id")]
    if not lead_status or not lead_sources:
        raise SystemExit("The CRM starter recipe (lead statuses and sources) has not been run on this site.")

    created_cfg = NOW - dt.timedelta(days=DAYS + 20)   # configuration "has existed for a while"

    # ----------------------------------------------------------------------------------------------------------------
    # 1. Contact Center configuration: queue group, queues, agent profiles
    # ----------------------------------------------------------------------------------------------------------------
    group_id = new_id()
    gdoc = s.doc(CC, T_QUEUE_GROUP, {
        "Name": "Customer care", "Description": "Everything customers phone or text us about.",
        "CreatedUtc": iso(created_cfg), "ItemId": group_id, "Properties": {}})
    s.index("ContactCenter_ActivityQueueGroupIndex", DocumentId=gdoc, ItemId=group_id, Name="Customer care")

    queue_defs = [  # key, name, description, SLA seconds, share of inbound demand
        ("sales", "Sales", "New business and renewal questions.", 20, 0.30),
        ("support", "Support", "Help with orders and the product.", 30, 0.45),
        ("billing", "Billing", "Invoices, payments and refunds.", 60, 0.25),
    ]
    queues = {}
    for key, name, desc, sla, _share in queue_defs:
        qid = new_id()
        queues[key] = qid
        qdoc = s.doc(CC, T_QUEUE, {
            "QueueGroupId": group_id, "Name": name, "Description": desc, "DefaultPriority": "Normal",
            "RoutingStrategy": "LongestIdle", "SlaThresholdSeconds": sla, "ReservationTimeoutSeconds": 30,
            "UnansweredOfferAction": "Requeue", "AfterHoursAction": "HoldInQueue", "Enabled": True,
            "CreatedUtc": iso(created_cfg), "ItemId": qid, "Properties": {}})
        s.index("ContactCenter_ActivityQueueIndex", DocumentId=qdoc, ItemId=qid, Name=name, QueueGroupId=group_id, Enabled=True)

    # Campaign ids are needed by the agent profiles (campaign entitlements) and by the virtual outbound queue.
    campaign_group_id = new_id()
    campaigns = {"renewals": new_id(), "welcome": new_id()}

    # Which queues each agent serves, and the working day (Eastern hours) each one keeps.
    agent_defs = {
        "jamie.chen": (["sales", "billing"], (9.0, 17.5)),
        "maria.lopez": (["sales", "support"], (9.0, 17.5)),
        "sam.patel": (["sales", "support"], (10.0, 18.5)),
        "alex.kim": (["support", "billing"], (8.0, 16.5)),
        "priya.shah": (["support", "billing"], (9.5, 18.0)),
    }
    agents = []  # dicts: user, name, user_id, item_id, queue_keys, shift
    for user_name, display in STAFF:
        qkeys, shift = agent_defs[user_name]
        aid = new_id()
        qids = [queues[k] for k in qkeys]
        agents.append({"user": user_name, "name": display, "user_id": users[user_name], "id": aid,
                       "queue_keys": qkeys, "queue_ids": qids, "hours": shift})
        adoc = s.doc(CC, T_AGENT, strip_nulls({
            "Name": display, "UserId": users[user_name], "UserName": user_name, "DisplayName": display,
            "MaxConcurrentInteractions": 1, "PresenceStatus": "Offline",
            "QueueIds": qids, "AllowedQueueIds": qids,
            "QueueMemberships": [{"QueueId": q, "Priority": 0, "DelaySeconds": 0} for q in qids],
            "CampaignIds": list(campaigns.values()), "AllowedCampaignIds": list(campaigns.values()),
            "Skills": [], "CreatedUtc": iso(created_cfg), "ItemId": aid, "Properties": {}}))
        s.index("ContactCenter_AgentProfileIndex", DocumentId=adoc, ItemId=aid, Name=display, UserId=users[user_name], PresenceStatus=0)
        for q in qids:  # AgentAllowedQueueIndex and AgentQueueMembershipIndex: one row per queue
            s.index("ContactCenter_AgentAllowedQueueIndex", DocumentId=adoc, ItemId=aid, QueueId=q.lower())
            s.index("ContactCenter_AgentQueueMembershipIndex", DocumentId=adoc, ItemId=aid, QueueId=q.lower(),
                    PresenceStatus=0, MaxConcurrentInteractions=1)

    agent_by_user_id = {a["user_id"]: a for a in agents}

    # ----------------------------------------------------------------------------------------------------------------
    # 2. Omnichannel configuration: campaign group, campaigns, dispositions (catalogs live in one dictionary document)
    # ----------------------------------------------------------------------------------------------------------------
    def dictionary(model, records):
        s.doc(DOC, dict_doc_type(model), {"Records": {r["ItemId"]: r for r in records}, "Identifier": new_id()})

    dictionary("OmnichannelCampaignGroup", [{
        "DisplayText": "Customer lifecycle", "Description": "Campaigns that keep existing customers happy.",
        "CreatedUtc": iso(created_cfg), "Author": "admin", "OwnerId": admin_id, "ItemId": campaign_group_id, "Properties": {}}])
    dictionary("OmnichannelCampaign", [
        {"DisplayText": "Autumn renewals", "Description": "Call customers whose plan renews this autumn.",
         "CampaignGroupId": campaign_group_id, "InteractionType": "Manual", "Channel": "Phone",
         "CreatedUtc": iso(created_cfg), "Author": "admin", "OwnerId": admin_id, "AllowAIToUpdateSubject": True,
         "ItemId": campaigns["renewals"], "Properties": {}},
        {"DisplayText": "New customer welcome", "Description": "A welcome call or text for every new customer.",
         "CampaignGroupId": campaign_group_id, "InteractionType": "Manual", "Channel": "Phone",
         "CreatedUtc": iso(created_cfg), "Author": "admin", "OwnerId": admin_id, "AllowAIToUpdateSubject": True,
         "ItemId": campaigns["welcome"], "Properties": {}}])

    disposition_defs = [  # key, name, outcome
        ("interested", "Interested", "None"), ("not_interested", "Not interested", "None"),
        ("callback", "Callback requested", "None"), ("no_answer", "No answer", "NoAnswer"),
        ("voicemail", "Left voicemail", "AnsweringMachine"), ("wrong_number", "Wrong number", "NotInService")]
    dispositions = {}
    records = []
    for key, name, outcome in disposition_defs:
        dispositions[key] = new_id()
        records.append({"Name": name, "Outcome": outcome, "CaptureDate": key == "callback",
                        "CreatedUtc": iso(created_cfg), "Author": "admin", "OwnerId": admin_id,
                        "ItemId": dispositions[key], "Properties": {}})
    dictionary("OmnichannelDisposition", records)

    # ----------------------------------------------------------------------------------------------------------------
    # 3. Contacts (40) and leads (60) - content items shaped like the ones the Content Import creates
    # ----------------------------------------------------------------------------------------------------------------
    names_used = set()

    def person():
        while True:
            first, last = rng.choice(FIRST), rng.choice(LAST)
            if (first, last) not in names_used:
                names_used.add((first, last))
                return first, last

    def verification(number):
        return {"PhoneNumber": number, "NormalizedPhoneNumber": number, "LastVerifiedUtc": None,
                "LastVerifiedByUserId": None, "VerificationProvider": None, "VerificationStatus": "Unverified",
                "VerificationResultJson": None, "VerificationAttemptCount": 0, "FailedAttemptCount": 0,
                "LastError": None, "LastAttemptUtc": None, "NextVerificationDueUtc": None}

    def content_item(kind, first, last, title, number, email, created, extra_part=None):
        part = kind  # the field part is named like the type ("Contact" / "Lead")
        item = {
            "ContentItemId": new_id(), "ContentItemVersionId": new_id(), "ContentType": kind,
            "DisplayText": f"{first} {last}", "Latest": True, "Published": True,
            "ModifiedUtc": iso(created), "PublishedUtc": iso(created), "CreatedUtc": iso(created),
            "Owner": admin_id, "Author": "admin", "TitlePart": {"Title": f"{first} {last}"},
            part: {"FirstName": {"Text": first}, "LastName": {"Text": last}, "JobTitle": {"Text": title}},
        }
        if extra_part:
            item.update(extra_part)
        item["OmnichannelContactPart"] = {"DoNotCall": False, "DoNotEmail": False, "DoNotSms": False,
                                          "TimeZoneId": "America/New_York", "DoNotCallUtc": None,
                                          "DoNotEmailUtc": None, "DoNotSmsUtc": None}
        item["ContactMethods"] = {"ContentItems": [
            {"ContentItemId": new_id(), "ContentItemVersionId": new_id(), "ContentType": "EmailAddress",
             "DisplayText": email, "Latest": False, "Published": False, "ModifiedUtc": None, "PublishedUtc": None,
             "CreatedUtc": None, "Owner": None, "Author": None, "EmailInfoPart": {"Email": {"Text": email}}},
            {"ContentItemId": new_id(), "ContentItemVersionId": new_id(), "ContentType": "PhoneNumber",
             "DisplayText": f"Cell: {number}", "Latest": False, "Published": False, "ModifiedUtc": None,
             "PublishedUtc": None, "CreatedUtc": None, "Owner": None, "Author": None,
             "PhoneNumberInfoPart": {"Number": {"PhoneNumber": number, "CountryCode": None, "NationalNumber": None},
                                     "Extension": None, "Type": {"Text": "Cell"}},
             "PhoneNumberVerificationPart": verification(number)}]}
        item["PhoneNumberVerificationPart"] = verification(number)
        return item

    def store_content_item(item, converted=False):
        doc_id = s.doc(DOC, T_CONTENT_ITEM, item)
        kind = item["ContentType"]
        created = dt.datetime.strptime(item["CreatedUtc"][:26], "%Y-%m-%dT%H:%M:%S.%f")
        number = item["PhoneNumberVerificationPart"]["PhoneNumber"]
        email = item["ContactMethods"]["ContentItems"][0]["EmailInfoPart"]["Email"]["Text"]
        cid = item["ContentItemId"]
        s.index("ContentItemIndex", DocumentId=doc_id, ContentItemId=cid, ContentItemVersionId=item["ContentItemVersionId"],
                Latest=True, Published=True, ContentType=kind, ModifiedUtc=created, PublishedUtc=created,
                CreatedUtc=created, Owner=admin_id, Author="admin", DisplayText=item["DisplayText"])
        s.index("OmnichannelContactIndex", DocumentId=doc_id, ContentItemId=cid, ContentType=kind, IsConverted=converted,
                Published=True, Latest=True, PrimaryCellPhoneNumber=number[2:], NormalizedPrimaryCellPhoneNumber=number,
                PrimaryHomePhoneNumber=None, NormalizedPrimaryHomePhoneNumber=None, PrimaryEmailAddress=email,
                TimeZoneId="America/New_York")
        s.index("OmnichannelContactPhoneNumberIndex", DocumentId=doc_id, ContentItemId=cid, ContentType=kind,
                IsConverted=converted, Published=True, Latest=True, PhoneType="Cell", NormalizedPhoneNumber=number)
        s.index("OmnichannelContactCommunicationPreferenceIndex", DocumentId=doc_id, ContentItemId=cid, DoNotCall=False,
                DoNotCallUtc=None, DoNotSms=False, DoNotSmsUtc=None, DoNotEmail=False, DoNotEmailUtc=None)
        s.index("PhoneNumberVerificationPartIndex", DocumentId=doc_id, ContentItemId=cid, PhoneNumber=number,
                NormalizedPhoneNumber=number, IsVerified=False, VerificationStatus=0, VerificationProvider=None,
                LastVerifiedUtc=None, NextVerificationDueUtc=None, FailedAttemptCount=0, LastAttemptUtc=None,
                CountryCode=None, Carrier=None, IsMobile=False, IsLandline=False, IsVoip=False, LineStatus=None)
        return doc_id

    contacts = []  # dict(id, type, name, phone)
    for i in range(40):
        first, last = person()
        number = f"+1212555{100 + i:04d}"
        email = f"{first}.{last}@example.com".lower()
        created = NOW - dt.timedelta(days=rng.uniform(DAYS, DAYS + 120))
        item = content_item("Contact", first, last, rng.choice(TITLES), number, email, created)
        store_content_item(item)
        contacts.append({"id": item["ContentItemId"], "type": "Contact", "name": f"{first} {last}", "phone": number})

    lead_status_weights = [("Open - Not Contacted", 22), ("Working - Contacted", 18), ("Nurturing", 8),
                           ("Closed - Not Converted", 7), ("Converted", 5)]
    leads = []
    lead_lists = ["Autumn webinar", "Trade show - September", "Website sign-ups"]
    for i in range(60):
        first, last = person()
        number = f"+1646555{100 + i:04d}"
        email = f"{first}.{last}@example.com".lower()
        created = NOW - dt.timedelta(days=rng.uniform(0, DAYS), hours=rng.uniform(0, 8))
        status_name = wchoice(lead_status_weights)
        status_id = lead_status[status_name]
        closed = status_name in ("Closed - Not Converted", "Converted")
        converted = status_name == "Converted"
        owner = rng.choice(agents)["user_id"]
        rating = wchoice([("Hot", 3), ("Warm", 4), ("Cold", 3)])
        source = rng.choice(lead_sources)
        list_name = rng.choice(lead_lists)
        converted_utc = (created + dt.timedelta(days=rng.uniform(1, 9))) if converted else None
        if converted_utc and converted_utc > NOW:
            converted_utc = NOW - dt.timedelta(hours=3)
        converted_contact = contacts[i % len(contacts)]["id"] if converted else None
        lead_part = {"LeadPart": {
            "IsClosed": closed, "IsConverted": converted, "Imports": [],
            "Company": {"Text": f"{last} {rng.choice(TRADES)}"}, "Source": {"ContentItemIds": [source]},
            "ListName": {"Text": list_name}, "Rating": {"Text": rating}, "Owner": {"UserIds": [owner]},
            "StatusId": status_id, "ConvertedUtc": iso(converted_utc) if converted else None, "ConvertedById": None,
            "ConvertedByUsername": None, "ConvertedContactItemId": converted_contact,
            "ConvertedContactType": "Contact" if converted else None, "ConvertedAccountItemId": None,
            "ConvertedOpportunityItemId": None, "LastScrubbedUtc": None}}
        item = content_item("Lead", first, last, rng.choice(TITLES), number, email, created, lead_part)
        doc_id = store_content_item(item, converted=converted)
        s.index("LeadIndex", DocumentId=doc_id, ContentItemId=item["ContentItemId"], ContentType="Lead", Published=True,
                Latest=True, StatusId=status_id, IsClosed=closed, IsConverted=converted, SourceId=source,
                ListName=list_name, Rating=rating, OwnerId=owner, ConvertedContactItemId=converted_contact,
                ConvertedUtc=converted_utc, CreatedUtc=created)
        if not closed:
            leads.append({"id": item["ContentItemId"], "type": "Lead", "name": f"{first} {last}", "phone": number})

    people = contacts + leads
    stranger_numbers = [f"+1212555{n:04d}" for n in range(140, 200)] + [f"+1646555{n:04d}" for n in range(160, 200)]

    # ----------------------------------------------------------------------------------------------------------------
    # 4. Working days of the five agents (shifts, lunch, breaks). Calls are only ever placed in a free slot of an
    #    on-shift agent, so the agent state events built from them later never overlap.
    # ----------------------------------------------------------------------------------------------------------------
    today = NOW.date()
    all_dates = [today - dt.timedelta(days=d) for d in range(DAYS - 1, -1, -1)]
    shifts = {}  # (agent index, ET date) -> dict(start, end, busy=[(start, end, kind, data)])
    for ai, agent in enumerate(agents):
        for d in all_dates:
            if d.weekday() >= 5 or rng.random() > 0.93:
                continue
            start = local_to_utc(d, agent["hours"][0] + rng.uniform(-0.15, 0.15))
            end = local_to_utc(d, agent["hours"][1] + rng.uniform(-0.2, 0.2))
            end = min(end, NOW - dt.timedelta(minutes=20))
            if end - start < dt.timedelta(minutes=45):
                continue
            shift = {"start": start, "end": end, "busy": []}
            lunch_start = start + dt.timedelta(hours=3.5 + rng.uniform(0, 1.0))
            for kind, begin, minutes in (("Short break", start + dt.timedelta(hours=1.5 + rng.uniform(-0.25, 0.25)), 15),
                                         ("Lunch", lunch_start, 45),
                                         ("Short break", start + dt.timedelta(hours=6.2 + rng.uniform(-0.25, 0.25)), 15)):
                finish = begin + dt.timedelta(minutes=minutes)
                if finish <= end - dt.timedelta(minutes=20):
                    shift["busy"].append((begin, finish, "state", ("Break", kind)))
            # Now and then a meeting, a training hour or a few minutes away from the desk.
            for state, reason, minutes, chance in (("Meeting", "Team meeting", 30, 0.30), ("Training", "Training", 60, 0.12),
                                                   ("Away", "Away from desk", 10, 0.25), ("Meeting", "Coaching", 30, 0.08)):
                if rng.random() < chance:
                    for _try in range(8):
                        begin = start + dt.timedelta(hours=rng.uniform(0.5, 7))
                        finish = begin + dt.timedelta(minutes=minutes)
                        if finish <= end - dt.timedelta(minutes=20) and all(
                                finish <= b or begin >= e for b, e, _k, _x in shift["busy"]):
                            shift["busy"].append((begin, finish, "state", (state, reason)))
                            break
            shifts[(ai, d)] = shift

    def agent_free(ai, begin, finish):
        shift = shifts.get((ai, utc_to_et_date(begin)))
        if not shift or begin < shift["start"] or finish > shift["end"]:
            return False
        return all(finish <= b or begin >= e for b, e, _k, _d in shift["busy"])

    def reserve(ai, begin, finish, kind, data):
        shifts[(ai, utc_to_et_date(begin))]["busy"].append((begin, finish, kind, data))

    # ----------------------------------------------------------------------------------------------------------------
    # Random shapes shared by the generators
    # ----------------------------------------------------------------------------------------------------------------
    def date_weight(d):
        age = (today - d).days
        weight = 1.7 if age <= 6 else 1.0
        return weight * (0.6 if age == 0 else 1.0)

    weekdays = [d for d in all_dates if d.weekday() < 5]
    weekdays_w = [date_weight(d) for d in weekdays]
    hour_weights = [(9, 0.9), (10, 1.3), (11, 1.4), (12, 0.8), (13, 1.0), (14, 1.3), (15, 1.2), (16, 0.9), (17, 0.35)]

    def business_arrival():
        d = rng.choices(weekdays, weights=weekdays_w, k=1)[0]
        hour = wchoice(hour_weights)
        frac = rng.uniform(0, 0.5 if hour == 17 else 1.0)
        return local_to_utc(d, hour + frac) + dt.timedelta(seconds=rng.randint(0, 59))

    def after_hours_arrival():
        d = rng.choice(all_dates)
        if d.weekday() >= 5:
            hour = rng.uniform(9, 17)
        else:
            hour = rng.choice([rng.uniform(6.5, 8.9), rng.uniform(18.7, 21.5)])
        return local_to_utc(d, hour)

    def answer_wait(queue_key):
        """Seconds a caller waits before an agent answers; shaped so each queue lands near (a bit under) its SLA."""
        shapes = {
            "sales": [((4, 12), 80), ((12, 20), 12), ((20, 40), 6), ((40, 90), 2)],            # SLA 20 s
            "support": [((5, 18), 74), ((18, 30), 18), ((30, 60), 6), ((60, 150), 2)],         # SLA 30 s
            "billing": [((5, 25), 72), ((25, 60), 22), ((60, 120), 5), ((120, 200), 1)],       # SLA 60 s
        }
        low, high = wchoice(shapes[queue_key])
        return rng.uniform(low, high)

    def talk_seconds(median_minutes, sigma=0.55, long_chance=0.03):
        if rng.random() < long_chance:
            return rng.uniform(900, 1320)
        return clamp(math.exp(rng.gauss(math.log(median_minutes * 60), sigma)), 60, 899)

    def wrap_seconds():
        return rng.triangular(30, 150, 60)

    def outbound_talk():
        return clamp(math.exp(rng.gauss(math.log(210), 0.6)), 45, 780)

    # ----------------------------------------------------------------------------------------------------------------
    # 5. Omnichannel activities (and the outbound interactions of the completed ones)
    # ----------------------------------------------------------------------------------------------------------------
    interactions = []  # dicts with the Interaction fields; written to the database further down
    events = []        # InteractionEvent dicts
    activities = []

    def phone_of(person_):
        return person_["phone"]

    def make_event(event_type, aggregate_type, aggregate_id, when, data, interaction_id=None, actor_type="System",
                   actor_id="system", component="Queues"):
        events.append({
            "ItemId": new_id(), "InteractionId": interaction_id, "EventType": event_type, "SchemaVersion": 1,
            "AggregateType": aggregate_type, "AggregateId": aggregate_id, "ActorId": actor_id, "ActorType": actor_type,
            "SourceComponent": component, "OccurredUtc": when, "RecordedUtc": when + dt.timedelta(milliseconds=200),
            "Data": json.dumps(data, separators=(",", ":")), "Properties": {}})

    # Only resting statuses: the product's OrphanedActivityRecoveryService turns AwaitingAgentResponse /
    # AwaitingCustomerAnswer / InProgress / Reserved / Dialing activities that have no live call into Pending or Failed
    # shortly after the site starts, so the seed never writes those.
    status_plan = (["Completed"] * 130 + ["Scheduled"] * 58 + ["Pending"] * 30 + ["NotStated"] * 6 + ["Failed"] * 14 +
                   ["Cancelled"] * 12)
    rng.shuffle(status_plan)
    open_statuses = {"Scheduled", "Pending", "NotStated"}
    sms_only = set()

    def completed_disposition(campaign_key, answered):
        if not answered:
            return wchoice([("no_answer", 78), ("voicemail", 22)])
        if campaign_key == "renewals":
            return wchoice([("interested", 34), ("not_interested", 24), ("callback", 22), ("wrong_number", 6)])
        return wchoice([("interested", 44), ("not_interested", 12), ("callback", 20), ("wrong_number", 4)])

    NOTES = {
        "interested": ["Wants a quote before the end of the month.", "Happy to renew; sending the paperwork.",
                       "Asked for the next steps by email."],
        "not_interested": ["Already signed with another provider.", "Not the right time, try again next quarter."],
        "callback": ["Asked us to call back tomorrow morning.", "Busy right now, call after 3 pm."],
        "no_answer": ["Rang out, no voicemail.", "No answer after several rings."],
        "voicemail": ["Left a short voicemail with our direct line."],
        "wrong_number": ["The number belongs to someone else."]}

    def source_for(channel):
        if channel == "Phone":
            return wchoice([("Manual", 45), ("PreviewDial", 30), ("Callback", 10), ("Workflow", 8), ("Api", 7)])
        return wchoice([("Manual", 55), ("Workflow", 30), ("Api", 15)])

    for n, status in enumerate(status_plan):
        campaign_key = "renewals" if rng.random() < 0.6 else "welcome"
        channel = "SMS" if (status in sms_only or rng.random() < 0.4) else "Phone"
        person_ = rng.choice(people if rng.random() < 0.8 else leads)
        owner = rng.choice(agents)
        activity = {"campaign_key": campaign_key, "channel": channel, "person": person_, "owner": owner, "status": status,
                    "id": new_id(), "source": source_for(channel)}
        activities.append(activity)

        if status == "Completed" or status in ("Failed", "Cancelled"):
            # Scheduled in the past, in business hours; a completed one is finished within a few hours of its time.
            scheduled = business_arrival()
            scheduled = min(scheduled, NOW - dt.timedelta(hours=4))
            activity["scheduled"] = scheduled
            activity["created"] = scheduled - dt.timedelta(hours=rng.uniform(1, 40))
        elif rng.random() < 0.45:
            # Overdue: still open although its time has passed.
            scheduled = business_arrival()
            scheduled = min(scheduled, NOW - dt.timedelta(hours=2))
            activity["scheduled"] = scheduled
            activity["created"] = scheduled - dt.timedelta(hours=rng.uniform(1, 40))
        else:
            # Still to come in the next three days; created over the last couple of weeks.
            activity["scheduled"] = NOW + dt.timedelta(hours=rng.uniform(1, 72))
            activity["created"] = NOW - dt.timedelta(hours=rng.uniform(2, 14 * 24))
        activity["created"] = max(activity["created"], NOW - dt.timedelta(days=DAYS))
        activity["created"] = min(activity["created"], NOW - dt.timedelta(minutes=30))

    def new_interaction(activity_obj, direction, channel, queue_id, agent, created, answered, ended, status="Ended",
                        wrap=None, recording=False, phone=None, person_=None):
        """Builds the dict of an Interaction; the caller adds queue history / transfers."""
        item = {
            "ItemId": new_id(), "Channel": channel, "Direction": direction, "Status": status,
            "ProviderName": "Telnyx" if (channel == "Sms" or rng.random() < 0.7) else "Asterisk",
            "CustomerAddress": phone, "QueueId": queue_id, "AgentId": agent["id"] if agent else None,
            "CorrelationId": new_id(), "CreatedUtc": created, "ModifiedUtc": (wrap[1] if wrap else ended) or created,
            "StartedUtc": created + dt.timedelta(seconds=1), "AnsweredUtc": answered, "EndedUtc": ended,
            "WrapUpStartedUtc": wrap[0] if wrap else None, "WrapUpCompletedUtc": wrap[1] if wrap else None,
            "QueueHistory": [], "TransferHistory": [], "TechnicalMetadata": {},
            "ActivityItemId": activity_obj["id"] if activity_obj else None,
            "CreatedById": agent["user_id"] if (agent and direction == "Outbound") else None,
            "CreatedByUserName": agent["user"] if (agent and direction == "Outbound") else None,
            "_agent": agent, "_person": person_}
        item["ProviderInteractionId"] = ("call_" if channel == "Voice" else "msg_") + new_id()[:20]
        if recording:
            item["RecordingState"] = "Stopped"
            item["RecordingReference"] = "rec_" + new_id()[:20]
            item["RecordingDisclosedUtc"] = created + dt.timedelta(seconds=2)
        interactions.append(item)
        return item

    # --- finish the activities: completed ones get an outbound interaction, the rest only their status -------------
    for activity in activities:
        status, channel, person_, owner = activity["status"], activity["channel"], activity["person"], activity["owner"]
        activity["attempts"] = wchoice([(1, 45), (2, 30), (3, 15), (4, 10)]) if status == "Completed" else \
            (wchoice([(1, 40), (2, 35), (3, 20), (4, 5)]) if status in ("Failed", "Cancelled") else
             wchoice([(1, 60), (2, 25), (3, 15)]))
        activity["completed"] = None
        activity["disposition"] = None
        activity["notes"] = None
        scheduled = activity["scheduled"]
        ai = agents.index(owner)
        queue_id = "__campaign-queue__" + campaigns[activity["campaign_key"]]

        if status == "Completed":
            if channel == "Phone":
                outcome = wchoice([("answered", 62), ("no_answer", 28), ("voicemail", 10)])
                started = scheduled + dt.timedelta(minutes=rng.uniform(0, 90))
                placed = None
                for _try in range(40):
                    ring = rng.uniform(6, 28)
                    talk = outbound_talk() if outcome == "answered" else rng.uniform(22, 45)
                    wrap = wrap_seconds() if outcome == "answered" else 10
                    begin = started + dt.timedelta(seconds=ring)
                    finish = begin + dt.timedelta(seconds=talk + wrap)
                    if finish < NOW - dt.timedelta(minutes=10) and agent_free(ai, started, finish):
                        placed = (started, ring, talk, wrap)
                        break
                    started += dt.timedelta(minutes=rng.uniform(4, 25))
                if placed:
                    started, ring, talk, wrap = placed
                    answered = started + dt.timedelta(seconds=ring) if outcome == "answered" else None
                    # Answered: talk time starts at the answer; unanswered: the attempt ends after `talk` seconds.
                    ended = (answered if answered else started) + dt.timedelta(seconds=talk)
                    wrap_window = (ended, ended + dt.timedelta(seconds=wrap)) if outcome == "answered" else None
                    interaction = new_interaction(activity, "Outbound", "Voice", queue_id, owner, started, answered, ended,
                                                  wrap=wrap_window, recording=outcome == "answered" and rng.random() < 0.85,
                                                  phone=phone_of(person_), person_=person_)
                    interaction["QueueHistory"] = []
                    reserve(ai, started, wrap_window[1] if wrap_window else ended + dt.timedelta(seconds=10), "call",
                            {"answered": answered or started, "ended": ended, "wrap_end": wrap_window[1] if wrap_window else ended,
                             "interaction": interaction, "answered_call": outcome == "answered"})
                    activity["completed"] = (wrap_window[1] if wrap_window else ended) + dt.timedelta(seconds=rng.uniform(5, 40))
                else:
                    activity["completed"] = scheduled + dt.timedelta(hours=rng.uniform(0.5, 3))
                    outcome = "no_answer"
                answered_flag = outcome == "answered"
            else:
                outcome = "reply" if rng.random() < 0.55 else "no_reply"
                started = scheduled + dt.timedelta(minutes=rng.uniform(0, 120))
                if outcome == "reply":
                    answered = started + dt.timedelta(seconds=rng.uniform(40, 1500))
                    ended = answered + dt.timedelta(seconds=rng.uniform(120, 780))
                    wrap = (ended, ended + dt.timedelta(seconds=rng.uniform(20, 90)))
                else:
                    answered, ended, wrap = None, started + dt.timedelta(hours=rng.uniform(2, 20)), None
                if (wrap[1] if wrap else ended) < NOW - dt.timedelta(minutes=10):
                    interaction = new_interaction(activity, "Outbound", "Sms", queue_id, owner, started, answered, ended,
                                                  wrap=wrap, phone=phone_of(person_), person_=person_)
                    activity["completed"] = (wrap[1] if wrap else ended) + dt.timedelta(seconds=rng.uniform(5, 40))
                else:
                    activity["completed"] = started + dt.timedelta(hours=1)
                answered_flag = outcome == "reply"
            activity["completed"] = min(activity["completed"], NOW - dt.timedelta(minutes=5))
            disposition = completed_disposition(activity["campaign_key"], answered_flag)
            activity["disposition"] = dispositions[disposition]
            activity["notes"] = rng.choice(NOTES[disposition])
        elif status == "Failed" and channel == "Phone":
            started = scheduled + dt.timedelta(minutes=rng.uniform(0, 60))
            for _try in range(30):
                finish = started + dt.timedelta(seconds=15)
                if finish < NOW - dt.timedelta(minutes=10) and agent_free(ai, started, finish):
                    interaction = new_interaction(activity, "Outbound", "Voice", queue_id, owner, started, None,
                                                  started + dt.timedelta(seconds=rng.uniform(3, 9)), status="Failed",
                                                  phone=phone_of(person_), person_=person_)
                    reserve(ai, started, finish, "failed", None)
                    break
                started += dt.timedelta(minutes=rng.uniform(4, 25))

    # ----------------------------------------------------------------------------------------------------------------
    # 6. Inbound interactions: fill up to ~450 interactions in total
    # ----------------------------------------------------------------------------------------------------------------
    inbound_total = max(200, target_total - len(interactions))
    inbound_sms = round(inbound_total * 0.12)
    inbound_voice = inbound_total - inbound_sms

    queue_weights = [(k, share) for k, _n, _d, _sla, share in queue_defs]
    median_minutes = {"sales": 4.2, "support": 6.5, "billing": 4.8}

    def pick_caller():
        if rng.random() < 0.55:
            p = rng.choice(people)
            return p["phone"], p
        return rng.choice(stranger_numbers), None

    def eligible(queue_key):
        return [i for i, a in enumerate(agents) if queue_key in a["queue_keys"]]

    def add_queue_events(interaction, queue_id, created, kind, wait):
        make_event("CallQueued", "Interaction", interaction["ItemId"], created + dt.timedelta(seconds=1),
                   {"InteractionId": interaction["ItemId"], "QueueId": queue_id, "Direction": "Inbound",
                    "State": "Queued"}, interaction["ItemId"], component="Queues")
        if kind == "abandoned":
            make_event("CallAbandoned", "Interaction", interaction["ItemId"], created + dt.timedelta(seconds=wait),
                       {"InteractionId": interaction["ItemId"], "QueueId": queue_id, "Direction": "Inbound",
                        "State": "Abandoned", "Reason": "caller_hangup", "DurationSeconds": round(wait, 1)},
                       interaction["ItemId"], actor_type="Customer", actor_id=interaction["CustomerAddress"] or "customer",
                       component="Queues")
        elif kind == "voicemail":
            make_event("CallSentToVoicemail", "Interaction", interaction["ItemId"], created + dt.timedelta(seconds=wait),
                       {"InteractionId": interaction["ItemId"], "QueueId": queue_id, "Direction": "Inbound",
                        "State": "Voicemail", "DurationSeconds": round(wait, 1)}, interaction["ItemId"], component="Queues")
        elif kind == "callback":
            make_event("CallbackRequested", "Interaction", interaction["ItemId"], created + dt.timedelta(seconds=wait),
                       {"InteractionId": interaction["ItemId"], "QueueId": queue_id, "Direction": "Inbound",
                        "State": "CallbackRequested", "Reason": "queued_callback", "DurationSeconds": round(wait, 1)},
                       interaction["ItemId"], component="Queues")

    def finish_unanswered(kind, queue_key, created, business):
        queue_id = queues[queue_key]
        phone, person_ = pick_caller()
        if kind == "abandoned":
            wait = wchoice([(rng.uniform(8, 30), 35), (rng.uniform(30, 75), 40), (rng.uniform(75, 170), 25)])
            reason = "Abandoned"
        elif kind == "voicemail":
            wait = rng.uniform(8, 25) if not business else rng.uniform(60, 150)
            reason = "Voicemail"
        else:
            wait = rng.uniform(45, 140)
            reason = "CallbackRequested"
        ended = created + dt.timedelta(seconds=wait + (rng.uniform(35, 90) if kind == "voicemail" else 0))
        interaction = new_interaction(None, "Inbound", "Voice", queue_id, None, created, None, ended, phone=phone, person_=person_)
        interaction["QueueHistory"] = [{"QueueId": queue_id, "EnteredUtc": created, "ExitedUtc": created + dt.timedelta(seconds=wait),
                                        "ExitReason": reason}]
        if kind == "voicemail":
            interaction["TechnicalMetadata"] = {"agentVoicemailProjection": True}
        elif kind == "callback":
            interaction["TechnicalMetadata"] = {"routing_terminal_reason": "queued_callback"}
        add_queue_events(interaction, queue_id, created, kind, wait)

    # The mix of outcomes is planned up front (exact counts, shuffled) so the report figures do not depend on luck.
    after_hours_n = round(inbound_voice * 0.08)
    plan = [("voicemail", False)] * round(after_hours_n * 0.75) + [("abandoned", False)] * (after_hours_n - round(after_hours_n * 0.75))
    business_n = inbound_voice - after_hours_n
    n_abandoned, n_voicemail, n_callback = round(business_n * 0.05), round(business_n * 0.03), round(business_n * 0.035)
    plan += [("abandoned", True)] * n_abandoned + [("voicemail", True)] * n_voicemail + [("callback", True)] * n_callback
    plan += [("answered", True)] * (business_n - n_abandoned - n_voicemail - n_callback)
    rng.shuffle(plan)

    voice_made = 0
    guard = 0
    while plan and guard < 50000:
        guard += 1
        kind, business = plan[-1]
        queue_key = wchoice(queue_weights)
        created = business_arrival() if business else after_hours_arrival()
        if created > NOW - dt.timedelta(minutes=45):
            continue
        if kind != "answered":
            finish_unanswered(kind, queue_key, created, business=business)
            plan.pop()
            voice_made += 1
            continue
        wait = answer_wait(queue_key)
        talk, wrap = talk_seconds(median_minutes[queue_key]), wrap_seconds()
        answered = created + dt.timedelta(seconds=wait)
        ended = answered + dt.timedelta(seconds=talk)
        wrap_end = ended + dt.timedelta(seconds=wrap)
        if wrap_end > NOW - dt.timedelta(minutes=10):
            continue
        candidates = eligible(queue_key)
        rng.shuffle(candidates)
        ai = next((i for i in candidates if agent_free(i, answered, wrap_end)), None)
        if ai is None:
            continue
        agent = agents[ai]
        queue_id = queues[queue_key]
        phone, person_ = pick_caller()
        interaction = new_interaction(None, "Inbound", "Voice", queue_id, agent, created, answered, ended,
                                      wrap=(ended, wrap_end), recording=rng.random() < 0.82, phone=phone, person_=person_)
        interaction["QueueHistory"] = [{"QueueId": queue_id, "EnteredUtc": created, "ExitedUtc": answered, "ExitReason": "Answered"}]
        if rng.random() < 0.05:   # a few transfers
            kind_t = wchoice([("Agent", 60), ("Queue", 25), ("External", 15)])
            requested = answered + dt.timedelta(seconds=talk * 0.7)
            others = [a for a in agents if a is not agent and queue_key in a["queue_keys"]]
            if kind_t == "Agent":
                target = rng.choice(others)["id"]
                result = "Answered"
            elif kind_t == "Queue":
                target = queues[rng.choice([k for k in queues if k != queue_key])]
                result = "Answered"
            else:
                target = "+12125550911"
                result = "Sent to external number"
            interaction["TransferHistory"] = [{
                "FromParticipantId": agent["id"], "ToParticipantId": target, "TargetType": kind_t,
                "RequestedUtc": requested, "CompletedUtc": requested + dt.timedelta(seconds=rng.uniform(6, 40)),
                "Result": result}]
        reserve(ai, answered, wrap_end, "call", {"answered": answered, "ended": ended, "wrap_end": wrap_end,
                                                 "interaction": interaction, "answered_call": True})
        plan.pop()
        voice_made += 1

    # Inbound SMS: customer texts, an agent answers within minutes.
    sms_made = 0
    guard = 0
    while sms_made < inbound_sms and guard < 5000:
        guard += 1
        created = business_arrival()
        if created > NOW - dt.timedelta(minutes=60):
            continue
        queue_key = wchoice(queue_weights)
        candidates = [i for i in eligible(queue_key) if (i, utc_to_et_date(created)) in shifts]
        if not candidates:
            continue
        agent = agents[rng.choice(candidates)]
        answered = created + dt.timedelta(seconds=rng.uniform(8, 75))
        ended = answered + dt.timedelta(seconds=rng.uniform(150, 840))
        wrap = (ended, ended + dt.timedelta(seconds=rng.uniform(20, 80)))
        if wrap[1] > NOW - dt.timedelta(minutes=10):
            continue
        phone, person_ = pick_caller()
        interaction = new_interaction(None, "Inbound", "Sms", queues[queue_key], agent, created, answered, ended, wrap=wrap,
                                      phone=phone, person_=person_)
        interaction["QueueHistory"] = [{"QueueId": queues[queue_key], "EnteredUtc": created, "ExitedUtc": answered, "ExitReason": "Answered"}]
        sms_made += 1

    # ----------------------------------------------------------------------------------------------------------------
    # 7. Write the activities
    # ----------------------------------------------------------------------------------------------------------------
    for activity in activities:
        status = activity["status"]
        owner = activity["owner"]
        person_ = activity["person"]
        channel = activity["channel"]
        open_ = status in open_statuses
        unassigned = open_ and rng.random() < 0.15
        assigned_to = None if unassigned else owner
        assignment = "Unassigned" if unassigned else "Assigned"
        urgency = wchoice([("Normal", 70), ("Medium", 12), ("High", 12), ("Low", 6)])
        campaign_id = campaigns[activity["campaign_key"]]
        created, scheduled = activity["created"], activity["scheduled"]
        terminal = None
        if status == "Failed":
            terminal = wchoice([("max_attempts_reached", 60), ("invalid_number", 40)])
        elif status == "Cancelled":
            terminal = "cancelled_by_user"
        body = strip_nulls({
            "Kind": "Call" if channel == "Phone" else "Sms", "Source": activity["source"], "Channel": channel,
            "InteractionType": "Manual", "PreferredDestination": person_["phone"],
            "ContactContentItemId": person_["id"], "ContactContentType": person_["type"],
            "ContactResolutionStatus": "Resolved", "CampaignId": campaign_id, "ScheduledUtc": iso(scheduled),
            "Instructions": "Renewal call: confirm the plan and the new price." if activity["campaign_key"] == "renewals"
            else "Welcome call: check the first week and answer questions.",
            "Attempts": activity["attempts"], "MaxAttempts": 4,
            "AssignedToId": assigned_to["user_id"] if assigned_to else None,
            "AssignedToUsername": assigned_to["user"] if assigned_to else None,
            "AssignedToUtc": iso(created + dt.timedelta(minutes=5)) if assigned_to else None,
            "AssignmentStatus": assignment, "CreatedById": admin_id, "CreatedByUsername": "admin",
            "CompletedUtc": iso(activity["completed"]) if status == "Completed" else None,
            "CompletedById": owner["user_id"] if status == "Completed" else None,
            "CompletedByUsername": owner["user"] if status == "Completed" else None,
            "DispositionedBy": "User" if status == "Completed" else None,
            "DispositionId": activity["disposition"] if status == "Completed" else None,
            "Notes": activity["notes"] if status == "Completed" else None,
            "CreatedUtc": iso(created), "UrgencyLevel": urgency, "Status": status, "TerminalReasonCode": terminal,
            "ItemId": activity["id"], "Properties": {}})
        doc_id = s.alloc(OM)
        body["Id"] = doc_id
        c.execute(f"insert into [{OM}] (Id, Type, Content, Version) values (?,?,?,1)", (doc_id, T_ACTIVITY, jdump(body)))
        s.counts["OmnichannelActivity"] = s.counts.get("OmnichannelActivity", 0) + 1
        s.index("Omnichannel_OmnichannelActivityIndex", DocumentId=doc_id, ItemId=activity["id"],
                Kind=1 if channel == "Phone" else 3, Source=activity["source"], Channel=channel,
                ChannelEndpointId=None, PreferredDestination=person_["phone"], AIProfileName=None,
                ContactContentItemId=person_["id"], ContactContentType=person_["type"], CampaignId=campaign_id,
                DialerProfileId=None, SubjectContentType=None, ScheduledUtc=scheduled,
                CompletedUtc=activity["completed"] if status == "Completed" else None, Attempts=activity["attempts"],
                AssignedToId=assigned_to["user_id"] if assigned_to else None,
                AssignedToUtc=created + dt.timedelta(minutes=5) if assigned_to else None,
                AssignmentStatus={"Unassigned": 0, "Assigned": 3}[assignment], ReservationId=None,
                ReservedById=None, ReservedUtc=None, ReservationExpiresUtc=None, CreatedById=admin_id,
                DispositionId=activity["disposition"] if status == "Completed" else None, CreatedUtc=created,
                UrgencyLevel={"Normal": 0, "Low": 2, "Medium": 3, "High": 4}[urgency],
                Status={"NotStated": 0, "Completed": 3, "Pending": 4, "Scheduled": 5, "Failed": 9, "Cancelled": 10}[status],
                InteractionType=0, AiEscalated=False, TerminalReasonCode=terminal)
    # NOTE: the numeric values above are the enum values of ActivityKind / ActivityAssignmentStatus /
    # ActivityUrgencyLevel / ActivityStatus; the index tables store enums as integers.

    # ----------------------------------------------------------------------------------------------------------------
    # 8. Write the interactions (+ their events)
    # ----------------------------------------------------------------------------------------------------------------
    status_int = {"Created": 0, "Ringing": 1, "Connected": 2, "Held": 3, "Transferring": 4, "Conferenced": 5, "Ended": 6, "Failed": 7}
    channel_int = {"Voice": 0, "Sms": 1}
    direction_int = {"Inbound": 0, "Outbound": 1}
    recording_int = {None: 0, "None": 0, "Recording": 1, "Paused": 2, "Stopped": 3}
    for it in sorted(interactions, key=lambda x: x["CreatedUtc"]):
        agent, person_ = it.pop("_agent"), it.pop("_person")
        participants = [{"Role": "Customer", "Identifier": it["CustomerAddress"], "DisplayName": person_["name"] if person_ else None,
                         "Address": it["CustomerAddress"], "JoinedUtc": iso(it["CreatedUtc"]),
                         "LeftUtc": iso(it["EndedUtc"])}]
        if agent and (it["AnsweredUtc"] or it["Direction"] == "Outbound"):
            participants.append({"Role": "Agent", "Identifier": agent["id"], "DisplayName": agent["name"],
                                 "JoinedUtc": iso(it["AnsweredUtc"] or it["StartedUtc"]), "LeftUtc": iso(it["EndedUtc"])})
        body = {
            "Channel": it["Channel"], "Direction": it["Direction"], "Status": it["Status"],
            "ActivityItemId": it["ActivityItemId"], "ProviderName": it["ProviderName"],
            "ProviderInteractionId": it["ProviderInteractionId"], "CustomerAddress": it["CustomerAddress"],
            "QueueId": it["QueueId"], "AgentId": it["AgentId"], "CorrelationId": it["CorrelationId"],
            "RecordingReference": it.get("RecordingReference"), "RecordingState": it.get("RecordingState", "None"),
            "RecordingDisclosedUtc": iso(it.get("RecordingDisclosedUtc")),
            "TechnicalMetadata": it["TechnicalMetadata"],
            "QueueHistory": [{"QueueId": q["QueueId"], "EnteredUtc": iso(q["EnteredUtc"]), "ExitedUtc": iso(q["ExitedUtc"]),
                              "ExitReason": q["ExitReason"]} for q in it["QueueHistory"]],
            "TransferHistory": [{"FromParticipantId": t["FromParticipantId"], "ToParticipantId": t["ToParticipantId"],
                                 "TargetType": t["TargetType"], "RequestedUtc": iso(t["RequestedUtc"]),
                                 "CompletedUtc": iso(t["CompletedUtc"]), "Result": t["Result"]} for t in it["TransferHistory"]],
            "CreatedById": it["CreatedById"], "CreatedByUserName": it["CreatedByUserName"],
            "CreatedUtc": iso(it["CreatedUtc"]), "ModifiedUtc": iso(it["ModifiedUtc"]), "StartedUtc": iso(it["StartedUtc"]),
            "AnsweredUtc": iso(it["AnsweredUtc"]), "EndedUtc": iso(it["EndedUtc"]),
            "WrapUpStartedUtc": iso(it["WrapUpStartedUtc"]), "WrapUpCompletedUtc": iso(it["WrapUpCompletedUtc"]),
            "Participants": participants, "ItemId": it["ItemId"], "Properties": {}}
        body = strip_nulls(body)
        doc_id = s.doc(CC, T_INTERACTION, body)
        s.index("ContactCenter_InteractionIndex", DocumentId=doc_id, ItemId=it["ItemId"], Channel=channel_int[it["Channel"]],
                Direction=direction_int[it["Direction"]], Status=status_int[it["Status"]],
                ActivityItemId=it["ActivityItemId"], ProviderName=it["ProviderName"],
                ProviderInteractionId=it["ProviderInteractionId"], ProviderLegId=None, AgentId=it["AgentId"],
                CorrelationId=it["CorrelationId"], CreatedUtc=it["CreatedUtc"], EndedUtc=it["EndedUtc"],
                WrapUpStartedUtc=it["WrapUpStartedUtc"], WrapUpCompletedUtc=it["WrapUpCompletedUtc"],
                RecordingLegalHold=False, RecordingState=recording_int[it.get("RecordingState")],
                RecordingPausedUtc=None, QueueId=it["QueueId"])

    # ----------------------------------------------------------------------------------------------------------------
    # 9. Agent state-change events from the shifts (sign in, calls, wrap-up, breaks, sign out)
    # ----------------------------------------------------------------------------------------------------------------
    state_int = {"Offline": 0, "Available": 1, "Reserved": 2, "Busy": 3, "WrapUp": 4, "Break": 5, "Away": 7, "Meeting": 9, "Training": 10}
    reason_ids = dict(reason_codes)
    for ai, agent in enumerate(agents):
        previous = "Offline"
        for (index, _d), shift in sorted(((k, v) for k, v in shifts.items() if k[0] == ai), key=lambda kv: kv[1]["start"]):
            def change(when, new_state, source, reason=None, interaction_id=None):
                nonlocal previous
                make_event("AgentStateChanged", "AgentProfile", agent["id"], when, {
                    "AgentId": agent["id"], "UserId": agent["user_id"], "PreviousState": state_int[previous],
                    "CurrentState": state_int[new_state], "ReasonCodeId": reason_ids.get(reason) if reason else None,
                    "ReasonName": reason, "Source": source, "InteractionId": interaction_id,
                    "QueueIds": [q.lower() for q in agent["queue_ids"]], "CampaignIds": list(campaigns.values()),
                    "ChangedUtc": iso(when)}, actor_type="Agent", actor_id=agent["user_id"], component="Agents")
                previous = new_state

            change(shift["start"], "Available", "SignIn")
            for begin, finish, kind, data in sorted(shift["busy"], key=lambda b: b[0]):
                if kind == "state":
                    change(begin, data[0], "SetState", reason=data[1])
                    change(finish, "Available", "SetState")
                elif kind == "call":
                    iid = data["interaction"]["ItemId"]
                    if data["answered_call"]:
                        change(data["answered"], "Busy", "Connected", interaction_id=iid)
                        change(data["ended"], "WrapUp", "WrapUpStarted", interaction_id=iid)
                        change(data["wrap_end"], "Available", "WorkCompleted", interaction_id=iid)
                    else:   # an outbound attempt nobody answered: the agent was busy dialling for a moment
                        change(begin, "Busy", "Connected", interaction_id=iid)
                        change(finish, "Available", "Released", interaction_id=iid)
            change(shift["end"], "Offline", "SignOut")

    # Write every event document + its index row, oldest first.
    for ev in sorted(events, key=lambda e: e["OccurredUtc"]):
        body = strip_nulls({
            "InteractionId": ev["InteractionId"], "EventType": ev["EventType"], "SchemaVersion": ev["SchemaVersion"],
            "AggregateType": ev["AggregateType"], "AggregateId": ev["AggregateId"], "ActorId": ev["ActorId"],
            "ActorType": ev["ActorType"], "SourceComponent": ev["SourceComponent"],
            "OccurredUtc": iso(ev["OccurredUtc"]), "RecordedUtc": iso(ev["RecordedUtc"]), "Data": ev["Data"],
            "ItemId": ev["ItemId"], "Properties": {}})
        doc_id = s.doc(CC, T_EVENT, body)
        s.index("ContactCenter_InteractionEventIndex", DocumentId=doc_id, ItemId=ev["ItemId"],
                InteractionId=ev["InteractionId"], EventType=ev["EventType"], AggregateType=ev["AggregateType"],
                CorrelationId=None, IdempotencyKey=None, IdempotencyClaimKey=ev["ItemId"], OccurredUtc=ev["OccurredUtc"],
                AggregateId=ev["AggregateId"])

    c.commit()
    c.execute("pragma wal_checkpoint(truncate)")
    c.close()

    print(f"Seeded {db_path} relative to {NOW:%Y-%m-%d %H:%M} UTC")
    for name, count in sorted(s.counts.items()):
        print(f"  {name:28s} {count}")


if __name__ == "__main__":
    if len(sys.argv) not in (2, 3):
        raise SystemExit("usage: python seed-reports-data.py <path to OrchardCore.db> [total interactions, default 450]")
    main(sys.argv[1], int(sys.argv[2]) if len(sys.argv) == 3 else 450)
