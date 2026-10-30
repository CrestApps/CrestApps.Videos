"""The slides of the Content Transfer Columns video."""
from brand import bullets, slide_base

WHY = [
    ("1", "The first row names the columns", "Each column feeds one part or field"),
    ("2", "Every type has its own columns", "The import page lists exactly the type you picked"),
    ("check", "The table is the contract", "Anything not in it is ignored"),
]

UPDATE = [
    ("1", "Export the items of a type", "ContentItemId is filled in for every row"),
    ("2", "Edit the values", "In a spreadsheet"),
    ("3", "Import the file again", "Rows with an id update, rows without one create"),
]

USE = [
    ("1", "Load a catalog", "From a spreadsheet"),
    ("2", "Fix a field", "On many items at once"),
    ("3", "Schedule publishing", "In bulk"),
    ("4", "Move content", "Between sites"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "An import file is a table")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("update-"):
        image = slide_base(chapter_title, label, "Update with a round trip")
        bullets(image, UPDATE, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four typical uses")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
