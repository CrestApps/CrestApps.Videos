"""The slides of the Telephony video."""
from brand import bullets, slide_base

WHY = [
    ("1", "A desk phone knows nothing", "It does not know who is calling, so every call starts from scratch"),
    ("2", "Every phone company is different", "Changing or adding one means new equipment and training"),
    ("3", "Colleagues need each other", "Reaching a coworker should not take a full phone number"),
    ("check", "A phone in the browser", "Connect the phone company once, and everyone calls from the same screen"),
]

HOW = [
    ("1", "Provider", "The phone system that carries your calls: Telnyx or Asterisk"),
    ("2", "Default provider", "The one the soft phone uses, which you can switch at any time"),
    ("3", "Soft phone", "The phone in the browser: keypad, recent calls and voicemail"),
    ("4", "Extension", "A short internal number that belongs to one user"),
]

CONNECTED = [
    ("1", "Default outbound caller id", "The number your customers see when a call does not bring its own"),
    ("2", "Webhook public key", "Proves that call events really come from Telnyx; calls do not connect without it"),
    ("3", "Noise suppression", "Cleans background sound out of the agent's or the caller's voice"),
    ("4", "Advanced", "Rarely changed network settings, each with a working default"),
]

USES = [
    ("1", "A small team without desk phones", "A browser phone and an extension for each person"),
    ("2", "A company with its own Asterisk server", "Keep the server, and call from next to the customer record"),
    ("3", "Moving to another phone company", "Connect the new provider, then change the default"),
    ("4", "Queues, campaigns and a dashboard", "Add the Contact Center on top of the same telephony"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Calls belong next to your customers")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Provider, soft phone, extension")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("tel-"):
        image = slide_base(chapter_title, label, "After you connect")
        bullets(image, CONNECTED, active={0, 1} if step_id == "tel-4" else {2, 3})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where a browser phone helps")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
