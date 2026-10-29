"""The slides of the AI Agents video."""
from brand import bullets, slide_base

WH = [
    ('1', 'A tool', 'One action, such as a search or the date'),
    ('2', 'An agent', 'A specialist assistant with its own tools'),
    ('3', 'The description decides', 'The assistant calls the agent that fits'),
]

TIP = [
    ('1', 'One narrow job', 'Said in the first sentence'),
    ('2', 'Only what it needs', 'Few tools, little knowledge'),
    ('3', 'Write for the reader', 'The assistant that reads the description'),
]


def slide(step_id, chapter_title, label):
    kinds = {
        'wh': ('Two ways to take action', WH, lambda n: set(range(n))),
        'tip': ('Small and clear', TIP, lambda n: {n - 1}),
    }
    prefix, _, number = step_id.partition("-")

    if prefix in kinds:
        heading, items, active = kinds[prefix]
        image = slide_base(chapter_title, label, heading)
        bullets(image, items, active=active(int(number)))
        return image

    raise SystemExit(f"No slide for {step_id}")
