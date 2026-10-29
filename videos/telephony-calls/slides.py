"""The slides of the Placing and handling calls video."""
from brand import bullets, slide_base

ABOUT = [
    ("1", "A phone with every control", "Dial, answer, hold, transfer and conference"),
    ("2", "No connected provider here", "The keypad and extension dialing are shown; call-time controls are described on slides"),
]

RING = [
    ("1", "Ringback tone", "The phone plays it while the number rings; it stops when the other person answers"),
    ("2", "Refused numbers", "Emergency, premium-rate and short numbers your administrator has not allowed"),
]

ANSWER = [
    ("1", "Incoming call window", "Shows the caller, the queue or who transferred the call, and matched records"),
    ("2", "Answer, Voicemail, Ignore", "Answer and open also opens a matched record"),
    ("3", "A queue call is an offer first", "With a countdown, in My workspace and the agent bar"),
]

DURING = [
    ("1", "Mute, Hold, Keypad", "Mute the microphone, hold the caller, send key presses"),
    ("2", "Hang up and End all", "Hang up is the only red button; End all asks you to confirm"),
    ("3", "Add call", "Holds this call and opens the keypad; Back to call returns"),
]

TRANSFER = [
    ("1", "Blind or Warm", "Straight to them, or speak to them first"),
    ("2", "Directory, number or extension", "A Contact Center call lists Agents, Queues and Outside numbers"),
    ("3", "Complete or Cancel transfer", "No answer in thirty seconds: the caller comes back to you"),
    ("4", "Depends on your phone system", "Asterisk: blind. Telnyx: blind, warm and conferences"),
]

CONFERENCE = [
    ("1", "Merge calls", "Tick two or more calls in Active calls; each participant has a hang-up button"),
    ("2", "Leave or End for all", "Leave keeps the others connected; End for all asks you to confirm"),
    ("3", "Then wrap-up", "Queue and campaign calls end in wrap-up; direct and dialed calls skip it"),
]

USES = [
    ("1", "Call a customer back", "From the Recent tab"),
    ("2", "Ask a colleague for help", "Type their name in Dial extension"),
    ("3", "A warm transfer", "The customer does not repeat the story"),
    ("4", "A conference", "A customer and a manager on one call"),
]


def slide(step_id, chapter_title, label):
    n = int(step_id[-1]) - 1
    table = {
        "abt-": ("What is shown, and what is described", ABOUT),
        "rng-": ("Ringing, and refused numbers", RING),
        "ans-": ("The Incoming call window", ANSWER),
        "dur-": ("During a call", DURING),
        "trf-": ("Transfer a call", TRANSFER),
        "cnf-": ("Conference calls", CONFERENCE),
        "use-": ("Where these controls help", USES),
    }
    for prefix, (title, items) in table.items():
        if step_id.startswith(prefix):
            image = slide_base(chapter_title, label, title)
            bullets(image, items, active={n})
            return image
    raise SystemExit(f"No slide for {step_id}")
