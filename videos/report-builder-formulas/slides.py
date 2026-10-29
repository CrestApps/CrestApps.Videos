"""The slides of the Report Builder formulas video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Stored values are not the answer", "Seconds, dates and names, when people ask for minutes, labels and rates"),
    ("2", "A formula with a name", "Then drag it onto columns, filters and visuals like any field"),
    ("3", "It never stops the report", "Dividing by zero or a missing value gives an empty result"),
]

USES = [
    ("1", "Minutes, not seconds", "Divide the seconds, and give the column a format with one decimal"),
    ("2", "A label to group by", "Large and Small orders, Long and Short calls"),
    ("3", "A rate once per group", "Answered over offered, converted over created, refunded over sold"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Values the data does not have")
        bullets(image, WHY, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where formulas fit")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
