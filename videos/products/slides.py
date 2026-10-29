"""The slides of the Products video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Prices drift", "Spreadsheets and scattered pages go out of date"),
    ("2", "Not everything is a cart item", "Goods, services, digital items, and things you only list"),
    ("3", "Price with the content", "A product is a page of your site with a price and rules"),
    ("check", "One price everywhere", "Shop, cart, checkout and orders read the same price"),
]

HOW = [
    ("1", "Products", "A content type with the Product part: price, SKU, sales"),
    ("2", "Product Listings", "The pages that show products, with filters and sorting"),
    ("3", "Currencies", "The currencies you sell in, plus the optional features"),
    ("check", "Set on the type", "How products sell is set once, and a product can override it"),
]

USE = [
    ("1", "A shop", "Goods, pictures, sale prices, filters and a cart"),
    ("2", "Listings without a cart", "Homes or cars with Call for price and an inquiry button"),
    ("3", "Wholesale", "Minimum quantities, packs, volume and role discounts"),
    ("4", "Gifts and kits", "Add-ons, bundles, upsells and cross-sells"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "One place for what you sell")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Products, listings and currencies")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to use products")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
