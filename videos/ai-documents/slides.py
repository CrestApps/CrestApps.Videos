"""The slides of the AI Documents video."""
from brand import bullets, slide_base

WHY = [
    ("1", "A file and a question", "A policy, a report, a contract"),
    ("2", "Attach it to the chat", "The assistant reads it and answers from it"),
    ("check", "Private to that chat", "Never part of the knowledge of the profile"),
]

FORMATS = [
    ("1", "Many kinds of files", "Text, Markdown, PDF, Word, PowerPoint, spreadsheets"),
    ("2", "Read and searched by meaning", "Split into passages; a very long file is refused"),
    ("3", "Charts and pictures", "Described by a vision model"),
]

CASES = [
    ("1", "A policy or a contract", "Ask about one clause"),
    ("2", "A report with charts", "Ask for the highest, the lowest or the trend"),
    ("3", "A specification", "Compare it with another file in the same chat"),
]


def slide(step_id, chapter_title, label):
    kinds = {
        "why": ("A file and a question", WHY, lambda n: set(range(n))),
        "fmt": ("Files, passages and figures", FORMATS, lambda n: {n - 1}),
        "case": ("What people attach", CASES, lambda n: {n - 1}),
    }
    prefix, _, number = step_id.partition("-")

    if prefix in kinds:
        heading, items, active = kinds[prefix]
        image = slide_base(chapter_title, label, heading)
        bullets(image, items, active=active(int(number)))
        return image

    raise SystemExit(f"No slide for {step_id}")
