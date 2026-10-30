"""The slides of the Recipes video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Setup by hand is slow", "And no two sites end up the same"),
    ("2", "A recipe is a script", "Content types, settings, roles and sample content"),
    ("check", "Run it anywhere", "Start a site, try a feature, or move a setup"),
]

USE = [
    ("1", "Start every site the same way", "From one recipe"),
    ("2", "Try a feature", "With sample content"),
    ("3", "Move a setup", "From a test site to a live one"),
    ("4", "Keep a copy", "A configuration you can restore"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "A setup you can run again")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four typical uses")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
