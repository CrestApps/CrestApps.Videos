"""The slides of the Buyer Account and Guest Orders video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Come back to an order", "Where is it, print an invoice, buy it again"),
    ("2", "Many never sign up", "Guest checkout keeps the sale easy"),
    ("3", "A way back for guests", "Without anyone else reading the order"),
    ("check", "Account, or secure link", "Track and find an order"),
]

HOW = [
    ("1", "My account", "A page for each feature that is on"),
    ("2", "The order link", "On the confirmation page and email, 60 days"),
    ("3", "Track and find", "Number and email together; links by email"),
    ("4", "Make it an account", "In one step, with the same email"),
]

USES = [
    ("1", "One-time buyers", "Guest checkout and a link back to the order"),
    ("2", "Regulars", "Orders, invoices, a wishlist, addresses"),
    ("3", "A support desk", "The order number and email, or Track your order"),
    ("4", "Guests into accounts", "A password on the confirmation page"),
]


def slide(step_id, chapter_title, label):
    n = int(step_id.split("-")[-1])
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Why buyers need a way back")
        bullets(image, WHY, active=set(range(n)))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Accounts and order links")
        bullets(image, HOW, active={n - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where it fits")
        bullets(image, USES, active={n - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
