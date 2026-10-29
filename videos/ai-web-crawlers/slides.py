"""The slides of the AI Web Crawlers video."""
from brand import bullets, slide_base

WHY = [
    ("1", "The answers are on your website", "Shipping, warranty and account help already exist"),
    ("2", "Copies go stale", "Pasting pages by hand takes time, and the site keeps changing"),
    ("3", "A crawler reads the site", "Page by page, on a schedule, into passages to search"),
    ("check", "Only the changes, with links", "New, changed and removed pages; sources link to the page"),
]

HOW = [
    ("1", "Knowledge base index", "Stores the passages, searched by meaning"),
    ("2", "Web data source", "The target: the knowledge the assistant answers from"),
    ("3", "Web crawler", "The reader: the Sitemap strategy finds the pages"),
    ("check", "A profile uses it", "Answers come from the pages, with links"),
]

CASES = [
    ("1", "A help center", "Answers from the articles you already publish"),
    ("2", "Product pages", "What each product does, with a link to it"),
    ("3", "Public documentation", "Developers land on the right page"),
    ("4", "Keep it polite", "Read only sites you may read, with few concurrent requests"),
]


def slide(step_id, chapter_title, label):
    kinds = {
        "why": ("Knowledge that follows the website", WHY, lambda n: set(range(n))),
        "how": ("An index, a data source, a crawler", HOW, lambda n: {n - 1}),
        "case": ("What teams crawl", CASES, lambda n: {n - 1}),
    }
    prefix, _, number = step_id.partition("-")

    if prefix in kinds:
        heading, items, active = kinds[prefix]
        image = slide_base(chapter_title, label, heading)
        bullets(image, items, active=active(int(number)))
        return image

    raise SystemExit(f"No slide for {step_id}")
