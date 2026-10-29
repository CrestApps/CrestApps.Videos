"""The slides of the Inventory video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Not there when paid for", "Overselling means refunds and lost trust"),
    ("2", "A spreadsheet is never right", "And gives no warning before a bestseller runs out"),
    ("3", "Counted and held", "Every product and variant, held while a buyer pays"),
    ("check", "Low stock, suppliers, shoppers", "Alerts, purchase orders and what is left"),
]

HOW = [
    ("1", "The Inventory part", "Switches counting on for a product type"),
    ("2", "Changes with history", "Received, adjusted or counted"),
    ("3", "Locations and purchase orders", "More than one place, and suppliers"),
    ("check", "Settings", "Threshold, hold time and who is told"),
]

USE = [
    ("1", "A small shop", "Count, get told when low, reorder"),
    ("2", "A shop and a warehouse", "Two locations, and moves between them"),
    ("3", "A stocktake", "Export, correct OnHand, import"),
    ("4", "A launch", "Ships from and backorders for pre-orders"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Sell what you have")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Counts, locations, purchase orders and alerts")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to use inventory")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
