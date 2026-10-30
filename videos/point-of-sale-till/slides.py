"""The slides of the Point of Sale: A Day at the Till video."""
from brand import bullets, slide_base

APP = [
    ("1", "A till app, over the commerce API", "The same products, prices, stock"),
    ("2", "No till app ships here", "Build one, or adapt one, on the API"),
    ("3", "Everything says who did it", "Staff sign in as themselves"),
]

DAY = [
    ("1", "Open a shift", "Count the opening float"),
    ("2", "Ring up a sale", "Products, codes, gift cards, a customer"),
    ("3", "Take the payment", "Cash, card, a reader, gift card, or a mix"),
    ("4", "Receipt, and cash in and out", "Print or email; banking, change"),
    ("5", "Returns, then close", "Refund at the till; count the drawer"),
]

USES = [
    ("1", "A shop with a counter", "Online and in-store, one stock count"),
    ("2", "A market stall", "A tablet, cash and a card reader"),
    ("3", "Several stores", "A register for each"),
]


def slide(step_id, chapter_title, label):
    n = int(step_id.split("-")[-1])
    kind = step_id.split("-")[0]
    pages = {"app": (APP, "The till app"), "day": (DAY, "A day at the till"), "use": (USES, "Where it fits")}

    if kind not in pages:
        raise SystemExit(f"No slide for {step_id}")

    items, title = pages[kind]
    image = slide_base(chapter_title, label, title)
    bullets(image, items, active={n - 1})
    return image
