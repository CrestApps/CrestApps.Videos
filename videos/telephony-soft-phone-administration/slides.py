"""The slides of the Soft Phone Administration video."""
from brand import bullets, slide_base

WHY = [
    ("1", "The same phone for everyone", "Right provider, right place, right country"),
    ("2", "Set once, for the whole site", "The Soft Phone tab, and the permission Use the telephony soft phone"),
]

USES = [
    ("1", "A new site", "Turn the phone on, set the default country"),
    ("2", "A help desk", "Turn on the diagnostics tab so agents test their own headset"),
    ("3", "Admin-only teams", "Show the phone on the admin dashboard, keep the front end clean"),
]

# step-id prefix -> (cards, heading, how the cards light up: "all" up to the step number, "one" the card of the step, or a fixed set per step id)
TABLE = {"why-": (WHY, "Why administer it", "all"), "use-": (USES, "Soft phone administration at work", "one")}


def slide(step_id, chapter_title, label):
    for prefix, (cards, heading, active) in TABLE.items():
        if step_id.startswith(prefix):
            image = slide_base(chapter_title, label, heading)
            number = int(step_id.split("-")[-1])
            lit = active.get(step_id) if isinstance(active, dict) else (set(range(number)) if active == "all" else {number - 1})
            bullets(image, cards, active=lit)
            return image
    raise SystemExit(f"No slide for {step_id}")
