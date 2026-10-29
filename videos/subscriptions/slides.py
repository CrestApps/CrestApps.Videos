"""The slides of the Subscriptions video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Not bought once", "Software, courses and clubs bill every month or year"),
    ("2", "By hand does not scale", "Invoices, reminders and a list of who has access"),
    ("3", "A plan bills by itself", "And gives roles while it is paid"),
    ("check", "Memberships are simpler", "A product that makes its buyer a member for a time"),
]

HOW = [
    ("1", "Plan", "Subscription stereotype, Product part, Subscription part"),
    ("2", "Sign up", "A Plans page, Sign up, the checkout"),
    ("3", "Agreements and My Plans", "Every subscription, and a subscriber's own"),
    ("check", "Memberships", "Members and their end dates"),
]

USE = [
    ("1", "A software plan", "Monthly price, a free trial, roles for features"),
    ("2", "A members-only club", "A yearly plan whose role opens content"),
    ("3", "A setup fee", "An initial amount, then the regular price"),
    ("4", "A season pass", "A billing cycle limit ends it on its own"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Selling what customers keep paying for")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Plans, sign-up, agreements and memberships")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to use subscriptions")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
