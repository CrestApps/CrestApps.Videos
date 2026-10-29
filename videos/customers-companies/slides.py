"""The slides of the Customers and Companies video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Know who buys", "A person or a business, with an account or as a guest"),
    ("2", "Businesses buy differently", "Several buyers, an invoice, their own prices, an approval"),
    ("3", "Shoppers expect more", "An address book, rewards, and control of their data"),
    ("check", "One place in the admin", "Customers and Companies"),
]

HOW = [
    ("1", "Customer", "How to address them, their tax details, addresses and marketing choice"),
    ("2", "Account", "The sign in and the email, kept on the user"),
    ("3", "Company", "A business with its buyers, terms, locations and prices"),
    ("4", "Rewards", "Store credit, loyalty points, referrals and checkout details"),
]

APPROVALS = [
    ("1", "Limits", "A buyer's order limit, and the company's monthly budget"),
    ("2", "Ask for approval", "The Company step at checkout says why, and asks"),
    ("3", "An admin decides", "Approve or reject from the Company page, with a note"),
    ("4", "Pay now, or canceled", "An approved order can be paid; a rejected one is canceled"),
]

USES = [
    ("1", "A shop for the public", "Quick guest checkout, and accounts with addresses and rewards"),
    ("2", "A wholesaler", "Companies on a Wholesale price list, net 30, with a purchase order number"),
    ("3", "A buying team", "Order limits and a monthly budget, approved by a manager"),
    ("4", "A privacy request", "Download what you hold, or erase it and keep your accounts"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "A list of emails is not enough")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Customers, accounts and companies")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("appr-"):
        image = slide_base(chapter_title, label, "Approving an order")
        bullets(image, APPROVALS, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where it fits")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
