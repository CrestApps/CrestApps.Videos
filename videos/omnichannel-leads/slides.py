"""The slides of the Omnichannel Leads video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Prospects are not customers", "A purchased list would fill the contact list with strangers"),
    ("2", "Worked like contacts", "Call, text, load as activities, retry with a subject flow"),
    ("check", "Convert the ones that qualify", "The lead becomes a contact, and its history moves with it"),
]

AUTO = [
    ("1", "Subject flow action", "Convert Lead when an activity ends with a chosen disposition"),
    ("2", "AI lead conversion", "On an Automatic load, the AI converts a lead that qualified"),
    ("3", "Workflows", "A Lead Converted event, and a Convert Lead task"),
    ("4", "Three CRM reports", "Lead funnel, conversion by source and list, opportunity pipeline"),
]

USE = [
    ("1", "Purchased lists", "Worked without polluting the contact list"),
    ("2", "Trade-show leads", "Hot ones called first"),
    ("3", "One-step conversion", "Contact, account and deal together"),
    ("4", "AI qualification", "Convert the leads that qualified by text or call"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Keep prospects apart from customers")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("auto-"):
        image = slide_base(chapter_title, label, "Subject flows, the AI and workflows")
        bullets(image, AUTO, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four typical uses")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
