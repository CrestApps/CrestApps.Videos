"""The slides of the Inbound entry points video."""
from brand import bullets, slide_base

WHY = [
    ("1", "A number does nothing alone", "Someone has to decide who answers it, and when"),
    ("2", "Rules end up scattered", "A setting on the number, another on the queue, another in the phone system"),
    ("check", "One front door", "An entry point holds the rules of the numbers it answers"),
]

HOW = [
    ("1", "Numbers", "Picked from Omnichannel Addresses; one entry point per number and channel"),
    ("2", "Routing", "A queue, one specific agent or an AI, with a priority for calls"),
    ("3", "Hours", "A business hours calendar, and what a closed call does"),
    ("4", "Voicemail", "The greeting, and whose inbox receives the message"),
]

TEST = [
    ("1", "Add the number and the queue", "Omnichannel Addresses, then Queues"),
    ("2", "Create the entry point", "Numbers, routing and hours"),
    ("3", "Sign an agent in and call", "The call is offered in My workspace"),
    ("4", "Watch the Live dashboard", "Waiting count first, then available to busy"),
]

USES = [
    ("1", "A main line with office hours", "Rings the support queue by day, takes voicemail at night"),
    ("2", "A personal line", "Rings one account manager, then takes a message"),
    ("3", "Sales and support apart", "Separate numbers, with sales at a higher priority"),
    ("4", "A text line that answers at once", "An auto-reply, and a closed auto-reply after hours"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "A number needs a front door")
        bullets(image, WHY, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Numbers, routing, hours and voicemail")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("tst-"):
        image = slide_base(chapter_title, label, "Before you publish a number")
        bullets(image, TEST, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where entry points help")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
