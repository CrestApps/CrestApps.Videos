"""The slides of the Set up your store video."""
from brand import bullets, slide_base

WHY = [
    ("1", "A store needs a few things first", "Details, payments, shipping and a product"),
    ("2", "Miss one and a customer notices", "A checkout that cannot take payment"),
    ("3", "A guide in order", "It shows what is done, and does the simple ones"),
    ("check", "Staff roles and a theme too", "The Storefront theme gives the look"),
]

START = [
    ("1", "Choose the Online store recipe", "When you set up a new site"),
    ("2", "Commerce is turned on", "Products, cart, checkout, orders, shipping, tax, payments"),
    ("3", "A catalog as the home page", "A Product type, a search and filters"),
    ("check", "The Storefront theme", "And steps for the features you turned on"),
]

USE = [
    ("1", "A first store", "Follow the steps, sell the same day"),
    ("2", "Only services", "Skip shipping"),
    ("3", "A growing team", "Add the staff roles"),
    ("4", "A trial run", "A test order before you open"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "From a new site to a first sale")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("sta-"):
        image = slide_base(chapter_title, label, "The Online store recipe")
        bullets(image, START, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to use the guide")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
