"""The slides of the Feeds and Subscriptions video."""
from brand import bullets, slide_base

GROUPS = {
    'why': ('Two ways to share with other calendar apps', 'all', [
        ('1', 'Many calendars', 'Outlook, Google Calendar and a phone all hold events'),
        ('2', 'A subscription address', 'Other apps follow your calendar and keep up to date'),
        ('3', 'Calendar files', 'Export, or import; events are matched, never added twice'),
    ]),
    'know': ('Good to know', 'all', [
        ('1', 'Calendar Feeds', 'Adds the address, export and import. Imports send no invitations'),
        ('2', 'Make a new address', 'The old address stops working at once'),
        ('3', 'One direction', 'No two-way sync with Google Calendar or Microsoft 365'),
    ]),
}


def slide(step_id, chapter_title, label):
    prefix, number = step_id.rsplit("-", 1)
    heading, mode, items = GROUPS[prefix]
    n = int(number)
    image = slide_base(chapter_title, label, heading)
    bullets(image, items, active=set(range(n)) if mode == "all" else {n - 1})
    return image
