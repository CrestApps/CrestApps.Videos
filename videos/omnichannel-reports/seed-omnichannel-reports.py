#!/usr/bin/env python3
"""
Second seed for the omnichannel-reports video. FAKE data only.

    python seed-omnichannel-reports.py <path to App_Data/Sites/Default/OrchardCore.db>

Run it on a STOPPED site, once, AFTER videos/reports-run-and-export/seed-reports-data.py (which creates the contacts,
leads, activities, campaigns, dispositions, queues and interactions). It only adds what the omnichannel reports need
to show a believable picture:

  * two more campaigns and a second campaign group (the activities are spread over four campaigns),
  * three addresses ("Address usage" report) and the address each activity used,
  * the real sources of activities: Manual, Preview/Power/Progressive/Predictive dialing, Callback and Automatic
    (an AI profile texting), instead of the generic ones the first seed writes,
  * AI conversations that were contained and others that were handed off to a live agent (the "AI handoff &
    containment" report), with the reason of each handoff,
  * activities completed before and after their scheduled time (the "Scheduled completion performance" report),
    other creators than the administrator, and a few assignment states,
  * more converted leads, and 26 sales opportunities in every stage (the CRM reports),
  * a few messaging conversations and messages (the Report Builder "Messaging" data sets).

Everything is relative to the current UTC time and deterministic. Names are the ones the first seed made up; phone
numbers are in the fictional 555-01xx range.
"""
import datetime as dt
import json
import random
import sqlite3
import sys

rng = random.Random(20261030)
NOW = dt.datetime.now(dt.timezone.utc).replace(tzinfo=None, microsecond=0)

OM = "Omnichannel_Document"
MS = "OmnichannelMessaging_Document"
DOC = "Document"
ASM_OM = "CrestApps.OrchardCore.Omnichannel.Core"
ALPHABET = "0123456789abcdefghijklmnopqrstuvwxyz"


def new_id():
    return "4" + "".join(rng.choice(ALPHABET) for _ in range(25))


def jdump(value):
    return json.dumps(value, ensure_ascii=False, separators=(",", ":"))


def iso(value):
    if value is None:
        return None
    return value.strftime("%Y-%m-%dT%H:%M:%S.") + f"{value.microsecond * 10:07d}Z"


def sql_dt(value):
    if value is None:
        return None
    text = value.strftime("%Y-%m-%d %H:%M:%S")
    if value.microsecond:
        text += "." + f"{value.microsecond * 10:07d}".rstrip("0")
    return text


def parse(text):
    if text is None:
        return None
    return dt.datetime.strptime(text[:19].replace(" ", "T"), "%Y-%m-%dT%H:%M:%S")


def wchoice(options):
    values, weights = zip(*options)
    return rng.choices(values, weights=weights, k=1)[0]


def dict_doc_type(model):
    return ("CrestApps.OrchardCore.Models.DictionaryDocument`1[[CrestApps.OrchardCore.Omnichannel.Core.Models."
            f"{model}, {ASM_OM}, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null]], "
            "CrestApps.OrchardCore.Abstractions")


class Db:
    def __init__(self, path):
        self.c = sqlite3.connect(path)
        self.next_id = {}

    def alloc(self, table):
        if table not in self.next_id:
            self.next_id[table] = self.c.execute(f"select coalesce(max(Id),0) from [{table}]").fetchone()[0]
        self.next_id[table] += 1
        return self.next_id[table]

    def doc(self, table, type_name, content):
        doc_id = self.alloc(table)
        self.c.execute(f"insert into [{table}] (Id, Type, Content, Version) values (?,?,?,1)", (doc_id, type_name, jdump(content)))
        return doc_id

    @staticmethod
    def val(value):
        if isinstance(value, dt.datetime):
            return sql_dt(value)
        if isinstance(value, bool):
            return 1 if value else 0
        return value

    def index(self, table, **columns):
        names = ",".join(f"[{k}]" for k in columns)
        marks = ",".join("?" for _ in columns)
        self.c.execute(f"insert into [{table}] ({names}) values ({marks})", [self.val(v) for v in columns.values()])

    def update(self, table, where, **columns):
        sets = ",".join(f"[{k}]=?" for k in columns)
        self.c.execute(f"update [{table}] set {sets} where {where[0]}", [self.val(v) for v in columns.values()] + list(where[1:]))


def main(path):
    db = Db(path)
    c = db.c

    if c.execute("select count(*) from Omnichannel_OmnichannelActivityIndex where Source in ('Automatic','PowerDial','ProgressiveDial','PredictiveDial')").fetchone()[0]:
        raise SystemExit("This site was already seeded by seed-omnichannel-reports.py.")

    users = {}
    for _id, content in c.execute("select Id, Content from Document where Type like 'OrchardCore.Users.Models.User,%'"):
        user = json.loads(content)
        users[user["UserName"]] = user["UserId"]
    agents = [users[n] for n in ("jamie.chen", "maria.lopez", "sam.patel", "alex.kim", "priya.shah")]
    agent_names = {users[n]: n for n in ("jamie.chen", "maria.lopez", "sam.patel", "alex.kim", "priya.shah")}
    admin_id = users["admin"]

    # ------------------------------------------------------------------------------------------------------------
    # Campaigns and groups (catalog dictionary documents of the Omnichannel collection)
    # ------------------------------------------------------------------------------------------------------------
    def dictionary(model):
        row = c.execute(f"select Id, Content from [{DOC}] where Type = ?", (dict_doc_type(model),)).fetchone()
        return row[0], json.loads(row[1])

    gid, groups = dictionary("OmnichannelCampaignGroup")
    group_ids = list(groups["Records"])
    lifecycle = group_ids[0]
    acquisition = new_id()
    groups["Records"][acquisition] = {
        "DisplayText": "Lead generation", "Description": "Campaigns that turn new leads into customers.",
        "CreatedUtc": iso(NOW - dt.timedelta(days=60)), "Author": "admin", "OwnerId": admin_id, "ItemId": acquisition, "Properties": {}}
    c.execute(f"update [{DOC}] set Content=? where Id=?", (jdump(groups), gid))

    cid_, campaigns = dictionary("OmnichannelCampaign")
    old_campaigns = list(campaigns["Records"])
    extra = [
        ("Win back lapsed customers", "Call customers who stopped ordering in the last year.", lifecycle, "Phone"),
        ("Trade show follow-up", "Text and call everyone who visited the booth.", acquisition, "SMS"),
    ]
    campaign_ids = list(old_campaigns)
    for name, description, group, channel in extra:
        item = new_id()
        campaign_ids.append(item)
        campaigns["Records"][item] = {
            "DisplayText": name, "Description": description, "CampaignGroupId": group, "InteractionType": "Manual",
            "Channel": channel, "CreatedUtc": iso(NOW - dt.timedelta(days=60)), "Author": "admin", "OwnerId": admin_id,
            "AllowAIToUpdateSubject": True, "ItemId": item, "Properties": {}}
    c.execute(f"update [{DOC}] set Content=? where Id=?", (jdump(campaigns), cid_))

    # ------------------------------------------------------------------------------------------------------------
    # Addresses ("Address usage")
    # ------------------------------------------------------------------------------------------------------------
    eid, endpoints = dictionary("OmnichannelChannelEndpoint")
    address_ids = {}
    for name, channel, address_type, value in [
        ("Sales line", "Phone", "Phone", "+12125550100"),
        ("Support line", "Phone", "Phone", "+12125550101"),
        ("Text line", "SMS", "Phone", "+12125550102"),
    ]:
        item = new_id()
        address_ids[name] = item
        endpoints["Records"][item] = {
            "DisplayText": name, "Channel": channel, "AddressType": address_type, "Capabilities": [channel],
            "MergedItemIds": [], "Value": value, "Description": "", "CreatedUtc": iso(NOW - dt.timedelta(days=60)),
            "Author": "admin", "OwnerId": admin_id, "ItemId": item, "Properties": {}}
    c.execute(f"update [{DOC}] set Content=? where Id=?", (jdump(endpoints), eid))

    # ------------------------------------------------------------------------------------------------------------
    # Activities
    # ------------------------------------------------------------------------------------------------------------
    unit = {"Normal": 0, "Low": 2, "Medium": 3, "High": 4}
    rows = c.execute(f"select Id, Content from [{OM}] where Type like '%OmnichannelActivity,%'").fetchall()
    ai_profile_id = new_id()

    for doc_id, content in rows:
        a = json.loads(content)
        status = a["Status"]
        channel = a["Channel"]
        sms = channel == "SMS"
        scheduled = parse(a["ScheduledUtc"])
        created = parse(a["CreatedUtc"])
        completed = parse(a.get("CompletedUtc"))
        changes = {}

        # The campaign: spread over four campaigns, keep the seed's two main ones the most used.
        campaign = wchoice(list(zip(campaign_ids, (38, 28, 18, 16))))
        if campaign_ids.index(campaign) == 3 and not sms:
            campaign = campaign_ids[2]
        a["CampaignId"] = campaign

        # The source.
        if sms:
            source = wchoice([("Automatic", 62), ("Manual", 38)]) if status in ("Completed", "Failed") else wchoice([("Automatic", 35), ("Manual", 65)])
        else:
            old = a["Source"]
            source = {"Workflow": wchoice([("PowerDial", 5), ("ProgressiveDial", 4), ("PredictiveDial", 3)]),
                      "Api": wchoice([("ProgressiveDial", 4), ("PredictiveDial", 3)]),
                      "Manual": wchoice([("Manual", 70), ("PowerDial", 12), ("ProgressiveDial", 10), ("PredictiveDial", 8)]),
                      }.get(old, old)
        a["Source"] = source

        # An automatic conversation: an AI profile texting. Some are handed off to a live agent.
        escalated = False
        ai_name = None
        if source == "Automatic":
            a["AIProfileId"] = ai_profile_id
            ai_name = "Qualify leads by text"
            a["InteractionType"] = "Automated"
            if status == "Completed":
                a["DispositionedBy"] = "AIAgent"
                a["DispositionedByAIProfileId"] = ai_profile_id
                if rng.random() < 0.30:
                    escalated = True
                    a["AiEscalated"] = True
                    a["TerminalReasonCode"] = wchoice([("handed_off_to_agent", 72), ("handed_off_after_hours_callback", 28)])
                    a["HandoffSummaryUtc"] = iso(created + dt.timedelta(minutes=rng.uniform(3, 40)))
                    a["HandoffSummary"] = "The customer asked for a person to explain the new price."
        interaction_type = 1 if source == "Automatic" else 0

        # The address.
        if sms:
            address = address_ids["Text line"]
        else:
            address = address_ids["Sales line"] if campaign in (campaign_ids[0], campaign_ids[2]) else address_ids["Support line"]
        a["ChannelEndpointId"] = address

        # The creator.
        creator_id, creator_name = admin_id, "admin"
        if source in ("Manual", "Callback"):
            creator_id = rng.choice(agents)
            creator_name = agent_names[creator_id]
        elif source == "Automatic":
            creator_id, creator_name = None, None
        a["CreatedById"] = creator_id
        a["CreatedByUsername"] = creator_name
        if creator_id is None:
            a.pop("CreatedById"); a.pop("CreatedByUsername")

        # Completed before or after the scheduled time.
        if status == "Completed" and completed is not None:
            if escalated or source == "Automatic":
                completed = max(created + dt.timedelta(minutes=6), min(completed, created + dt.timedelta(hours=rng.uniform(0.2, 3))))
            elif rng.random() < 0.42:
                completed = scheduled - dt.timedelta(minutes=rng.uniform(1, 38))
                completed = max(completed, created + dt.timedelta(minutes=2))
            completed = min(completed, NOW - dt.timedelta(minutes=5))
            a["CompletedUtc"] = iso(completed)

        # A few assignment states beyond Assigned / Unassigned.
        assignment = a["AssignmentStatus"]
        if status in ("Pending", "Scheduled", "NotStated") and assignment == "Unassigned" and rng.random() < 0.55:
            assignment = "Available"
        if status in ("Failed", "Cancelled") and rng.random() < 0.35:
            assignment = "Released"
        a["AssignmentStatus"] = assignment
        a["Kind"] = "Sms" if sms else "Call"

        content_out = jdump(a)
        c.execute(f"update [{OM}] set Content=? where Id=?", (content_out, doc_id))
        assignment_int = {"Unassigned": 0, "Available": 1, "Reserved": 2, "Assigned": 3, "InProgress": 4, "Released": 5}[assignment]
        db.update("Omnichannel_OmnichannelActivityIndex", ("DocumentId=?", doc_id),
                  Source=source, CampaignId=campaign, ChannelEndpointId=address, AIProfileName=ai_name,
                  CompletedUtc=completed if status == "Completed" else None, CreatedById=creator_id,
                  AssignmentStatus=assignment_int, AiEscalated=escalated, TerminalReasonCode=a.get("TerminalReasonCode"),
                  InteractionType=interaction_type)

    # ------------------------------------------------------------------------------------------------------------
    # Leads: more conversions
    # ------------------------------------------------------------------------------------------------------------
    statuses = dict_status = None
    srow = c.execute(f"select Content from [{DOC}] where Type like '%LeadStatus,%'").fetchone()
    status_records = json.loads(srow[0])["Records"]
    converted_status = next(k for k, v in status_records.items() if v["Name"] == "Converted")
    contacts = [r[0] for r in c.execute("select ContentItemId from ContentItemIndex where ContentType='Contact' and Published=1")]
    open_leads = c.execute("select DocumentId, ContentItemId, CreatedUtc from LeadIndex where IsConverted=0 and IsClosed=0 order by Id").fetchall()
    rng.shuffle(open_leads)
    converted_leads = []

    for doc_id, item_id, created_text in open_leads[:11]:
        row = c.execute(f"select Content from [{DOC}] where Id=?", (doc_id,)).fetchone()
        item = json.loads(row[0])
        created = parse(created_text)
        when = min(created + dt.timedelta(days=rng.uniform(1, 8), hours=rng.uniform(0, 6)), NOW - dt.timedelta(hours=5))
        contact = rng.choice(contacts)
        part = item["LeadPart"]
        part.update({"IsClosed": True, "IsConverted": True, "StatusId": converted_status, "ConvertedUtc": iso(when),
                     "ConvertedContactItemId": contact, "ConvertedContactType": "Contact"})
        c.execute(f"update [{DOC}] set Content=? where Id=?", (jdump(item), doc_id))
        db.update("LeadIndex", ("DocumentId=?", doc_id), StatusId=converted_status, IsClosed=True, IsConverted=True,
                  ConvertedContactItemId=contact, ConvertedUtc=when)
        for table in ("OmnichannelContactIndex", "OmnichannelContactPhoneNumberIndex"):
            db.update(table, ("DocumentId=?", doc_id), IsConverted=True)
        converted_leads.append(item_id)

    # ------------------------------------------------------------------------------------------------------------
    # Sales opportunities
    # ------------------------------------------------------------------------------------------------------------
    orow = c.execute(f"select Content from [{DOC}] where Type like '%OpportunityStage,%'").fetchone()
    stages = sorted(json.loads(orow[0])["Records"].values(), key=lambda s: s["Order"])
    stage_by_name = {s["Name"]: s for s in stages}
    lead_sources = [r[0] for r in c.execute("select ContentItemId from ContentItemIndex where ContentType='LeadSource' and Published=1")]
    prefixes = ["Renewal", "New plan", "Upgrade", "Expansion", "Multi-year renewal", "Pilot"]
    last_names = [r[0] for r in c.execute("select DisplayText from ContentItemIndex where ContentType='Contact' and Published=1 limit 26")]
    plan = (["Prospecting"] * 6 + ["Qualification"] * 5 + ["Proposal"] * 5 + ["Negotiation"] * 3
            + ["Closed Won"] * 4 + ["Closed Lost"] * 3)
    contact_rows = c.execute("select ContentItemId, DisplayText from ContentItemIndex where ContentType='Contact' and Published=1 order by Id").fetchall()

    for n, stage_name in enumerate(plan):
        stage = stage_by_name[stage_name]
        person = contact_rows[n % len(contact_rows)]
        title = f"{prefixes[n % len(prefixes)]} - {person[1].split(' ')[-1]}"
        amount = float(rng.choice([2400, 3600, 4800, 7200, 9600, 12000, 18000, 24000, 36000]))
        closed = stage["IsClosed"]
        close_date = (NOW - dt.timedelta(days=rng.uniform(1, 30))) if closed else (NOW + dt.timedelta(days=rng.uniform(5, 75)))
        created = NOW - dt.timedelta(days=rng.uniform(10, 44))
        owner = rng.choice(agents)
        source = rng.choice(lead_sources)
        item_id = new_id()
        item = {
            "ContentItemId": item_id, "ContentItemVersionId": new_id(), "ContentType": "SalesOpportunity",
            "DisplayText": title, "Latest": True, "Published": True, "ModifiedUtc": iso(created), "PublishedUtc": iso(created),
            "CreatedUtc": iso(created), "Owner": admin_id, "Author": "admin", "TitlePart": {"Title": title},
            "OpportunityPart": {
                "StageId": stage["ItemId"], "IsClosed": closed, "IsWon": stage["IsWon"], "Probability": stage["Probability"],
                "Amount": {"Value": amount}, "CloseDate": {"Value": iso(close_date)}, "Owner": {"UserIds": [owner]},
                "Source": {"ContentItemIds": [source]}, "CampaignId": campaign_ids[n % 2],
                "PrimaryContact": {"ContentItemIds": [person[0]]},
                "ConvertedFromLeadItemId": converted_leads[n] if n < len(converted_leads) and n % 2 == 0 else None}}
        doc_id = db.doc(DOC, "OrchardCore.ContentManagement.ContentItem, OrchardCore.ContentManagement.Abstractions", item)
        db.index("ContentItemIndex", DocumentId=doc_id, ContentItemId=item_id, ContentItemVersionId=item["ContentItemVersionId"],
                 Latest=True, Published=True, ContentType="SalesOpportunity", ModifiedUtc=created, PublishedUtc=created,
                 CreatedUtc=created, Owner=admin_id, Author="admin", DisplayText=title)
        db.index("OpportunityIndex", DocumentId=doc_id, ContentItemId=item_id, ContentType="SalesOpportunity", Published=True,
                 Latest=True, StageId=stage["ItemId"], IsClosed=closed, IsWon=stage["IsWon"], Probability=stage["Probability"],
                 Amount=amount, CloseDate=close_date, OwnerId=owner, AccountContentItemId=None, CampaignId=campaign_ids[n % 2],
                 PrimaryContactItemId=person[0], SourceId=source,
                 ConvertedFromLeadItemId=item["OpportunityPart"]["ConvertedFromLeadItemId"], CreatedUtc=created)

    # ------------------------------------------------------------------------------------------------------------
    # Messaging conversations and messages (the Report Builder "Messaging" data sets)
    # ------------------------------------------------------------------------------------------------------------
    cc_agents = []
    for _id, content in c.execute("select Id, Content from ContactCenter_Document where Type like '%AgentProfile,%'"):
        profile = json.loads(content)
        cc_agents.append(profile["ItemId"])
    phones = [r[0] for r in c.execute("select PrimaryCellPhoneNumber from OmnichannelContactIndex where PrimaryCellPhoneNumber is not null order by Id limit 40")]
    conversation_type = "CrestApps.OrchardCore.Omnichannel.Messaging.Core.Models.MessagingConversation, CrestApps.OrchardCore.Omnichannel.Messaging.Core"
    message_type = "CrestApps.OrchardCore.Omnichannel.Core.Models.OmnichannelMessage, CrestApps.OrchardCore.Omnichannel.Core"
    inbound_lines = ["Hi, can I move my appointment to Thursday?", "Do you have the new price list?", "Thanks, that works for me.",
                     "I did not get the invoice.", "Can someone call me back after 3?", "Is the renewal still open?"]
    outbound_lines = ["Hello, this is Contoso. Happy to help with that.", "Yes, Thursday at 10 works.", "I sent the price list to your email.",
                      "We will call you after 3 pm today.", "Your renewal is open until the end of the month."]
    for n in range(28):
        started = NOW - dt.timedelta(days=rng.uniform(0.2, 28), hours=rng.uniform(0, 6))
        phone = "+1" + phones[n % len(phones)] if not phones[n % len(phones)].startswith("+") else phones[n % len(phones)]
        agent = rng.choice(cc_agents)
        closed = rng.random() < 0.55
        conv_id = new_id()
        step = rng.randint(3, 6)
        responded = started + dt.timedelta(minutes=rng.uniform(1, 25))
        last = started + dt.timedelta(minutes=rng.uniform(20, 240))
        conversation = {
            "Channel": "SMS", "ServiceAddress": "+12125550102", "ContactAddress": phone, "OwnerType": "Queue",
            "AssignedAgentId": agent, "AssignmentStatus": "Assigned", "Status": "Closed" if closed else "Open",
            "IsRead": True, "UnreadCount": 0, "LastMessageUtc": iso(last), "LastActivityUtc": iso(last),
            "LastMessagePreview": rng.choice(inbound_lines), "AssignedUtc": iso(started + dt.timedelta(minutes=1)),
            "ReassignmentAttempts": 0, "CreatedUtc": iso(started), "ModifiedUtc": iso(last),
            "FirstResponseDueUtc": iso(started + dt.timedelta(minutes=15)), "FirstRespondedUtc": iso(responded),
            "FirstResponseBreached": responded > started + dt.timedelta(minutes=15), "History": [],
            "ItemId": conv_id, "Properties": {}}
        doc_id = db.doc(MS, conversation_type, conversation)
        db.index("OmnichannelMessaging_MessagingConversationIndex", DocumentId=doc_id, ItemId=conv_id, Channel="SMS",
                 ServiceAddress="+12125550102", ContactAddress=phone, ContactContentItemId=None,
                 CustomerKey=f"address:SMS:{phone}", OwnerType="Queue", OwnerId=None, AssignedAgentId=agent,
                 AssignmentStatus="Assigned", Status=conversation["Status"], IsRead=True, LastMessageUtc=last,
                 LastActivityUtc=last, UnreadCount=0, AssignedUtc=started + dt.timedelta(minutes=1),
                 FirstResponseDueUtc=started + dt.timedelta(minutes=15))
        when = started
        for k in range(step):
            inbound = k % 2 == 0
            when = when + dt.timedelta(minutes=rng.uniform(1, 18))
            message = {
                "Id": new_id(), "Channel": "SMS", "CustomerAddress": phone, "ServiceAddress": "+12125550102",
                "Content": rng.choice(inbound_lines if inbound else outbound_lines), "CreatedUtc": iso(when),
                "IsInbound": inbound, "ConversationId": conv_id, "MediaReferences": [], "Properties": {}}
            if not inbound:
                message["SentByAgentId"] = agent
                message["DeliveryStatus"] = wchoice([("Delivered", 86), ("Sent", 6), ("Undelivered", 5), ("Failed", 3)])
                if message["DeliveryStatus"] in ("Undelivered", "Failed"):
                    message["ErrorCode"] = wchoice([("30003", 1), ("30005", 1)])
            mid = db.doc(OM, message_type, message)
            db.index("Omnichannel_OmnichannelMessageIndex", DocumentId=mid, Channel="SMS", CustomerAddress=phone,
                     ServiceAddress="+12125550102", CreatedUtc=when, IsInbound=inbound, ConversationId=conv_id,
                     ProviderMessageId=None)

    c.commit()
    print("Seeded the omnichannel report data:", len(rows), "activities updated,", len(converted_leads), "more converted leads,", len(plan), "opportunities")


if __name__ == "__main__":
    if len(sys.argv) != 2:
        raise SystemExit(__doc__)
    main(sys.argv[1])
