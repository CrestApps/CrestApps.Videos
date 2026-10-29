"""The slides of the Export and Import Your AI Setup video."""
from brand import bullets, slide_base

WH = [
    ('1', 'Connections and deployments', 'Providers and models'),
    ('2', 'Profiles and templates', 'The assistants'),
    ('3', 'Tool instances and data sources', 'Tools and knowledge settings'),
    ('4', 'MCP', 'Connections, prompts and resources'),
]

TIP = [
    ('1', 'Enable the same features', 'On both sites'),
    ('2', 'Test on a copy', 'Keep the file private'),
    ('3', 'Sync knowledge again', 'Uploaded and indexed content does not travel'),
]


def slide(step_id, chapter_title, label):
    kinds = {
        'wh': ('Eight kinds of AI items', WH, lambda n: set(range(n))),
        'tip': ('A safe move', TIP, lambda n: {n - 1}),
    }
    prefix, _, number = step_id.partition("-")

    if prefix in kinds:
        heading, items, active = kinds[prefix]
        image = slide_base(chapter_title, label, heading)
        bullets(image, items, active=active(int(number)))
        return image

    raise SystemExit(f"No slide for {step_id}")
