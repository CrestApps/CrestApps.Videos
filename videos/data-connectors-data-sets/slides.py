"""The slides of the Connected data sets video."""
from brand import bullets, slide_base

WHAT = [
    ("1", "A resource of a connection", "A file, an API path, a table or view, or an index"),
    ("2", "A name that stays", "The technical name is what reports and pipelines store"),
    ("3", "Live or imported", "Ask the system every time, or copy the rows on a schedule"),
    ("4", "Fields you review", "Names, types and which fields identify a row"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("what-"):
        image = slide_base(chapter_title, label, "What a data set holds")
        bullets(image, WHAT, active={int(step_id[-1]) - 1} if step_id != "what-1" else {0})
        return image
    raise SystemExit(f"No slide for {step_id}")
