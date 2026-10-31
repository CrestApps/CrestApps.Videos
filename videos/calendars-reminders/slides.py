"""The slides of the Calendar Reminders video."""
from brand import bullets, slide_base

GROUPS = {
    'why': ('An event you cannot miss by looking away', 'all', [
        ('1', 'Easy to miss', 'You are busy in another part of the site'),
        ('2', 'A reminder on screen', 'On every admin page you have open, before the event starts'),
        ('3', 'It stays until dismissed', 'Looking away for a moment cannot make you miss it'),
        ('4', 'Owner and invited users', 'Each invited user who has not declined sees it'),
    ]),
    'flow': ('The same moments, for workflows', 'all', [
        ('1', 'Checked every minute', 'When an event is about to start, and when it has ended'),
        ('2', 'Two workflow events', 'Starting Soon (minutes before the start) and Completed'),
        ('3', 'Once per occurrence', 'For example, email attendees 15 minutes before a meeting'),
    ]),
}


def slide(step_id, chapter_title, label):
    prefix, number = step_id.rsplit("-", 1)
    heading, mode, items = GROUPS[prefix]
    n = int(number)
    image = slide_base(chapter_title, label, heading)
    bullets(image, items, active=set(range(n)) if mode == "all" else {n - 1})
    return image
