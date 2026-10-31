"""The slides of the Calendar Invitations video."""
from brand import bullets, slide_base

GROUPS = {
    'what': ('Three emails, from one feature', 'all', [
        ('1', 'Invitation', 'With a calendar file for Outlook, Google Calendar or a phone'),
        ('2', 'Update', 'When the title, time, place or repeat pattern changes'),
        ('3', 'Cancellation', 'When the event is cancelled or deleted'),
        ('4', 'Answer links', 'Accept, Maybe and Decline, and guests need no sign-in'),
    ]),
    'know': ('When no email is sent', 'all', [
        ('1', 'Recipes, deployments and imports', 'Never send invitations'),
        ('2', 'Email not set up, or a failure', 'The event is still saved; the log records the failure'),
        ('3', 'No Base url on the site', 'The email has no answer links; the calendar file still works'),
    ]),
}


def slide(step_id, chapter_title, label):
    prefix, number = step_id.rsplit("-", 1)
    heading, mode, items = GROUPS[prefix]
    n = int(number)
    image = slide_base(chapter_title, label, heading)
    bullets(image, items, active=set(range(n)) if mode == "all" else {n - 1})
    return image
