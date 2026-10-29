"""The slides of the Agent states video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Lunch is not a system problem", "A manager needs to know why an agent is not taking work"),
    ("2", "A reason code names the reason", "Each one belongs to a built-in presence state"),
    ("check", "State and reason, together", "Reports and the Live dashboard show both"),
]

HOW = [
    ("1", "Name and state", "Break, Away, Do not disturb, Meeting, Training or After-hours unavailable"),
    ("2", "Sort order", "A lower number appears higher in the presence menu"),
    ("3", "Starting codes", "Seven come with the site; change, delete or add your own"),
]

NOTES = [
    ("1", "Request break", "While an agent is busy, the break starts as soon as the work ends"),
    ("2", "No codes at all", "The menu falls back to Away, Do not disturb, Meeting, Training and After-hours unavailable"),
    ("3", "The starting codes", "Short break, Lunch, Away from desk, Team meeting, Coaching, Training, System issue"),
]

USES = [
    ("1", "Lunch apart from short breaks", "Reports show the real length of each"),
    ("2", "Coaching", "Supervisors know an agent is in a review, not idle"),
    ("3", "System issue", "See how much time a tooling problem costs"),
    ("4", "After-hours unavailable", "For agents who work a split shift"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Knowing where the time goes")
        bullets(image, WHY, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Reason codes and presence states")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("nts-"):
        image = slide_base(chapter_title, label, "Break requests and fallbacks")
        bullets(image, NOTES, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where reason codes help")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
