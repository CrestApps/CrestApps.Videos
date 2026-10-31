"""The slides of the Connected data in reports and pipelines video."""
from brand import bullets, slide_base

WHERE = [
    ("1", "The Report Builder", "Add data set, next to content items and users"),
    ("2", "Data pipelines", "The Read a data source step"),
    ("3", "Connected data (live)", "Read from the other system when the report runs"),
    ("4", "Connected data (imported)", "Read from the last copy: fast, and available when the source is down"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("where-"):
        image = slide_base(chapter_title, label, "Where connected data appears")
        bullets(image, WHERE, active={"where-1": {0, 1}, "where-2": {2, 3}, "where-3": {2, 3}}[step_id])
        return image
    raise SystemExit(f"No slide for {step_id}")
