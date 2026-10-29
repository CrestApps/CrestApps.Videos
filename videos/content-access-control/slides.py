"""The slides of the Content Access Control video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Not every page is for everyone", "A supervisor handbook, an internal price list, notes for one team"),
    ("2", "Permissions cover a whole type", "Two pages of one type are open to the same people"),
    ("3", "Pick the roles on each item", "The editor decides who may view that page"),
    ("check", "Everyone else is refused", "The rest of the site stays open as before"),
]

HOW = [
    ("1", "The Role Picker part", "Lets an editor pick one or more roles on an item"),
    ("2", "Restrict content", "One setting on the type turns the picker into an access rule"),
    ("3", "Any one picked role is enough", "No roles picked means the item stays open to everyone"),
    ("check", "Viewing only", "Editing and publishing follow the usual permissions"),
]

USE = [
    ("1", "Handbooks and guides", "Only supervisors read the escalation guide"),
    ("2", "Internal price lists", "Discount rules for the sales team"),
    ("3", "Partner or department documents", "Each limited to its own role"),
    ("4", "Team announcements", "Procedures for a single team"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "One page for everyone, another for one team")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "The Role Picker part and one setting")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four pages that are not for everyone")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
