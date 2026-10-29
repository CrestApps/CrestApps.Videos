"""The slides of the Voice Media video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Callers hear the same audio", "While they wait, and when a menu asks them to choose"),
    ("2", "Providers can lose files", "And a copy at one provider is gone when you change provider"),
    ("3", "A master copy on your site", "A new copy is sent to the provider when it loses its own"),
    ("check", "Upload once, pick by name", "On a queue, or in a phone menu"),
]

BADGES = [
    ("1", "Master copy", "The platform holds the audio; plays on any provider"),
    ("2", "Provider copy only", "An older clip that exists only at its provider: re-upload it to protect it"),
    ("3", "No audio", "The clip has no audio yet; badges name the providers that hold a copy"),
]

USE = [
    ("1", "On a queue", "Pick it as Hold music on the While callers wait card"),
    ("2", "In an IVR menu", "Pick it as the menu's Recorded prompt"),
    ("3", "The first play at a provider", "It is sent a copy from the master: a short delay, once"),
    ("4", "Your voicemail greeting", "Kept the same way: a master copy, sent to the provider"),
]

AFTER = [
    ("1", "Delete removes every copy", "The master copy, and every provider's copy"),
    ("2", "Update queues and menus first", "They fall back to their spoken text, or to silence"),
    ("3", "Moving to another site", "Entries travel in deployment plans, but the audio does not: upload it again"),
]

USES = [
    ("1", "One hold music for every queue", "Uploaded once"),
    ("2", "Prompts in each language", "Picked in the menus of your entry points"),
    ("3", "Moving to a new provider", "Re-sync the clips: nothing is uploaded again"),
    ("4", "A seasonal greeting", "Replace the file of the same clip: everything that uses it changes"),
]


def slide(step_id, chapter_title, label):
    table = {"why-": (WHY, "Audio that survives a change of provider"), "badge-": (BADGES, "What is stored for each clip"),
             "use-": (USE, "On a queue, or in a phone menu"), "del-": (AFTER, "After a clip is deleted"), "uses-": (USES, "What the library does for you")}
    for prefix, (cards, title) in sorted(table.items(), key=lambda kv: -len(kv[0])):
        if step_id.startswith(prefix):
            image = slide_base(chapter_title, label, title)
            number = int(step_id.split("-")[-1])
            bullets(image, cards, active=set(range(number)) if prefix == "why-" else ({number - 1} if prefix != "del-" else {number - 2}))
            return image
    raise SystemExit(f"No slide for {step_id}")
