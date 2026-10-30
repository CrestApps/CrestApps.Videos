"""The slides of the Messaging: Routing and Transfers video."""
from brand import bullets, slide_base

WHY = [
    ("1", "A shared list can mean no one", "Everyone sees the text, so no one feels it is theirs"),
    ("2", "A busy team wants a free person", "And a way on when that person does not pick it up"),
    ("3", "Some questions belong elsewhere", "Move the conversation, not the customer"),
]

HOW = [
    ("1", "Entry point", "Sends a number's texts to an agent or a queue"),
    ("2", "Shared pool", "Every agent of the queue sees it, the first to claim owns it"),
    ("3", "Routed", "Pushed to one available agent with the fewest conversations"),
    ("4", "Never lost", "After five minutes it moves on, then falls back to the pool"),
]

USES = [
    ("1", "A busy support team", "Routed distribution spreads the load"),
    ("2", "A hand-over with a note", "The customer does not repeat the question"),
    ("3", "Stepping away", "Turn Available off, send the rest back to the queue"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "The right person, without a scramble")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "From the number to the agent")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Ways to use it")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
