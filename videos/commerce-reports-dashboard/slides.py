"""The slides of the Commerce reports and dashboard video."""
from brand import bullets, slide_base

WHY = [
    ("1", "How much did we sell?", "By day, product, category and country"),
    ("2", "Who buys, and do they return?", "Customers, cohorts and checkouts"),
    ("3", "What needs doing?", "Orders to ship, stock, disputes"),
    ("check", "Answers without a spreadsheet", "And exports when you want one"),
]

MORE = [
    ("1", "Orders", "Sales, products, customers, tax, refunds, disputes"),
    ("2", "Stock and shipping", "Stock on hand, movements and shipping times"),
    ("3", "Downloads and reviews", "Links used, ratings and reviews to approve"),
    ("check", "Subscriptions", "New, expiring and revenue"),
]

USE = [
    ("1", "A weekly look", "Sales and orders to ship on the dashboard"),
    ("2", "The tax return", "The one-stop-shop report, as a file"),
    ("3", "Your own report", "Copy a ready-made one and change it"),
    ("4", "Your accountant", "Export to Excel or PDF"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "What the store can answer")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("more-"):
        image = slide_base(chapter_title, label, "Reports from each part of the store")
        bullets(image, MORE, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to use reports")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
