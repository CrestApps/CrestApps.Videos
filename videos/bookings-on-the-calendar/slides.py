"""The slides of the Bookings on the calendar video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Already there", "Turn on Calendars, and every booking is also a calendar event"),
    ("2", "One calendar called Bookings", "Named after the product and the buyer, as long as the booking"),
    ("3", "Tentative, confirmed or cancelled", "Held and requested bookings are tentative; booked ones are confirmed"),
    ("check", "Changed only in Bookings", "The calendar and the orders always agree"),
]

USE = [
    ("1", "One place", "Bookings next to meetings, time off and the rest of the schedule"),
    ("2", "What is real", "Tentative while held or requested, confirmed once booked, cancelled when canceled"),
    ("3", "Share it", "Calendars, Manage Calendars: give your staff the Bookings calendar"),
    ("4", "Block shared resources", "Block with calendar keeps one guide, room or vehicle from being sold twice"),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Bookings and the shared calendar")
        bullets(image, WHY, active={0})
        return image
    if step_id == "why-2":
        image = slide_base(chapter_title, label, "One calendar for every booking")
        bullets(image, WHY, active={1, 2, 3})
        return image
    if step_id in ("use-1", "use-2", "use-3", "use-4"):
        n = int(step_id[-1]) - 1
        image = slide_base(chapter_title, label, "Bookings on the calendar, every day")
        bullets(image, USE, active={n})
        return image
    raise SystemExit(f"No slide for {step_id}")
