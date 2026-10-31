"""The slides of the Calendars schedules and team availability video."""
from brand import bullets, slide_base

COUNTED = [
    ("1", "Busy", "A confirmed event shown as Busy: the time is taken"),
    ("2", "Tentative", "A tentative event: the time may be taken, and shows in yellow"),
    ("3", "Free", "An event shown as Free, or a kind that leaves time free: a holiday, a reminder, a milestone"),
    ("4", "Cancelled", "A cancelled event is gone from the calendar and from the count"),
]


def slide(step_id, chapter_title, label):
    if step_id == "fb-1":
        image = slide_base(chapter_title, label, "Counted from all of a person's calendars")
        bullets(image, COUNTED, active={0, 1})
        return image
    if step_id == "fb-2":
        image = slide_base(chapter_title, label, "Counted from all of a person's calendars")
        bullets(image, COUNTED, active={2, 3})
        return image
    raise SystemExit(f"No slide for {step_id}")
