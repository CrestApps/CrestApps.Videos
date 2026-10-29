"""The slides of the Subscriptions lifecycle video."""
from brand import bullets, slide_base

WHY = [
    ("1", "A subscription is not one sale", "It starts, renews, fails, pauses and ends"),
    ("2", "Each moment decides access", "And whether the subscriber is billed"),
    ("3", "One place to see them all", "Status, history, pause, resume, cancel"),
    ("check", "Subscribers manage their plans", "Memberships can be ended early"),
]

STATUSES = [
    ("1", "Active and Trial", "Billed, with access"),
    ("2", "Past due", "A renewal failed: access through the grace period"),
    ("3", "Paused, Canceled, Expired", "Suspended, ended, or all cycles billed"),
    ("check", "Roles are removed", "When a subscription stops being current"),
]

KNOW = [
    ("1", "Subscription settings", "Guest sign-up and the currency"),
    ("2", "Grace after a failed payment", "Seven days, not a setting there"),
    ("3", "Subscription reports", "New, active, canceled, due to renew"),
    ("check", "Workflow events", "Start a workflow when something happens"),
]

MEMBERS = [
    ("1", "Commerce, Memberships", "Products bought once that put a buyer in a role"),
    ("2", "Filter and search", "Active, Expired, Ended early"),
    ("3", "End now", "Ends it at once, and refunds nothing"),
    ("check", "Subscription memberships", "Are managed under Agreements"),
]

USE = [
    ("1", "A holiday", "Pause billing, resume later"),
    ("2", "A cancellation", "Keep access until the paid period ends"),
    ("3", "A failed card", "Time to fix it, in the grace period"),
    ("4", "A refunded member", "End the membership now"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "From first payment to the last")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("sta-"):
        image = slide_base(chapter_title, label, "Active, past due, paused, canceled, expired")
        bullets(image, STATUSES, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("mem-"):
        image = slide_base(chapter_title, label, "End a membership early")
        bullets(image, MEMBERS, active={int(step_id[-1]) - 1 if step_id[-1] == "1" else 2})
        return image
    if step_id.startswith("kno-"):
        image = slide_base(chapter_title, label, "Guests, grace and reports")
        bullets(image, KNOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to manage subscriptions")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
