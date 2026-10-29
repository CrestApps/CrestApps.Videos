"""The slides of the AI Data Sources video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Uploads do not scale", "Nobody re-uploads every article each time it changes"),
    ("2", "The content is already indexed", "Your site keeps it in a search index"),
    ("3", "A data source reads the index", "Passages, kept in step as the content changes"),
    ("check", "Answers with sources", "Numbered sources under the answer"),
]

HOW = [
    ("1", "Source index", "The content index of your site"),
    ("2", "Knowledge base index", "The destination: passages searched by meaning"),
    ("3", "Data source", "Links the two, with a field mapping"),
    ("check", "A profile uses it", "The assistant answers from the data source"),
]

CASES = [
    ("1", "Help articles", "Answers from what your editors publish"),
    ("2", "Products", "The catalog, with the product named in the answer"),
    ("3", "News and policies", "Change a page, and the knowledge follows"),
    ("4", "Several sources", "More than one data source can feed a knowledge base"),
]


def slide(step_id, chapter_title, label):
    kinds = {
        "why": ("Large and changing content", WHY, lambda n: set(range(n))),
        "how": ("Two indexes and a data source", HOW, lambda n: {n - 1}),
        "case": ("What teams index", CASES, lambda n: {n - 1}),
    }
    prefix, _, number = step_id.partition("-")

    if prefix in kinds:
        heading, items, active = kinds[prefix]
        image = slide_base(chapter_title, label, heading)
        bullets(image, items, active=active(int(number)))
        return image

    raise SystemExit(f"No slide for {step_id}")
