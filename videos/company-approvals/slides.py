"""The slides of the Company Approvals and Pay on Account video."""
from brand import bullets, slide_base

WHY = [
    ("1", "The company's money", "So the company wants a say in what is bought"),
    ("2", "A purchase order number", "So accounting can match the invoice"),
    ("3", "Pay later", "A small deposit now, the rest on an invoice"),
    ("check", "Companies do all of it", "Buyers shop and check out as usual"),
]

HOW = [
    ("1", "Buyers and admins", "Each signs in as themselves"),
    ("2", "Terms", "Net days, deposit, credit limit, purchase order"),
    ("3", "Limits", "A buyer's order limit, the company's monthly budget"),
    ("4", "Approve, then pay", "The deposit now, the rest on account"),
]

PUNCH = [
    ("1", "Open your store from their system", "The buyer fills a cart at the company's prices"),
    ("2", "Transfer the cart back", "Items, quantities, prices and SKUs"),
]

USES = [
    ("1", "A trade supplier", "A purchase order, a deposit, an invoice due in 30 days"),
    ("2", "A multi-site business", "Buyers order up to a limit, a manager approves the rest"),
    ("3", "A monthly budget", "Orders past it wait for an admin"),
    ("4", "A large order", "A quote with their purchase order"),
]


def slide(step_id, chapter_title, label):
    n = int(step_id.split("-")[-1])
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Why companies buy differently")
        bullets(image, WHY, active=set(range(n)))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Buyers, terms and limits")
        bullets(image, HOW, active={n - 1})
        return image
    if step_id.startswith("punch-"):
        image = slide_base(chapter_title, label, "Procurement punchout")
        bullets(image, PUNCH, active={n - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where it fits")
        bullets(image, USES, active={n - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
