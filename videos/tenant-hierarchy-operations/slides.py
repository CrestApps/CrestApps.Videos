"""The slides of the Tenant Hierarchy Operations video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Dozens of client sites", "Some need attention every day"),
    ("2", "One list to act from", "Find a site, see its state, change one or many"),
    ("check", "The platform keeps order", "Parents, clients, and what happens to orphans"),
]

USE = [
    ("1", "Pause and resume", "The sites of clients who have not paid"),
    ("2", "Find what needs attention", "With the state filter"),
    ("3", "Favorites", "Your most important clients, one click away"),
    ("4", "Tidy the hierarchy", "After a parent changes"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Many sites, one list")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four typical uses")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
