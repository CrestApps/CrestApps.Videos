"""The slides of the Calendars: Features and Deployment video."""
from brand import bullets, slide_base

GROUPS = {
    'which': ('Which feature for which job', 'all', [
        ('1', 'Shared and team calendars', 'Calendars, and Team Calendars for teams'),
        ('2', 'Rooms, equipment and offices', 'Resource Calendars, Office Calendars'),
        ('3', 'Email, reminders, feeds, external', 'Calendar Invitations, Reminders, Feeds, External Calendars'),
    ]),
    'imp': ('What the import does', 'all', [
        ('1', 'Creates what is missing, updates the rest', "Matched by each record's identifier; safe to repeat"),
        ('2', "The kind's feature must be on", 'A room needs Resource Calendars on the other site'),
        ('3', 'No overlap check, no invitations', 'A recipe with the Calendars step behaves the same way'),
    ]),
    'fix': ('Troubleshooting', 'one', [
        ('1', 'I cannot see a calendar', 'It is private and not shared with you'),
        ('2', 'I see the time, not the event', 'Free and busy only, or a private event'),
        ('3', 'Refused: already booked or full', 'A room, equipment or office has no room then'),
        ('4', 'No invitation email', "Email, Calendar Invitations and the site's Base url"),
    ]),
}


def slide(step_id, chapter_title, label):
    prefix, number = step_id.rsplit("-", 1)
    heading, mode, items = GROUPS[prefix]
    n = int(number)
    image = slide_base(chapter_title, label, heading)
    bullets(image, items, active=set(range(n)) if mode == "all" else {n - 1})
    return image
