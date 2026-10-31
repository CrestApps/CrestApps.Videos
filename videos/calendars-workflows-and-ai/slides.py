"""The slides of the Calendars in Workflows and AI video."""
from brand import bullets, slide_base

GROUPS = {
    'why': ('Routine calendar work, done for you', 'all', [
        ('1', 'Routine moments', 'Time off is booked; a meeting is about to start'),
        ('2', 'Workflows act', 'Calendar events, tasks and a free time search'),
        ('3', 'AI assistants answer', 'What is on my calendar? When can we meet? Move it to Friday'),
        ('4', "With a person's access", 'Never beyond what that person may do'),
    ]),
    'chat': ('An example conversation with an assistant', 'one', [
        ('1', '"What is on my calendar this week?"', 'Find calendar events (up to 62 days)'),
        ('2', '"Find 30 minutes with Sam on Thursday"', 'Find free time (people and rooms, up to 31 days)'),
        ('3', '"Book it"', 'Create calendar event'),
        ('4', '"Move it to Friday" / "Cancel it"', 'Move calendar event, Cancel calendar event'),
    ]),
    'know': ('Good to know', 'all', [
        ('1', 'Workflows ignore their own changes', 'A workflow cannot start itself again'),
        ('2', 'Starting Soon and Completed', 'Need Calendar Reminders; once for every occurrence'),
        ('3', 'AI tools need the AI feature', 'Customer appointment tools belong to Scheduling'),
    ]),
}


def slide(step_id, chapter_title, label):
    prefix, number = step_id.rsplit("-", 1)
    heading, mode, items = GROUPS[prefix]
    n = int(number)
    image = slide_base(chapter_title, label, heading)
    bullets(image, items, active=set(range(n)) if mode == "all" else {n - 1})
    return image
