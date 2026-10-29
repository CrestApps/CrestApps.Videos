"""The slides of the Contact Center Settings video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Some rules cannot be left to agents", "Recording and what callers are told must be the same on every call"),
    ("2", "Agents need numbers", "A number to call and text from, and a safe list to transfer to"),
    ("3", "Card numbers need protection", "Nobody on the team should hear or see them"),
    ("check", "One page, four tabs", "A tab appears only when its feature is on"),
]

DISC = [
    ("1", "Inbound call on an entry point", "The platform, word for word, before the welcome message"),
    ("2", "Turn-based AI voice agent", "The platform, word for word, at the start of the opening line"),
    ("3", "Realtime AI voice agent", "The model, told to say it first: read some transcripts to check"),
    ("4", "Outbound or skipped announcement", "The agent reads it, then presses I told the customer"),
]

USES = [
    ("1", "Every party must consent", "Disclosure on, and no recording until the caller has heard it"),
    ("2", "A retention policy", "Set the days, and use legal hold while a case is open"),
    ("3", "An after-hours answering service", "Listed as an approved destination for transfers"),
    ("4", "Taking payments", "Secure capture and pausing keep card details out of the recording"),
]


def slide(step_id, chapter_title, label):
    table = {"why-": (WHY, "Decide once, apply everywhere"), "disc-": (DISC, "Who says the disclosure, and when"),
             "use-": (USES, "What the settings do for you")}
    for prefix, (cards, title) in table.items():
        if step_id.startswith(prefix):
            image = slide_base(chapter_title, label, title)
            number = int(step_id.split("-")[-1])
            bullets(image, cards, active=set(range(number)) if prefix == "why-" else {number - 1})
            return image
    raise SystemExit(f"No slide for {step_id}")
