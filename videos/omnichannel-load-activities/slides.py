"""The slides of the Omnichannel Load Activities video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Create work in bulk", "Describe the contacts and the work once"),
    ("2", "Count before you run", "See how many records match and would be loaded"),
    ("check", "Report and repeat", "Skipped contacts are explained, and a load can be cloned"),
]

REPORT = [
    ("1", "Why a contact is skipped", "Open activity, opted out, no address, not in service, limit"),
    ("2", "Interrupted loads resume", "Within about ten minutes, up to three times, nobody twice"),
    ("3", "A failed load goes back to New", "Its page says why, so you can fix it and load again"),
]

USE = [
    ("1", "Start a call day", "Pick the right time zones"),
    ("2", "Retry the no answers", "With the last activity filters"),
    ("3", "Share a list", "Fairly between several agents"),
    ("4", "Preview first", "Count matching records before anyone calls"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Loads create activities in bulk")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("report-"):
        image = slide_base(chapter_title, label, "What a load tells you")
        bullets(image, REPORT, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four typical uses")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
