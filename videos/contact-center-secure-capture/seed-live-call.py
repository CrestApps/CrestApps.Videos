#!/usr/bin/env python3
"""
Seeds ONE live inbound voice call for the signed-in `admin` agent on a FAKE demo CrestApps.OrchardCore site, so the
agent workspace (/Admin/contact-center/workspace) shows an active call panel with "Collect data securely" and
"Pause recording" - the starting point of the "Secure data capture" video.

    python seed-live-call.py <App_Data dir> [--reset] [--secure-capture on|off] [--hide-softphone] [--answered-seconds-ago N]

* Run it on a STOPPED site (it writes the SQLite file directly). <App_Data dir> is the folder that contains Sites/Default.
* Idempotent: running it again does nothing to the call unless you pass --reset, which removes the call (and every secure
  capture session / event that was recorded against it) and creates it again with fresh times (relative to now, UTC).
* Everything is fake: caller "Casey Morgan", +1 212 555 0142, queue "Support" (must already exist), agent = the admin user.

What it creates (documents + the index rows YesSql keeps for them)
  - an agent profile for `admin` (or updates the one that exists): signed into the Support queue, presence Busy
  - a "Contact" content item "Casey Morgan" (and the Contact content type if the site has none, so "Open customer record" works)
  - an Omnichannel activity (Inbound, Call, In progress, assigned to admin) linked to that contact: the workspace takes the
    customer name and the "Complete activity" button from it
  - the Interaction (Voice, Inbound, Connected, provider Asterisk, recording state Recording, disclosure already given)
  - the CallSession that CallControlAuthorizationService requires before a secure capture may start (non terminal, same
    provider and call id as the interaction, owned by the agent)
  - four audit events (InteractionCreated, CallQueued, CallConnected, RecordingStarted)
  - site settings: Secure Data Capture enabled (--secure-capture on, the default) and "Allow agents to pause recording"
    (so the "Pause recording" button shows). Use --secure-capture off to leave the capture setting off, so the settings
    page can be shown being turned on.

Why provider "Asterisk": the workspace only shows "Pause recording" when the call's voice provider reports the
Recording and RecordingPause capabilities (Asterisk does, Telnyx does not). Nothing ever reaches the provider: the site is
started behind a dead proxy and the pause simply fails (the capture itself carries on).
"""
import argparse
import datetime as dt
import json
import os
import random
import sqlite3
import sys

TAG = "seed-live-call"
CC = "ContactCenter_Document"
OM = "Omnichannel_Document"
DOC = "Document"
ASM_CC = "CrestApps.OrchardCore.ContactCenter.Core"
ASM_OM = "CrestApps.OrchardCore.Omnichannel.Core"
T_AGENT = f"CrestApps.OrchardCore.ContactCenter.Core.Models.AgentProfile, {ASM_CC}"
T_INTERACTION = f"CrestApps.OrchardCore.ContactCenter.Core.Models.Interaction, {ASM_CC}"
T_EVENT = f"CrestApps.OrchardCore.ContactCenter.Core.Models.InteractionEvent, {ASM_CC}"
T_CALL_SESSION = f"CrestApps.OrchardCore.ContactCenter.Core.Models.CallSession, {ASM_CC}"
T_ACTIVITY = f"CrestApps.OrchardCore.Omnichannel.Core.Models.OmnichannelActivity, {ASM_OM}"
T_CONTENT_ITEM = "OrchardCore.ContentManagement.ContentItem, OrchardCore.ContentManagement.Abstractions"

CALLER_NAME = "Casey Morgan"
CALLER_PHONE = "+12125550142"
QUEUE_NAME = "Support"
PBX_CALL_ID = "1798590000.4421"          # Asterisk unique id of the (fake) call
ALPHABET = "0123456789abcdefghijklmnopqrstuvwxyz"
rng = random.Random("seed-live-call-v1")   # fixed ids, so the script is idempotent and a reset recreates the same ids


def new_id():
    return "4" + "".join(rng.choice(ALPHABET) for _ in range(25))


# Fixed identifiers (generated in a fixed order).
CONTACT_ID, CONTACT_VERSION_ID, ACTIVITY_ID, INTERACTION_ID, SESSION_ID = (new_id() for _ in range(5))
CORRELATION_ID = "seed-" + new_id()[:20]
CUSTOMER_LEG_ID, AGENT_LEG_ID = "PJSIP/trunk-0000c0de", "PJSIP/agent-0000c0df"
AGENT_PROFILE_ID = new_id()
EVENT_IDS = [new_id() for _ in range(4)]


def jdump(value):
    return json.dumps(value, ensure_ascii=False, separators=(",", ":"))


def iso(value):
    return None if value is None else value.strftime("%Y-%m-%dT%H:%M:%S.") + f"{value.microsecond * 10:07d}Z"


def sql_dt(value):
    if value is None:
        return None
    text = value.strftime("%Y-%m-%d %H:%M:%S")
    if value.microsecond:
        text += "." + f"{value.microsecond * 10:07d}".rstrip("0")
    return text


def strip_nulls(value):
    return {k: v for k, v in value.items() if v is not None}


class Store:
    def __init__(self, path):
        self.conn = sqlite3.connect(path, timeout=30)
        self.conn.execute("pragma busy_timeout=30000")
        self.next_id = {}

    def tables(self):
        return {r[0] for r in self.conn.execute("select name from sqlite_master where type='table'")}

    def alloc(self, table):
        if table not in self.next_id:
            self.next_id[table] = self.conn.execute(f"select coalesce(max(Id),0) from [{table}]").fetchone()[0]
        self.next_id[table] += 1
        return self.next_id[table]

    def doc(self, table, type_name, content):
        doc_id = self.alloc(table)
        self.conn.execute(f"insert into [{table}] (Id, Type, Content, Version) values (?,?,?,1)",
                          (doc_id, type_name, jdump(content)))
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

    def index_tables(self, collection_table):
        """The index tables YesSql keeps for a document table (they all carry a DocumentId column)."""
        names = []
        for t in sorted(self.tables()):
            cols = {r[1] for r in self.conn.execute(f"pragma table_info([{t}])")}
            if "DocumentId" not in cols or t.endswith("_Document"):
                continue
            if collection_table == CC and t.startswith("ContactCenter_"):
                names.append(t)
            elif collection_table == OM and t.startswith("Omnichannel_"):
                names.append(t)
            elif collection_table == DOC and t == "ContentItemIndex":
                names.append(t)
        return names


def delete_documents(s, table, where_sql, params):
    ids = [r[0] for r in s.conn.execute(f"select Id from [{table}] where {where_sql}", params)]
    if not ids:
        return 0
    marks = ",".join("?" for _ in ids)
    for t in s.index_tables(table):
        s.conn.execute(f"delete from [{t}] where DocumentId in ({marks})", ids)
    s.conn.execute(f"delete from [{table}] where Id in ({marks})", ids)
    return len(ids)


def reset(s):
    removed = 0
    # Everything the seed made, plus whatever a capture recorded against the call (sessions, events, secure capture events).
    removed += delete_documents(s, CC, "(Content like ? or Content like ?) and Type not like '%AgentProfile%'",
                                (f'%"SeedTag":"{TAG}"%', f'%{INTERACTION_ID}%'))
    removed += delete_documents(s, OM, "Content like ?", (f'%"SeedTag":"{TAG}"%',))
    removed += delete_documents(s, DOC, "Type = ? and Content like ?", (T_CONTENT_ITEM, f'%"ContentItemId":"{CONTACT_ID}"%'))
    return removed


def find_admin(c):
    cols = [r[1] for r in c.execute("pragma table_info(UserIndex)")]
    row = c.execute("select * from UserIndex where NormalizedUserName='ADMIN'").fetchone()
    if not row:
        raise SystemExit("There is no user named admin on this site.")
    return row[cols.index("UserId")]


def find_queue(c, name):
    row = c.execute("select ItemId from ContactCenter_ActivityQueueIndex where Name=?", (name,)).fetchone()
    if not row:
        raise SystemExit(f'The queue "{name}" does not exist on this site.')
    return row[0]


def ensure_contact_type(c):
    row = c.execute("select Id, Content from Document where Type like 'OrchardCore.ContentManagement.Metadata.Records.ContentDefinitionRecord%'").fetchone()
    definition = json.loads(row[1])
    if any(t["Name"] == "Contact" for t in definition["ContentTypeDefinitionRecords"]):
        return False
    definition["ContentTypeDefinitionRecords"].append({
        "Name": "Contact", "DisplayName": "Contact",
        "Settings": {"ContentTypeSettings": {"Creatable": True, "Listable": True, "Draftable": False, "Versionable": False, "Securable": True}},
        "ContentTypePartDefinitionRecords": [
            {"PartName": "TitlePart", "Name": "TitlePart", "Settings": {"ContentTypePartSettings": {"Position": "1"}}}]})
    definition["Identifier"] = new_id()
    c.execute("update Document set Content=? where Id=?", (jdump(definition), row[0]))
    return True


def patch_site_settings(c, secure_capture_on, hide_softphone=False):
    row = c.execute("select Id, Content from Document where Type like 'OrchardCore.Settings.SiteSettings%'").fetchone()
    site = json.loads(row[1])
    props = site.setdefault("Properties", {})
    props["SecureCaptureSettings"] = {"Enabled": bool(secure_capture_on), "LinkTimeToLiveSeconds": 300, "PauseRecordingDuringCapture": True}
    recording = props.setdefault("ContactCenterRecordingSettings", {})
    recording["AllowAgentSecurePause"] = True
    recording.setdefault("RequirePauseReason", False)
    if hide_softphone:
        # The floating soft phone panel ("On a call ...") otherwise sits over the page on every admin screen during the call.
        props.setdefault("SoftPhoneWidgetSettings", {})["DisplayOnAdmin"] = False
    site["Identifier"] = new_id()
    c.execute("update Document set Content=? where Id=?", (jdump(site), row[0]))


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("app_data", help="the App_Data folder of the (stopped) site")
    parser.add_argument("--reset", action="store_true", help="remove the call (and its capture sessions/events) and create it again")
    parser.add_argument("--secure-capture", choices=["on", "off"], default="on", help="state of the Secure Data Capture setting (default on)")
    parser.add_argument("--hide-softphone", action="store_true", help="do not show the floating soft phone panel on admin pages")
    parser.add_argument("--answered-seconds-ago", type=int, default=75, help="how long ago the agent answered (the talk time starts here)")
    args = parser.parse_args()

    db_path = os.path.join(args.app_data, "Sites", "Default", "OrchardCore.db")
    if not os.path.exists(db_path):
        raise SystemExit(f"{db_path} does not exist.")

    now = dt.datetime.now(dt.timezone.utc).replace(tzinfo=None, microsecond=0)
    answered = now - dt.timedelta(seconds=args.answered_seconds_ago)
    created = answered - dt.timedelta(seconds=14)          # rang and waited 14 seconds in the queue
    queued = created + dt.timedelta(milliseconds=500)

    s = Store(db_path)
    c = s.conn
    admin_id = find_admin(c)
    queue_id = find_queue(c, QUEUE_NAME)

    patch_site_settings(c, args.secure_capture == "on", args.hide_softphone)

    if args.reset:
        print(f"reset: removed {reset(s)} documents")
    elif c.execute("select 1 from ContactCenter_InteractionIndex where ItemId=?", (INTERACTION_ID,)).fetchone():
        c.commit()
        print("The live call already exists (use --reset to create it again with fresh times). Settings applied.")
        return

    created_type = ensure_contact_type(c)

    # ---- agent profile of admin: signed into Support, busy on the call --------------------------------------------------
    existing = c.execute("select DocumentId, ItemId from ContactCenter_AgentProfileIndex where UserId=?", (admin_id,)).fetchone()
    if existing:
        doc_id, agent_id = existing
        profile = json.loads(c.execute(f"select Content from [{CC}] where Id=?", (doc_id,)).fetchone()[0])
    else:
        doc_id, agent_id, profile = None, AGENT_PROFILE_ID, {
            "Name": admin_id, "UserId": admin_id, "UserName": "admin", "DisplayName": "admin", "MaxConcurrentInteractions": 1,
            "AllowedCampaignIds": [], "CampaignIds": [], "Skills": [], "SkillProficiencies": [],
            "CreatedUtc": iso(now - dt.timedelta(days=30)), "ItemId": AGENT_PROFILE_ID, "Properties": {"SeedTag": TAG}}
    profile.update({
        "PresenceStatus": "Busy", "PresenceChangedUtc": iso(answered), "LastAssignedUtc": iso(answered),
        "QueueIds": [queue_id], "AllowedQueueIds": [queue_id],
        "QueueMemberships": [{"QueueId": queue_id, "Priority": 0, "DelaySeconds": 0}], "ModifiedUtc": iso(now)})
    if doc_id is None:
        doc_id = s.doc(CC, T_AGENT, profile)
        s.index("ContactCenter_AgentProfileIndex", DocumentId=doc_id, ItemId=agent_id, Name=profile["Name"], UserId=admin_id, PresenceStatus=3)
    else:
        c.execute(f"update [{CC}] set Content=? where Id=?", (jdump(profile), doc_id))
        c.execute("update ContactCenter_AgentProfileIndex set PresenceStatus=3 where DocumentId=?", (doc_id,))
        c.execute("delete from ContactCenter_AgentAllowedQueueIndex where DocumentId=?", (doc_id,))
        c.execute("delete from ContactCenter_AgentQueueMembershipIndex where DocumentId=?", (doc_id,))
    s.index("ContactCenter_AgentAllowedQueueIndex", DocumentId=doc_id, ItemId=agent_id, QueueId=queue_id.lower())
    s.index("ContactCenter_AgentQueueMembershipIndex", DocumentId=doc_id, ItemId=agent_id, QueueId=queue_id.lower(),
            PresenceStatus=3, MaxConcurrentInteractions=1)

    # ---- the caller as a Contact content item ---------------------------------------------------------------------------
    contact = {
        "ContentItemId": CONTACT_ID, "ContentItemVersionId": CONTACT_VERSION_ID, "ContentType": "Contact",
        "DisplayText": CALLER_NAME, "Latest": True, "Published": True, "ModifiedUtc": iso(created), "PublishedUtc": iso(created),
        "CreatedUtc": iso(created), "Owner": admin_id, "Author": "admin", "TitlePart": {"Title": CALLER_NAME}}
    cdoc = s.doc(DOC, T_CONTENT_ITEM, contact)
    s.index("ContentItemIndex", DocumentId=cdoc, ContentItemId=CONTACT_ID, ContentItemVersionId=CONTACT_VERSION_ID, Latest=True,
            Published=True, ContentType="Contact", ModifiedUtc=created, PublishedUtc=created, CreatedUtc=created, Owner=admin_id,
            Author="admin", DisplayText=CALLER_NAME)

    # ---- the Omnichannel activity of the inbound call -------------------------------------------------------------------
    activity = strip_nulls({
        "Kind": "Call", "Source": "Inbound", "Channel": "Phone", "InteractionType": "Manual", "PreferredDestination": CALLER_PHONE,
        "ContactContentItemId": CONTACT_ID, "ContactContentType": "Contact", "ContactResolutionStatus": "Resolved",
        "ScheduledUtc": iso(created), "Attempts": 1, "AssignedToId": admin_id, "AssignedToUsername": "admin",
        "AssignedToUtc": iso(answered), "AssignmentStatus": "Assigned", "CreatedById": admin_id, "CreatedByUsername": "admin",
        "CreatedUtc": iso(created), "UrgencyLevel": "Normal", "Status": "InProgress", "ItemId": ACTIVITY_ID,
        "Properties": {"SeedTag": TAG}})
    adoc = s.alloc(OM)
    activity["Id"] = adoc
    c.execute(f"insert into [{OM}] (Id, Type, Content, Version) values (?,?,?,1)", (adoc, T_ACTIVITY, jdump(activity)))
    s.index("Omnichannel_OmnichannelActivityIndex", DocumentId=adoc, ItemId=ACTIVITY_ID, Kind=1, Source="Inbound", Channel="Phone",
            ChannelEndpointId=None, PreferredDestination=CALLER_PHONE, AIProfileName=None, ContactContentItemId=CONTACT_ID,
            ContactContentType="Contact", CampaignId=None, DialerProfileId=None, SubjectContentType=None, ScheduledUtc=created,
            CompletedUtc=None, Attempts=1, AssignedToId=admin_id, AssignedToUtc=answered, AssignmentStatus=3, ReservationId=None,
            ReservedById=None, ReservedUtc=None, ReservationExpiresUtc=None, CreatedById=admin_id, DispositionId=None,
            CreatedUtc=created, UrgencyLevel=0, Status=8, InteractionType=0, AiEscalated=False, TerminalReasonCode=None)

    # ---- the interaction ------------------------------------------------------------------------------------------------
    interaction = strip_nulls({
        "EntityProperties": {}, "Channel": "Voice", "Direction": "Inbound", "Status": "Connected", "ActivityItemId": ACTIVITY_ID,
        "ProviderName": "Asterisk", "ProviderInteractionId": PBX_CALL_ID, "ProviderLegId": CUSTOMER_LEG_ID,
        "CustomerAddress": CALLER_PHONE, "QueueId": queue_id, "AgentId": agent_id, "CorrelationId": CORRELATION_ID,
        "RecordingState": "Recording", "RecordingReference": f"asterisk-rec-{INTERACTION_ID[:16]}", "RecordingLegalHold": False,
        "RecordingDisclosedUtc": iso(answered), "TechnicalMetadata": {},
        "QueueHistory": [{"QueueId": queue_id, "EnteredUtc": iso(queued), "ExitedUtc": iso(answered), "ExitReason": "Assigned"}],
        "TransferHistory": [],
        "Participants": [
            {"Role": "Customer", "Identifier": CALLER_PHONE, "DisplayName": CALLER_NAME, "Address": CALLER_PHONE, "JoinedUtc": iso(created)},
            {"Role": "Agent", "Identifier": agent_id, "DisplayName": "admin", "Address": "admin", "JoinedUtc": iso(answered)}],
        "CreatedUtc": iso(created), "StartedUtc": iso(created), "AnsweredUtc": iso(answered), "ModifiedUtc": iso(answered),
        "CreatedById": admin_id, "CreatedByUserName": "admin", "ItemId": INTERACTION_ID, "Properties": {"SeedTag": TAG}})
    idoc = s.doc(CC, T_INTERACTION, interaction)
    s.index("ContactCenter_InteractionIndex", DocumentId=idoc, ItemId=INTERACTION_ID, Channel=0, Direction=0, Status=2,
            ActivityItemId=ACTIVITY_ID, ProviderName="Asterisk", ProviderInteractionId=PBX_CALL_ID, ProviderLegId=CUSTOMER_LEG_ID,
            AgentId=agent_id, CorrelationId=CORRELATION_ID, CreatedUtc=created, EndedUtc=None, WrapUpStartedUtc=None,
            WrapUpCompletedUtc=None, RecordingLegalHold=False, RecordingState=1, RecordingPausedUtc=None, QueueId=queue_id)

    # ---- the call session (the ownership check of a secure capture reads it) ---------------------------------------------
    session = strip_nulls({
        "InteractionId": INTERACTION_ID, "ActivityItemId": ACTIVITY_ID, "ProviderName": "Asterisk", "ProviderCallId": PBX_CALL_ID,
        "DeliveryModel": "ServerSideAcd", "Direction": "Inbound", "State": "Connected", "QueueId": queue_id, "AgentId": agent_id,
        "Legs": [
            {"ProviderLegId": CUSTOMER_LEG_ID, "Role": "Customer", "Status": "Answered", "Address": CALLER_PHONE,
             "StartedUtc": iso(created), "AnsweredUtc": iso(created + dt.timedelta(seconds=1))},
            {"ProviderLegId": AGENT_LEG_ID, "Role": "Agent", "Status": "Answered", "Address": "admin", "AgentId": agent_id,
             "StartedUtc": iso(answered - dt.timedelta(seconds=8)), "AnsweredUtc": iso(answered)}],
        "PriorBridges": [], "Consults": [], "Relationships": [], "MonitorSessions": [], "FromAddress": CALLER_PHONE,
        "IsOnHold": False, "HoldPlacedByAgent": False, "IsMuted": False, "RecordingState": "Recording",
        "RecordingReference": interaction["RecordingReference"], "CreatedUtc": iso(created), "StartedUtc": iso(created),
        "AnsweredUtc": iso(answered), "TalkSeconds": 0.0, "HoldSeconds": 0, "Metadata": {}, "ModifiedUtc": iso(answered),
        "LastProviderEventUtc": iso(answered), "ItemId": SESSION_ID, "Properties": {"SeedTag": TAG}})
    sdoc = s.doc(CC, T_CALL_SESSION, session)
    s.index("ContactCenter_CallSessionIndex", DocumentId=sdoc, ItemId=SESSION_ID, InteractionId=INTERACTION_ID,
            ActivityItemId=ACTIVITY_ID, ProviderName="Asterisk", ProviderCallId=PBX_CALL_ID,
            ProviderCallClaimKey=f"Asterisk|{PBX_CALL_ID}", State=3, AgentId=agent_id, CreatedUtc=created, EndedUtc=None, QueueId=queue_id)
    for leg_id, role, leg_agent, started in ((CUSTOMER_LEG_ID, 1, None, created), (AGENT_LEG_ID, 2, agent_id, answered)):
        s.index("ContactCenter_CallSessionLegIndex", DocumentId=sdoc, ItemId=SESSION_ID, ProviderLegId=leg_id, Role=role,
                AgentId=leg_agent, InteractionId=INTERACTION_ID, StartedUtc=started)

    # ---- audit events ---------------------------------------------------------------------------------------------------
    base = {"InteractionId": INTERACTION_ID, "ProviderName": "Asterisk", "Direction": "Inbound", "QueueId": queue_id}
    events = [
        ("InteractionCreated", created, "System", "system", "Interactions", {"Channel": "Voice", "Direction": "Inbound"}),
        ("CallQueued", queued, "System", "system", "Routing", base),
        ("CallConnected", answered + dt.timedelta(milliseconds=300), "Provider", "Asterisk", "Routing", {**base, "AgentId": agent_id}),
        ("RecordingStarted", answered + dt.timedelta(milliseconds=700), "System", "system", "Routing",
         {"InteractionId": INTERACTION_ID, "RecordingReference": interaction["RecordingReference"]}),
    ]
    for (kind, when, actor_type, actor, component, data), event_id in zip(events, EVENT_IDS):
        body = {"EntityProperties": {}, "InteractionId": INTERACTION_ID, "EventType": kind, "SchemaVersion": 1,
                "AggregateType": "Interaction", "AggregateId": INTERACTION_ID, "CorrelationId": CORRELATION_ID, "ActorId": actor,
                "ActorType": actor_type, "SourceComponent": component, "OccurredUtc": iso(when),
                "RecordedUtc": iso(when + dt.timedelta(milliseconds=30)), "ItemId": event_id, "Properties": {"SeedTag": TAG},
                "Data": jdump(data)}
        edoc = s.doc(CC, T_EVENT, body)
        s.index("ContactCenter_InteractionEventIndex", DocumentId=edoc, ItemId=event_id, InteractionId=INTERACTION_ID, EventType=kind,
                AggregateType="Interaction", CorrelationId=CORRELATION_ID, IdempotencyKey=None, IdempotencyClaimKey=event_id,
                OccurredUtc=when, AggregateId=INTERACTION_ID)

    c.commit()
    c.execute("pragma wal_checkpoint(truncate)")
    c.close()
    print(f"Seeded the live call for admin: {CALLER_NAME} {CALLER_PHONE} on {QUEUE_NAME}, answered {args.answered_seconds_ago}s ago "
          f"(interaction {INTERACTION_ID}); Secure Data Capture setting {args.secure_capture}"
          + ("; created the Contact content type" if created_type else ""))


if __name__ == "__main__":
    main()
