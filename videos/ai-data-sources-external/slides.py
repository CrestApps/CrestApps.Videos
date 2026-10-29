"""The slides of the External Data Sources video."""
from brand import bullets, slide_base

WHY = [
    ("1", "The knowledge is already there", "A search index or a table that another team maintains"),
    ("2", "Read it where it is", "No copy to go stale; the knowledge base follows it"),
    ("check", "One type for each", "Elasticsearch, Azure AI Search or PostgreSQL"),
]

SHARE = [
    ("1", "A connection", "Where to read: the index and address, or the database and table"),
    ("2", "A destination", "A knowledge base index of your site, as for every data source"),
    ("3", "A field mapping", "The names of the content, title and key fields"),
]

CASES = [
    ("1", "A product team's index", "Support answers from the notes they maintain"),
    ("2", "A shared Azure AI Search index", "Manuals used by several applications"),
    ("3", "A database table", "Articles or notes, read without a copy"),
]


def slide(step_id, chapter_title, label):
    kinds = {
        "why": ("Read the knowledge where it is", WHY, lambda n: set(range(n))),
        "share": ("What every source shares", SHARE, lambda n: {n - 1}),
        "case": ("What teams connect", CASES, lambda n: {n - 1}),
    }
    prefix, _, number = step_id.partition("-")

    if prefix in kinds:
        heading, items, active = kinds[prefix]
        image = slide_base(chapter_title, label, heading)
        bullets(image, items, active=active(int(number)))
        return image

    raise SystemExit(f"No slide for {step_id}")
