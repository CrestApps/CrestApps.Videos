"""The slides of the Omnichannel Workflows video."""
from brand import bullets, slide_base

MOMENTS = [
    ("1", "A call nobody answered", "Someone has to text or call the customer back"),
    ("2", "A dialer attempt that rang out", "The contact needs another try, or a message"),
    ("3", "A lead that qualifies", "It should become a contact, with an account and a deal"),
    ("4", "By hand, it is slow", "People forget, and it depends on who is on shift"),
]

PARTS = [
    ("1", "An event starts it", "A call connects, an attempt completes, a lead converts"),
    ("2", "Tasks do the work", "Send a text, schedule a callback, queue an activity, hand off"),
    ("3", "Outcomes decide what is next", "Done, Failed and the others each connect to the next task"),
    ("4", "A canvas, not code", "You drag and connect the steps, and the platform runs them every time"),
]

VALUES = [
    ("1", "The kind of event", "For example a call connecting, and which call or conversation it is about"),
    ("2", "Who acted, and which agent", "An agent, a supervisor, a workflow, the provider or the platform"),
    ("3", "The event's own data", "The number the dialer called, and how the attempt ended"),
    ("4", "Read by Liquid fields", "Any task field that accepts Liquid can read them; the hint shows an example"),
]

OUTCOMES = {
    "out-1": [
        ("1", "Place Call or Send Message", "Done, Already Started, Failed: it never calls a customer twice"),
        ("2", "Contact Center Event", "Matched, Ignored"),
        ("3", "Lead Converted", "Done"),
    ],
    "out-2": [
        ("1", "Most tasks", "Done, Failed: Set Agent Presence, Enqueue Activity, Schedule Callback, Send Text Message"),
        ("2", "Find Agent Numbers", "Done, NotFound"),
        ("3", "Hand Off to Live Agent", "Connected, Waiting In Queue, Callback Scheduled, Failed"),
    ],
    "out-3": [
        ("1", "Schedule Dialer Retry", "Scheduled, Exhausted when no attempt is left, Failed"),
        ("2", "Start and Stop Call Recording", "Done, Failed, Indeterminate"),
        ("3", "Indeterminate", "The provider may have changed it, but the platform could not confirm"),
        ("4", "Convert Lead", "Converted, Failed"),
    ],
}

LIMITS = [
    ("1", "Transfer a live call", "Stays an action people take on the call"),
    ("2", "Pick a particular agent", "Routing decides, from presence, skills and entitlements"),
    ("3", "Use Enqueue Activity", "Put the work on a queue and let routing choose"),
]

USES = [
    ("1", "Record every call", "Call connected, then Start Call Recording"),
    ("2", "Follow up on a missed call", "Dialer attempt completed, then Send Text Message or Schedule Dialer Retry"),
    ("3", "Escalate an automated conversation", "Hand Off to Live Agent, or Schedule Callback after hours"),
    ("4", "Welcome a new customer", "Convert Lead, then Lead Converted starts the welcome"),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Moments that need follow-up")
        bullets(image, MOMENTS)
        return image
    if step_id == "why-2":
        image = slide_base(chapter_title, label, "How a workflow is put together")
        bullets(image, PARTS)
        return image
    if step_id == "val-1":
        image = slide_base(chapter_title, label, "What the event tells the workflow")
        bullets(image, VALUES)
        return image
    if step_id in OUTCOMES:
        image = slide_base(chapter_title, label, "How each task can end")
        bullets(image, OUTCOMES[step_id])
        return image
    if step_id in ("use-1", "use-2"):
        image = slide_base(chapter_title, label, "Workflows to start from")
        bullets(image, USES, active={0, 1} if step_id == "use-1" else {2, 3})
        return image
    if step_id == "use-3":
        image = slide_base(chapter_title, label, "What stays with people")
        bullets(image, LIMITS)
        return image
    raise SystemExit(f"No slide for {step_id}")
