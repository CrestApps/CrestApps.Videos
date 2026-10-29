"""The slides of the Orders video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Sales must be remembered", "Who bought what, what they paid, what happens next"),
    ("2", "One record", "Items, totals, customer, payment and shipments"),
    ("3", "The place to fix things", "Refund, cancel, add a note, send the confirmation"),
    ("check", "Paperwork from the same record", "Confirmation, invoice, packing slip, receipts"),
]

HOW = [
    ("1", "Orders", "The list, and an order page with its actions"),
    ("2", "Create order", "For a buyer who did not use the checkout"),
    ("3", "Returns and gift cards", "Items sent back, and cards that pay for orders"),
    ("check", "Settings", "Orders, Emails, Receipts and Returns"),
]

USE = [
    ("1", "Support", "Find an order by email, add a note, refund part of it"),
    ("2", "Phone sales", "Create order, and the buyer pays through a link"),
    ("3", "Bank transfers", "Payment due until you record the payment"),
    ("4", "Gifts", "Issue a gift card by hand for an apology or a prize"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "One record for every sale")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Orders, returns, gift cards and settings")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to use orders")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
