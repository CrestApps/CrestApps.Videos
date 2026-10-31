"""The slides of the Scheduling: book and manage appointments video."""
from brand import bullets, slide_base

STATUSES = [
    ("1", "Held and Requested", "A time kept for a customer still booking, or a request waiting for someone to confirm it"),
    ("2", "Confirmed", "The appointment is booked, and its time is taken on the calendar"),
    ("3", "Completed and No-show", "It took place, or the customer didn't come"),
    ("4", "Cancelled and Expired", "It was cancelled, or the hold ran out before the booking was finished"),
]

USE = [
    ("1", "Move an appointment", "Search by customer, open it, and pick another open time"),
    ("2", "Confirm requests", "Filter by Requested, then confirm or cancel each one"),
    ("3", "Close the visit", "Took place, or No-show when the customer didn't come"),
]


def slide(step_id, chapter_title, label):
    if step_id == "life-1":
        image = slide_base(chapter_title, label, "From a request to a booking")
        bullets(image, STATUSES, active={0, 1})
        return image
    if step_id == "life-2":
        image = slide_base(chapter_title, label, "After the time, or instead of it")
        bullets(image, STATUSES, active={2, 3})
        return image
    if step_id == "use-1":
        image = slide_base(chapter_title, label, "A customer calls to change a time")
        bullets(image, USE, active={0})
        return image
    if step_id == "use-2":
        image = slide_base(chapter_title, label, "Requests wait for you")
        bullets(image, USE, active={1})
        return image
    if step_id == "use-3":
        image = slide_base(chapter_title, label, "Close each visit")
        bullets(image, USE, active={2})
        return image
    raise SystemExit(f"No slide for {step_id}")
