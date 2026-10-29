"""The slides of the Report Builder administration video."""
from brand import bullets, slide_base

LIMITS = [
    ("1", "Batches, not everything", "Each data set is read in batches; big joins and totals use temporary files"),
    ("2", "A warning, not a surprise", "The numbers cover the rows that were read: narrow the period or filter more"),
    ("3", "Keep a date filter", "A new report starts with the last 30 days; scheduled views prepare heavy data"),
]

FIXES = [
    ("1", "No Report Builder under Reports", "The feature is off, or the person lacks the build permission"),
    ("2", "Fewer records than expected", "The new-report date filter is the last 30 days: pick a longer period"),
    ("3", "Totals too high after a join", "A join repeats a row for each match: count distinct values"),
    ("4", "A viewer sees data they cannot open", "Expected: a shared report reads data with its owner's access"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("lim-"):
        image = slide_base(chapter_title, label, "Large data")
        bullets(image, LIMITS, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("fix-"):
        image = slide_base(chapter_title, label, "Common problems")
        bullets(image, FIXES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
