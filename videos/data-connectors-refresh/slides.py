"""The slides of the Refreshing imported data sets video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Live", "Every read asks the other system, so the rows are always current"),
    ("2", "Imported", "The rows are copied into the site and reports read the copy"),
    ("3", "Always available", "Reports keep working when the other system is down"),
    ("4", "Replaced as a whole", "A new copy is published only when every row is written"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Live or imported")
        bullets(image, WHY, active={"why-1": {0, 1}, "why-2": {1, 2}, "why-3": {3}, "why-4": {0, 1, 2, 3}}[step_id])
        return image
    raise SystemExit(f"No slide for {step_id}")
