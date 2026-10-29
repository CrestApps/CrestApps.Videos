"""The slides of the Queues video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Many customers, one moment", "Someone must decide who is served first, and by which agent"),
    ("2", "A queue holds the work", "It offers each item to the right agent, and keeps callers informed"),
    ("check", "It plans for trouble", "Nobody answers, you are closed, or the line is too long"),
]

HOW = [
    ("1", "Queue group (optional)", "Organizes queues for administration and reports; never changes routing"),
    ("2", "Queue cards", "Routing, service levels, skills, hours and overflow, limits, waiting and callback"),
    ("3", "Entry points and agents", "Calls reach a queue through an entry point; agents sign in to take work"),
]

USES = [
    ("1", "A support line", "The longest idle agent first, and overflow to a general queue after two minutes"),
    ("2", "A Spanish queue", "Only agents with the Spanish skill are offered the work"),
    ("3", "A sales queue that closes", "Business hours, and voicemail at night"),
    ("4", "A busy line with callbacks", "Customers hang up and keep their place in line"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Work waits in an orderly line")
        bullets(image, WHY, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Groups, queues and their cards")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where queues help")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
