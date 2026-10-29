"""The slides of the AI Data Extraction and Post-Session Processing video."""
from brand import bullets, slide_base

MOM = [
    ('1', 'While chatting', 'Data extraction picks out details'),
    ('2', 'After the chat', 'Post-session tasks analyze it'),
    ('3', 'Over time', 'Metrics: resolved, liked, converted'),
]

RES = [
    ('1', 'AI reports', 'One row for each chat'),
    ('2', 'Workflows', 'Send details onward, or notify a person'),
    ('check', 'Chat profiles only', 'Analytics feature on, profile saved once'),
]


def slide(step_id, chapter_title, label):
    kinds = {
        'mom': ('Three moments', MOM, lambda n: set(range(n))),
        'res': ('Where the results go', RES, lambda n: set(range(n))),
    }
    prefix, _, number = step_id.partition("-")

    if prefix in kinds:
        heading, items, active = kinds[prefix]
        image = slide_base(chapter_title, label, heading)
        bullets(image, items, active=active(int(number)))
        return image

    raise SystemExit(f"No slide for {step_id}")
