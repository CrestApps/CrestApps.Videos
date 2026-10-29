"""The slides of the Inbound Call Center video."""
from brand import bullets, slide_base

WHO = [
    ("1", "Managers, supervisors and agents", "A phone line for customers, run by a team"),
    ("2", "A welcome, a menu, and a queue", "Hold music, place in line, a free agent with the skills"),
    ("3", "After hours", "Held, voicemail, another queue, or refused"),
    ("check", "Answer, record, watch, report", "Agents answer, supervisors watch live"),
]

NEEDS = [
    ("1", "Agent Entitlements", "Recommended once you have more than one team"),
    ("2", "A phone provider account", "With the numbers customers will call"),
    ("3", "The Agent role", "Plus the permission to use the telephony soft phone"),
    ("4", "The Supervisor role", "For team leads"),
]

ORDER = [
    ("1", "What customers dial", "Numbers, opening hours, hold music and prompts"),
    ("2", "Where calls wait", "A queue for each team, such as Sales and Support"),
    ("3", "Who works them", "Entitlements, skills and break reasons"),
    ("4", "Route the number, then the rules", "An entry point, the house rules, the phone apps"),
]

ROLES = [
    ("1", "Agents get ready", "Phone app, sign in to queues, Available, a greeting"),
    ("2", "A call arrives", "Offer with a countdown, accept, talk, Wrap-up, disposition"),
    ("3", "Supervisors watch", "Live dashboard, listen, whisper, barge, shared voicemail"),
    ("4", "Reports", "Call insights, Queue usage and Agent productivity"),
]

TIPS = [
    ("1", "Test during opening hours", "Call the number, listen to the welcome and the menu"),
    ("2", "Test hold and after hours", "No agent free, then outside the calendar's hours"),
    ("3", "Start with one queue and one entry point", "Add menus and overflow after, and use Clone"),
    ("4", "Callbacks need the Outbound Dialer", "And an AI can answer the line instead"),
]


def slide(step_id, chapter_title, label):
    table = {
        "who-": ("Who it is for", WHO),
        "order-": ("The set-up order", ORDER),
        "role-": ("What each person does", ROLES),
        "tip-": ("Before the first real call", TIPS),
    }
    for prefix, (heading, items) in table.items():
        if step_id.startswith(prefix):
            n = int(step_id.split("-")[-1])
            image = slide_base(chapter_title, label, heading)
            bullets(image, items, active={min(n, len(items)) - 1})
            return image
    if step_id == "need-1":
        image = slide_base(chapter_title, label, "What you need")
        bullets(image, NEEDS)
        return image
    raise SystemExit(f"No slide for {step_id}")
