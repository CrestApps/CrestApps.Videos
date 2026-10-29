"""The slides of the Report widgets on the dashboard video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Choose a report once", "Any report you may view, or a Report Builder report"),
    ("2", "A period that moves", "The last 30 days is always the 30 days up to now"),
    ("3", "Kept for a few minutes", "The dashboard does not run the report on every visit"),
    ("4", "Checked for each viewer", "A report you may not open shows a notice, not numbers"),
]

USES = [
    ("1", "A supervisor's start of day", "Queue service level for yesterday, as figures only"),
    ("2", "A sales lead's board", "Lead funnel for the month, beside a Report Builder report"),
    ("3", "A quiet wall display", "A longer cache time keeps the dashboard fast for everyone"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "A report that is always on the page")
        bullets(image, WHY, active={"why-1": {0}, "why-2": {1}, "why-3": {2, 3}}[step_id])
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where widgets fit")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
