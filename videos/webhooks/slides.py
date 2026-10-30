"""The slides of the Webhooks video."""
from brand import bullets, slide_base

WHY = [
    ("1", "A paid order, at once", "Not by asking every few minutes"),
    ("2", "Many systems need to know", "Accounting, stock, mailing lists"),
    ("check", "A short message, the moment it happens", "To an address you choose"),
]

HOW = [
    ("1", "A webhook", "A name, an address, and the events"),
    ("2", "A signature", "So the receiver knows it is you"),
    ("3", "Retries", "Up to eight tries over about two days"),
    ("4", "A log", "Every message, tries and errors"),
]

EVENTS = [
    ("1", "Orders", "Placed, paid, canceled, refunded"),
    ("2", "Shipments", "Shipped, delivered"),
    ("3", "Stock and customers", "Changed, low, sold out, customer created"),
]

SIGN = [
    ("1", "Headers", "Event id, name, time and signature"),
    ("2", "A keyed hash", "Of the timestamp and the message"),
    ("3", "Keep the secret secret", "In the receiver's settings only"),
]

RETRIES = [
    ("1", "Answer in 15 seconds", "With a success, such as 200"),
    ("2", "Tried again", "1 min, 5 min, 30 min, 2 h, 6 h, 12 h, 24 h"),
    ("3", "A good receiver", "Answers fast, drops repeats, ignores order"),
]

USES = [
    ("1", "A warehouse", "Paid orders and shipments"),
    ("2", "Accounting", "Orders and refunds"),
    ("3", "A marketplace", "Stock counts in step"),
    ("4", "A mailing list", "Each new customer"),
]


def slide(step_id, chapter_title, label):
    n = int(step_id.split("-")[-1])
    kind = step_id.split("-")[0]
    pages = {"why": (WHY, "Why webhooks"), "how": (HOW, "Events, a secret and a log"), "ev": (EVENTS, "The events"),
             "sig": (SIGN, "Signed messages"), "ret": (RETRIES, "Deliveries and retries"), "use": (USES, "Where it fits")}

    if kind not in pages:
        raise SystemExit(f"No slide for {step_id}")

    items, title = pages[kind]
    image = slide_base(chapter_title, label, title)
    bullets(image, items, active=set(range(n)) if kind == "why" else {n - 1})
    return image
