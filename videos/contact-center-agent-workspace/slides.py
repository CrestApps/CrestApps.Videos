"""The slides of the Agent workspace video."""
from brand import bullets, slide_base

WHY = [
    ("1", "A shift has a rhythm", "Sign in, say you are ready, take work, finish the notes"),
    ("2", "Three places carry it", "The soft phone, My workspace, and the docked agent bar on every admin page"),
]

SHIFT = [
    ("1", "Sign in", "Open the soft phone and My workspace; choose today's queues and campaigns"),
    ("2", "Set your presence", "Available when ready, or a reason when you are not"),
    ("3", "Take the work", "Accept or decline each offer; dialer work opens its activity"),
    ("4", "Finish", "Notes, a disposition, complete the activity; check Recent activity"),
]

USES = [
    ("1", "Two queues at once", "Watch both waiting counts in Signed in to"),
    ("2", "Working the customer records", "Answer from the bar on any page"),
    ("3", "Stepping away for lunch", "A reason code, then back to Available"),
    ("4", "A new agent", "The same steps every day"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "One place for a whole shift")
        bullets(image, WHY, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("gl-"):
        image = slide_base(chapter_title, label, "Your shift at a glance")
        bullets(image, SHIFT, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where the workspace helps")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
