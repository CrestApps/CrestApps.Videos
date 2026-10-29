"""The slides of the Dispositions video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Every activity ends in some way", "No answer, call back, booked, do not contact"),
    ("2", "Agents and the AI pick the answer", "That answer is a disposition"),
    ("3", "The subject flow decides what next", "Finish, try again later, or a follow-up"),
    ("4", "Reports count them", "How many calls ended in each outcome"),
]

OUTCOMES = [
    ("1", "Outcome: None, or one for the platform", "Set it only on dispositions the platform applies by itself"),
    ("2", "Number not in service, No answer, Busy", "A dead number is marked; unanswered calls are closed"),
    ("3", "Answering machine, Rejected, Call failed", "And Disconnected, when the customer hung up early"),
    ("4", "Applied before an agent is connected", "So the agent never sees those calls"),
]

WHICH = [
    ("1", "The subject flow's disposition first", "So the flow's actions run, such as Try Again"),
    ("2", "Then fallbacks, then one is created", "A created disposition is in no flow: add it to call again"),
    ("3", "On the Complete page only with an action", "Set do not call is an action of the subject flow"),
]

USES = [
    ("1", "A test-drive campaign", "Test Drive Booked, Call Back Later, Not Interested"),
    ("2", "Dead numbers marked for you", "Give a disposition the Number not in service outcome"),
    ("3", "Lead stages kept up to date", "A disposition can set the lead status"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Every activity ends with an answer")
        bullets(image, WHY, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("out-"):
        image = slide_base(chapter_title, label, "Outcomes the platform applies")
        bullets(image, OUTCOMES, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("which-"):
        image = slide_base(chapter_title, label, "Which disposition is used")
        bullets(image, WHICH, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "How teams use dispositions")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
