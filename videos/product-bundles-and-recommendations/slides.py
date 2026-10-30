"""The slides of the Product bundles and recommendations video."""
from brand import bullets, slide_base

WHY = [
    ("1", "A bigger basket", "Kits, sets and suggestions sell more"),
    ("2", "Shoppers choose faster", "Compare products side by side"),
    ("3", "Stock stays right", "A kit sells its parts"),
    ("check", "Each one is a switch", "Turn on only what you need"),
]

KINDS = [
    ("1", "Bundle", "Several products sold as one, at its own price"),
    ("2", "Grouped product", "Several products on one page, one Add to cart"),
    ("3", "Upsells and cross-sells", "Better choices and things that go with it"),
    ("check", "Compare", "Up to four products side by side"),
]

USE = [
    ("1", "A starter kit", "A bundle of three products at one price"),
    ("2", "A set", "A grouped product for a matching collection"),
    ("3", "A bigger basket", "Cross-sells in the cart"),
    ("4", "Hard choices", "Compare, with a row for each specification"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Sell more, and help shoppers choose")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("kind-"):
        image = slide_base(chapter_title, label, "Four features")
        bullets(image, KINDS, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to use them")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
