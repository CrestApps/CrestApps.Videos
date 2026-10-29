"""The slides of the Omnichannel Activities video."""
from brand import bullets, slide_base

WHY = [
    ("1", "One piece of work", "A call to make, a text to send, or an inbound call to log"),
    ("2", "The subject decides", "Its flow turns each disposition into the next step"),
    ("check", "The agent records the outcome", "The next try, follow-up or close happens on its own"),
]

AI = [
    ("1", "Dispositioned by", "A person, the dialer, or the AI agent with its profile"),
    ("2", "Review AI conversation", "Opens the transcript of an automated activity"),
    ("3", "The AI assistant's summary", "A banner on the activity the AI handed to a person"),
]

USE = [
    ("1", "Work the list in order", "Using each contact's local time"),
    ("2", "Record every call the same way", "So the next step happens on its own"),
    ("3", "Log inbound calls", "Even from callers who are not contacts yet"),
    ("4", "Review the AI", "And step in when it hands over"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Every call or text is an activity")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("ai-"):
        image = slide_base(chapter_title, label, "When an AI worked the activity")
        bullets(image, AI, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four typical uses")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
