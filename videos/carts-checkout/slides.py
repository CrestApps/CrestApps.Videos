"""The slides of the Carts and Checkout video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Sales are lost on the way", "Between choosing a product and paying for it"),
    ("2", "A cart that remembers", "For guests and customers, with today's prices"),
    ("3", "One guided path", "Numbered steps, a running summary, your payment methods"),
    ("check", "A paid order", "The same checkout takes every payment"),
]

HOW = [
    ("1", "Carts", "Add to cart, the mini cart and the cart page"),
    ("2", "Checkout", "Contact, Shipping and Payment, only when needed"),
    ("3", "Part payment", "Codes, gift cards, store credit and points"),
    ("check", "Settings", "Layout, billing address and your own fields"),
]

USE = [
    ("1", "Guest checkout", "A name and an email, then pay"),
    ("2", "One page", "Every step on one page for phones"),
    ("3", "Part payment", "A gift card or credit pays what it covers"),
    ("4", "Digital goods", "No Shipping step, and fields of your own"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "From a product to a paid order")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "The cart, the steps and the settings")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to use the checkout")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
