"""The slides of the Purchase orders and stock alerts video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Running out costs sales", "Ordering too much ties up money"),
    ("2", "A list, not memory", "What to reorder, and what has arrived"),
    ("3", "Orders from suppliers", "Received into a location, then on hand"),
    ("check", "Alerts for shoppers and staff", "Back in stock, and low stock"),
]

HOW = [
    ("1", "A draft", "Filled with the items that are low"),
    ("2", "Mark as ordered", "Items lock; costs can still change"),
    ("3", "Receive some or all", "Units go on hand, with a history"),
    ("check", "Partly received, then Received", "Or canceled"),
]

USE = [
    ("1", "A weekly reorder", "Start from low stock"),
    ("2", "A delivery in parts", "Receive what arrived now"),
    ("3", "A sold out product", "Shoppers are emailed when it is back"),
    ("4", "A supplier that cannot deliver", "Cancel what is still to come"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Reorder before you run out")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Draft, ordered, received")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to use purchase orders")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
