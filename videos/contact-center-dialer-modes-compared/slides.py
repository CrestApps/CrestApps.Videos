"""The slides of the Dialer Modes Compared video."""
from brand import bullets, slide_base

QUESTIONS = [
    ("1", "Who starts the call?", "An agent presses Dial, or the system dials"),
    ("2", "Is an agent held first?", "Offered or reserved before the customer's phone rings"),
    ("3", "What can go wrong?", "Abandoned calls, expired offers and lost time"),
]

NOT_MODES = [
    ("1", "Manual", "An older profile saved as Manual is shown as Preview"),
    ("2", "AI voice agent", "It places calls on its own, and has its own video"),
]

PREVIEW = [
    ("1", "The record is offered", "To an agent signed in to the campaign, with a countdown"),
    ("2", "The agent reviews it", "Before any call exists"),
    ("3", "Dial or Skip", "Dial places the call after the compliance checks"),
    ("4", "The agent is on the call", "From the first ring"),
]

PREVIEW_RISK = [
    ("1", "The offer runs out", "The record goes to another agent"),
    ("2", "The call ends unconnected", "The dialer records the outcome, the agent returns to Ready"),
    ("check", "No abandoned calls", "The agent places the call and listens to it"),
]

POWER = [
    ("1", "An agent is available", "Signed in to the campaign, status Available"),
    ("2", "The agent is reserved", "Held for the call, and waiting for a connected call"),
    ("3", "The dialer rings the customer", "One reserved agent for every call"),
    ("4", "A person answers", "The agent is joined and the activity opens on its own"),
]

POWER_RISK = [
    ("1", "An abandoned call", "A person answers and no agent is joined within two seconds"),
    ("2", "The abandoned call message", "Played instead of silence"),
    ("3", "No answer, busy or voicemail", "The dialer records it, the agent returns to Available"),
]

PRED_RISK = [
    ("1", "Nobody is free", "A person answers, and no agent can take the call"),
    ("2", "The call is abandoned", "The person hears the abandoned call message"),
    ("3", "The cap and the message are required", "Over-dial cannot be saved without them"),
    ("check", "A safe fallback", "One call per reserved agent until it can trust its numbers"),
]

WHO = [
    ("1", "Preview", "The agent"),
    ("2", "Power", "Nobody: the system dials"),
    ("3", "Progressive", "Nobody: the system dials"),
    ("4", "Predictive", "Nobody: the system dials"),
]

HELD = [
    ("1", "Preview", "Yes: the agent is offered the record"),
    ("2", "Power", "Yes: reserved for each call"),
    ("3", "Progressive", "Yes: reserved for each call"),
    ("4", "Predictive", "Yes, except with Over-dial"),
]

ABANDON = [
    ("1", "Preview", "No: the agent places the call and listens"),
    ("2", "Power", "Yes, if the agent is not joined in time"),
    ("3", "Progressive", "Yes, in the same way"),
    ("4", "Predictive", "Yes, and by design with Over-dial"),
]

FEATURE = [
    ("1", "Preview", "Contact Center Outbound Dialer"),
    ("2", "Power", "Contact Center Paced Dialing"),
    ("3", "Progressive", "Contact Center Paced Dialing"),
    ("4", "Predictive", "Contact Center Paced Dialing"),
]

CHOOSE = [
    ("1", "Start with Preview", "A new team, a new list, or a high value list"),
    ("2", "Move to Progressive or Power", "A steady list, and agents who wait between calls"),
    ("3", "Predictive when you need the brake", "Over-dial only when it is measured and allowed"),
    ("4", "One profile for each campaign", "So each campaign's abandonment rate is its own"),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Three questions for every mode")
        bullets(image, QUESTIONS)
        return image
    if step_id == "why-2":
        image = slide_base(chapter_title, label, "Two things that are not dialer modes")
        bullets(image, NOT_MODES)
        return image
    if step_id in ("prev-1", "prev-2"):
        image = slide_base(chapter_title, label, "Preview: the agent decides")
        bullets(image, PREVIEW, active={0, 1, 2} if step_id == "prev-1" else {2, 3})
        return image
    if step_id == "prev-4":
        image = slide_base(chapter_title, label, "Preview: what can go wrong")
        bullets(image, PREVIEW_RISK)
        return image
    if step_id in ("pow-1", "pow-2"):
        image = slide_base(chapter_title, label, "Power: the system dials for a reserved agent")
        bullets(image, POWER, active={0, 1, 2} if step_id == "pow-1" else {1, 3})
        return image
    if step_id in ("pow-5", "pow-6"):
        image = slide_base(chapter_title, label, "Power and Progressive: what can go wrong")
        bullets(image, POWER_RISK, active={0, 1} if step_id == "pow-5" else {2})
        return image
    if step_id == "pred-4":
        image = slide_base(chapter_title, label, "Predictive: what can go wrong")
        bullets(image, PRED_RISK)
        return image
    if step_id == "cmp-1":
        image = slide_base(chapter_title, label, "Who presses Dial?")
        bullets(image, WHO)
        return image
    if step_id == "cmp-2":
        image = slide_base(chapter_title, label, "Is an agent held before the phone rings?")
        bullets(image, HELD)
        return image
    if step_id == "cmp-3":
        image = slide_base(chapter_title, label, "Can a call be abandoned?")
        bullets(image, ABANDON)
        return image
    if step_id == "cmp-4":
        image = slide_base(chapter_title, label, "Which feature offers the mode?")
        bullets(image, FEATURE)
        return image
    if step_id.startswith("ch-"):
        image = slide_base(chapter_title, label, "A path to follow")
        bullets(image, CHOOSE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
