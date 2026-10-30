"""The slides of the Omnichannel AI Handoff video."""
from brand import bullets, slide_base

WHY = [
    ("1", "The AI does the first questions", "Short, patient, around the clock"),
    ("2", "A person takes over", "Through a queue, with what the AI learned"),
    ("check", "Set on the subject", "Every conversation of it follows the same rules"),
]

WHEN = [
    ("1", "The customer asks for a human", "Always the safest case to tick"),
    ("2", "A qualified, ready lead", "Hand the sale to a person"),
    ("3", "Frustrated, or the AI cannot help", "Stop the AI before it makes it worse"),
]

CALLS = [
    ("1", "A free agent", "The call rings the agents of the queue"),
    ("2", "Nobody free", "The caller is told they are in the queue and holds"),
    ("3", "The queue is closed", "A callback is booked and the call ends"),
]

SUMMARY = [
    ("1", "The AI assistant's summary", "A banner on the Complete page, not part of the notes"),
    ("2", "Review AI conversation", "The full transcript, read-only"),
]

USE = [
    ("1", "A sales line", "A person takes over when the customer is ready"),
    ("2", "Frustrated customers", "Handed to a person at once"),
    ("3", "After-hours calls", "A callback instead of a lost call"),
    ("4", "One queue per team", "Each subject hands to the right one"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "The AI starts, a person finishes")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("when-"):
        image = slide_base(chapter_title, label, "Three cases the AI can hand off")
        bullets(image, WHEN, active={int(step_id[-1]) - 1} if step_id[-1] != "3" else {2})
        return image
    if step_id.startswith("calls-"):
        image = slide_base(chapter_title, label, "What a call experiences")
        bullets(image, CALLS, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("summary-"):
        image = slide_base(chapter_title, label, "What the agent reads")
        bullets(image, SUMMARY, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four typical uses")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
