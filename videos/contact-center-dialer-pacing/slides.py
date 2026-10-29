"""The slides of the Dialer Pacing video."""
from brand import bullets, slide_base

WHY = [
    ("1", "The customer is dialed first", "An agent is connected once a person answers"),
    ("2", "A call can be abandoned", "A person answers and no agent joins within two seconds"),
    ("check", "A profile guards against it", "It measures the rate, can stop dialing, and plays a message"),
]

MEASURED = [
    ("1", "Measured abandonment", "Only calls a person answered"),
    ("2", "For each profile, not each campaign", "Campaigns that share a profile are measured together"),
]

LIVE = [
    ("1", "No agent is reserved while calls ring", "The free agent who waited longest takes the call"),
    ("2", "It starts only when it can trust its numbers", "Until then it dials one call per reserved agent"),
    ("3", "Watch the first days", "Start low, and read the Last pacing decision table"),
]

RECORDS = [
    ("1", "Checks before every call", "Valid, in service, opted in, window open, cap, registry"),
    ("2", "Failed, Completed or Cancelled", "Or tried again in a later cycle"),
    ("3", "Dispositioned by the dialer", "No answer, Busy, Machine, Rejected, Failed, Disconnected"),
]

RETRIES = [
    ("1", "The subject flow decides first", "Try Again and its Maximum attempts, else the profile's"),
    ("2", "No disposition in the flow", "The dialer picks the outcome and retries after the delay"),
    ("3", "The next attempt", "A picked date, the schedule hours, else the Retry delay"),
]

USES = [
    ("1", "A long list and a small team", "Power with one or two calls per agent"),
    ("2", "A campaign under calling rules", "The cap, the message, and Measured abandonment"),
    ("3", "Numbers that were not answered", "Called again on their own, within your limits"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "When no agent is there")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("meas-"):
        image = slide_base(chapter_title, label, "What is measured")
        bullets(image, MEASURED, active={0, 1})
        return image
    if step_id.startswith("live-"):
        image = slide_base(chapter_title, label, "Over-dial in a live campaign")
        bullets(image, LIVE, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("rec-"):
        image = slide_base(chapter_title, label, "Every record, every call")
        bullets(image, RECORDS, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("retry-"):
        image = slide_base(chapter_title, label, "Who decides to call again")
        bullets(image, RETRIES, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where pacing settings help")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
