"""The slides of the Selling Sites video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Agencies, platforms, services", "Sell sites, not just products"),
    ("2", "A site of their own", "With their own administrator"),
    ("check", "A failed setup never loses the sale", "It is retried, then handed to a person"),
]

HOW = [
    ("1", "A plan with Tenant Onboarding", "And the recipe for the new site"),
    ("2", "A Your new site step", "Name, title, address, administrator"),
    ("3", "Created in the background", "The buyer follows it on My Sites"),
    ("4", "Site provisioning", "Every job, and Retry now"),
]

STATUS = [
    ("1", "My Sites", "Being created, Retrying, Ready, Needs help"),
    ("2", "Site provisioning", "Waiting, Created, Retrying, Abandoned"),
    ("3", "Abandoned", "A person creates the site, then Mark as created"),
    ("4", "When the subscription ends", "The site is disabled, not deleted"),
]

USES = [
    ("1", "An agency", "A starter site for each client"),
    ("2", "A software service", "A tenant for each subscriber"),
    ("3", "A blogging platform", "A blog for each member"),
]


def slide(step_id, chapter_title, label):
    n = int(step_id.split("-")[-1])
    kind = step_id.split("-")[0]
    pages = {"why": (WHY, "Why sell sites"), "how": (HOW, "Plan, checkout, creation"),
             "stat": (STATUS, "Statuses"), "use": (USES, "Where it fits")}

    if kind not in pages:
        raise SystemExit(f"No slide for {step_id}")

    items, title = pages[kind]
    image = slide_base(chapter_title, label, title)
    bullets(image, items, active=set(range(n)) if kind == "why" else {n - 1})
    return image
