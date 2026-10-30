"""The slides of the Omnichannel Manage Activities in Bulk video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Plans change", "An agent is away, a campaign ends, a list moves up"),
    ("2", "Everything open, in one view", "Whoever owns it, filtered to exactly what you mean"),
    ("check", "One action, many activities", "Live calls are never touched"),
]

DIALER = [
    ("1", "Live calls are protected", "Dialing, ringing and in-call activities stay unchanged"),
    ("2", "Change Dialer Profile", "A tenth action, with the outbound dialer on"),
    ("3", "One profile per campaign queue", "Move one profile's activities at a time"),
]

USE = [
    ("1", "Reassign", "An absent agent's work to the team"),
    ("2", "Reprioritize", "Everything for one campaign"),
    ("3", "Move", "Activities to a new subject or campaign"),
    ("4", "Purge", "Work that is no longer needed"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Change many activities at once")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("dialer-"):
        image = slide_base(chapter_title, label, "What a bulk action leaves alone")
        bullets(image, DIALER, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four typical uses")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
