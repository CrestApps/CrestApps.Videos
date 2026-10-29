"""The slides of the Asterisk Setup video."""
from brand import bullets, slide_base

NEEDS = [
    ("1", "Call control", "The platform runs Asterisk through its ARI web interface: an address, a user name and a password"),
    ("2", "Audio in the browser", "A secure web socket address, a domain, ICE servers and a database of temporary browser logins"),
]

CAN = [
    ("1", "What agents can do", "Dial, hang up, answer, reject, hold, mute, blind transfer, merge, send keypad digits; recording and supervisor listen, whisper and barge"),
    ("2", "What needs Telnyx", "AI voice calls: Asterisk cannot place or answer them"),
]

# step-id prefix -> (cards, heading, how the cards light up: "all" up to the step number, "one" the card of the step, or a fixed set per step id)
TABLE = {"needs-": (NEEDS, "What Asterisk needs", "all"), "can-": (CAN, "What Asterisk can do", "one")}


def slide(step_id, chapter_title, label):
    for prefix, (cards, heading, active) in TABLE.items():
        if step_id.startswith(prefix):
            image = slide_base(chapter_title, label, heading)
            number = int(step_id.split("-")[-1])
            lit = active.get(step_id) if isinstance(active, dict) else (set(range(number)) if active == "all" else {number - 1})
            bullets(image, cards, active=lit)
            return image
    raise SystemExit(f"No slide for {step_id}")
