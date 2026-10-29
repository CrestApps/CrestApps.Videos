"""The slides of the Skills and entitlements video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Not every agent fits every call", "A Spanish speaker, a billing expert, a senior agent"),
    ("2", "Skills describe what an agent can do", "Spanish, Billing, Tier 2 support; queues can require or prefer them"),
    ("check", "Entitlements say where an agent may work", "Queues, campaigns, order of work and skills, in one record"),
]

HOW = [
    ("1", "Skills", "Created once, in a list, with the work distribution feature"),
    ("2", "One entitlement record per agent", "Allowed queues with priority and delay, campaigns, and skills from 1 to 5"),
    ("3", "A feature you turn on", "Without it, any agent may sign in to any queue, and there is no skills screen"),
]

USES = [
    ("1", "A Spanish queue", "Only agents with the Spanish skill are offered the work"),
    ("2", "A backup agent", "A delay on the sales queue, so the agent helps only when the team is busy"),
    ("3", "A senior agent", "Tier 2 support at proficiency 5, for the hardest questions"),
    ("4", "A supervisor", "A record for the queues they supervise, so the dashboard shows their team"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "The right agent for every customer")
        bullets(image, WHY, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Skills, entitlements and queues")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where skills and entitlements help")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
