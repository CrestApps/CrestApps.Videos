"""The slides of the Product listings and attributes video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Browsing needs filters, sorts and search", "More so as the catalog grows"),
    ("2", "What shoppers filter on depends on what you sell", "Size and color, bedrooms, mileage"),
    ("3", "Fields of your product types", "Offered by a listing page"),
    ("check", "Chips on cards, specification tables", "The same fields, shown where they help"),
]

HOW = [
    ("1", "Product listing settings on a field", "Filterable, Sortable, Searchable, shown where"),
    ("2", "A listing page", "Types, filters, sorts, layout"),
    ("3", "Visitors", "Filter, sort, search, switch, page"),
    ("check", "Taxonomies", "A filter for each level of the tree"),
]

MORE = [
    ("1", "Ranges", "Ready made, or typed by you"),
    ("2", "Swatches and taxonomies", "Colors, and Make then Model"),
    ("3", "Product pages and search", "Layout, SEO and search indexes"),
    ("check", "Samples", "Real estate and vehicles"),
]

USE = [
    ("1", "A shop", "Brand, size, color and a price range"),
    ("2", "Homes", "Ranges for bedrooms and square feet"),
    ("3", "Cars", "Make then model, year, mileage"),
    ("4", "A short list of plans", "No filters at all"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Let shoppers find the right product")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Fields, listings and visitors")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("mor-"):
        image = slide_base(chapter_title, label, "Ranges, swatches, taxonomies and samples")
        bullets(image, MORE, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to use listings")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
