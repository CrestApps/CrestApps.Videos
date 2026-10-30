"""The slides of the Content Transfer Recovery video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Real files have surprises", "A missing title, a bad number, an unreadable date"),
    ("2", "Every good row is saved", "And the rows that failed are listed with a reason"),
    ("check", "Fix only what failed", "Instead of starting over"),
]

STALL = [
    ("1", "A stalled import fails after ten minutes", "Resume continues from where it stopped"),
    ("2", "Exports are built in the background", "You are notified when the file is ready"),
    ("3", "Transfers run in order", "A second big file waits its turn"),
]

USE = [
    ("1", "Import big files", "Without fear of one bad row"),
    ("2", "Fix only the failed rows", "And import them again"),
    ("3", "Recover an interrupted import", "With Resume"),
    ("4", "Keep the list tidy", "Remove old entries"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Big files rarely import perfectly")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("stall-"):
        image = slide_base(chapter_title, label, "Stalled imports and queued exports")
        bullets(image, STALL, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four typical uses")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
