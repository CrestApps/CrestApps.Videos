"""The slides of the Memberships video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Content worth paying for", "Guides, articles, downloads"),
    ("2", "Access for a year, a month, or for good", "A membership sells it"),
    ("check", "The door opens, and closes, by itself", "No editing roles by hand"),
]

HOW = [
    ("1", "A product with the Membership part", "The roles, and the days it lasts"),
    ("2", "The Role Picker", "Which roles can see an item"),
    ("3", "Buy, and become a member", "Buying again adds days"),
    ("4", "It ends", "End date, canceled, refunded, or End now"),
]

USES = [
    ("1", "A club", "An annual membership for members' guides"),
    ("2", "A publication", "A monthly membership for premium articles"),
    ("3", "A course site", "A lifetime membership for the library"),
]


def slide(step_id, chapter_title, label):
    n = int(step_id.split("-")[-1])
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Why memberships")
        bullets(image, WHY, active=set(range(n)))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Product, roles and content")
        bullets(image, HOW, active={n - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where it fits")
        bullets(image, USES, active={n - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
