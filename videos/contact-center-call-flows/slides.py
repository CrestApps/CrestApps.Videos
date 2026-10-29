"""The slides of the Call Flows video."""
from brand import bullets, slide_base

INBOUND = [
    ("1", "The recording notice, the welcome, the menu", "Spoken in that order, on the entry point"),
    ("2", "The queue", "The queue-full check, then the treatment while the caller waits"),
    ("3", "The offer", "The agent accepts, then can hold, transfer or conference"),
    ("4", "Wrap-up and the disposition", "Until the activity is completed, or fifteen minutes pass"),
]

CHECKS = [
    ("1", "The number", "Valid, and not known to be out of service"),
    ("2", "Attempts", "The record has attempts left, and the retry delay has passed"),
    ("3", "The contact", "Has not opted out, and the calling window is open"),
    ("4", "The rules", "The abandonment cap allows dialing, and no national registry lists the number"),
]

OUTCOMES = [
    ("1", "No answer, busy, answering machine", "Recorded by the dialer, and retried"),
    ("2", "Disconnected before the agent joined", "Recorded by the dialer, and retried"),
    ("3", "Rejected or failed", "Recorded, and not retried"),
    ("4", "Number not in service", "Recorded, and never dialed again"),
]

CALLBACK = [
    ("1", "The caller presses the callback key", "They leave the queue, and a task creates the callback within a minute"),
    ("2", "Queued again at the queue's default priority", "An agent is offered it like a preview call"),
    ("3", "The platform dials the customer", "After the agent accepts"),
    ("4", "A built-in preview profile", "Skips do-not-call and the calling window: the customer asked"),
]

USES = [
    ("1", "Walk your flow from the top", "Notice, welcome, menu, queue, then the limits"),
    ("2", "Offer callbacks on a busy queue", "To cut the abandon rate"),
    ("3", "Give every outcome a next step", "In the dispositions of the subject flow"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("inb-"):
        image = slide_base(chapter_title, label, "The life of an inbound call")
        n = int(step_id[-1])
        bullets(image, INBOUND, active={0} if n == 1 else {1, 2} if n == 2 else {2, 3})
        return image
    if step_id in ("out-1", "out-2"):
        image = slide_base(chapter_title, label, "Checked before an outbound call")
        bullets(image, CHECKS, active={0, 1} if step_id == "out-1" else {2, 3})
        return image
    if step_id == "out-3":
        image = slide_base(chapter_title, label, "A call that ends early")
        bullets(image, OUTCOMES)
        return image
    if step_id.startswith("cb-"):
        image = slide_base(chapter_title, label, "The life of a callback")
        n = int(step_id[-1])
        bullets(image, CALLBACK, active={0} if n == 1 else {1, 2} if n == 2 else {3})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where it helps")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
