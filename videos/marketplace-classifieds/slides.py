"""The slides of the Marketplace and Classifieds video."""
from brand import bullets, slide_base

WHY = [
    ("1", "More to sell", "You cannot make or stock everything yourself"),
    ("2", "A marketplace", "Vendors list products; you keep a commission and pay them later"),
    ("3", "Classifieds", "Visitors post listings; you approve them; they expire and renew"),
    ("check", "People and listings you did not create", "Both bring more to your site"),
]

HOW = [
    ("1", "Vendors", "Sellers with the people who run them, chosen on each product"),
    ("2", "Commission, hold, payout", "The store keeps its share, and pays the rest when it is safe"),
    ("3", "Classified types", "A content type visitors can post, approved by a moderator"),
    ("4", "Terms and upgrades", "Listings run for a term; upgrades feature or bump them"),
]

USES = [
    ("1", "A craft marketplace", "Makers as vendors, paid after the return period"),
    ("2", "Services", "Release a sale when the work is delivered"),
    ("3", "A local noticeboard", "Ads approved by a moderator, expiring on their own"),
    ("4", "A directory or property site", "Featured listings, searched by distance"),
]


def slide(step_id, chapter_title, label):
    n = int(step_id.split("-")[-1])
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Let other people sell")
        bullets(image, WHY, active=set(range(n)))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Vendors, and posted listings")
        bullets(image, HOW, active={n - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where it fits")
        bullets(image, USES, active={n - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
