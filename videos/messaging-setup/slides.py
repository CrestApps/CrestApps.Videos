"""The slides of the Messaging: Setting Up Text Messages video."""
from brand import bullets, slide_base

NEED = [
    ("1", "Features", "SMS Messaging Channel brings the rest"),
    ("2", "Provider", "Twilio, Telnyx or Azure, and a default"),
    ("3", "Numbers", "Each one is a text address"),
    ("4", "Entry point", "Where a number's texts go"),
    ("5", "Permissions", "Who can use the inbox"),
    ("6", "Webhook", "The address your provider calls"),
]

NEED_ACTIVE = {1: {0}, 2: {1, 2}, 3: {3, 4}, 4: {5}}

OTHERS = [
    ("1", "More providers", "Telnyx and Azure Communication Services add a tab"),
    ("2", "No screen in the admin", "Azure Event Grid and Azure Blob Storage"),
]

USES = [
    ("1", "A shop", "One number, one Support queue, one shared inbox"),
    ("2", "Two brands", "Each number pinned to its own provider"),
    ("3", "A new number", "Tested with SMS Test before customers see it"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("need-"):
        image = slide_base(chapter_title, label, "From a new site to an inbox that works")
        bullets(image, NEED, active=NEED_ACTIVE[int(step_id[-1])])
        return image
    if step_id.startswith("oth-"):
        image = slide_base(chapter_title, label, "Beyond Twilio")
        bullets(image, OTHERS, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Ways to use it")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
