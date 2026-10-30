"""The slides of the Omnichannel Subjects video."""
from brand import bullets, slide_base

WHY = [
    ("1", "What it is about", "A sales call, a support request, a welcome call"),
    ("2", "Outbound or inbound", "Work you start, or work the customer starts"),
    ("check", "The same handling every time", "The subject carries the fields and the flow"),
]

AUTO = [
    ("1", "The AI answers", "Address, AI profile, goal, speech models and voice"),
    ("2", "Guard rails", "What the AI may update, timeout, delay and opt-out words"),
    ("3", "Live agent handoff", "A queue and the reasons to escalate"),
]

USE = [
    ("1", "A subject per kind of call", "Outbound work your team starts"),
    ("2", "Log inbound calls", "With a manual inbound subject"),
    ("3", "Capture what matters", "With fields on the subject"),
    ("4", "Let the AI answer", "With an automated subject"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "A subject is what a conversation is about")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("auto-"):
        image = slide_base(chapter_title, label, "Settings of an automated subject")
        bullets(image, AUTO, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four typical uses")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
