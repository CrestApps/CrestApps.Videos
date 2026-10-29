"""The slides of the Dashboard Diagnostics video."""
from brand import bullets, slide_base

BANNER = [
    ("1", "Dialer idle", "Work is due, and no agent is available"),
    ("2", "Dialer holding back", "The abandonment policy, a compliance check or calls in flight"),
    ("3", "Dialer stalled", "Due work is waiting, and no cycle completed for several minutes"),
    ("4", "Dialer not pacing", "The pacing lock was not acquired, so every cycle skips itself"),
]

REASONS = [
    ("1", "Scheduled for later, Waiting out the retry delay", "Not due yet"),
    ("2", "Out of attempts, No number to dial, Number not in service", "The record cannot be dialed"),
    ("3", "Do not call, On the national do-not-call registry", "The contact must not be called"),
    ("4", "Outside the calling window, Abandonment rate cap reached", "Dialing is paused for now"),
]

SETUP = [
    ("1", "The queue is turned off", "Or its business hours are closed"),
    ("2", "The dialer profile is turned off", "Or it no longer exists"),
    ("3", "Not in the queue", "So routing never sees it"),
    ("4", "Assigned to a user for manual work", "The dialer leaves it alone"),
]

QUALITY = [
    ("1", "Three of the last five calls were poor", "An alert goes to the supervisors of the agent's queues"),
    ("2", "The likely cause", "No audio, packet loss, jitter, latency, the customers' lines"),
    ("3", "Only while the dashboard is open", "Dismiss hides it in your browser only"),
]

FREE = [
    ("1", "A warning badge on the agent card", "The agent holds work that nothing backs"),
    ("2", "More, then Free agent", "The panel lists what is held, and asks you to confirm"),
    ("3", "A live call is never ended", "If the provider confirms the call is alive, nothing is released"),
    ("4", "Needs a permission, and is audited", "To take over, end, transfer and record live calls"),
]

WHERE = [
    ("1", "The banner", "The state of the dialer, in one line"),
    ("2", "Why isn't this dialing", "The reason for each queue and campaign"),
    ("3", "The Dialer decision log", "Every decision, by record or by agent"),
]

USES = [
    ("1", "At the start of a shift", "Look at the banner, and at the panel if it is amber"),
    ("2", "When agents report poor audio", "The alert's cause points at their network or the provider"),
    ("3", "When an agent gets no calls", "Look for the badge, and use Free agent"),
]


def slide(step_id, chapter_title, label):
    if step_id == "bst-1":
        image = slide_base(chapter_title, label, "Amber: attention")
        bullets(image, BANNER, active={0, 1})
        return image
    if step_id == "bst-2":
        image = slide_base(chapter_title, label, "Red: act")
        bullets(image, BANNER, active={2, 3})
        return image
    if step_id == "rsn-1":
        image = slide_base(chapter_title, label, "Why records are held back")
        bullets(image, REASONS)
        return image
    if step_id == "rsn-2":
        image = slide_base(chapter_title, label, "Reasons that point at setup")
        bullets(image, SETUP)
        return image
    if step_id.startswith("cq-"):
        image = slide_base(chapter_title, label, "Call quality alerts")
        bullets(image, QUALITY, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("fr-"):
        image = slide_base(chapter_title, label, "Free agent")
        n = int(step_id[-1])
        bullets(image, FREE, active={0} if n == 1 else {1, 2} if n == 2 else {3})
        return image
    if step_id.startswith("whr-"):
        image = slide_base(chapter_title, label, "Where to look")
        bullets(image, WHERE)
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where it helps")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
