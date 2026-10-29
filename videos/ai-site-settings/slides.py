"""The slides of the AI Site Settings video."""
from brand import bullets, slide_base

PG = [
    ('1', 'A stack of cards', 'Each card belongs to a feature'),
    ('2', 'Cards follow features', 'Enable the feature to see its card'),
    ('check', 'Saving may restart the site', 'As the banner says'),
]

TIP = [
    ('1', 'Defaults move profiles', 'Profiles that follow them change too'),
    ('2', 'Match the embedding', 'The same model as your indexes'),
    ('3', 'Read the hints', 'Every field explains itself'),
]


def slide(step_id, chapter_title, label):
    kinds = {
        'pg': ('One page, many cards', PG, lambda n: set(range(n))),
        'tip': ('Tips', TIP, lambda n: {n - 1}),
    }
    prefix, _, number = step_id.partition("-")

    if prefix in kinds:
        heading, items, active = kinds[prefix]
        image = slide_base(chapter_title, label, heading)
        bullets(image, items, active=active(int(number)))
        return image

    raise SystemExit(f"No slide for {step_id}")
