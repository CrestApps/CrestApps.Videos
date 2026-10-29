"""The slides of the Coupons and discounts video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Discounts by hand do not scale", "Editing prices, and remembering to put them back"),
    ("2", "A leaked code keeps discounting", "Unless it has a limit or an end date"),
    ("3", "Codes, automatic discounts, buy X get Y", "With dates, limits and rules"),
    ("check", "You decide what combines", "And every use is counted and recorded"),
]

HOW = [
    ("1", "A discount, and how it is applied", "Percentage, amount, free shipping or buy X get Y"),
    ("2", "When it works", "Dates, limits, minimums, products and categories"),
    ("3", "What it combines with", "The buyer gets the combination that takes off the most"),
    ("check", "Share and generate", "Discount links and unique single-use codes"),
]

USE = [
    ("1", "A welcome offer", "A percentage off the first order"),
    ("2", "A sale that runs itself", "Automatic, or buy two get one free"),
    ("3", "Cards and emails", "Unique single-use codes"),
    ("4", "A free shipping weekend", "Combines with product discounts"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Discounts you control")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Codes, automatic discounts and limits")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to use coupons")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
