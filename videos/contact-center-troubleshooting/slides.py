"""The slides of the Contact Center Troubleshooting video."""
from brand import bullets, slide_base

USE = [
    ("1", "Find the symptom", "Then work through its checks in order"),
    ("check", "Three usual causes", "A missing permission, a missing setup record, a phone not signed in"),
]

MOMENT = [
    ("1", "Buttons that need the right moment", "Listen, Whisper, Barge, Complete activity"),
    ("2", "The Agent role has no soft phone", "Ask for the permission to use the telephony soft phone"),
]

LISTS = [
    ("1", "An empty activity list", "Only manual, assigned, not started activities are listed"),
    ("2", "An empty report", "The date range starts on today: widen it, clear the filters"),
]

RING = [
    ("1", "The app, your sign-in, Available", "The Windows app keeps running in the tray, not after Quit"),
    ("2", "Queue open, number routed, skills", "Routing pauses while the queue's calendar is closed"),
    ("3", "A missed offer, or Phone unreachable", "Check the soft phone, then set yourself Available"),
    ("4", "Nothing rings for anyone", "Ask your administrator: the Telnyx Webhook public key"),
]

AUDIO = [
    ("1", "Microphone, devices, mute and hold", "Retry microphone, and the headset button"),
    ("2", "Poor connection", "Wi-Fi, a wired connection, another Region"),
    ("3", "Silent for everyone", "Ask your administrator: often a network or firewall setting"),
]

TEXTS = [
    ("1", "Texts not sent or received", "The number needs Text messages ticked, and opt-outs"),
    ("2", "Missing from your inbox", "The Unassigned filter, and the Available switch"),
    ("3", "The AI does not answer", "The entry point's Route to, the profile, the feature"),
    ("4", "An automatic load stays silent", "The load status, a few minutes, and opt-outs"),
]

DIALER = [
    ("1", "Agents and the profile", "Signed in to the campaign, Available, profile enabled"),
    ("2", "Window, cap and records", "Calendar open, cap not reached, records left"),
]

QUICK = [
    ("1", "Stuck in wrap-up", "Pick a disposition and click Complete"),
    ("2", "Cannot find a contact", "Search, fewer digits, leads are apart, skipped rows"),
    ("3", "A load created less", "Read the load's report, and create a new load"),
]

STUCK = [
    ("1", "Write it down", "What you did, what you expected, what happened, and when"),
    ("check", "Send it to your administrator", "IT has the runbooks and the production support page"),
]

# step id -> (heading, cards, active index or indexes)
MAP = {
    "use-1": ("How to use the page", USE, {0}), "use-2": ("How to use the page", USE, {1}),
    "moment-1": ("At the right moment", MOMENT, {0, 1}),
    "list-1": ("Empty pages", LISTS, {0}), "list-2": ("Empty pages", LISTS, {1}),
    "ring-2": ("My phone does not ring", RING, {0}), "ring-3": ("My phone does not ring", RING, {1}),
    "ring-4": ("My phone does not ring", RING, {2, 3}),
    "audio-1": ("No audio, or poor audio", AUDIO, {0}), "audio-3": ("No audio, or poor audio", AUDIO, {1, 2}),
    "text-1": ("Texts and the AI", TEXTS, {0, 1}), "text-3": ("Texts and the AI", TEXTS, {2, 3}),
    "dialer-2": ("The dialer is not calling", DIALER, {0}), "dialer-3": ("The dialer is not calling", DIALER, {1}),
    "quick-1": ("Quick answers", QUICK, {0}), "quick-2": ("Quick answers", QUICK, {1}), "quick-3": ("Quick answers", QUICK, {2}),
    "stuck-1": ("Still stuck", STUCK, {0, 1}),
}


def slide(step_id, chapter_title, label):
    if step_id in MAP:
        heading, items, active = MAP[step_id]
        image = slide_base(chapter_title, label, heading)
        bullets(image, items, active=active)
        return image
    raise SystemExit(f"No slide for {step_id}")
