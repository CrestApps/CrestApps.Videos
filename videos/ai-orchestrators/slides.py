"""The slides of the AI Orchestrators video."""
from brand import bullets, slide_base

ENG = [
    ('1', 'Default', 'Your connections, deployments, tools and knowledge'),
    ('2', 'Copilot', 'GitHub sign-in or your own provider key'),
    ('3', 'Claude', 'An Anthropic API key'),
]

TIP = [
    ('1', 'Start with Default', 'It works with everything'),
    ('2', 'Set up credentials first', 'Before choosing Copilot or Claude'),
    ('3', 'Keep secrets safe', 'Stored securely, never shown again'),
]


def slide(step_id, chapter_title, label):
    kinds = {
        'eng': ('What an orchestrator does', ENG, lambda n: set(range(n))),
        'tip': ('Choosing well', TIP, lambda n: {n - 1}),
    }
    prefix, _, number = step_id.partition("-")

    if prefix in kinds:
        heading, items, active = kinds[prefix]
        image = slide_base(chapter_title, label, heading)
        bullets(image, items, active=active(int(number)))
        return image

    raise SystemExit(f"No slide for {step_id}")
