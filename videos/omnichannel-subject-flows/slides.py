"""The slides of the Omnichannel Subject Flows video."""
from brand import bullets, slide_base

WHY = [
    ("1", "A call ends", "Someone must decide what happens next"),
    ("2", "A flow holds that decision", "One or more actions for each disposition"),
    ("check", "Agents pick the outcome", "The follow-up is created the same way every time"),
]

PREVIEW = [
    ("1", "A preview before saving", "The actions that will run for the chosen disposition"),
    ("2", "Schedule at and Preparation notes", "The notes become the follow-up's instructions"),
    ("3", "Changes apply to new completions", "Activities already scheduled keep working"),
]

USE = [
    ("1", "Retry no answers", "A few times, then stop"),
    ("2", "Book follow-ups", "When a customer is interested"),
    ("3", "Respect a no", "Flag the contact Do not call"),
    ("4", "Convert at once", "When a lead qualifies"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "The disposition decides the next step")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("preview-"):
        image = slide_base(chapter_title, label, "What the agent sees")
        bullets(image, PREVIEW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four typical uses")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
