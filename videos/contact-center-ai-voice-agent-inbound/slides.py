"""The slides of the AI Voice Agent for Inbound Calls video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Route to: AI voice agent", "An entry point can send a number to the agent"),
    ("2", "No welcome, menu or queue", "The caller talks to the agent at once"),
    ("3", "Telnyx only", "A call on Asterisk is rejected: it cannot be answered"),
    ("4", "An automated activity", "The call is logged, so you can review it afterwards"),
]

DURING = [
    ("1", "The recording notice, then the greeting", "Turn-based: the opening message. Live: the model's own words"),
    ("2", "A silent caller", "Never taken for a voicemail: the agent asks, then ends politely"),
    ("3", "Handoff to a person", "The agent sees an AI assistant's summary of the call"),
]

USES = [
    ("1", "A front desk that never misses a call", "Answers, takes messages, transfers when someone is free"),
    ("2", "A line for busy seasons", "The wait does not depend on how many agents are signed in"),
    ("3", "After-hours triage", "Collect the reason, and follow up in the morning"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "An entry point that answers with an AI agent")
        bullets(image, WHY, active={0, 1} if step_id == "why-1" else {2} if step_id == "why-2" else {3, 0})
        return image
    if step_id.startswith("dur-"):
        image = slide_base(chapter_title, label, "What callers experience")
        bullets(image, DURING, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where it fits")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
