"""The slides of the Customers, Guests and Privacy video."""
from brand import bullets, slide_base

WHY = [
    ("1", "A record for every buyer", "Even when they never sign up"),
    ("2", "Orders follow the account", "Without anyone claiming another's orders"),
    ("3", "Data should not live for ever", "Retention, access and erasure"),
    ("check", "Your own questions", "A gift message at checkout"),
]

HOW = [
    ("1", "Guest customers", "One per guest checkout"),
    ("2", "Merged", "Orders move to the confirmed account"),
    ("3", "Retention", "Two settings, applied every day"),
    ("4", "Checkout details", "Your fields, and staff My Account"),
]

USES = [
    ("1", "Many guests", "Keep their data for a limited time"),
    ("2", "A gift shop", "A gift message kept with the order"),
    ("3", "Regulars", "Guests who sign up find their orders"),
    ("4", "A privacy request", "Download, or erase, personal data"),
]


def slide(step_id, chapter_title, label):
    n = int(step_id.split("-")[-1])
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Why this matters")
        bullets(image, WHY, active=set(range(n)))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Guests, accounts and settings")
        bullets(image, HOW, active={n - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where it fits")
        bullets(image, USES, active={n - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
