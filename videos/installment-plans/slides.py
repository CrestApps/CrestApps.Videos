"""The slides of the Installment Plans video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Bigger sales, easier to close", "Split the price"),
    ("2", "First payment today", "The rest collected on time"),
    ("3", "A clear schedule", "What they pay, and when"),
    ("check", "A down payment, then a schedule", "From a saved card, or by invoice"),
]

HOW = [
    ("1", "Total and down payment", "The rest splits into equal payments"),
    ("2", "Due dates", "Weekly, every two weeks, monthly, quarterly"),
    ("3", "Collected by", "The saved card, or an invoice"),
    ("4", "Every payment is a transaction", "In Transactions and My Transactions"),
]

CARD = [
    ("1", "A provider that keeps a card", "Such as Stripe"),
    ("2", "Charge the down payment", "The plan becomes Active"),
    ("3", "Each due date", "Charge the card, or invoice; reminders and receipts"),
    ("4", "Charge now", "Early, or to retry a declined card"),
]

DECLINED = [
    ("1", "Declined", "Failed payment, Past due plan, customer told"),
    ("2", "Retries", "1, 3 and 5 days by default, then Due"),
    ("3", "Statuses", "Plan and payment statuses"),
]

CHECKOUT = [
    ("1", "Choose how to pay", "Pay in full, or 3 monthly payments"),
    ("2", "A plan with no down payment", "First payment now, card saved"),
    ("3", "Left without paying", "The plan closes; the balance is still owed"),
]

USES = [
    ("1", "A contractor", "A down payment, then monthly payments"),
    ("2", "A training provider", "A course fee in three payments"),
    ("3", "An equipment seller", "A card kept, charged on each due date"),
    ("4", "A store", "Customers split a large balance at checkout"),
]


def slide(step_id, chapter_title, label):
    n = int(step_id.split("-")[-1])
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Why installment plans")
        bullets(image, WHY, active=set(range(n)))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "How a plan works")
        bullets(image, HOW, active={n - 1})
        return image
    if step_id.startswith("card-"):
        image = slide_base(chapter_title, label, "Taking the card")
        bullets(image, CARD, active={n - 1})
        return image
    if step_id.startswith("dec-"):
        image = slide_base(chapter_title, label, "Declined cards")
        bullets(image, DECLINED, active={n - 1})
        return image
    if step_id.startswith("chk-"):
        image = slide_base(chapter_title, label, "At checkout")
        bullets(image, CHECKOUT, active={n - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where it fits")
        bullets(image, USES, active={n - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
