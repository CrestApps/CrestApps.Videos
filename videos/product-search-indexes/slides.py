"""The slides of the Product search indexes video."""
from brand import bullets, slide_base

WHY = [
    ("1", "The database search suits most catalogs", "It is the default"),
    ("2", "A search server for large catalogs", "Or when the best match should be first"),
    ("3", "Same filters, counts, sorts and pages", "Prefix matching, ranked results"),
    ("check", "A feature is not enough", "Create the index, and choose it"),
]

HOW = [
    ("1", "Turn on a product search feature", "Elasticsearch, Lucene or Azure AI Search"),
    ("2", "Add the index", "Search, Indexes, Add"),
    ("3", "Choose it on the listing", "The Search index setting"),
    ("check", "Elasticsearch and Azure AI Search", "Need their connection first"),
]

KNOW = [
    ("1", "A fallback", "The database is used if the server is unreachable"),
    ("2", "The other engines", "Work the same way"),
    ("3", "Keyword search can be off", "And a listing can have no filters"),
    ("check", "Not needed for a small shop", "Add one when it helps"),
]

USE = [
    ("1", "A large catalog", "Search stays fast"),
    ("2", "Ranking matters", "The best match first"),
    ("3", "An existing search server", "Elasticsearch or Azure AI Search"),
    ("4", "A small shop", "Keep the database"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Search that ranks the best match first")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Feature, index, listing")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("kno-"):
        image = slide_base(chapter_title, label, "Fallback and the other engines")
        bullets(image, KNOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to use a search index")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
