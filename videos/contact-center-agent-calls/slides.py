"""The slides of the Offers, calls and wrap-up video."""
from brand import bullets, slide_base

ABOUT = [
    ("1", "A live call is needed", "Offers, calls and wrap-up appear only with a connected phone provider and a real caller"),
    ("2", "This site never places a call", "Its providers are unreachable on purpose, so slides describe those screens"),
]

OFFER = [
    ("1", "The offer", "My workspace and the agent bar show the caller, the queue and a countdown"),
    ("2", "Accept", "Answering while the call connects, then the call moves to Active interaction"),
    ("3", "Decline or ignore", "The caller returns to the queue; the unanswered offer action applies after the countdown"),
    ("4", "Phone unreachable", "Your state becomes Away with that reason; set yourself Available again"),
]

DIALER = [
    ("1", "Preview", "Review the record, then click Dial; or Skip"),
    ("2", "Power and progressive", "Nothing to press: the dialer calls, and Complete activity opens as the call connects"),
    ("3", "Reserved, then On a call", "No answer, or a machine: back to Available with nothing to complete"),
]

SECURE = [
    ("1", "Pause recording", "While the customer reads card details; Resume recording as soon as they finish"),
    ("2", "Collect data securely", "A one-time link; only a masked value is kept"),
    ("3", "Nobody listens in", "Supervisors cannot listen while paused, and every pause is audited"),
]

WRAP = [
    ("1", "Wrap-up", "It does not end on a timer; completing the activity ends it"),
    ("2", "Complete activity", "Check the details, pick a disposition, add notes and save"),
    ("3", "A safety net", "An open activity ends wrap-up after fifteen minutes, with no disposition"),
]

USES = [
    ("1", "A support call", "Accept, help, then record the outcome in wrap-up"),
    ("2", "A preview campaign", "Read the record before pressing Dial"),
    ("3", "A card payment", "Pause the recording while the customer reads the number"),
    ("4", "Leaving early", "Decline, and another agent takes the call"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("abt-"):
        image = slide_base(chapter_title, label, "What is shown, and what is described")
        bullets(image, ABOUT, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("off-"):
        image = slide_base(chapter_title, label, "An offer arrives")
        bullets(image, OFFER, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("dlr-"):
        image = slide_base(chapter_title, label, "Calls from a campaign")
        bullets(image, DIALER, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("sec-"):
        image = slide_base(chapter_title, label, "Card details on a recorded call")
        bullets(image, SECURE, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("wrp-"):
        image = slide_base(chapter_title, label, "Finish your notes")
        bullets(image, WRAP, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where this flow helps")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
