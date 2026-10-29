"""The slides of the Contact Center Secure Capture video."""
from brand import bullets, slide_base

WHY = [
    ("1", "A number read aloud is everywhere", "In the agent's ears, in the recording, sometimes on paper"),
    ("2", "The customer types it instead", "A one-time link, opened on the customer's own device, tokenized on submit"),
    ("check", "Nobody on the call sees it", "Agent, supervisor and recording never hold the value; a security code is discarded"),
]

KEPT = [
    ("1", "When it ends", "Thank you page, recording resumes; an unused link expires and recording resumes on its own"),
    ("2", "What is kept", "A masked reference, such as the last four digits; no security code; no admin list shows it today"),
    ("3", "Events", "Started, completed and cancelled are recorded as events a workflow can react to"),
]

USES = [
    ("1", "Card payment by phone", "The customer enters the card on the secure page"),
    ("2", "A bank account for a refund", "Collected without being heard"),
    ("3", "An identity check", "A national identification number, with the same one-time link"),
    ("4", "A clean recording", "A reviewer never hears a card number"),
]


def slide(step_id, chapter_title, label):
    table = {"why-": (WHY, "Why secure capture"), "kept-": (KEPT, "What is kept"), "use-": (USES, "Secure capture at work")}
    for prefix, (cards, title) in table.items():
        if step_id.startswith(prefix):
            image = slide_base(chapter_title, label, title)
            number = int(step_id.split("-")[-1])
            bullets(image, cards, active=set(range(number)) if prefix == "why-" else {number - 1})
            return image
    raise SystemExit(f"No slide for {step_id}")
