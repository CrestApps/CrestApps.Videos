"""The slides of the Live dashboard video."""
from brand import bullets, slide_base

WHY = [
    ("1", "See problems while they happen", "A backing-up queue, a long call, a service level about to break"),
    ("2", "The floor in real time", "Who is waiting, who is free, who is on a call, and what a supervisor can do about it"),
]

OWN = [
    ("1", "Your own queues", "The queues and campaigns on your own entitlement record; you act only on those"),
    ("2", "No record, empty dashboard", "Even for an administrator; give each supervisor a record"),
]

LISTEN = [
    ("1", "Your own soft phone", "Keep it open and signed in; it answers by itself when you start"),
    ("2", "Listen, Whisper, Barge", "Hear only; coach the agent; join the call"),
    ("3", "Stop", "Only your part of the call ends; a paused recording cannot be monitored"),
]

TAKEOVER = [
    ("1", "Take over", "The call is yours and the agent is released; you need your own agent profile"),
    ("2", "End, transfer, record", "End call, Transfer to a queue, an agent or a number, Start or Stop recording"),
    ("3", "State and messages", "Set Available, Set Not ready, Set Break, Sign out of queues, Message"),
]

WHO = [
    ("1", "Monitor", "Watch, listen, whisper, barge and message"),
    ("2", "Take over", "Take over, end, transfer, record, and set agents' state; every action is audited"),
]

USES = [
    ("1", "A wallboard", "Waiting counts and breaches on the floor"),
    ("2", "A backing-up queue", "Ask agents to return from a break"),
    ("3", "Coaching", "Whisper to a new agent on a hard call"),
    ("4", "A manager's call", "Take over when a customer asks for one"),
]


def slide(step_id, chapter_title, label):
    n = int(step_id[-1]) - 1
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "See problems while they happen")
        bullets(image, WHY, active={n})
        return image
    if step_id.startswith("own-"):
        image = slide_base(chapter_title, label, "Entitlements decide what you see")
        bullets(image, OWN, active={n})
        return image
    if step_id.startswith("lis-"):
        image = slide_base(chapter_title, label, "Step into a live call")
        bullets(image, LISTEN, active={n})
        return image
    if step_id.startswith("tko-"):
        image = slide_base(chapter_title, label, "Take over and intervene")
        bullets(image, TAKEOVER, active={n})
        return image
    if step_id.startswith("who-"):
        image = slide_base(chapter_title, label, "Two permissions")
        bullets(image, WHO, active={n})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where the dashboard helps")
        bullets(image, USES, active={n})
        return image
    raise SystemExit(f"No slide for {step_id}")
