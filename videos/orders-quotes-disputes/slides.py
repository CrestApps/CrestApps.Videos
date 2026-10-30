"""The slides of the Orders, quotes and disputes video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Big orders need a price", "Quotes for trade and bulk buyers"),
    ("2", "Paid orders sometimes grow", "Add items to a paid order"),
    ("3", "More from every checkout", "An order bump, and a recovery email"),
    ("check", "Know the risks", "Disputes and risky payments"),
]

RISK = [
    ("1", "Disputed", "A red box and a list mark: the buyer asked their bank"),
    ("2", "Why and what next", "The reason, the status and a note for each change"),
    ("3", "Risky", "Stripe's fraud check rated the payment elevated or highest"),
    ("check", "Look before you ship", "Compare the addresses, and refund if it is wrong"),
]

USE = [
    ("1", "A trade buyer", "A quote with your own prices"),
    ("2", "A phone order", "Add items to the paid order"),
    ("3", "A small extra", "An order bump on the payment step"),
    ("4", "A left checkout", "A recovery email after an hour"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Beyond the ordinary order")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("risk-"):
        image = slide_base(chapter_title, label, "Disputes and risk, from Stripe")
        bullets(image, RISK, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to use them")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
