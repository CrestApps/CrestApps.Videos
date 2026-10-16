"""The slides of the AI Profile Builder video."""
from brand import bullets, slide_base

WHY = [
    ("1", "The system message is everything", "An automated text or call behaves only as well as its instructions"),
    ("2", "Writing it by hand is guesswork", "Guess every situation, try it in a chat, edit, and try again"),
    ("3", "Fixes break other replies", "A rule added for one reply can undo one that used to work"),
    ("check", "Teach it by role-play", "Say what the AI got wrong, and it rewrites its own instructions"),
]

HOW = [
    ("1", "A chat with a coach beside it", "Role-play in text or by voice, and give feedback on any reply"),
    ("2", "The coach remembers every fix", "It reads all earlier feedback, including the replies you threw away"),
    ("3", "Revisions and replay", "Restore any version, and check that earlier fixes still hold"),
    ("check", "Save it as an AI profile", "Create a profile, or update the one you started from"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Instructions that cover every conversation")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Role-play, feedback, and a rewrite")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
