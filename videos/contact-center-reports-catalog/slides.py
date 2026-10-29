"""The slides of the Contact Center Report Catalog video."""
from brand import bullets, slide_base

MAP1 = [
    ("1", "Executive", "One dashboard for leaders: the main numbers and charts"),
    ("2", "Operations", "How the center performed over time, by hour, day, channel and outcome"),
    ("3", "Queue & Routing", "How each queue served its callers, and why work is not moving"),
    ("4", "Agent Performance", "How each agent worked: calls, handle time, utilization"),
]

MAP2 = [
    ("5", "Workforce & Payroll", "The time agents spend in each state, day by day"),
    ("6", "Billing & Usage", "What the center used, by queue, agent, channel, provider and day"),
    ("7", "Compliance & Audit", "The records behind the totals, and recording coverage"),
    ("8", "Technical & IT", "Providers, call legs and failed calls"),
]

USES = [
    ("1", "Is the center healthy this week?", "Executive performance dashboard"),
    ("2", "Are callers waiting too long?", "Queue service level, then High-wait interaction detail"),
    ("3", "Who needs coaching, who was on the clock?", "Agent productivity, Agent handle time analysis, Reconciled payroll timecard"),
    ("4", "Are calls recorded, is a provider failing?", "Recording coverage, Provider performance, Failed interaction detail"),
]


def slide(step_id, chapter_title, label):
    if step_id == "map-1":
        image = slide_base(chapter_title, label, "Eight categories, one kind of question each")
        bullets(image, MAP1, active={0, 1, 2, 3})
        return image
    if step_id == "map-2":
        image = slide_base(chapter_title, label, "Eight categories, one kind of question each")
        bullets(image, MAP2, active={0, 1, 2, 3})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Which report for which question")
        number = int(step_id.split("-")[-1])
        active = {0, 1} if number == 1 else ({2} if number == 2 else {3})
        bullets(image, USES, active=active)
        return image
    raise SystemExit(f"No slide for {step_id}")
