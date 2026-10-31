"""The slides of the Office Calendars video."""
from brand import bullets, slide_base

GROUPS = {
    'why': ('Who is in, and which desks are free', 'all', [
        ('1', 'Some days in, some days away', 'Nobody knows who will be in, or if a desk is free'),
        ('2', 'One page answers both', 'Mark the days you are in, book a desk, register a visitor'),
        ('3', 'A desk count', 'When the office is full, the next booking is refused'),
        ('4', 'Four kinds of events', 'In office, Desk booking, Visitor, Office closed'),
    ]),
    'know': ('What the desk count counts', 'all', [
        ('1', 'Busy events take a desk', 'In office, Desk booking and Visitor. Free events and Office closed do not'),
        ('2', "The office's own time zone", 'New events use it, and everyone sees them in their own'),
        ('3', 'Visibility and sharing', 'They decide who sees the office. A disabled office takes no new events'),
    ]),
}


def slide(step_id, chapter_title, label):
    prefix, number = step_id.rsplit("-", 1)
    heading, mode, items = GROUPS[prefix]
    n = int(number)
    image = slide_base(chapter_title, label, heading)
    bullets(image, items, active=set(range(n)) if mode == "all" else {n - 1})
    return image
