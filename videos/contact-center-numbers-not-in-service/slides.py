"""The slides of the Numbers Not In Service video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Lists are never clean", "Disconnected, never valid, or changed hands"),
    ("2", "The network says not in service", "When a call reaches one of those numbers"),
    ("check", "The platform remembers it", "No campaign loads or dials the number again"),
]

FLOW = [
    ("1", "The call is placed", "SIP 404, 410, 484 or 604: unallocated, not found"),
    ("2", "No agent is connected", "An automatic call never starts the AI conversation"),
    ("3", "The activity is completed", "With the not-in-service disposition, and a note"),
    ("4", "The number joins the list", "With the campaign and activity that found it"),
]

NOTE = [
    ("1", "Some carriers play a recording", "The call looks answered, so the agent picks the disposition"),
    ("2", "The number is marked the same way", "And lookups catch many of them before they are dialed"),
]

WHERE = [
    ("1", "Loading activities", "The contact's next number is used instead"),
    ("2", "The dialer", "A record whose number was added later is cancelled"),
    ("3", "Automatic loads", "The activity is cancelled, not called or texted"),
    ("4", "The contact card", "A red Not in service badge, with the reason on hover"),
]

LOOKUP = [
    ("1", "Invalid or inactive line", "Added to the list before anybody dials it"),
    ("2", "Unreachable is not added", "Switched off or out of coverage may come back"),
    ("3", "Active again later", "A lookup mark is removed; a call's mark stays"),
]

REPORT = [
    ("1", "A Not in service column", "In the Campaign summary and Subject inventory reports"),
    ("2", "How clean was the list?", "And how much dialing went on dead numbers"),
]

USES = [
    ("1", "Before a big campaign", "Mark the numbers you already know are dead"),
    ("2", "A number given to somebody new", "Release it with Allow dialing"),
    ("3", "Judge a purchased list", "See how many of its numbers were dead"),
]


def slide(step_id, chapter_title, label):
    table = {
        "why-": ("Dead numbers waste calls", WHY),
        "flow-": ("The same for every dialer", FLOW),
        "note-": ("When the call looks answered", NOTE),
        "where-": ("Where numbers are skipped", WHERE),
        "look-": ("Number lookups", LOOKUP),
        "rep-": ("How clean was the list", REPORT),
        "use-": ("How teams use the list", USES),
    }
    for prefix, (heading, items) in table.items():
        if step_id.startswith(prefix):
            image = slide_base(chapter_title, label, heading)
            bullets(image, items, active={0, 1} if prefix == "note-" else {int(step_id.split("-")[-1]) - 1})
            return image
    raise SystemExit(f"No slide for {step_id}")
