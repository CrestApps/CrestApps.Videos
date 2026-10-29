"""The slides of the Voicemail video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Calls nobody can answer", "Agents step away, queues fill up, calls come after hours"),
    ("2", "A message is still a customer", "But an unheard message is no better than a missed call"),
    ("check", "A greeting, and a way to follow up", "Someone listens and calls back, exactly once"),
]

WHERE = [
    ("1", "Your own inbox", "Played from the soft phone's Voicemail tab"),
    ("2", "A queue's shared voicemail box", "Worked by the team under Interaction Center > Shared voicemail"),
]

INBOX = [
    ("1", "Only your own tab", "You can play and delete exactly the messages in it"),
    ("2", "Unanswered, or you clicked Voicemail", "A call to you goes to your inbox"),
    ("3", "A queue call that reached you", "Offer rang out with Send to voicemail, or the last agent offered after the maximum wait"),
    ("4", "Any other queue call", "The entry point's Voicemail inbox; with nobody set, nobody's inbox"),
]

ACTIONS = [
    ("1", "Claim and call back", "I'll handle this, then Call back from your own soft phone"),
    ("2", "Finish", "Mark done with an optional note; Return to queue; Reopen"),
    ("3", "Supervisor actions", "Take over and Delete need the manage permission; legal hold blocks delete"),
]

USES = [
    ("1", "A greeting that helps", "An email address and a callback time"),
    ("2", "After hours", "Shared box claimed and cleared by the morning team"),
    ("3", "A busy line", "A supervisor is the Voicemail inbox, so every message is heard"),
    ("4", "Before lunch", "Filter by Open and check nothing waits without an owner"),
]


def slide(step_id, chapter_title, label):
    table = {"why-": (WHY, "No call should disappear"), "where-": (WHERE, "Two places a message can go"),
             "inbox-": (INBOX, "Whose inbox a voicemail goes to"), "act-": (ACTIONS, "Working a shared message"),
             "use-": (USES, "What voicemail does for you")}
    for prefix, (cards, title) in table.items():
        if step_id.startswith(prefix):
            image = slide_base(chapter_title, label, title)
            number = int(step_id.split("-")[-1])
            bullets(image, cards, active=set(range(number)) if prefix == "why-" else {number - 1})
            return image
    raise SystemExit(f"No slide for {step_id}")
