"""The slides of the Payments video."""
from brand import bullets, slide_base

WHY = [
    ("1", "More than a card form", "Taken, recorded and matched to an order"),
    ("2", "Cards and offline methods", "Stripe, bank transfer, cash on delivery, pay later"),
    ("3", "Money back", "Refund in full or in part from the same record"),
    ("check", "Other systems hear at once", "Webhooks tell them when money moves"),
]

HOW = [
    ("1", "Stripe", "Test keys and real keys for cards"),
    ("2", "Offline payments", "Pay Later, Bank transfer, Cash on delivery"),
    ("3", "Transactions, Payments, Refunds", "What is owed, what was taken, what went back"),
    ("check", "Reminders and webhooks", "Chase late payers, and notify other systems"),
]

USE = [
    ("1", "Try before you sell", "A whole trial sale on Stripe's test keys"),
    ("2", "Invoice customers", "Payment due until you record the transfer"),
    ("3", "Chase late payers", "Scheduled reminders do the writing"),
    ("4", "Tell other systems", "Webhooks for orders and shipments"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Taking, recording and returning money")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Providers, balances, payments and webhooks")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to use payments")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
