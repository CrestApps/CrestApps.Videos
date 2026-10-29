"""The slides of the Phone Number Verification video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Lists are full of dead numbers", "Every call to one is agent time that does not come back"),
    ("2", "The kind of line matters", "A text to a landline is never read"),
    ("3", "Manual checks do not scale", "And the services charge for every check"),
    ("check", "Check once, automatically", "The answer is shown next to the number"),
]

WHEN = [
    ("1", "A new or changed number", "The contact's preferred number: cell, home, office, work, other"),
    ("2", "A check that is due again", "After the revalidation interval, by a background task"),
    ("3", "A retry from the queue", "Never more often than this, because checks cost money"),
    ("check", "No provider yet", "Numbers wait, and are checked once a provider is available"),
]

USES = [
    ("1", "Before a calling campaign", "Agents work the verified numbers; invalid ones are set aside"),
    ("2", "Before a text campaign", "Only mobile numbers can read a text"),
    ("3", "Keeping data clean", "The revalidation interval checks old numbers again"),
    ("4", "Watching for outages", "A jump in Needs attention means the service or its key failed"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Stop spending agent time on dead numbers")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("when-"):
        image = slide_base(chapter_title, label, "Only when a check is needed")
        bullets(image, WHEN, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where verification pays off")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
