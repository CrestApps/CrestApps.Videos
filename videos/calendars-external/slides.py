"""The slides of the External Calendars video."""
from brand import bullets, slide_base

GROUPS = {
    'why': ("Other people's calendars, next to yours", 'all', [
        ('1', 'Published elsewhere', 'Partners, vendors and other teams keep their own calendars'),
        ('2', 'A secret iCalendar address', "Google Calendar and Outlook show one in a calendar's settings"),
        ('3', 'Copied in, kept up to date', 'Read only: the other calendar owns the events'),
        ('4', 'No sign-in, no two-way sync', 'Google and Microsoft 365 sign-in is not available'),
    ]),
    'know': ('Good to know', 'all', [
        ('1', 'Treat the address like a password', 'Anyone who has it can read that calendar'),
        ('2', 'Reachable from the internet', 'Private and local network addresses are refused'),
        ('3', 'One direction', 'Untick the box to stop following the calendar'),
    ]),
}


def slide(step_id, chapter_title, label):
    prefix, number = step_id.rsplit("-", 1)
    heading, mode, items = GROUPS[prefix]
    n = int(number)
    image = slide_base(chapter_title, label, heading)
    bullets(image, items, active=set(range(n)) if mode == "all" else {n - 1})
    return image
