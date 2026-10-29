"""The slides of the Build an AI Knowledge Assistant video."""
from brand import bullets, slide_base

GET = [
    ("1", "Searches your knowledge first", "Your documents and your data, before it answers"),
    ("2", "Shows where answers came from", "Numbered sources under each answer"),
    ("check", "Can be strict", "Says so when it cannot find the answer"),
]

BEFORE = [
    ("1", "Search indexes", "On a search service from your technical team"),
    ("2", "An AI connection", "A chat deployment and an embedding deployment"),
    ("3", "Ready here", "The documents index and a help articles data source"),
]

TIPS = [
    ("1", "A few good documents", "Out of date files lead to out of date answers"),
    ("2", "Keep it current", "Data sources follow the site; replace documents by hand"),
    ("3", "People's own files", "Allow session document uploads on the profile"),
]


def slide(step_id, chapter_title, label):
    kinds = {
        "get": ("What you get", GET, lambda n: set(range(n))),
        "pre": ("Before you start", BEFORE, lambda n: {n - 1}),
        "tip": ("Keep the knowledge good", TIPS, lambda n: {n - 1}),
    }
    prefix, _, number = step_id.partition("-")

    if prefix in kinds:
        heading, items, active = kinds[prefix]
        image = slide_base(chapter_title, label, heading)
        bullets(image, items, active=active(int(number)))
        return image

    raise SystemExit(f"No slide for {step_id}")
