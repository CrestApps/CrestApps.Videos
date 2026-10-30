"""The slides of the AI Usage Analytics video."""
from brand import bullets, slide_base

Q = [
    ('1', 'Models', 'Which models and how many requests'),
    ('2', 'Features and purposes', 'Chat, SMS, voice, API, workflows'),
    ('3', 'Billing units', 'Tokens, minutes, characters, images'),
    ('4', 'Phone calls', 'Talk time, silence and handoffs'),
]

VO = [
    ('1', 'Calls and AI minutes', 'How many calls, how long the assistant spoke'),
    ('2', 'Caller and silence minutes', 'How long the caller spoke, how long nobody did'),
    ('3', 'Average call and handoff rate', 'Length of a call, share handed to an agent'),
    ('4', 'Barge-ins per call', 'Callers talking over the assistant'),
]

TIP = [
    ('1', 'Compare with the invoice', 'Requests are counted, not only chats'),
    ('2', 'Pick the model for each feature', 'Cheaper models for searching and summaries'),
    ('3', 'Turn it on early', 'Nothing before that can be reported'),
]


def slide(step_id, chapter_title, label):
    kinds = {
        'q': ('What the report answers', Q, lambda n: set(range(n))),
        'vo': ('Voice figures', VO, lambda n: set(range(n))),
        'tip': ('From figures to decisions', TIP, lambda n: {n - 1}),
    }
    prefix, _, number = step_id.partition("-")

    if prefix in kinds:
        heading, items, active = kinds[prefix]
        image = slide_base(chapter_title, label, heading)
        bullets(image, items, active=active(int(number)))
        return image

    raise SystemExit(f"No slide for {step_id}")
