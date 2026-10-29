"""The slides of the Call Recordings video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Notes are not enough", "Complaints and coaching need what was really said"),
    ("2", "Recordings are personal data", "Who may listen has to be controlled"),
    ("3", "AI calls need a transcript", "Read it, and jump to the moment you care about"),
    ("check", "One page for every recorded call", "Search, play, transcript and erase"),
]

WHO = [
    ("1", "Listen to own call recordings", "Only the calls you were the agent on: the Agent role"),
    ("2", "Listen to anyone's call recordings", "Every recorded call, and search by agent: the Supervisor role"),
]

WHICH = [
    ("1", "The feature and the setting", "Call Recording is on, and Record every call automatically is checked"),
    ("2", "Queue, dialer and AI voice calls", "From when an agent is connected, or the call is answered"),
    ("3", "Soft phone keypad calls", "From when the other side answers; extension calls are not recorded"),
    ("4", "Listed once saved", "It can take a minute after the call ends"),
]

CALL = [
    ("1", "Play and View", "Play in the list, one at a time; View opens the call page"),
    ("2", "The call page", "Who was on the call, the player, and the transcript"),
    ("3", "Transcripts", "For AI voice agent calls; calls between people are not transcribed"),
    ("4", "One line of the transcript", "Time, copy, play, silence, speaker and the words"),
    ("check", "Times are close, not exact", "Lines before a handoff show dashes and have no play button"),
]

ERASE = [
    ("1", "Delete call recordings", "A permission only Administrator has by default"),
    ("2", "Write the reason, click Erase", "For example, a customer asked for their data to be deleted"),
    ("3", "Gone for good", "A recording under legal hold cannot be erased"),
]

USES = [
    ("1", "Coaching an agent", "Search their calls for the week and play a few"),
    ("2", "Handling a complaint", "Search by the customer's number and read the moment"),
    ("3", "Reviewing an AI voice agent", "Read the transcripts and jump to where a call went wrong"),
    ("4", "A data deletion request", "Erase the recording, with the reason written on it"),
]


def slide(step_id, chapter_title, label):
    table = {"why-": (WHY, "What was said, in the caller's own words"), "who-": (WHO, "Two permissions"),
             "which-": (WHICH, "Which calls are recorded"), "call-": (CALL, "Play a call and read the transcript"),
             "erase-": (ERASE, "Erase a recording"), "use-": (USES, "What the page is used for")}
    for prefix, (cards, title) in table.items():
        if step_id.startswith(prefix):
            image = slide_base(chapter_title, label, title)
            number = int(step_id.split("-")[-1])
            bullets(image, cards, active=set(range(number)) if prefix == "why-" else {number - 1})
            return image
    raise SystemExit(f"No slide for {step_id}")
