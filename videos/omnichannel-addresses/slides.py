"""The slides of the Omnichannel Addresses video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Numbers have jobs", "Dial out, send texts, match incoming calls and texts"),
    ("2", "Listed once", "A checkbox for voice calls, text messages, or both"),
    ("check", "One place to manage them", "Every feature picks its number from this list"),
]

VOICE = [
    ("1", "Agents who dial from the number", "Others use the default numbers in Settings"),
    ("2", "Caller ID precedence", "Load, agent line, profile caller ID, provider default"),
    ("3", "Entry points answer", "An address is never deleted, and a number is listed once"),
]

USE = [
    ("1", "List every number", "In one place"),
    ("2", "Give agents their number", "One number per agent"),
    ("3", "Pick a campaign's sender", "For automatic texts"),
    ("4", "Clone a line", "A new number in seconds"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Every number the business owns")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("voice-"):
        image = slide_base(chapter_title, label, "Voice calls and the number shown")
        bullets(image, VOICE, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four typical uses")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
