"""The slides of the Calendars getting started video."""
from brand import bullets, slide_base

TYPES = [
    ("1", "General", "A shared calendar that belongs to nobody in particular, such as the company calendar"),
    ("2", "Personal", "Your own calendar: everyone gets one the first time they open the calendar"),
    ("3", "Team", "Belongs to a role: everyone in the team sees it and adds to it"),
    ("4", "Room, Equipment, Office", "Bookings of a room or an item, and who is in the office"),
]

EVENT_KINDS = [
    ("1", "General", "Event, Meeting, Holiday, Reminder"),
    ("2", "Personal", "Event, Meeting, Focus time, Out of office"),
    ("3", "Team", "Meeting, On call, Out of office, Milestone"),
    ("4", "Room and Equipment", "Booking or Reservation, Maintenance"),
]


def slide(step_id, chapter_title, label):
    if step_id == "kinds-1":
        image = slide_base(chapter_title, label, "Every calendar has a type")
        bullets(image, TYPES, active={0, 1})
        return image
    if step_id == "kinds-2":
        image = slide_base(chapter_title, label, "Every calendar has a type")
        bullets(image, TYPES, active={2, 3})
        return image
    if step_id == "kinds-3":
        image = slide_base(chapter_title, label, "Each type has its own kinds of events")
        bullets(image, EVENT_KINDS)
        return image
    raise SystemExit(f"No slide for {step_id}")
