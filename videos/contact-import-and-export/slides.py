"""The slides of the Contact Import and Export video."""
from brand import bullets, slide_base

WHY = [
    ("1", "A number identifies a person", "Twice in the list means called twice"),
    ("2", "Lists have repeats", "People you already have, or the same person on two rows"),
    ("check", "The import cleans as it loads", "Skips duplicates and reads numbers with a country"),
]

CHECKS = [
    ("1", "National do-not-call registries", "Numbers on them are left out"),
    ("2", "Nothing disappears silently", "Skipped rows are counted as errors, with the reason"),
    ("3", "Leads have the same checks", "Plus a list name and a lead status"),
]

USE = [
    ("1", "Load a list without duplicates", "Purchased or exported"),
    ("2", "Clean a list", "Before it is called"),
    ("3", "Export the last result", "Of every contact"),
    ("4", "Prepare the next call list", "From the last outcome"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Contacts are not ordinary content")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("checks-"):
        image = slide_base(chapter_title, label, "What else the import checks")
        bullets(image, CHECKS, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four typical uses")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
