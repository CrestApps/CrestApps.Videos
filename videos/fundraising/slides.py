"""The slides of the Fundraising video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Donors choose their amount", "And may give in honor, or in memory, of someone"),
    ("2", "A goal, and progress", "Donors like to see that their gift matters"),
    ("3", "A receipt for their taxes", "With the organization's name and tax number"),
    ("check", "One module, the same checkout", "Campaigns on top of your cart and orders"),
]

HOW = [
    ("1", "Settings", "Who donations go to, and what receipts say"),
    ("2", "A campaign", "A product with a goal, suggested amounts, an end date"),
    ("3", "Donors give", "From the campaign's page, through the checkout"),
    ("4", "Receipts", "Emailed, and kept under Donations in the donor's account"),
]

USES = [
    ("1", "A library or school fundraiser", "One campaign, a goal and a closing date"),
    ("2", "A standing appeal", "No goal and no end date: always open"),
    ("3", "A memorial fund", "Gifts in memory of someone, kept on each receipt"),
    ("4", "Year-end giving", "A tax receipt for every donor"),
]


def slide(step_id, chapter_title, label):
    n = int(step_id.split("-")[-1])
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "A gift of any size")
        bullets(image, WHY, active=set(range(n)))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Receipts, a campaign, donors")
        bullets(image, HOW, active={n - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where it fits")
        bullets(image, USES, active={n - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
