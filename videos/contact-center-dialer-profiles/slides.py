"""The slides of the Dialer Profiles video."""
from brand import bullets, slide_base

WHY = [
    ("1", "How should each call start?", "An agent presses dial, or the system dials for them"),
    ("2", "Which rules must every call pass?", "The number customers see, who is off limits, the hours"),
    ("3", "Decide once", "The same answers for every list and every call"),
    ("check", "One profile, many campaigns", "You pick the profile when you load a list of calls"),
]

MODES = [
    ("1", "Preview", "The agent reviews the record, then clicks Dial or Skip"),
    ("2", "Power", "The system dials for available agents, one agent per call"),
    ("3", "Progressive", "Like Power, with a fixed limit of calls per pacing cycle"),
    ("4", "Predictive", "One agent per call, fewer calls as abandonment nears its limit"),
]

USES = [
    ("1", "Start with Preview", "Agents see every record first, so it is the easiest to train"),
    ("2", "Move to Power or Progressive", "When the team is comfortable and the list is steady"),
    ("3", "One profile for each campaign", "When its abandonment rate must be measured on its own"),
    ("4", "Turn Enabled off", "Pauses every load that uses the profile"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Decide how calls start, once")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("mode-"):
        image = slide_base(chapter_title, label, "Who presses dial?")
        bullets(image, MODES, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "How teams use profiles")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
