"""The slides of the Omnichannel Contacts video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Every conversation needs a person", "Otherwise a caller is a stranger and everything is asked again"),
    ("2", "One record per person", "Name, numbers, email, time zone and contact choices"),
    ("3", "History on one screen", "Every activity, call and conversation links back"),
    ("check", "An ordinary content item", "Add your own fields to the contact type"),
]

USE = [
    ("1", "Recognize callers", "By any number on the contact"),
    ("2", "Respect their choices", "Do not call, do not text, do not email"),
    ("3", "Call at a sensible hour", "Using the contact's time zone"),
    ("4", "Load and export lists", "With each contact's latest activity"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "One record for every conversation")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four typical uses")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
