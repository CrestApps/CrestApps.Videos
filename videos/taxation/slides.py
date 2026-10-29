"""The slides of the Taxation video."""
from brand import bullets, slide_base

WHY = [
    ("1", "The right tax depends", "On what you sell and where the buyer is"),
    ("2", "By hand, it goes stale", "Rates change, and every mistake is yours"),
    ("3", "Rules in one place", "Which tax, where, for which products, at what rate"),
    ("check", "Charged and recorded", "Shown at checkout and kept on the order"),
]

HOW = [
    ("1", "Categories and types", "Group products, and name the tax"),
    ("2", "Jurisdictions", "A country, state, county or city"),
    ("3", "Rules and tables", "Which tax applies where, at what rate"),
    ("check", "Origin and classification", "Where you sell from, and the Taxation part"),
]

USE = [
    ("1", "A US shop", "A state sales tax, by category"),
    ("2", "A shop in the EU", "VAT for every country, prices include VAT"),
    ("3", "Selling abroad", "Import duty on large orders"),
    ("4", "Business customers", "Rules for Business customers, reverse charge"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "The right tax on every order")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Categories, jurisdictions, rules and tables")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to use taxation")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
