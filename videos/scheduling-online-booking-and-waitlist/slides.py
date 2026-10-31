"""The slides of the Scheduling: online booking and the waitlist video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Book at any hour", "No phone call, and only times that are really free are offered"),
    ("2", "One page per appointment type", "A day, a time, and the customer's details"),
    ("3", "A private link", "To move or cancel the appointment without signing in"),
    ("check", "Staff stay in control", "A type can wait for someone to confirm each request"),
]

MAIL = [
    ("1", "Appointment Confirmed", "A workflow event: send the confirmation with the manage link"),
    ("2", "Appointment Requested", "Tell the customer that the request was received"),
    ("3", "Waitlist Slot Available", "Email a waiting customer when a time frees up"),
    ("check", "Built in the Workflows editor", "Scheduling has its own events and tasks there"),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Customers book on their own")
        bullets(image, WHY, active={0})
        return image
    if step_id == "why-2":
        image = slide_base(chapter_title, label, "A page, a link, and control")
        bullets(image, WHY, active={1, 2, 3})
        return image
    if step_id == "mail-1":
        image = slide_base(chapter_title, label, "Email comes from a workflow")
        bullets(image, MAIL, active={0, 1})
        return image
    if step_id == "mail-2":
        image = slide_base(chapter_title, label, "Email comes from a workflow")
        bullets(image, MAIL, active={2, 3})
        return image
    raise SystemExit(f"No slide for {step_id}")
