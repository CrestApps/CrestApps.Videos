"""The slides of the Product import and feeds video."""
from brand import bullets, slide_base

WHY = [
    ("1", "A spreadsheet is the quickest way", "To fill or change a catalog"),
    ("2", "Moving from another store", "Means moving a catalog, not retyping it"),
    ("3", "A column for each product detail", "Rows matched to products by SKU"),
    ("check", "A feed for shopping ads", "Google, Meta and TikTok"),
]

HOW = [
    ("1", "Content Transfer", "Bulk Import and Bulk Export"),
    ("2", "A row for each product", "Price, SKU, sale dates as columns"),
    ("3", "Export, edit, import back", "The same columns"),
    ("check", "Product Feeds", "Brand, GTIN, category for shopping ads"),
]

MOVE = [
    ("1", "Export from Shopify or WooCommerce", "Import it as your product type"),
    ("2", "Variants", "Need the Product variants part and the same option names"),
    ("3", "Categories and brands", "Matched by name to taxonomy terms"),
    ("check", "Images are not downloaded", "Add photos in the media library"),
]

USE = [
    ("1", "A new catalog", "Fill in the template, and import"),
    ("2", "A price change", "Export, edit, import back"),
    ("3", "A move", "Import another store's export"),
    ("4", "Shopping ads", "Publish a feed"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Move products in, and out to shopping ads")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Import, export and feeds")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("mov-"):
        image = slide_base(chapter_title, label, "Shopify and WooCommerce")
        bullets(image, MOVE, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to use import and feeds")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
