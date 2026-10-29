"""The slides of the Routing Explained video."""
from brand import bullets, slide_base

CHAIN = [
    ("1", "Who may take a call", "Available, entitled to the queue, signed in to it, with a live session"),
    ("2", "Skills and capacity", "Filters: they remove agents who lack a skill, or who are full"),
    ("3", "The routing strategy", "Longest idle, Round robin or Least busy ranks the rest"),
    ("4", "History and bonuses", "A sticky agent moves up, agents who declined this call move back"),
]

CAPACITY = [
    ("1", "Capacity is one by default", "How many interactions an agent holds at once"),
    ("2", "No field on screen", "It comes from a recipe or a deployment"),
    ("3", "One voice call at a time", "An agent on a call is never offered another call"),
]

AUTO = [
    ("1", "The entry point's priority", "Where a call starts, instead of the queue's default"),
    ("2", "Rings again within four hours", "Raised to High"),
    ("3", "Returns a callback within 24 hours", "Raised to Highest"),
    ("4", "Only raised, never lowered", "Straight-to-queue calls only: welcome, notice or menu use the entry point's priority"),
]

OFFERS = [
    ("1", "Decline or ring out", "The call returns to the queue, and the agent goes to the back of the idle line"),
    ("2", "First: agents who have not had the call", "Then earlier decliners, the earliest first"),
    ("3", "The only agent free", "Is offered the call again"),
]

USES = [
    ("1", "Skilled agents for complex calls", "Relax after lets the requirement lapse, so nobody waits forever"),
    ("2", "A priority line for key customers", "The entry point's priority, with aging for everyone else"),
    ("3", "Spread or reward", "Round robin spreads work, Longest idle rewards the longest wait"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("chain-"):
        image = slide_base(chapter_title, label, "How a waiting call finds an agent")
        n = int(step_id[-1])
        bullets(image, CHAIN, active={0} if n == 1 else {1, 2} if n == 2 else {3})
        return image
    if step_id == "cap-1":
        image = slide_base(chapter_title, label, "Agent capacity")
        bullets(image, CAPACITY)
        return image
    if step_id == "pri-2":
        image = slide_base(chapter_title, label, "Automatic priority")
        bullets(image, AUTO)
        return image
    if step_id.startswith("off-"):
        image = slide_base(chapter_title, label, "After a declined or missed offer")
        bullets(image, OFFERS, active={0} if step_id == "off-1" else {1, 2})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where it helps")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
