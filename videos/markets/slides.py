"""The slides of the Markets video."""
from brand import bullets, slide_base

WHY = [
    ("1", "A foreign price puts shoppers off", "They cannot tell what they would really pay"),
    ("2", "Pricing again for every country", "A job that never ends, with rates that move"),
    ("3", "A market converts for you", "A currency and an exchange rate for chosen countries"),
    ("check", "Shop, cart, checkout and order", "All in the market's currency, with tax included"),
]

HOW = [
    ("1", "Market", "A currency and an exchange rate for countries"),
    ("2", "Prices", "Entered prices are used; others converted and rounded up"),
    ("3", "Market picker", "Shoppers choose, and the choice is remembered"),
    ("check", "Not sold in, shipping, tax", "Per market rules around the price"),
]

LIMITS = [
    ("1", "Languages", "A market can set the storefront's language"),
    ("2", "Store currency for sorting", "Sorts, price filters and reports"),
    ("3", "Rates do not update", "Edit the market when a rate moves"),
    ("check", "The admin", "Always shows your store's own prices"),
]

USE = [
    ("1", "Selling in Europe", "Euros, several countries, VAT included"),
    ("2", "A country of its own", "Its own currency and rounding"),
    ("3", "Tidy prices", "Round up to .99 or a whole amount"),
    ("4", "Restricted goods", "Leave a product out of a market"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Sell abroad in the shopper's currency")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Markets, the picker and the rules")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("lim-"):
        image = slide_base(chapter_title, label, "Limits and languages")
        bullets(image, LIMITS, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to use markets")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
