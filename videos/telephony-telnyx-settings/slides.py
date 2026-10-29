"""The slides of the Telnyx Settings video."""
from brand import bullets, slide_base

WHAT = [
    ("1", "One A.P.I. key", "Connect Telnyx creates the application, the SIP connection, the voice profile and the webhook for you"),
    ("2", "More once connected", "The options below the connection appear only after you connect a real account"),
]

CON = [
    ("1", "Connected to Telnyx", "The connection ids it created; Disconnect deletes them after you confirm"),
    ("2", "Caller id and webhook key", "Default outbound caller id, and the Webhook public key: without it every call event is rejected"),
]

TUNE = [
    ("1", "Noise suppression", "Off, Krisp, DeepFilterNet, Denoiser or ai-coustics; Light, Balanced or Strong; agent's voice, caller's voice, or both"),
    ("2", "Answering machine detection", "Premium, Standard or Off, so a message is left after the tone"),
    ("3", "Text-to-speech", "Voice and language of menus, the voicemail greeting and the callback offer"),
]

ADV = [
    ("1", "Audio test and orphaned calls", "Audio test destination, and Calls with no local record: Report or End call"),
    ("2", "Region, credentials, codecs", "Soft phone region, Credential lifetime, Preferred audio codecs: change only when asked"),
]

# step-id prefix -> (cards, heading, how the cards light up: "all" up to the step number, "one" the card of the step, or a fixed set per step id)
TABLE = {"what-": (WHAT, "What the Telnyx tab does", "all"), "con-": (CON, "After you connect", "one"), "tune-": (TUNE, "Noise, answering machines and speech", "one"), "adv-": (ADV, "Advanced settings", "one")}


def slide(step_id, chapter_title, label):
    for prefix, (cards, heading, active) in TABLE.items():
        if step_id.startswith(prefix):
            image = slide_base(chapter_title, label, heading)
            number = int(step_id.split("-")[-1])
            lit = active.get(step_id) if isinstance(active, dict) else (set(range(number)) if active == "all" else {number - 1})
            bullets(image, cards, active=lit)
            return image
    raise SystemExit(f"No slide for {step_id}")
