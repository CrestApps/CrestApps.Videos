"""The slides of the AI Chat Widgets video."""
from brand import bullets, slide_base

TWO = [
    ("1", "The admin widget", "An assistant on every admin page, for your staff"),
    ("2", "The website widget", "An ordinary widget in a zone, for your visitors"),
    ("3", "One chat profile", "Instructions, chat mode and ratings all apply"),
]

PROTECT = [
    ("1", "Limits", "Messages and new chats per visitor, per short time"),
    ("2", "Blocked tricks", "Messages that try to override the instructions"),
    ("3", "Stay focused", "Clear instructions, and answers from your own content"),
]

USES = [
    ("1", "A shop", "A website assistant in the footer"),
    ("2", "An editor", "The admin assistant, without leaving the page"),
    ("3", "A support lead", "An admin assistant with tools"),
]


def slide(step_id, chapter_title, label):
    table = {
        "two-": ("Two widgets, one profile", TWO),
        "pro-": ("Keep a public assistant safe", PROTECT),
        "use-": ("Ways to use it", USES),
    }
    for prefix, (subtitle, items) in table.items():
        if step_id.startswith(prefix):
            image = slide_base(chapter_title, label, subtitle)
            bullets(image, items, active={int(step_id[-1]) - 1})
            return image
    raise SystemExit(f"No slide for {step_id}")
