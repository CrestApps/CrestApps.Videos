"""The slides of the Scheduling: people, rooms and appointment types video."""
from brand import bullets, slide_base

WHY = [
    ("1", "A time depends on many things", "Who is working, which room is free, and what is already booked"),
    ("2", "Resources", "The people, rooms and equipment that take appointments"),
    ("3", "Appointment types", "Each kind of appointment, how long it lasts, and who can take it"),
    ("check", "Only real free times", "The same person or room is never booked twice"),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Customers want a time")
        bullets(image, WHY, active={0})
        return image
    if step_id == "why-2":
        image = slide_base(chapter_title, label, "Two things you set up once")
        bullets(image, WHY, active={1, 2})
        return image
    if step_id == "why-3":
        image = slide_base(chapter_title, label, "Only real free times are offered")
        bullets(image, WHY, active={3})
        return image
    raise SystemExit(f"No slide for {step_id}")
