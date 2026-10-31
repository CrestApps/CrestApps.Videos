"""The slides of the Calendars categories and colors video."""
from brand import bullets, slide_base

ORDER = [
    ("1", "The event's own color", "Tick Use its own color in the event editor and pick one"),
    ("2", "Its category's color", "The category you pick for the event"),
    ("3", "Its kind of event's color", "A meeting, a holiday, a focus time or an on call has its own"),
    ("4", "Its calendar's color", "The color chosen when the calendar was created"),
]


def slide(step_id, chapter_title, label):
    if step_id == "order-1":
        image = slide_base(chapter_title, label, "An event is drawn in the first color that applies")
        bullets(image, ORDER, active={0, 1})
        return image
    if step_id == "order-2":
        image = slide_base(chapter_title, label, "An event is drawn in the first color that applies")
        bullets(image, ORDER, active={2, 3})
        return image
    raise SystemExit(f"No slide for {step_id}")
