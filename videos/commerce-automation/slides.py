"""The slides of the Commerce Automation video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Things happen all day", "Paid, canceled, low stock, shipped"),
    ("2", "Someone has to know", "A buyer, the team, the warehouse"),
    ("3", "Tell the store once", "When this happens, do that"),
    ("check", "Events and recipes", "Commerce adds its moments to Workflows"),
]

HOW = [
    ("1", "An event starts it", "Order and stock events, shipment events"),
    ("2", "Tasks run next", "Email, a web address, content, and more"),
    ("3", "Details come along", "Order number, total, customer, reason"),
    ("4", "Added turned off", "Check it before it sends"),
]

CHOOSE = [
    ("1", "Workflows", "Run inside your site: email, content, a web address"),
    ("2", "Webhooks", "Tell another system, signed, with retries"),
]

USES = [
    ("1", "A thank-you email", "When an order is paid"),
    ("2", "A cancel alert", "When an order is canceled, with the reason"),
    ("3", "A low stock warning", "When a product runs low"),
    ("4", "A shipped message", "Carrier and tracking number"),
]


def slide(step_id, chapter_title, label):
    n = int(step_id.split("-")[-1])
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Why automate")
        bullets(image, WHY, active=set(range(n)))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Events, tasks and details")
        bullets(image, HOW, active={n - 1})
        return image
    if step_id.startswith("choose-"):
        image = slide_base(chapter_title, label, "Workflows or webhooks")
        bullets(image, CHOOSE, active={n - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where it fits")
        bullets(image, USES, active={n - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
