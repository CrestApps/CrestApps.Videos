#!/usr/bin/env python3
"""Seeds a CrestApps.OrchardCore demo site with FAKE Contact Center history so the Contact Center reports
(Executive, Operations, Queue & Routing, Agent Performance, Workforce & Payroll, Billing & Usage,
Compliance & Audit, Technical & IT) show the numbers of a busy contact center.

    python seed-reports.py <App_Data dir> [--reset] [--scale 1.0] [--seed 20260101] [--days 40]

From scratch: copy the base snapshot (the site with the Contact Center and Reports features enabled and the five fake
users) to a new App_Data folder, run this script on it, start the site, open Reports. About 5 seconds.

RUN IT WITH THE SITE STOPPED (it refuses to run while the site has the database open). The script writes straight into <App_Data>/Sites/Default/OrchardCore.db (SQLite)
using the same document and index conventions as the product, then checkpoints the WAL file.

What must exist first (nothing else; everything below is created by the script when it is missing)
---------------------------------------------------------------------------------------------------
  * A site that went through setup with the Contact Center, Reports, Telnyx and Asterisk features enabled
    (snapshot "snap-reports-base"): the tables ContactCenter_* and the seven default agent state reason codes
    (Short break, Lunch, Away from desk, Team meeting, Training, Coaching, System issue) must be there.
  * The users jamie.chen, maria.lopez, sam.patel, alex.kim and priya.shah.

What the script creates
-----------------------
  * Two queue groups (Customer care, Revenue) and four queues (Support, Billing, New sales, Renewals), each with a
    service-level threshold. Existing groups/queues with the same name are reused, never duplicated.
  * An agent profile for each of the five users, entitled to three queues each (an existing profile is kept; the
    queues are added to its entitlements).
  * About 40 days of history, relative to the moment the script runs (all times UTC, the working day follows the
    site time zone, normally America/New_York):
      - interactions (voice, a few SMS and chat; inbound ~70 %, outbound ~30 %; answered, abandoned, voicemail,
        callback requested, failed, not connected) with their queue history, transfers, recordings, call events,
        offers, call sessions with call legs and call quality measurements;
      - agent presence history (sign in, available / reserved / busy / wrap-up, lunch, short breaks, meetings,
        training, coaching, sign out) for every weekday, so the Workforce & Payroll reports reconcile.
    The last 14 days are dense, the earlier ones thinner. Today is not seeded (no half-finished shift).
  * Every transactional document it adds carries the property SeedTag = "seed-reports", so --reset can remove
    exactly those documents (interactions, events, call sessions, call quality records) before seeding again.
    Without --reset the script refuses to run a second time. Queues, groups and agent profiles are never deleted.

Fake people and numbers only: customers are made-up names with +1 (212/646/718/917) 555-0100..0199 numbers.
"""

import argparse
import json
import math
import os
import random
import sqlite3
import sys
from datetime import date, datetime, time, timedelta, timezone

TAG = "seed-reports"
CORE_ASM = "CrestApps.OrchardCore.ContactCenter.Core"
MODELS_NS = "CrestApps.OrchardCore.ContactCenter.Core.Models"

# ---------------------------------------------------------------------------------------------------------------
# Static configuration
# ---------------------------------------------------------------------------------------------------------------

GROUPS = [
    ("Customer care", "Support and billing queues for existing customers."),
    ("Revenue", "Sales and renewal queues."),
]

# name, group, service-level threshold (s), description, share of inbound calls, median wait (s), median talk (min)
QUEUES = [
    ("Support", "Customer care", 60, "Product questions and technical help.", 0.34, 17, 6.8),
    ("Billing", "Customer care", 45, "Invoices, payments and refunds.", 0.24, 14, 5.0),
    ("New sales", "Revenue", 30, "New customers asking about plans and pricing.", 0.22, 9, 6.2),
    ("Renewals", "Revenue", 45, "Subscription renewals and upgrades.", 0.20, 12, 4.6),
]

# user, display name, queues, speed (handle-time factor), preference (lower = picked more), first sign-in offset
# (minutes after 08:00), sign-out offset (minutes after 17:00), lunch offset (minutes after 11:30)
AGENTS = [
    ("jamie.chen", "Jamie Chen", ["Support", "Renewals"], 1.00, 1.00, 10, 5, 45),
    ("maria.lopez", "Maria Lopez", ["Support", "Billing"], 0.92, 0.95, 40, 20, 90),
    ("sam.patel", "Sam Patel", ["New sales", "Renewals"], 1.05, 0.80, 0, 0, 30),
    ("alex.kim", "Alex Kim", ["Support", "Billing", "New sales"], 1.22, 1.15, 50, 30, 0),
    ("priya.shah", "Priya Shah", ["Billing", "New sales", "Renewals"], 0.85, 0.85, 25, 10, 75),
]

PROVIDERS = [("Telnyx", 0.62), ("Asterisk", 0.38)]
PROVIDER_FAIL = {"Telnyx": 0.032, "Asterisk": 0.016}
PROVIDER_RECORDED = {"Telnyx": 0.95, "Asterisk": 0.80}
PROVIDER_ERRORS = {
    "Telnyx": [
        ("SIP 503 Service Unavailable from carrier", 503, "Failed"),
        ("Call rejected by the destination carrier (D51)", 603, "Rejected"),
        ("Origination failed: destination unreachable", 480, "Failed"),
        ("Media negotiation failed (488 Not Acceptable Here)", 488, "Failed"),
    ],
    "Asterisk": [
        ("PJSIP 408 Request Timeout toward the trunk", 408, "Failed"),
        ("Congestion on the outbound trunk (CHANUNAVAIL)", 503, "Congestion"),
        ("SIP 603 Decline from the destination", 603, "Rejected"),
    ],
}

HOUR_WEIGHTS = {9: 0.90, 10: 1.15, 11: 1.20, 12: 0.75, 13: 0.95, 14: 1.10, 15: 1.05, 16: 0.80}
DOW_FACTOR = {0: 1.12, 1: 1.05, 2: 1.00, 3: 0.95, 4: 0.88}  # Monday .. Friday

RECENT_DAYS = 14
RECENT_WEEKDAY_BASE = 140   # interactions on a typical recent weekday (before the day factor)
OLDER_WEEKDAY_BASE = 52
WEEKEND_BASE = 6
MAX_VOICE_WAIT = 170.0
MAX_TEXT_WAIT = 900.0
OUTBOUND_SHARE = 0.28

FIRST_NAMES = ["Dana", "Marcus", "Elena", "Tobias", "Imani", "Gregor", "Noor", "Callum", "Sofia", "Wendell",
               "Priscilla", "Anders", "Keiko", "Mateo", "Odette", "Rashad", "Hazel", "Ibrahim", "Lena", "Victor",
               "Mirela", "Jonah", "Tamsin", "Devon", "Yusra", "Bastian", "Nadia", "Harlan", "Esme", "Cyrus"]
LAST_NAMES = ["Whitfield", "Okafor", "Lindqvist", "Marchetti", "Delacroix", "Haverford", "Nakamura", "Oyelaran",
              "Castellano", "Brightwater", "Petrov", "Alvarenga", "Thornbury", "Mbeki", "Fairweather", "Kowalczyk",
              "Santoro", "Ashcombe", "Bellamy", "Dragomir"]
AREA_CODES = ["212", "646", "718", "917"]
CAPTURE_DEVICES = ["Jabra Evolve2 65 (Headset)", "Plantronics Blackwire 5220", "Default - Headset Microphone (Realtek Audio)",
                   "Logitech H390 USB Headset", "Poly Voyager 4320"]

# Agent state numbers (AgentPresenceStatus)
OFFLINE, AVAILABLE, RESERVED, BUSY, WRAPUP, BREAK, REQUESTBREAK, AWAY, DND, MEETING, TRAINING = range(11)

# ---------------------------------------------------------------------------------------------------------------
# Small helpers
# ---------------------------------------------------------------------------------------------------------------

B36 = "0123456789abcdefghijklmnopqrstuvwxyz"


def new_id(rng):
    return "4" + "".join(rng.choice(B36) for _ in range(25))


def iso(dt):
    """UTC datetime (naive) -> the ISO form the product writes (fraction trimmed, Z suffix)."""
    s = dt.strftime("%Y-%m-%dT%H:%M:%S")

    if dt.microsecond:
        s += "." + ("%06d" % dt.microsecond).rstrip("0")

    return s + "Z"


def sqlts(dt):
    """UTC datetime (naive) -> the text SQLite holds for a DATETIME index column."""
    if dt is None:
        return None

    s = dt.strftime("%Y-%m-%d %H:%M:%S")

    if dt.microsecond:
        s += "." + ("%06d" % dt.microsecond).rstrip("0")

    return s


def lognorm(rng, median, sigma, lo=None, hi=None):
    value = median * math.exp(rng.gauss(0.0, sigma))

    if lo is not None:
        value = max(lo, value)

    if hi is not None:
        value = min(hi, value)

    return value


def weighted(rng, pairs):
    total = sum(weight for _, weight in pairs)
    pick = rng.random() * total

    for item, weight in pairs:
        pick -= weight

        if pick <= 0:
            return item

    return pairs[-1][0]


def poisson(rng, lam):
    if lam <= 0:
        return 0

    if lam > 30:
        return max(0, int(round(rng.gauss(lam, math.sqrt(lam)))))

    limit, k, p = math.exp(-lam), 0, 1.0

    while True:
        p *= rng.random()

        if p <= limit:
            return k

        k += 1


def compact(value):
    return json.dumps(value, separators=(",", ":"), ensure_ascii=False)


class LocalClock:
    """Converts the tenant's local working day to UTC without a tzdata dependency problem."""

    def __init__(self, tz_id):
        self.tz = None

        try:
            from zoneinfo import ZoneInfo

            self.tz = ZoneInfo(tz_id)
        except Exception:  # noqa: BLE001
            self.tz = None

    @staticmethod
    def _us_dst(naive_local):
        year = naive_local.year
        march = date(year, 3, 8)
        march = march + timedelta(days=(6 - march.weekday()) % 7)      # second Sunday of March
        november = date(year, 11, 1)
        november = november + timedelta(days=(6 - november.weekday()) % 7)  # first Sunday of November
        start = datetime.combine(march, time(2, 0))
        end = datetime.combine(november, time(2, 0))

        return start <= naive_local < end

    def to_utc(self, naive_local):
        if self.tz is not None:
            return naive_local.replace(tzinfo=self.tz).astimezone(timezone.utc).replace(tzinfo=None)

        return naive_local + timedelta(hours=4 if self._us_dst(naive_local) else 5)

    def to_local(self, naive_utc):
        if self.tz is not None:
            return naive_utc.replace(tzinfo=timezone.utc).astimezone(self.tz).replace(tzinfo=None)

        probe = naive_utc - timedelta(hours=5)

        return naive_utc - timedelta(hours=4 if self._us_dst(probe) else 5)


# ---------------------------------------------------------------------------------------------------------------
# Database access
# ---------------------------------------------------------------------------------------------------------------

class Db:
    def __init__(self, path):
        self.con = sqlite3.connect(path, timeout=3)

        try:
            # Exclusive locking fails at once while the site (or anything else) has the database open.
            self.con.execute("PRAGMA locking_mode=EXCLUSIVE")
            self.con.execute("select count(*) from sqlite_master").fetchone()
            self.con.execute("BEGIN IMMEDIATE")
            self.con.execute("ROLLBACK")
        except sqlite3.OperationalError:
            raise SystemExit("The database is in use: stop the site before running this script.")

        self.next_id = (self.con.execute("select coalesce(max(Id),0) from ContactCenter_Document").fetchone()[0] or 0) + 1
        self.docs = []        # (id, type, content)
        self.index = {}       # table -> [row dict]

    def type_name(self, short):
        return f"{MODELS_NS}.{short}, {CORE_ASM}"

    def add(self, short_type, content):
        doc_id = self.next_id
        self.next_id += 1
        self.docs.append((doc_id, self.type_name(short_type), compact(content)))

        return doc_id

    def row(self, table, **values):
        self.index.setdefault(table, []).append(values)

    def table_columns(self, table):
        return [r[1] for r in self.con.execute(f'pragma table_info("{table}")')]

    def flush(self):
        cur = self.con.cursor()
        cur.executemany("insert into ContactCenter_Document (Id, Type, Content, Version) values (?,?,?,1)", self.docs)

        for table, rows in self.index.items():
            columns = self.table_columns(table)

            for row in rows:
                unknown = [key for key in row if key not in columns]

                if unknown:
                    raise RuntimeError(f"{table} has no column(s) {unknown}")

            keys = sorted({key for row in rows for key in row})
            sql = f'insert into "{table}" ({",".join(chr(34) + k + chr(34) for k in keys)}) values ({",".join("?" * len(keys))})'
            cur.executemany(sql, [[row.get(key) for key in keys] for row in rows])

        self.con.commit()
        self.docs, self.index = [], {}


INDEX_TABLES = [
    "ContactCenter_InteractionIndex", "ContactCenter_InteractionEventIndex", "ContactCenter_CallSessionIndex",
    "ContactCenter_CallSessionLegIndex", "ContactCenter_CallQualityRecordIndex",
]


def remove_seeded(con):
    ids = [r[0] for r in con.execute("select Id from ContactCenter_Document where Content like ?", (f'%"SeedTag":"{TAG}"%',))]

    if not ids:
        return 0

    for chunk_start in range(0, len(ids), 500):
        chunk = ids[chunk_start:chunk_start + 500]
        marks = ",".join("?" * len(chunk))

        for table in INDEX_TABLES:
            con.execute(f'delete from "{table}" where DocumentId in ({marks})', chunk)

        con.execute(f"delete from ContactCenter_Document where Id in ({marks})", chunk)

    con.commit()

    return len(ids)


# ---------------------------------------------------------------------------------------------------------------
# Prerequisites: groups, queues, agent profiles
# ---------------------------------------------------------------------------------------------------------------

def ensure_prerequisites(db, rng, now):
    con = db.con
    created = {"groups": 0, "queues": 0, "agents": 0}

    groups = {}

    for name, description in GROUPS:
        row = con.execute("select ItemId from ContactCenter_ActivityQueueGroupIndex where Name=? collate nocase", (name,)).fetchone()

        if row:
            groups[name] = row[0]
            continue

        item_id = new_id(rng)
        doc_id = db.add("ActivityQueueGroup", {"Name": name, "Description": description, "CreatedUtc": iso(now), "ItemId": item_id, "Properties": {}})
        db.row("ContactCenter_ActivityQueueGroupIndex", DocumentId=doc_id, ItemId=item_id, Name=name)
        groups[name] = item_id
        created["groups"] += 1

    queues = {}

    for name, group, sla, description, _share, _wait, _talk in QUEUES:
        row = con.execute("select ItemId, Enabled from ContactCenter_ActivityQueueIndex where Name=? collate nocase", (name,)).fetchone()

        if row:
            queues[name] = row[0]
            continue

        item_id = new_id(rng)
        content = {
            "QueueGroupId": groups[group], "Name": name, "Description": description, "DefaultPriority": "Normal",
            "RoutingStrategy": "LongestIdle", "PreferStickyAgent": False, "EnableSlaAging": False,
            "SlaThresholdSeconds": sla, "ReservationTimeoutSeconds": 30, "UnansweredOfferAction": "Requeue",
            "AfterHoursAction": "HoldInQueue", "OverflowAfterSeconds": 0, "RequiredSkills": [], "Enabled": True,
            "CreatedUtc": iso(now), "FirstResponseTargetSeconds": 0, "SkillRequirements": [], "OverflowTargets": [],
            "MaxWaitSeconds": 0, "MaxWaitAction": "None", "MaxQueueSize": 0, "QueueFullAction": "None",
            "Treatment": {"AnnouncementIntervalSeconds": 0, "AnnouncePosition": False, "AnnounceEstimatedWait": False,
                          "CallbackOfferAfterSeconds": 0, "MinimumEstimateSeconds": 30, "MaximumEstimateSeconds": 1800,
                          "AverageHandleTimeSeconds": 0},
            "ItemId": item_id, "Properties": {},
        }
        doc_id = db.add("ActivityQueue", content)
        db.row("ContactCenter_ActivityQueueIndex", DocumentId=doc_id, ItemId=item_id, Name=name, QueueGroupId=groups[group], Enabled=1)
        queues[name] = item_id
        created["queues"] += 1

    users = {name.lower(): user_id for user_id, name in con.execute("select UserId, NormalizedUserName from UserIndex")}
    agents = {}

    for user, display, queue_names, speed, pref, in_off, out_off, lunch_off in AGENTS:
        user_id = users.get(user.lower())

        if user_id is None:
            raise SystemExit(f"The user '{user}' does not exist on this site. Create the five fake users first.")

        wanted = [queues[name] for name in queue_names]
        row = con.execute("select DocumentId, ItemId from ContactCenter_AgentProfileIndex where UserId=?", (user_id,)).fetchone()

        if row:
            doc_id, item_id = row
            doc = json.loads(con.execute("select Content from ContactCenter_Document where Id=?", (doc_id,)).fetchone()[0])
            allowed = list(doc.get("AllowedQueueIds") or [])
            missing = [q for q in wanted if q.lower() not in [a.lower() for a in allowed]]

            if missing:
                doc["AllowedQueueIds"] = allowed + missing
                con.execute("update ContactCenter_Document set Content=?, Version=Version+1 where Id=?", (compact(doc), doc_id))

                for queue_id in missing:
                    con.execute("insert into ContactCenter_AgentAllowedQueueIndex (DocumentId, ItemId, QueueId) values (?,?,?)",
                                (doc_id, item_id, queue_id.lower()))

                con.commit()

            agents[user] = {"item_id": item_id, "user_id": user_id, "doc_id": doc_id, "existing": True}
            continue

        item_id = new_id(rng)
        content = {
            "Name": user_id, "UserId": user_id, "UserName": user, "DisplayName": user, "MaxConcurrentInteractions": 1,
            "PresenceStatus": "Offline", "QueueIds": [], "QueueMemberships": [], "CampaignIds": [],
            "AllowedQueueIds": wanted, "AllowedCampaignIds": [], "Skills": [], "CreatedUtc": iso(now - timedelta(days=60)),
            "SkillProficiencies": [], "ItemId": item_id, "Properties": {},
        }
        doc_id = db.add("AgentProfile", content)
        db.row("ContactCenter_AgentProfileIndex", DocumentId=doc_id, ItemId=item_id, Name=user_id, UserId=user_id, PresenceStatus=0)

        for queue_id in wanted:
            db.row("ContactCenter_AgentAllowedQueueIndex", DocumentId=doc_id, ItemId=item_id, QueueId=queue_id.lower())

        agents[user] = {"item_id": item_id, "user_id": user_id, "doc_id": doc_id, "existing": False}
        created["agents"] += 1

    return groups, queues, agents, created


def load_reason_codes(con):
    codes = {}

    for doc_type, content in con.execute("select Type, Content from ContactCenter_Document where Type like '%AgentStateReasonCode%'"):
        doc = json.loads(content)
        codes[doc["Name"]] = doc["ItemId"]

    return codes


# ---------------------------------------------------------------------------------------------------------------
# The simulation
# ---------------------------------------------------------------------------------------------------------------

class AgentDay:
    def __init__(self, cfg, info, queue_ids, shift_start, shift_end, blocks):
        self.user = cfg[0]
        self.display = cfg[1]
        self.speed = cfg[3]
        self.pref = cfg[4]
        self.item_id = info["item_id"]
        self.user_id = info["user_id"]
        self.queue_names = cfg[2]
        self.queue_ids = queue_ids
        self.shift_start = shift_start
        self.shift_end = shift_end
        self.blocks = blocks            # (start, end, status, reason)
        self.free_at = shift_start + timedelta(seconds=60)
        self.work = []                  # (reserved_at|None, busy_at, ended, wrap_end, interaction_id)

    def fit(self, start, duration):
        """Earliest start >= start that is not inside a break and lets the call end before the shift does. A call that
        runs into the start of a break is allowed: the agent takes the break after it (see commit)."""
        t = max(start, self.free_at)

        while True:
            moved = False

            for block_start, block_end, _status, _reason in self.blocks:
                if block_start <= t < block_end:
                    t = block_end + timedelta(seconds=5)
                    moved = True

            if not moved:
                break

        return t if t + duration <= self.shift_end else None

    def commit(self, start, wrap_end):
        """Books the agent until wrap_end. A break that was due while the agent is busy starts when they are free."""
        self.free_at = wrap_end
        earliest = wrap_end + timedelta(seconds=5)
        limit = self.shift_end - timedelta(minutes=2)
        blocks = []
        previous_end = None

        for block_start, block_end, status, reason in self.blocks:
            length = block_end - block_start

            if block_start >= start:
                if block_start < earliest:
                    block_start = earliest

                if previous_end is not None and block_start < previous_end + timedelta(seconds=90):
                    block_start = previous_end + timedelta(seconds=90)

                block_end = block_start + length

            if block_start >= limit:
                continue

            block_end = min(block_end, limit)
            blocks.append((block_start, block_end, status, reason))
            previous_end = block_end

        self.blocks = blocks


class Customers:
    def __init__(self, rng, count=170):
        self.numbers = []
        used = set()

        while len(self.numbers) < count:
            number = f"+1{rng.choice(AREA_CODES)}555{rng.randint(100, 199):04d}"

            if number not in used:
                used.add(number)
                self.numbers.append((number, f"{rng.choice(FIRST_NAMES)} {rng.choice(LAST_NAMES)}"))

        self.weights = [1.0 / (i + 3) ** 0.8 for i in range(count)]

    def pick(self, rng):
        return rng.choices(self.numbers, weights=self.weights, k=1)[0]


def build_blocks(rng, local_date, cfg, weekday, week_index, coaching_agents, training_agent, clock):
    """Non-call blocks of an agent's day as (start_utc, end_utc, status, reason), local times converted."""
    user = cfg[0]
    candidates = []

    def at(hour, minute):
        return datetime.combine(local_date, time(hour, minute))

    lunch_start = at(11, 30) + timedelta(minutes=cfg[7] + rng.randint(-10, 10))
    candidates.append((lunch_start, lunch_start + timedelta(minutes=rng.choice([30, 30, 45, 45, 60])), BREAK, "Lunch"))

    if weekday == 2:
        start = at(15, 30) if user in ("jamie.chen", "maria.lopez", "sam.patel") else at(16, 5)
        candidates.append((start, start + timedelta(minutes=30), MEETING, "Team meeting"))

    if weekday == 3 and user in coaching_agents:
        start = at(14, 0) + timedelta(minutes=rng.randint(0, 20))
        candidates.append((start, start + timedelta(minutes=30), MEETING, "Coaching"))

    if weekday == 4 and week_index % 2 == 0 and user == training_agent:
        start = at(13, 30)
        candidates.append((start, start + timedelta(minutes=rng.choice([60, 90])), TRAINING, "Training"))

    if rng.random() < 0.88:
        start = at(10, 5) + timedelta(minutes=rng.randint(0, 45))
        candidates.append((start, start + timedelta(minutes=15), BREAK, "Short break"))

    if rng.random() < 0.85:
        start = at(14, 40) + timedelta(minutes=rng.randint(0, 60))
        candidates.append((start, start + timedelta(minutes=15), BREAK, "Short break"))

    if rng.random() < 0.28:
        start = at(10, 0) + timedelta(minutes=rng.randint(0, 360))
        candidates.append((start, start + timedelta(minutes=rng.randint(5, 12)), AWAY, "Away from desk"))

    if rng.random() < 0.09:
        start = at(10, 0) + timedelta(minutes=rng.randint(0, 300))
        candidates.append((start, start + timedelta(minutes=rng.randint(15, 25)), DND, "System issue"))

    chosen = []

    for start, end, status, reason in candidates:
        if any(start < other_end + timedelta(minutes=10) and end + timedelta(minutes=10) > other_start for other_start, other_end, _s, _r in chosen):
            continue

        chosen.append((start, end, status, reason))

    return sorted((clock.to_utc(start), clock.to_utc(end), status, reason) for start, end, status, reason in chosen)


def simulate(db, rng, now, clock, days, scale, queue_ids, agents, reason_codes):
    """Returns (interaction records, agent day schedules)."""
    customers = Customers(rng)
    today_local = clock.to_local(now).date()
    queue_by_name = {q[0]: q for q in QUEUES}
    records = []
    agent_days = []

    for offset in range(days, 0, -1):
        local_date = today_local - timedelta(days=offset)
        weekday = local_date.weekday()
        weekend = weekday >= 5
        recent = offset <= RECENT_DAYS
        week_index = local_date.isocalendar()[1]
        day_wait_factor = math.exp(rng.gauss(0, 0.22))
        coaching_agents = {AGENTS[i][0] for i in rng.sample(range(len(AGENTS)), 2)}
        training_agent = AGENTS[week_index % len(AGENTS)][0]

        # --- agents of the day
        todays = []

        if not weekend:
            for cfg in AGENTS:
                info = agents[cfg[0]]
                start_local = datetime.combine(local_date, time(8, 0)) + timedelta(minutes=cfg[5] + rng.randint(-10, 10))
                start_local = max(start_local, datetime.combine(local_date, time(7, 50)))
                start_local = min(start_local, datetime.combine(local_date, time(8, 58)))
                end_local = datetime.combine(local_date, time(17, 0)) + timedelta(minutes=cfg[6] + rng.randint(0, 25))
                blocks = build_blocks(rng, local_date, cfg, weekday, week_index, coaching_agents, training_agent, clock)
                todays.append(AgentDay(cfg, info, [queue_ids[name] for name in cfg[2]], clock.to_utc(start_local), clock.to_utc(end_local), blocks))

        agent_days.extend(todays)

        # --- arrivals of the day
        base = (WEEKEND_BASE if weekend else (RECENT_WEEKDAY_BASE if recent else OLDER_WEEKDAY_BASE)) * scale
        factor = 1.0 if weekend else DOW_FACTOR[weekday] * math.exp(rng.gauss(0, 0.10))
        total = max(0, int(round(base * factor)))
        arrivals = []

        if weekend:
            for _ in range(total):
                local = datetime.combine(local_date, time(8, 0)) + timedelta(seconds=rng.randint(0, 12 * 3600))
                arrivals.append(("in_after", clock.to_utc(local), None))
        else:
            n_out = int(round(total * OUTBOUND_SHARE))
            n_in = total - n_out
            n_after = poisson(rng, 2.0 * scale * (1.4 if recent else 0.8))
            hours = list(HOUR_WEIGHTS.items())

            for _ in range(n_in):
                hour = weighted(rng, hours)
                local = datetime.combine(local_date, time(hour, 0)) + timedelta(seconds=rng.randint(0, 3599))
                arrivals.append(("in", clock.to_utc(local), weighted(rng, [(q[0], q[4]) for q in QUEUES])))

            for _ in range(n_out):
                hour = weighted(rng, [(h, 0.6 + w * 0.4) for h, w in hours])
                local = datetime.combine(local_date, time(hour, 0)) + timedelta(seconds=rng.randint(0, 3599))
                arrivals.append(("out", clock.to_utc(local), None))

            for _ in range(n_after):
                if rng.random() < 0.45:
                    local = datetime.combine(local_date, time(6, 30)) + timedelta(seconds=rng.randint(0, int(2.5 * 3600)))
                else:
                    local = datetime.combine(local_date, time(17, 0)) + timedelta(seconds=rng.randint(60, int(4.5 * 3600)))

                arrivals.append(("in_after", clock.to_utc(local), None))

        arrivals.sort(key=lambda a: a[1])

        for kind, when, queue_name in arrivals:
            when = when.replace(microsecond=rng.randint(0, 999) * 1000)

            if kind == "in":
                records.append(inbound(rng, when, queue_by_name[queue_name], queue_ids, todays, customers, day_wait_factor))
            elif kind == "in_after":
                records.append(after_hours(rng, when, rng.choice(QUEUES), queue_ids, customers))
            else:
                record = outbound(rng, when, todays, queue_ids, customers)

                if record:
                    records.append(record)

    return [r for r in records if r], agent_days


STATS = {"converted": 0, "after_hours": 0, "text_unserved": 0}


def pick_provider(rng):
    return weighted(rng, PROVIDERS)


def provider_ids(rng, provider, when):
    if provider == "Telnyx":
        token = "".join(rng.choice(B36 + "ABCDEFGHIJKLMNOPQRSTUVWXYZ_-") for _ in range(46))

        return "v3:" + token, "".join(rng.choice("0123456789abcdef") for _ in range(8)) + "-" + "".join(rng.choice("0123456789abcdef") for _ in range(4)) + "-4" + "".join(rng.choice("0123456789abcdef") for _ in range(3)) + "-" + "".join(rng.choice("0123456789abcdef") for _ in range(4)) + "-" + "".join(rng.choice("0123456789abcdef") for _ in range(12))

    stamp = int((when - datetime(1970, 1, 1)).total_seconds())

    return f"{stamp}.{rng.randint(100, 9999)}", f"PJSIP/trunk-{rng.randint(1, 0xFFFFF):08x}"


def draw_talk(rng, queue_median_minutes, speed, outbound=False):
    if rng.random() < 0.022:
        return rng.uniform(15.5, 36.0) * 60.0

    median = (queue_median_minutes * (0.72 if outbound else 1.0)) * 60.0 * speed

    return lognorm(rng, median, 0.62, 60.0, 14.6 * 60.0)


def draw_wrap(rng, speed):
    return lognorm(rng, 62.0 * speed, 0.55, 15.0, 260.0)


def base_record(rng, when, channel, direction, queue_id, queue_name, customers, provider):
    number, name = customers.pick(rng)
    provider_call, provider_leg = provider_ids(rng, provider, when)

    return {
        "id": new_id(rng), "channel": channel, "direction": direction, "created": when, "queue_id": queue_id, "queue": queue_name,
        "customer": number, "customer_name": name, "provider": provider, "provider_call": provider_call, "provider_leg": provider_leg,
        "correlation": "seed-" + new_id(rng)[:20], "agent": None, "answered": None, "ended": None, "wrap_start": None, "wrap_end": None,
        "outcome": None, "status": "Ended", "recorded": False, "transfers": [], "holds": [], "extra_offers": [], "metadata": {},
        "wait": 0.0, "queued": False, "ring": 0.0, "error": None, "talk": 0.0, "hangup": "NormalClearing",
    }


def inbound(rng, when, queue, queue_ids, todays, customers, day_wait_factor):
    queue_name, _group, _sla, _desc, _share, median_wait, median_talk = queue
    queue_id = queue_ids[queue_name]
    channel = weighted(rng, [("Voice", 0.91), ("Sms", 0.05), ("Chat", 0.04)])

    if channel != "Voice":
        return inbound_text(rng, when, channel, queue, queue_id, todays, customers)

    provider = pick_provider(rng)
    record = base_record(rng, when, "Voice", "Inbound", queue_id, queue_name, customers, provider)
    outcome = weighted(rng, [("answered", 0.865), ("abandoned", 0.06), ("voicemail", 0.03), ("callback", 0.025), ("failed", 0.02)])

    if rng.random() < PROVIDER_FAIL[provider]:
        outcome = "failed"

    if outcome == "failed":
        return make_failed(rng, record)

    if outcome == "answered":
        base_wait = lognorm(rng, median_wait * day_wait_factor, 0.78, 6.0, 150.0)
        talk = draw_talk(rng, median_talk, 1.0)
        wrap = draw_wrap(rng, 1.0)
        best = None

        ring = rng.uniform(3.0, 11.0)

        for ad in todays:
            if queue_id not in ad.queue_ids:
                continue

            duration = timedelta(seconds=ring + talk * ad.speed + wrap * ad.speed + 8)
            earliest = max(when + timedelta(seconds=base_wait - ring), when + timedelta(seconds=2))
            start = ad.fit(earliest, duration)

            if start is None or (start + timedelta(seconds=ring) - when).total_seconds() > MAX_VOICE_WAIT:
                continue

            score = (start - when).total_seconds() * 0.35 + rng.random() * 25 * ad.pref + ad.pref * 6

            if best is None or score < best[0]:
                best = (score, ad, start)

        if best is not None:
            _score, ad, ring_start = best
            record["ring"] = ring
            answered = ring_start + timedelta(seconds=ring)
            talk_seconds = talk * ad.speed
            wrap_seconds = wrap * ad.speed
            ended = answered + timedelta(seconds=talk_seconds)
            wrap_end = ended + timedelta(seconds=wrap_seconds)
            ad.commit(ring_start, wrap_end)
            ad.work.append((ring_start, answered, ended, wrap_end, record["id"], False))
            record.update(agent=ad, answered=answered, ended=ended, wrap_start=ended, wrap_end=wrap_end if rng.random() < 0.965 else None,
                          outcome="answered", wait=(answered - when).total_seconds(), queued=True, talk=talk_seconds, ring_start=ring_start)

            if rng.random() < PROVIDER_RECORDED[provider]:
                record["recorded"] = True

            if rng.random() < 0.22:
                length = rng.uniform(18, 150)
                held_at = answered + timedelta(seconds=talk_seconds * rng.uniform(0.2, 0.55))
                record["holds"].append((held_at, min(length, talk_seconds * 0.4)))

            if rng.random() < 0.07:
                add_transfer(rng, record, ad, todays, queue_id)

            if rng.random() < 0.09:
                others = [x for x in todays if queue_id in x.queue_ids and x is not ad]

                if others:
                    record["extra_offers"].append((rng.choice(others), ring_start - timedelta(seconds=rng.uniform(18, 26)), "missed"))

            return record

        STATS["converted"] += 1
        outcome = "abandoned" if rng.random() < 0.65 else "voicemail"

    if outcome == "abandoned":
        wait = lognorm(rng, 52, 0.55, 10.0, 168.0)
        record.update(outcome="abandoned", queued=True, wait=wait, ended=when + timedelta(seconds=wait + 0.6))
        record["hangup"] = "Canceled"

        if rng.random() < 0.15:
            record["wait"] = wait = rng.uniform(5, 14)
            record["ended"] = when + timedelta(seconds=wait + 0.6)

        return record

    if outcome == "voicemail":
        wait = rng.uniform(125, 178)
        message = rng.uniform(15, 80)
        record.update(outcome="voicemail", queued=True, wait=wait, answered=when + timedelta(seconds=wait + 2), ended=when + timedelta(seconds=wait + 2 + message))
        record["metadata"] = {"agentVoicemailProjection": True, "reasonCode": "queue_max_wait_voicemail"}

        return record

    wait = lognorm(rng, 42, 0.4, 18.0, 120.0)
    record.update(outcome="callback", queued=True, wait=wait, ended=when + timedelta(seconds=wait + rng.uniform(18, 34)))
    record["metadata"] = {"routing_terminal_reason": "queued_callback"}

    return record


def add_transfer(rng, record, ad, todays, queue_id):
    kind = weighted(rng, [("Queue", 0.45), ("Agent", 0.4), ("External", 0.15)])
    requested = record["answered"] + timedelta(seconds=record["talk"] * rng.uniform(0.35, 0.8))
    others = [x for x in todays if x is not ad]
    result = "Completed"

    if kind == "Agent" and not others:
        kind = "External"

    if kind == "Queue":
        to_id = rng.choice([q[0] for q in QUEUES if q[0] != record["queue"]])
        record["_transfer_queue_name"] = to_id
    elif kind == "Agent":
        to_id = rng.choice(others).item_id
    else:
        to_id = f"+1{rng.choice(AREA_CODES)}555{rng.randint(100, 199):04d}"

    if rng.random() < 0.12:
        result = rng.choice(["Failed: destination busy", "Failed: no answer", "Cancelled by the agent"])

    completed = requested + timedelta(seconds=rng.uniform(6, 38) if result == "Completed" else rng.uniform(15, 35))
    record["transfers"].append({"from": ad.item_id, "to": to_id, "type": kind, "requested": requested, "completed": completed, "result": result})


def inbound_text(rng, when, channel, queue, queue_id, todays, customers):
    provider = "Telnyx" if channel == "Sms" else "Webchat"
    record = base_record(rng, when, channel, "Inbound", queue_id, queue[0], customers, provider)
    record["provider_leg"] = None

    if channel == "Chat":
        record["customer"] = None
        record["customer_name"] = "Web visitor"

    if rng.random() > 0.94:
        wait = lognorm(rng, 150, 0.5, 30, 700)
        record.update(outcome="abandoned", wait=wait, ended=when + timedelta(seconds=wait))

        return record

    wait = lognorm(rng, 45 if channel == "Sms" else 25, 0.6, 8.0, MAX_TEXT_WAIT - 100)
    talk = lognorm(rng, 7 * 60, 0.6, 90, 25 * 60)
    wrap = draw_wrap(rng, 1.0)
    best = None

    for ad in todays:
        if queue_id not in ad.queue_ids:
            continue

        start = ad.fit(when + timedelta(seconds=wait), timedelta(seconds=(talk + wrap) * ad.speed + 8))

        if start is not None and (start - when).total_seconds() <= MAX_TEXT_WAIT and (best is None or start < best[1]):
            best = (ad, start)

    if best is None:
        record.update(outcome="abandoned", wait=wait, ended=when + timedelta(seconds=wait))

        return record

    ad, answered = best
    talk_seconds = talk * ad.speed
    ended = answered + timedelta(seconds=talk_seconds)
    wrap_end = ended + timedelta(seconds=wrap * ad.speed)
    ad.commit(answered, wrap_end)
    ad.work.append((None, answered, ended, wrap_end, record["id"], False))
    record.update(agent=ad, answered=answered, ended=ended, wrap_start=ended, wrap_end=wrap_end, outcome="answered", wait=(answered - when).total_seconds(), talk=talk_seconds)

    return record


def make_failed(rng, record):
    message, sip, hangup = rng.choice(PROVIDER_ERRORS[record["provider"]])
    record.update(outcome="failed", status="Failed", ended=record["created"] + timedelta(seconds=rng.uniform(1.5, 6.0)), error=(message, sip), hangup=hangup)
    record["metadata"] = {"providerError": message, "sipResponseCode": sip, "providerHangupCause": hangup}

    return record


def after_hours(rng, when, queue, queue_ids, customers):
    STATS["after_hours"] += 1
    provider = pick_provider(rng)
    record = base_record(rng, when, "Voice", "Inbound", queue_ids[queue[0]], queue[0], customers, provider)

    if rng.random() < 0.78:
        message = rng.uniform(14, 75)
        record.update(outcome="voicemail", answered=when + timedelta(seconds=7), ended=when + timedelta(seconds=7 + message), wait=0.0)
        record["metadata"] = {"agentVoicemailProjection": True, "reasonCode": "entry_point_closed_voicemail"}
    else:
        record.update(outcome="abandoned_greeting", ended=when + timedelta(seconds=rng.uniform(4, 22)), wait=0.0)
        record["hangup"] = "Canceled"

    return record


def outbound(rng, when, todays, queue_ids, customers):
    if not todays:
        return None

    channel = "Sms" if rng.random() < 0.10 else "Voice"
    provider = "Telnyx" if channel == "Sms" else pick_provider(rng)
    failed = channel == "Voice" and rng.random() < PROVIDER_FAIL[provider] * 1.6
    connected = rng.random() < (0.55 if channel == "Sms" else 0.64)
    ring = rng.uniform(5, 24) if connected else rng.uniform(22, 38)
    base_talk = lognorm(rng, 4.2 * 60, 0.62, 60.0, 14.6 * 60.0) if channel == "Voice" else lognorm(rng, 4 * 60, 0.6, 60, 20 * 60)

    if channel == "Voice" and rng.random() < 0.02:
        base_talk = rng.uniform(15.5, 30.0) * 60.0

    base_wrap = draw_wrap(rng, 1.0)
    best = None

    for ad in todays:
        if failed:
            duration = timedelta(seconds=30)
        elif connected:
            duration = timedelta(seconds=ring + (base_talk + base_wrap) * ad.speed + 8)
        else:
            duration = timedelta(seconds=ring + 20)

        start = ad.fit(when, duration)

        if start is None or start - when > timedelta(seconds=75):
            continue

        score = (start - when).total_seconds() + rng.random() * 200 * ad.pref

        if best is None or score < best[0]:
            best = (score, ad, start)

    if best is None:
        return None

    _score, ad, start = best
    revenue = [n for n in ad.queue_names if n in ("New sales", "Renewals")]
    queue_name = rng.choice(revenue) if revenue and rng.random() < 0.75 else rng.choice(ad.queue_names)
    record = base_record(rng, start, channel, "Outbound", queue_ids[queue_name], queue_name, customers, provider)
    record["created_by"] = ad

    if channel == "Sms":
        record["provider_leg"] = None

    if failed:
        make_failed(rng, record)
        record["agent"] = ad
        ad.commit(start, record["ended"] + timedelta(seconds=20))
        ad.work.append((None, start, record["ended"], ad.free_at, record["id"], True))

        return record

    if connected:
        talk = base_talk * ad.speed
        answered = start + timedelta(seconds=ring)
        ended = answered + timedelta(seconds=talk)
        wrap_end = ended + timedelta(seconds=base_wrap * ad.speed)
        ad.commit(start, wrap_end)
        ad.work.append((None, start, ended, wrap_end, record["id"], True))
        record.update(agent=ad, answered=answered, ended=ended, wrap_start=ended, wrap_end=wrap_end if rng.random() < 0.97 else None,
                      outcome="answered", wait=ring, talk=talk, ring=ring)

        if channel == "Voice":
            if rng.random() < PROVIDER_RECORDED[provider]:
                record["recorded"] = True

            if rng.random() < 0.18:
                record["holds"].append((answered + timedelta(seconds=talk * rng.uniform(0.2, 0.5)), min(rng.uniform(15, 90), talk * 0.4)))

            if rng.random() < 0.03:
                add_transfer(rng, record, ad, todays, record["queue_id"])

        return record

    ended = start + timedelta(seconds=ring + rng.uniform(1, 4))
    ad.commit(start, ended + timedelta(seconds=10))
    ad.work.append((None, start, ended, ad.free_at, record["id"], True))
    record.update(agent=ad, outcome="not_connected", ended=ended, ring=ring)
    record["hangup"] = rng.choice(["NoAnswer", "NoAnswer", "Busy"])

    return record


# ---------------------------------------------------------------------------------------------------------------
# Documents
# ---------------------------------------------------------------------------------------------------------------

def event(rng, interaction_id, correlation, event_type, when, data=None, aggregate_type="Interaction", aggregate_id=None, actor_type="System", actor_id="system", component="Routing"):
    return {
        "InteractionId": interaction_id, "EventType": event_type, "SchemaVersion": 1, "AggregateType": aggregate_type,
        "AggregateId": aggregate_id or interaction_id, "CorrelationId": correlation, "ActorId": actor_id, "ActorType": actor_type,
        "SourceComponent": component, "OccurredUtc": when, "RecordedUtc": when + timedelta(milliseconds=rng.randint(4, 60)),
        "Data": compact(data) if data is not None else None,
    }


def build_interaction_events(rng, rec, queue_names):
    ev = []
    iid, corr, created = rec["id"], rec["correlation"], rec["created"]
    outcome, agent = rec["outcome"], rec["agent"]
    voice = rec["channel"] == "Voice"
    base = {"InteractionId": iid, "ProviderName": rec["provider"], "Direction": rec["direction"], "QueueId": rec["queue_id"]}

    def add(event_type, when, data=None, **kw):
        ev.append(event(rng, iid, corr, event_type, when, data, **kw))

    add("InteractionCreated", created, {"Channel": rec["channel"], "Direction": rec["direction"]}, component="Interactions")

    if not voice:
        if outcome == "answered":
            add("InteractionStarted", rec["answered"], {"AgentId": agent.item_id}, actor_type="Agent", actor_id=agent.user_id, component="Interactions")

        add("InteractionEnded", rec["ended"], None, component="Interactions")

        return ev

    if rec["direction"] == "Outbound":
        add("DialStarted", created + timedelta(milliseconds=300), {**base, "AgentId": agent.item_id if agent else None, "PhoneNumber": rec["customer"]}, actor_type="Agent", actor_id=agent.user_id if agent else "system")

        if outcome == "failed":
            add("DialFailed", rec["ended"], {**base, "Reason": rec["error"][0], "Details": {"sip": str(rec["error"][1])}}, actor_type="Provider", actor_id=rec["provider"])
            add("InteractionFailed", rec["ended"] + timedelta(milliseconds=40), {"Reason": rec["error"][0], "ProviderName": rec["provider"]}, component="Interactions")
        elif outcome == "answered":
            add("AgentLegAnswered", rec["answered"], {**base, "AgentId": agent.item_id}, actor_type="Provider", actor_id=rec["provider"])
            add("CallConnected", rec["answered"] + timedelta(milliseconds=120), {**base, "AgentId": agent.item_id}, actor_type="Provider", actor_id=rec["provider"])
        else:
            add("CallEnded", rec["ended"], {**base, "HangupCause": rec["hangup"], "AgentId": agent.item_id if agent else None}, actor_type="Provider", actor_id=rec["provider"])

        add_call_tail(add, rec, base, agent)

        return ev

    # ---- inbound voice
    if outcome == "failed":
        add("InteractionFailed", rec["ended"], {"Reason": rec["error"][0], "ProviderName": rec["provider"], "SipResponseCode": rec["error"][1]}, component="Interactions", actor_type="Provider", actor_id=rec["provider"])
        add("CallEnded", rec["ended"] + timedelta(milliseconds=30), {**base, "HangupCause": rec["hangup"], "ProviderHangupCause": str(rec["error"][1])}, actor_type="Provider", actor_id=rec["provider"])

        return ev

    if outcome in ("voicemail", "abandoned_greeting") and not rec["queued"]:
        if outcome == "voicemail":
            add("CallSentToVoicemail", created + timedelta(seconds=6), {**base, "Reason": rec["metadata"].get("reasonCode")})
        else:
            add("CallEnded", rec["ended"], {**base, "HangupCause": "Canceled", "HangupSource": "Caller"}, actor_type="Customer", actor_id="caller")
            return ev

        add("CallEnded", rec["ended"], {**base, "HangupCause": "NormalClearing", "HangupSource": "Caller"}, actor_type="Customer", actor_id="caller")

        return ev

    queued_at = created + timedelta(milliseconds=500)
    add("CallQueued", queued_at, {**base})

    for other, when, kind in rec["extra_offers"]:
        reservation = new_id(rng)
        add("OfferPresented", when, {"ReservationId": reservation, "InteractionId": iid, "QueueId": rec["queue_id"], "AgentId": other.item_id, "UserId": other.user_id, "Channel": "Voice", "PresentedUtc": iso(when)}, aggregate_type="ActivityReservation", aggregate_id=reservation, component="Routing")
        settled = when + timedelta(seconds=22)
        add("OfferMissed", settled, {"ReservationId": reservation, "InteractionId": iid, "QueueId": rec["queue_id"], "AgentId": other.item_id, "UserId": other.user_id, "Channel": "Voice", "Reason": "NoAnswer", "PresentedUtc": iso(when), "SettledUtc": iso(settled), "RingSeconds": 22.0}, aggregate_type="ActivityReservation", aggregate_id=reservation)

    if outcome == "answered":
        reservation = new_id(rng)
        ring_start = rec["ring_start"]
        add("OfferPresented", ring_start, {"ReservationId": reservation, "InteractionId": iid, "QueueId": rec["queue_id"], "AgentId": agent.item_id, "UserId": agent.user_id, "Channel": "Voice", "PresentedUtc": iso(ring_start)}, aggregate_type="ActivityReservation", aggregate_id=reservation)
        add("OfferAccepted", rec["answered"], {"ReservationId": reservation, "InteractionId": iid, "QueueId": rec["queue_id"], "AgentId": agent.item_id, "UserId": agent.user_id, "Channel": "Voice", "PresentedUtc": iso(ring_start), "SettledUtc": iso(rec["answered"]), "RingSeconds": round(rec["ring"], 2)}, aggregate_type="ActivityReservation", aggregate_id=reservation, actor_type="Agent", actor_id=agent.user_id)
        add("CallDequeued", rec["answered"] - timedelta(milliseconds=150), {**base, "AgentId": agent.item_id, "State": "Assigned", "DurationSeconds": round(rec["wait"], 2)})
        add("AgentLegAnswered", rec["answered"], {**base, "AgentId": agent.item_id}, actor_type="Provider", actor_id=rec["provider"])
        add("CallConnected", rec["answered"] + timedelta(milliseconds=120), {**base, "AgentId": agent.item_id}, actor_type="Provider", actor_id=rec["provider"])
        add_call_tail(add, rec, base, agent)

        return ev

    if outcome == "abandoned":
        add("CallAbandoned", rec["ended"], {**base, "DurationSeconds": round(rec["wait"], 2), "HangupSource": "Caller"}, actor_type="Customer", actor_id="caller")
        add("CallDequeued", rec["ended"] + timedelta(milliseconds=40), {**base, "State": "Removed", "DurationSeconds": round(rec["wait"], 2), "Reason": "Abandoned"})
        add("CallEnded", rec["ended"] + timedelta(milliseconds=90), {**base, "HangupCause": "Canceled", "HangupSource": "Caller"}, actor_type="Provider", actor_id=rec["provider"])
    elif outcome == "voicemail":
        left = queued_at + timedelta(seconds=rec["wait"])
        add("CallDequeued", left, {**base, "State": "Removed", "DurationSeconds": round(rec["wait"], 2), "Reason": "MaxWait"})
        add("CallSentToVoicemail", left + timedelta(milliseconds=200), {**base, "Reason": "queue_max_wait_voicemail"})
        add("CallEnded", rec["ended"], {**base, "HangupCause": "NormalClearing", "HangupSource": "Caller"}, actor_type="Provider", actor_id=rec["provider"])
    elif outcome == "callback":
        accepted = queued_at + timedelta(seconds=rec["wait"])
        add("CallbackRequested", accepted, {**base, "DurationSeconds": round(rec["wait"], 2), "Reason": "queued_callback"}, actor_type="Customer", actor_id="caller")
        add("CallDequeued", accepted + timedelta(milliseconds=60), {**base, "State": "Removed", "DurationSeconds": round(rec["wait"], 2), "Reason": "queued_callback"})
        add("CallEnded", rec["ended"], {**base, "HangupCause": "NormalClearing", "HangupSource": "Platform"}, actor_type="Provider", actor_id=rec["provider"])

    return ev


def add_call_tail(add, rec, base, agent):
    if rec["outcome"] != "answered":
        return

    for held_at, length in rec["holds"]:
        add("CallHeld", held_at, {**base, "AgentId": agent.item_id}, actor_type="Agent", actor_id=agent.user_id)
        add("CallResumed", held_at + timedelta(seconds=length), {**base, "AgentId": agent.item_id, "DurationSeconds": round(length, 2)}, actor_type="Agent", actor_id=agent.user_id)

    for transfer in rec["transfers"]:
        add("InteractionTransferred", transfer["requested"], {"InteractionId": rec["id"], "FromAgentId": agent.item_id, "TargetType": transfer["type"], "Result": transfer["result"]}, actor_type="Agent", actor_id=agent.user_id, component="Interactions")

    if rec["recorded"]:
        add("RecordingStarted", rec["answered"] + timedelta(milliseconds=400), {"InteractionId": rec["id"], "RecordingReference": rec["recording_ref"]})
        add("RecordingStopped", rec["ended"], {"InteractionId": rec["id"], "RecordingReference": rec["recording_ref"]})

    add("CallEnded", rec["ended"], {**base, "AgentId": agent.item_id, "HangupCause": "NormalClearing", "HangupSource": rng_choice_source(rec)}, actor_type="Provider", actor_id=rec["provider"])


def rng_choice_source(rec):
    return "Caller" if (int(rec["id"][1:4], 36) % 3) else "Agent"


def interaction_doc(rng, rec, queue_ids_by_name):
    agent = rec["agent"]
    outcome = rec["outcome"]
    direction = rec["direction"]
    answered = rec["answered"]
    ended = rec["ended"]
    queue_history = []

    if rec["queued"]:
        exit_reason = {"answered": "Assigned", "abandoned": "Abandoned", "voicemail": "Voicemail", "callback": "CallbackRequested"}.get(outcome)
        exit_at = (answered - timedelta(milliseconds=150)) if outcome == "answered" else (rec["created"] + timedelta(seconds=rec["wait"] + 0.5))
        queue_history.append({"QueueId": rec["queue_id"], "EnteredUtc": iso(rec["created"] + timedelta(milliseconds=500)), "ExitedUtc": iso(exit_at), "ExitReason": exit_reason})

    transfer_history = []

    for transfer in rec["transfers"]:
        to_id = transfer["to"]

        if transfer["type"] == "Queue":
            to_id = queue_ids_by_name[rec["_transfer_queue_name"]]
            queue_history.append({"QueueId": to_id, "EnteredUtc": iso(transfer["completed"]), "ExitedUtc": iso(ended) if ended else None, "ExitReason": "Assigned"})

        transfer_history.append({"FromParticipantId": transfer["from"], "ToParticipantId": to_id, "TargetType": transfer["type"], "RequestedUtc": iso(transfer["requested"]), "CompletedUtc": iso(transfer["completed"]), "Result": transfer["result"]})

    participants = [{"Role": "Customer", "Identifier": rec["customer"] or "web-visitor", "DisplayName": rec["customer_name"], "Address": rec["customer"], "JoinedUtc": iso(rec["created"]), "LeftUtc": iso(ended) if ended else None}]

    if agent is not None and outcome == "answered":
        participants.append({"Role": "Agent", "Identifier": agent.user_id, "DisplayName": agent.display, "Address": agent.user, "JoinedUtc": iso(answered), "LeftUtc": iso(ended)})

    created_by = rec.get("created_by")
    recording_ref = None

    if rec["recorded"]:
        if rec["provider"] == "Telnyx":
            recording_ref = "telnyx-rec-" + rec["id"][:16]
        else:
            recording_ref = f"asterisk/{rec['created']:%Y/%m/%d}/{rec['provider_call'].replace('.', '-')}.wav"

        rec["recording_ref"] = recording_ref

    doc = {
        "EntityProperties": {}, "Channel": rec["channel"], "Direction": direction, "Status": rec["status"],
        "ProviderName": rec["provider"], "ProviderInteractionId": rec["provider_call"], "CustomerAddress": rec["customer"],
        "QueueId": rec["queue_id"], "CorrelationId": rec["correlation"], "RecordingState": "Stopped" if recording_ref else "None",
        "RecordingLegalHold": False, "TechnicalMetadata": rec["metadata"], "QueueHistory": queue_history, "TransferHistory": transfer_history,
        "CreatedUtc": iso(rec["created"]), "StartedUtc": iso(rec["created"]), "Participants": participants, "ItemId": rec["id"],
        "Properties": {"SeedTag": TAG},
    }

    if rec["provider_leg"]:
        doc["ProviderLegId"] = rec["provider_leg"]

    if agent is not None:
        doc["AgentId"] = agent.item_id

    if created_by is not None:
        doc["CreatedById"] = created_by.user_id
        doc["CreatedByUserName"] = created_by.user

    if recording_ref:
        doc["RecordingReference"] = recording_ref
        doc["RecordingDisclosedUtc"] = iso(answered)

    if answered:
        doc["AnsweredUtc"] = iso(answered)

    if ended:
        doc["EndedUtc"] = iso(ended)

    if rec["wrap_start"] and outcome == "answered":
        doc["WrapUpStartedUtc"] = iso(rec["wrap_start"])

        if rec["wrap_end"]:
            doc["WrapUpCompletedUtc"] = iso(rec["wrap_end"])

    modified = rec["wrap_end"] or ended or rec["created"]
    doc["ModifiedUtc"] = iso(modified)

    return doc


def interaction_index(rec, doc_id):
    return dict(
        DocumentId=doc_id, ItemId=rec["id"], Channel=["Voice", "Sms", "Email", "Chat"].index(rec["channel"]),
        Direction=["Inbound", "Outbound"].index(rec["direction"]), Status=["Created", "Ringing", "Connected", "Held", "Transferring", "Conferenced", "Ended", "Failed"].index(rec["status"]),
        ProviderName=rec["provider"], ProviderInteractionId=rec["provider_call"], ProviderLegId=rec["provider_leg"],
        AgentId=rec["agent"].item_id if rec["agent"] is not None else None, CorrelationId=rec["correlation"], CreatedUtc=sqlts(rec["created"]),
        EndedUtc=sqlts(rec["ended"]), WrapUpStartedUtc=sqlts(rec["wrap_start"]) if rec["outcome"] == "answered" else None,
        WrapUpCompletedUtc=sqlts(rec["wrap_end"]) if rec["outcome"] == "answered" else None, RecordingLegalHold=0,
        RecordingState=3 if rec["recorded"] else 0, QueueId=rec["queue_id"],
    )


def call_session(rng, rec):
    agent = rec["agent"]
    leg_customer = {"ProviderLegId": rec["provider_leg"], "Role": "Customer", "Status": "Ended", "Address": rec["customer"], "StartedUtc": iso(rec["created"]), "HangupCause": rec["hangup"] if rec["outcome"] != "answered" else "NormalClearing"}
    legs = []
    inbound_call = rec["direction"] == "Inbound"
    state = {"answered": "Ended", "abandoned": "Ended", "voicemail": "Ended", "abandoned_greeting": "Ended", "callback": "Ended", "failed": "Failed", "not_connected": "NoAnswer"}[rec["outcome"]]

    if rec["outcome"] == "failed":
        leg_customer["Status"] = "Failed"
        leg_customer["EndedUtc"] = iso(rec["ended"])
        legs.append(leg_customer)
    else:
        if inbound_call or rec["outcome"] == "answered":
            leg_customer["AnsweredUtc"] = iso(rec["answered"] if rec["outcome"] == "answered" and not inbound_call else rec["created"] + timedelta(seconds=1))

        leg_customer["EndedUtc"] = iso(rec["ended"])

        if rec["outcome"] == "not_connected":
            leg_customer["Status"] = "Failed"

        legs.append(leg_customer)

    for other, when, _kind in rec["extra_offers"]:
        legs.append({"ProviderLegId": f"{rec['provider_leg'] or rec['id']}-a{len(legs)}", "Role": "Agent", "Status": "Failed", "Address": other.user, "AgentId": other.item_id, "StartedUtc": iso(when), "EndedUtc": iso(when + timedelta(seconds=22)), "HangupCause": "NoAnswer"})

    if agent is not None and rec["outcome"] == "answered":
        start = rec.get("ring_start") or rec["created"] + timedelta(seconds=1)
        legs.append({"ProviderLegId": f"{rec['provider_leg'] or rec['id']}-a", "Role": "Agent", "Status": "Ended", "Address": agent.user, "AgentId": agent.item_id, "StartedUtc": iso(start), "AnsweredUtc": iso(rec["answered"]), "EndedUtc": iso(rec["ended"]), "HangupCause": "NormalClearing"})

    hold = sum(length for _at, length in rec["holds"])
    doc = {
        "InteractionId": rec["id"], "ProviderName": rec["provider"], "ProviderCallId": rec["provider_call"], "DeliveryModel": "ServerSideAcd",
        "Direction": rec["direction"], "State": state, "QueueId": rec["queue_id"], "Legs": legs, "PriorBridges": [], "Consults": [],
        "Relationships": [], "MonitorSessions": [], "FromAddress": rec["customer"] if inbound_call else None, "ToAddress": None if inbound_call else rec["customer"],
        "IsOnHold": False, "HoldPlacedByAgent": False, "IsMuted": False, "RecordingState": "Stopped" if rec["recorded"] else "None",
        "CreatedUtc": iso(rec["created"]), "StartedUtc": iso(rec["created"]), "HangupCause": rec["hangup"], "TalkSeconds": round(max(0.0, rec["talk"] - hold), 2),
        "HoldSeconds": round(hold, 2), "Metadata": {}, "ModifiedUtc": iso(rec["ended"]), "ItemId": new_id(rng), "Properties": {"SeedTag": TAG},
    }

    if rec["recorded"]:
        doc["RecordingReference"] = rec["recording_ref"]

    if agent is not None:
        doc["AgentId"] = agent.item_id

    if rec["answered"]:
        doc["AnsweredUtc"] = iso(rec["answered"])

    if rec["ended"]:
        doc["EndedUtc"] = iso(rec["ended"])

    return doc, legs


def call_quality(rng, rec, agent_bias):
    agent = rec["agent"]
    bad = agent_bias.get(agent.user, 0.04)
    roll = rng.random()
    transport = weighted(rng, [("host", 0.52), ("srflx", 0.38), ("relay", 0.10)])

    if roll < bad:
        kind = weighted(rng, [("loss", 0.35), ("jitter", 0.2), ("latency", 0.2), ("noaudio_in", 0.1), ("noaudio_out", 0.15)])
        rating = "Poor"
    elif roll < bad + 0.11:
        kind, rating = "degraded", "Degraded"
    else:
        kind, rating = "good", "Good"

    duration_ms = int((rec["ended"] - rec["answered"]).total_seconds() * 1000)
    report = {
        "CallId": rec["provider_call"], "Direction": rec["direction"].lower(), "Codec": "opus", "LocalCandidateType": transport,
        "RemoteCandidateType": "host", "PacketsReceived": int(duration_ms / 20), "PacketsLost": 0, "LossPercent": round(rng.uniform(0, 0.4), 2),
        "JitterMs": round(rng.uniform(3, 14), 1), "RoundTripTimeMs": round(rng.uniform(25, 110), 0), "BytesReceived": int(duration_ms * 1.6),
        "MicrophoneLevel": round(rng.uniform(0.08, 0.4), 2), "CaptureReported": True, "BytesSent": int(duration_ms * 1.6),
        "PacketsSent": int(duration_ms / 20), "CaptureDevice": rng.choice(CAPTURE_DEVICES), "SendCodec": "opus", "Final": True,
        "SampleCount": max(2, duration_ms // 5000), "DurationMs": duration_ms, "ProviderCallControlId": rec["provider_call"], "ProviderLegId": (rec["provider_leg"] or "") + "-a",
    }
    mos = rng.uniform(4.1, 4.45)

    if kind == "degraded":
        mos = rng.uniform(3.62, 3.98)
        report["LossPercent"] = round(rng.uniform(1.0, 3.2), 2)
        report["JitterMs"] = round(rng.uniform(16, 28), 1)
    elif kind == "loss":
        mos = rng.uniform(2.3, 3.4)
        report["LossPercent"] = round(rng.uniform(5.2, 14), 2)
        report["PacketsLost"] = int(report["PacketsReceived"] * report["LossPercent"] / 100)
    elif kind == "jitter":
        mos = rng.uniform(2.9, 3.45)
        report["JitterMs"] = round(rng.uniform(34, 90), 1)
        report["LossPercent"] = round(rng.uniform(0.5, 3.0), 2)
    elif kind == "latency":
        mos = rng.uniform(3.0, 3.48)
        report["RoundTripTimeMs"] = round(rng.uniform(320, 640), 0)
        report["LocalCandidateType"] = "relay"
    elif kind == "noaudio_in":
        mos = 0.0
        report["BytesReceived"] = 0
    elif kind == "noaudio_out":
        mos = rng.uniform(3.9, 4.3)
        report["OutboundAudioStalled"] = True
        report["PacketsSent"] = 0
        report["BytesSent"] = 0

    report["Mos"] = round(mos, 2)
    report["AvgMos"] = round(mos, 2)
    report["MinMos"] = round(max(0.0, mos - rng.uniform(0.05, 0.5)), 2)
    report["MaxLossPercent"] = round(report["LossPercent"] * rng.uniform(1.0, 1.8), 2)
    report["Poor"] = rating == "Poor"
    observed = rec["ended"] + timedelta(seconds=rng.uniform(1, 4))

    doc = {
        "RecordKey": f"Browser|{rec['provider_call']}", "Source": "Browser", "Rating": rating, "ProviderName": rec["provider"],
        "ProviderCallControlId": rec["provider_call"], "ProviderLegId": report["ProviderLegId"], "InteractionId": rec["id"], "LegRole": "Agent",
        "AgentId": agent.item_id, "UserId": agent.user_id, "QueueId": rec["queue_id"], "Mos": round(mos, 2), "LossPercent": report["LossPercent"],
        "JitterMs": report["JitterMs"], "RoundTripMs": report["RoundTripTimeMs"], "DurationSeconds": duration_ms / 1000.0, "Browser": report,
        "ObservedUtc": iso(observed), "CreatedUtc": iso(observed), "ItemId": new_id(rng), "Properties": {"SeedTag": TAG},
    }

    return doc, observed


# ---------------------------------------------------------------------------------------------------------------
# Agent presence
# ---------------------------------------------------------------------------------------------------------------

def agent_events(rng, agent_day, queue_ids, reason_codes):
    """The AgentStateChanged events of one agent's day, chained so every change leaves the state the previous one entered."""
    items = []                      # (time, state, source, reason, interaction_id)

    for reserved_at, busy_at, ended, wrap_end, interaction_id, direct in agent_day.work:
        if reserved_at is not None:
            items.append((reserved_at, RESERVED, "Reserved", None, interaction_id))

        items.append((busy_at, BUSY, "Connected", None, interaction_id))
        items.append((ended, WRAPUP, "WrapUpStarted", None, interaction_id))
        items.append((wrap_end, AVAILABLE, "WorkCompleted", None, interaction_id))

    for start, end, status, reason in agent_day.blocks:
        items.append((start, status, "SetState", reason, None))
        items.append((end, AVAILABLE, "SetState", None, None))

    items.sort(key=lambda x: x[0])
    result = []
    previous = OFFLINE
    when = agent_day.shift_start
    sequence = [(when, AVAILABLE, "SignIn", None, None)] + items + [(agent_day.shift_end, OFFLINE, "SignOut", None, None)]
    last_time = None

    for at, state, source, reason, interaction_id in sequence:
        if last_time is not None and at <= last_time:
            at = last_time + timedelta(seconds=1)

        if state == previous and source not in ("SignIn", "SignOut"):
            continue

        result.append((at, previous, state, source, reason, interaction_id))
        previous = state
        last_time = at

    return result


def state_event(rng, agent_day, at, previous, state, source, reason, interaction_id, reason_codes):
    data = {
        "AgentId": agent_day.item_id, "UserId": agent_day.user_id, "PreviousState": previous, "CurrentState": state, "RequestedState": None,
        "ReasonCodeId": reason_codes.get(reason) if reason else None, "ReasonName": reason, "Source": source, "InteractionId": interaction_id,
        "ReservationId": None, "ReleaseReason": None, "AgentSessionId": None,
        "QueueIds": agent_day.queue_ids if state != OFFLINE else [], "CampaignIds": [], "ChangedUtc": iso(at),
    }
    actor_type = "System" if source in ("Reserved", "Connected", "WrapUpStarted", "WorkCompleted") else "Agent"
    actor_id = "system" if actor_type == "System" else agent_day.user_id

    return event(rng, None, "seed-agent-" + agent_day.item_id[:12], "AgentStateChanged", at, data, aggregate_type="AgentProfile",
                 aggregate_id=agent_day.item_id, actor_type=actor_type, actor_id=actor_id, component="Agents")


# ---------------------------------------------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------------------------------------------

def main():
    parser = argparse.ArgumentParser(description="Seed fake Contact Center history for the report demos.")
    parser.add_argument("app_data", help="the site's App_Data folder (the site must be stopped)")
    parser.add_argument("--reset", action="store_true", help="remove the history this script added before and seed it again")
    parser.add_argument("--scale", type=float, default=1.0, help="interaction volume factor (1.0 is about 2500 interactions in 40 days, about 650 in the last 7; 0.25 is about 600 in all)")
    parser.add_argument("--seed", type=int, default=20260101, help="random seed (the same seed gives the same history)")
    parser.add_argument("--days", type=int, default=40, help="days of history before today")
    args = parser.parse_args()

    db_path = os.path.join(args.app_data, "Sites", "Default", "OrchardCore.db")

    if not os.path.exists(db_path):
        raise SystemExit(f"Cannot find {db_path}")

    db = Db(db_path)

    existing = db.con.execute("select count(*) from ContactCenter_Document where Content like ?", (f'%"SeedTag":"{TAG}"%',)).fetchone()[0]

    if existing and not args.reset:
        raise SystemExit(f"This site already holds {existing} seeded documents. Run again with --reset to replace them (or start from a fresh copy of the base snapshot).")

    if existing:
        print(f"Removing {remove_seeded(db.con)} documents added by an earlier run ...")
        db.next_id = (db.con.execute("select coalesce(max(Id),0) from ContactCenter_Document").fetchone()[0] or 0) + 1

    site = db.con.execute("select Content from Document where Type like 'OrchardCore.Settings.SiteSettings%'").fetchone()
    tz_id = (json.loads(site[0]).get("TimeZoneId") if site else None) or "America/New_York"
    clock = LocalClock(tz_id)
    now = datetime.now(timezone.utc).replace(tzinfo=None, microsecond=0)
    rng = random.Random(args.seed)

    # a separate stream, so the history is the same whether or not the queues and agent profiles already existed
    groups, queues, agents, created = ensure_prerequisites(db, random.Random(args.seed ^ 0x5EED), now)
    reason_codes = load_reason_codes(db.con)

    for needed in ("Lunch", "Short break", "Team meeting"):
        if needed not in reason_codes:
            print(f"warning: the agent state reason code '{needed}' does not exist; its changes are recorded without a reason code")

    print(f"Prerequisites: {created['groups']} queue groups, {created['queues']} queues, {created['agents']} agent profiles created (existing ones are reused).")

    records, agent_days = simulate(db, rng, now, clock, args.days, args.scale, queues, agents, reason_codes)
    queue_names_by_id = {v: k for k, v in queues.items()}
    agent_bias = {"alex.kim": 0.10, "jamie.chen": 0.04, "maria.lopez": 0.03, "sam.patel": 0.05, "priya.shah": 0.02}
    counts = {}
    n_events = n_sessions = n_quality = 0

    for rec in records:
        counts[rec["outcome"]] = counts.get(rec["outcome"], 0) + 1
        doc = interaction_doc(rng, rec, queues)
        doc_id = db.add("Interaction", doc)
        db.row("ContactCenter_InteractionIndex", **interaction_index(rec, doc_id))

        for ev in build_interaction_events(rng, rec, queue_names_by_id):
            db_event(db, rng, ev)
            n_events += 1

        if rec["channel"] == "Voice":
            session_doc, legs = call_session(rng, rec)
            session_id = db.add("CallSession", session_doc)
            db.row("ContactCenter_CallSessionIndex", DocumentId=session_id, ItemId=session_doc["ItemId"], InteractionId=rec["id"], ProviderName=rec["provider"],
                   ProviderCallId=rec["provider_call"], ProviderCallClaimKey=f"{rec['provider']}|{rec['provider_call']}", State=["Planned", "Dialing", "Ringing", "Connected", "OnHold", "Ending", "Ended", "Failed", "NoAnswer", "Rejected", "Canceled", "Transferred"].index(session_doc["State"]),
                   AgentId=session_doc.get("AgentId"), CreatedUtc=sqlts(rec["created"]), EndedUtc=sqlts(rec["ended"]), QueueId=rec["queue_id"])

            for leg in legs:
                if leg.get("ProviderLegId"):
                    db.row("ContactCenter_CallSessionLegIndex", DocumentId=session_id, ItemId=session_doc["ItemId"], ProviderLegId=leg["ProviderLegId"],
                           Role=["Unknown", "Customer", "Agent", "Consult"].index(leg["Role"]), AgentId=leg.get("AgentId"), InteractionId=rec["id"],
                           StartedUtc=sqlts(datetime.strptime(leg["StartedUtc"].rstrip("Z").split(".")[0], "%Y-%m-%dT%H:%M:%S")))

            n_sessions += 1

            if rec["outcome"] == "answered" and rng.random() < 0.88:
                quality_doc, observed = call_quality(rng, rec, agent_bias)
                quality_id = db.add("CallQualityRecord", quality_doc)
                db.row("ContactCenter_CallQualityRecordIndex", DocumentId=quality_id, ItemId=quality_doc["ItemId"], RecordKey=quality_doc["RecordKey"],
                       Source=0, Rating=["Good", "Degraded", "Poor"].index(quality_doc["Rating"]), InteractionId=rec["id"], AgentId=quality_doc["AgentId"],
                       ObservedUtc=sqlts(observed), QueueId=rec["queue_id"])
                n_quality += 1

    state_events = 0

    for agent_day in agent_days:
        for at, previous, state, source, reason, interaction_id in agent_events(rng, agent_day, queues, reason_codes):
            ev = state_event(rng, agent_day, at, previous, state, source, reason, interaction_id, reason_codes)
            db_event(db, rng, ev, tagged=True)
            state_events += 1

    # the agent profiles end the history signed out; tell them when
    last_out = {}

    for agent_day in agent_days:
        last_out[agent_day.user] = max(last_out.get(agent_day.user, agent_day.shift_end), agent_day.shift_end)

    db.flush()

    for user, when in last_out.items():
        info = agents[user]
        doc = json.loads(db.con.execute("select Content from ContactCenter_Document where Id=?", (info["doc_id"],)).fetchone()[0])
        doc["PresenceChangedUtc"] = iso(when)
        doc["LastWorkCompletedUtc"] = iso(when - timedelta(minutes=10))
        db.con.execute("update ContactCenter_Document set Content=?, Version=Version+1 where Id=?", (compact(doc), info["doc_id"]))

    db.con.commit()
    db.con.execute("PRAGMA wal_checkpoint(TRUNCATE)")
    db.con.close()

    first = min(r["created"] for r in records)
    last = max(r["created"] for r in records)
    recent_cut = now - timedelta(days=7)
    print(f"Seeded {len(records)} interactions ({first:%Y-%m-%d} .. {last:%Y-%m-%d} UTC), {n_events} interaction events, {state_events} agent presence changes, {n_sessions} call sessions, {n_quality} call quality records.")
    print("Outcomes: " + ", ".join(f"{k} {v}" for k, v in sorted(counts.items(), key=lambda kv: -kv[1])))
    print(f"Last 7 days: {sum(1 for r in records if r['created'] >= recent_cut)} interactions.")


def db_event(db, rng, ev, tagged=True):
    content = {
        "EntityProperties": {}, "InteractionId": ev["InteractionId"], "EventType": ev["EventType"], "SchemaVersion": 1, "AggregateType": ev["AggregateType"],
        "AggregateId": ev["AggregateId"], "CorrelationId": ev["CorrelationId"], "ActorId": ev["ActorId"], "ActorType": ev["ActorType"],
        "SourceComponent": ev["SourceComponent"], "OccurredUtc": iso(ev["OccurredUtc"]), "RecordedUtc": iso(ev["RecordedUtc"]),
        "ItemId": new_id(rng), "Properties": {"SeedTag": TAG},
    }

    if ev["Data"] is not None:
        content["Data"] = ev["Data"]

    if content["InteractionId"] is None:
        del content["InteractionId"]

    doc_id = db.add("InteractionEvent", content)
    db.row("ContactCenter_InteractionEventIndex", DocumentId=doc_id, ItemId=content["ItemId"], InteractionId=ev["InteractionId"], EventType=ev["EventType"],
           AggregateType=ev["AggregateType"], AggregateId=ev["AggregateId"], CorrelationId=ev["CorrelationId"], IdempotencyKey=None,
           IdempotencyClaimKey=content["ItemId"], OccurredUtc=sqlts(ev["OccurredUtc"]))


if __name__ == "__main__":
    sys.exit(main())
