"""The slides of the AI Memory video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Chats start from nothing", "People repeat their name, role and preferred style"),
    ("2", "Memory keeps a few facts", "About each signed-in person, for later chats"),
    ("3", "Memory is private", "An assistant only reads the memory of the person it talks to"),
    ("check", "Lasting, non-sensitive facts", "Never passwords, keys or card numbers"),
]

HOW = [
    ("1", "A search index", "Memories are found by meaning, so you create it once"),
    ("2", "The site settings", "Which index holds the memories, and how many come back"),
    ("3", "The profile", "Memory is off until you turn it on for a profile"),
    ("check", "People stay in control", "Ask what is remembered, or clear it all"),
]

CASES = [
    ("1", "Staff assistants", "Names, roles and preferred styles"),
    ("2", "Specialist assistants", "The projects and topics a person works on most"),
    ("3", "Fresh starts", "Forget one thing, or clear everything from the profile"),
    ("4", "Not for secrets", "Stable, non-sensitive facts only: public assistants stay without it"),
]


def slide(step_id, chapter_title, label):
    kinds = {
        "why": ("Why memory", WHY, lambda n: set(range(n))),
        "how": ("An index, the settings, the profile", HOW, lambda n: {n - 1}),
        "case": ("Where memory helps", CASES, lambda n: {n - 1}),
    }
    prefix, _, number = step_id.partition("-")

    if prefix in kinds:
        heading, items, active = kinds[prefix]
        image = slide_base(chapter_title, label, heading)
        bullets(image, items, active=active(int(number)))
        return image

    raise SystemExit(f"No slide for {step_id}")
