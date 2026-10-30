"""The slides of the Messaging: Templates and Broadcasts video."""
from brand import bullets, slide_base

WHY = [
    ("1", "The same questions all day", "Hours, address, return policy"),
    ("2", "Templates", "One answer, the same words, one click away"),
    ("3", "Broadcasts", "One message to many people, without sending each one"),
]

AFTER = [
    ("1", "Private conversations", "Replies arrive in the inbox like any text"),
    ("2", "Retries", "After 1, 5, 15 and 60 minutes, then marked failed"),
    ("3", "What a broadcast skips", "Quiet hours and do-not-call lists are not checked"),
]

KEYWORDS = [
    ("1", "STOP", "Unsubscribes, closes the conversation, marks Do not SMS"),
    ("2", "START", "Subscribes again, only for people who opted out"),
    ("3", "HELP", "Sends the help reply, with how to unsubscribe"),
    ("4", "In a broadcast", "Opted-out people are marked failed at once"),
]

USES = [
    ("1", "A bike shop", "Hours, address and returns as templates"),
    ("2", "A clinic", "A closing notice the day before a holiday"),
    ("3", "A weekend sale", "Only for customers who agreed to offers"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Type less, reach more")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("aft-"):
        image = slide_base(chapter_title, label, "After the broadcast is queued")
        bullets(image, AFTER, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("kw-"):
        image = slide_base(chapter_title, label, "Words the SMS channel always answers")
        bullets(image, KEYWORDS, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Ways to use it")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
