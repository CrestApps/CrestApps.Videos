"""The slides of the Scheduling: how open times are worked out video."""
from brand import bullets, slide_base

RULES = [
    ("1", "Inside the working hours", "The whole appointment fits in the hours of the resource, or of every resource together"),
    ("2", "Clear of busy time", "Nothing busy on the resource's calendars, and free time around appointments kept clear"),
    ("3", "Within the booking limits", "Minimum notice, how far ahead, most per day, and places left in a class"),
    ("check", "Then it is offered", "Start times step by the value set on the appointment type"),
]

CHECKS = [
    ("1", "The appointment type has resources", "Add at least one, and check Pick: any, share out, or all together"),
    ("2", "The resources take appointments", "Takes appointments is ticked, and the working hours cover the day"),
    ("3", "The limits allow the day", "Minimum notice, Book up to, and Most per resource per day"),
    ("4", "Nothing else is busy", "Blocked time or events on the resource's calendar, or the staff member's own"),
]


def slide(step_id, chapter_title, label):
    if step_id == "rules-1":
        image = slide_base(chapter_title, label, "Working hours come first")
        bullets(image, RULES, active={0})
        return image
    if step_id == "rules-2":
        image = slide_base(chapter_title, label, "Busy time and free time")
        bullets(image, RULES, active={1})
        return image
    if step_id == "rules-3":
        image = slide_base(chapter_title, label, "Then the limits")
        bullets(image, RULES, active={2, 3})
        return image
    if step_id == "trouble-1":
        image = slide_base(chapter_title, label, "No times on a day? Check")
        bullets(image, CHECKS, active={0, 1})
        return image
    if step_id == "trouble-2":
        image = slide_base(chapter_title, label, "No times on a day? Check")
        bullets(image, CHECKS, active={2, 3})
        return image
    raise SystemExit(f"No slide for {step_id}")
