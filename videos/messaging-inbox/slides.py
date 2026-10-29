"""The slides of the Messaging Inbox video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Customers text like friends", "They expect an answer soon"),
    ("2", "One phone is a single point of failure", "Missed when that person is away, and nobody else sees the thread"),
    ("check", "A shared inbox", "Any agent claims a text, and the history stays with the conversation"),
]

HOW = [
    ("1", "The list", "Customers, newest first, with unread badges"),
    ("2", "The conversation", "The thread, and claim, transfer, close, mark spam"),
    ("3", "The composer", "Message box, attach button, channel"),
    ("4", "The Customer card", "Who it is, and the contact records that match"),
]

USES = [
    ("1", "A repair shop", "Tell customers a bike is ready"),
    ("2", "A support team", "One number, whoever is free claims the next text"),
    ("3", "A sales team", "Favorites, and starting the conversation"),
    ("4", "A manager", "The Unassigned view shows what nobody has taken"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Texts need a team")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Four areas of one page")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Ways to use it")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
