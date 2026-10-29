"""The slides of the Contact Center Configuration and Deployment video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Hours of setup", "Queues, entry points, skills, business hours and dialer profiles are slow to rebuild by hand"),
    ("2", "A standard deployment plan", "Build it, export it, review it, replay it on another site"),
    ("check", "Safe to repeat", "Existing entries update in place, new ones keep their identifier, a refused one is reported and skipped"),
]

NOT = [
    ("1", "Traffic stays behind", "Activities, interactions, call sessions, queue items, recordings and shared voicemail"),
    ("2", "Agent profiles stay", "What a manager grants travels in the Agent Entitlements step, matched by user name"),
    ("3", "No secrets in a plan", "Phone provider settings never carry a credential"),
]

USES = [
    ("1", "Staging to production", "Build and test on a staging site, then replay the plan"),
    ("2", "Source control", "Keep the exported plan, so a queue change is a reviewed difference"),
    ("3", "A second region", "Start from the first plan, then change only the numbers"),
    ("4", "Put it back", "Replay the last good plan after a mistake"),
]


def slide(step_id, chapter_title, label):
    table = {"why-": (WHY, "Why export the configuration"), "not-": (NOT, "What does not travel"), "use-": (USES, "Plans at work")}
    for prefix, (cards, title) in table.items():
        if step_id.startswith(prefix):
            image = slide_base(chapter_title, label, title)
            number = int(step_id.split("-")[-1])
            bullets(image, cards, active=set(range(number)) if prefix == "why-" else {number - 1})
            return image
    raise SystemExit(f"No slide for {step_id}")
