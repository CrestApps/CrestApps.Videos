"""The slides of the AI Prompt Security video."""
from brand import bullets, slide_base

RISK = [
    ('1', 'Prompt injection', 'Text that overrides the instructions'),
    ('2', 'Prompt leakage', 'Answers that reveal instructions or tools'),
    ('3', 'Abuse', 'Too many messages or chats'),
    ('check', 'Shared site settings', 'A profile can add limits of its own'),
]

TIP = [
    ('1', 'Protect public assistants', 'Injection detection and the security preamble'),
    ('2', 'Tune the threshold', 'Raise it for false blocks, lower it for misses'),
    ('3', 'Embedded widgets', 'Need cross-site embedding'),
]


def slide(step_id, chapter_title, label):
    kinds = {
        'risk': ('Four risks', RISK, lambda n: set(range(n))),
        'tip': ('Using it well', TIP, lambda n: {n - 1}),
    }
    prefix, _, number = step_id.partition("-")

    if prefix in kinds:
        heading, items, active = kinds[prefix]
        image = slide_base(chapter_title, label, heading)
        bullets(image, items, active=active(int(number)))
        return image

    raise SystemExit(f"No slide for {step_id}")
