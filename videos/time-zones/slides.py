"""The slides of the Time Zones video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Every date has a time zone", "The site, a team member or a contact has to choose one"),
    ("2", "Hundreds of technical names", "Hard to scan, and most are zones nobody on your team uses"),
    ("3", "The wrong zone, or several names for one place", "Times shown to customers and colleagues drift"),
    ("check", "You decide the list", "Every menu shows only your friendly names, in the same order"),
]

HOW = [
    ("1", "A time zone map", "A friendly name linked to one real time zone"),
    ("2", "A starter list", "About twenty common maps that you can edit, delete or add to"),
    ("3", "Every menu follows the list", "Site default, a person's own zone, contact and activity screens"),
    ("check", "No map, no choice", "A zone without a map cannot be picked anywhere"),
]

USE = [
    ("1", "Teams in a few offices", "Everyone picks from the same short list"),
    ("2", "Event, delivery and opening times", "Shown to visitors in the right local time"),
    ("3", "Contact centers", "Agents set the zone of each contact and activity"),
    ("4", "One name per place", "Eastern Time, not several technical names"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Hundreds of technical names, or the few you use")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "A time zone map and where it appears")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four typical uses")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
