"""The slides of the Headless API video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Your own storefront", "In any framework"),
    ("2", "An app, a till, a back office", "Phone app, point of sale, warehouse, accounting"),
    ("check", "The same rules everywhere", "Products, prices, coupons, tax, stock holds"),
]

HOW = [
    ("1", "API features", "One per commerce module"),
    ("2", "OpenAPI", "Explorer pages for developers"),
    ("3", "Sign-in server", "A token with every call"),
    ("4", "CORS", "For a storefront on another address"),
]

CALLERS = [
    ("1", "A guest", "Browse, fill a cart, check out"),
    ("2", "A signed-in buyer", "Account, orders, returns, shipments, credit"),
    ("3", "Staff or a server", "The same permissions as the admin pages"),
    ("4", "What it cannot do", "Store setup, your checkout fields, telling other systems"),
]

USES = [
    ("1", "A headless storefront", "Your own website"),
    ("2", "A phone app", "Shop and follow orders"),
    ("3", "A point of sale", "At the counter"),
    ("4", "A back office", "Warehouse and accounting"),
]


def slide(step_id, chapter_title, label):
    n = int(step_id.split("-")[-1])
    kind = step_id.split("-")[0]
    pages = {"why": (WHY, "Why a commerce API"), "how": (HOW, "Features, explorers, sign-in and access"),
             "caller": (CALLERS, "Who can do what"), "use": (USES, "Where it fits")}

    if kind not in pages:
        raise SystemExit(f"No slide for {step_id}")

    items, title = pages[kind]
    image = slide_base(chapter_title, label, title)
    bullets(image, items, active=set(range(n)) if kind == "why" else {n - 1})
    return image
