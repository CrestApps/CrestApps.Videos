"""The slides of the AI Outbound Calls video."""
from brand import bullets, slide_base

WHY = [
    ("1", "An automated activity", "A contact, a subject and an AI profile: no dialer profile"),
    ("2", "Telnyx only", "The Asterisk provider cannot place or answer AI calls"),
    ("3", "The first call can wait", "A background task starts due activities every five minutes"),
    ("4", "A workflow starts it at once", "And skips the checks before the call"),
]

SCREEN = [
    ("1", "Number not in service", "The activity is cancelled"),
    ("2", "The contact opted out of calls", "The activity is cancelled, with the reason in its notes"),
    ("3", "Do-not-call lists", "Checked when Contact Center Outbound Dialer is on"),
    ("4", "Calling window", "Checked only when your administrator turned it on"),
]

DURING = [
    ("1", "The greeting", "The opening message, after the recording notice if there is one"),
    ("2", "Voicemail", "It waits for the greeting to end and leaves one message"),
    ("3", "Silence", "It asks twice if the person is there, then says goodbye"),
    ("4", "Handoff", "To the handoff queue, with an AI assistant's summary for the agent"),
]

REVIEW = [
    ("1", "Cancelled by screening", "The call is never placed"),
    ("2", "The reason is kept", "In the notes of the activity"),
    ("3", "Nobody is bothered", "An opted-out contact or a dead number is never dialed"),
]

USES = [
    ("1", "Confirm appointments", "Call each customer, note cancellations, transfer changes"),
    ("2", "Qualify leads", "Learn what they need and when, transfer the good fits"),
    ("3", "Follow up at the right time", "Schedule the load so calls go out when it suits contacts"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "An AI call is not a dialer mode")
        bullets(image, WHY, active={0} if step_id == "why-1" else {1} if step_id == "why-2" else {2, 3})
        return image
    if step_id.startswith("scr-"):
        image = slide_base(chapter_title, label, "Checks before each call")
        bullets(image, SCREEN, active={0, 1} if step_id == "scr-1" else {2, 3} if step_id == "scr-2" else {0, 1, 2, 3})
        return image
    if step_id.startswith("dur-"):
        image = slide_base(chapter_title, label, "What the agent does")
        bullets(image, DURING, active={int(step_id[-1]) - 1})
        return image
    if step_id == "rev-3":
        image = slide_base(chapter_title, label, "When screening cancels an activity")
        bullets(image, REVIEW)
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where it fits")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
