"""The slides of the Omnichannel Cadences video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Customers go quiet", "One well-timed nudge can bring them back"),
    ("2", "Build it once", "A list of nudges and the silence before each"),
    ("check", "Replies and STOP end it", "A customer who answers is never nudged"),
]

USE = [
    ("1", "The clock restarts", "Delays add up after every message sent"),
    ("2", "Steps are the limit", "The most nudges a customer will get"),
    ("3", "Attach it to a load", "Under Re-engagement, with optional business hours"),
    ("4", "Copied when the load runs", "Later edits do not change live conversations"),
]

USES = [
    ("1", "Win back quiet leads", "A text that stopped getting replies"),
    ("2", "Space the nudges", "Hours or days apart"),
    ("3", "Let the AI write them", "From the conversation so far"),
    ("4", "Respect business hours", "Only while you are open"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Follow-ups for quiet customers")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("uses-"):
        image = slide_base(chapter_title, label, "Four typical uses")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "How the timing works")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
