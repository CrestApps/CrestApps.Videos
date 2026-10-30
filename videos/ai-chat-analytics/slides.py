"""The slides of the AI Chat Analytics video."""
from brand import bullets, slide_base

Q = [
    ('1', 'Use', 'How much, when, who'),
    ('2', 'Resolution', 'Solved or not, judged by the AI'),
    ('3', 'Ratings', 'Thumbs up and thumbs down'),
    ('4', 'Collected data and goals', 'Emails, leads, conversions'),
]

TIP = [
    ('1', 'Read unresolved chats', 'They show the gaps in your knowledge'),
    ('2', 'Watch abandonment and thumbs down', 'Instructions or knowledge need work'),
    ('3', 'Tokens are a guide', 'Compare billing with the usage report'),
]


def slide(step_id, chapter_title, label):
    kinds = {
        'q': ('What the reports answer', Q, lambda n: set(range(n))),
        'tip': ('From figures to fixes', TIP, lambda n: {n - 1}),
    }
    prefix, _, number = step_id.partition("-")

    if prefix in kinds:
        heading, items, active = kinds[prefix]
        image = slide_base(chapter_title, label, heading)
        bullets(image, items, active=active(int(number)))
        return image

    raise SystemExit(f"No slide for {step_id}")
