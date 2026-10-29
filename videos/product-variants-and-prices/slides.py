"""The slides of the Product variants and prices video."""
from brand import bullets, slide_base

WHY = [
    ("1", "One product that comes in versions", "Not six products"),
    ("2", "Six items are six things to keep in step", "Pages, prices, choices"),
    ("3", "A variant for each combination", "Its own SKU, price and picture"),
    ("check", "Product prices go further", "Once, monthly, yearly, or buyer's choice"),
]

HOW = [
    ("1", "Options on the type", "Size, Color: once"),
    ("2", "Values on each product", "A variant for every combination"),
    ("3", "Shoppers pick a variant", "The cart and order record it"),
    ("check", "Product prices", "A separate part for ways to buy"),
]

USE = [
    ("1", "Clothing", "Sizes and colors, a SKU each"),
    ("2", "A premium finish", "Its own price and picture"),
    ("3", "A club", "Once, monthly or yearly"),
    ("4", "A donation", "The buyer names the amount"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "One product, many ways to buy it")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Options, variants and prices")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to use variants and prices")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
