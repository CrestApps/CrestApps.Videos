"""The slides of the Contact Center Roles and Permissions video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Different jobs, different screens", "An agent takes calls, a supervisor watches the floor, an administrator runs the site"),
    ("2", "A permission and a role", "A permission allows one thing; a role is a named set of permissions; a user gets every permission of every role"),
    ("check", "Three ready-made names", "Administrator, Agent and Supervisor come with default permissions"),
]

USES = [
    ("1", "A team lead who also takes calls", "Agent and Supervisor roles, plus an entitlement record with the team's queues"),
    ("2", "A quality reviewer", "A role made by hand: Listen to anyone's call recordings and View Contact Center reports"),
    ("3", "Soft phone for every agent", "Tick Use the telephony soft phone once, on the Agent role"),
    ("4", "Listen without deleting", "Only administrators delete call recordings"),
]


def slide(step_id, chapter_title, label):
    table = {"why-": (WHY, "Why roles"), "use-": (USES, "Roles at work")}
    for prefix, (cards, title) in table.items():
        if step_id.startswith(prefix):
            image = slide_base(chapter_title, label, title)
            number = int(step_id.split("-")[-1])
            bullets(image, cards, active=set(range(number)) if prefix == "why-" else {number - 1})
            return image
    raise SystemExit(f"No slide for {step_id}")
