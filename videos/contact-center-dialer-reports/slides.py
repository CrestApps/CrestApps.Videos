"""The slides of the Dialer Reports video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Is the dialer running?", "Dialer activity and health"),
    ("2", "Why was this record not called?", "Dialer decision log, by record"),
    ("3", "Why did no agent get the call?", "Dialer decision log, by agent"),
]

STATES = [
    ("1", "Dialing", "Calls were placed in the last cycle"),
    ("2", "Idle: no agent available", "Work is due, nobody is ready"),
    ("3", "Holding back", "A rule keeps it from dialing, such as the abandonment policy"),
    ("4", "Nothing to call", "No record is due"),
]

STATES2 = [
    ("1", "Starting", "Work arrived, and the first cycle has not finished"),
    ("2", "Stalled", "Work is due, and no cycle completed for about five minutes"),
    ("3", "Not pacing", "The pacing lock was not acquired, so cycles skip themselves"),
    ("4", "Ask your administrator", "Stalled and Not pacing are not settings you can change"),
]

SUPP = [
    ("1", "No number to dial", "The record has no number"),
    ("2", "Out of attempts", "The attempt limit is reached"),
    ("3", "Waiting out the retry delay", "Too soon after the last call"),
    ("4", "Do not call", "The contact opted out, or the number is on a national registry"),
]

SUPP2 = [
    ("1", "Outside the calling window", "The contact's local time is not open"),
    ("2", "Abandonment rate cap reached", "Automatic dialing is paused"),
    ("3", "Compliance check unavailable", "Temporary: it is tried again later"),
    ("4", "Number not in service", "Known to be dead, so never dialed"),
]

ROUTING = [
    ("1", "No agents available", "Nobody was ready to take the call"),
    ("2", "No available agent matched the routing policy", "Skills, capacity or entitlements ruled the agents out"),
    ("3", "Agent selected", "The highest ranked eligible agent was reserved"),
]

PRED = [
    ("1", "When the last cycle ran", "For each campaign the profile dials"),
    ("2", "What it decided", "Over-dialing, one call per reserved agent, or not dialing, and why"),
    ("3", "What it measured", "Calls placed, free agents, calls ringing, answer rate, abandonment"),
]

USES = [
    ("1", "A campaign went quiet", "Open Dialer activity and health: the state and the Why column"),
    ("2", "A contact says nobody called", "Find the activity in the decision log and read the reason"),
    ("3", "An agent got no calls", "Filter the decision log by agent and read Candidates"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "What the reports answer")
        bullets(image, WHY, active={0} if step_id == "why-1" else {1, 2})
        return image
    if step_id == "st-1":
        image = slide_base(chapter_title, label, "Dialer states")
        bullets(image, STATES)
        return image
    if step_id == "st-2":
        image = slide_base(chapter_title, label, "Dialer states, continued")
        bullets(image, STATES2)
        return image
    if step_id == "rea-1":
        image = slide_base(chapter_title, label, "Why a record was not dialed")
        bullets(image, SUPP)
        return image
    if step_id == "rea-2":
        image = slide_base(chapter_title, label, "Why a record was not dialed, continued")
        bullets(image, SUPP2)
        return image
    if step_id == "rea-3":
        image = slide_base(chapter_title, label, "Why no agent was assigned")
        bullets(image, ROUTING)
        return image
    if step_id.startswith("pred-"):
        image = slide_base(chapter_title, label, "Last pacing decision on a Predictive profile")
        bullets(image, PRED, active={0, 1} if step_id == "pred-1" else {2})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where it helps")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
