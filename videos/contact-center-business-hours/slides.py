"""The slides of the Business Hours video."""
from brand import bullets, slide_base

WHY = [
    ("1", "After hours needs another answer", "Ringing empty queues and night calls waste time"),
    ("2", "Each feature guessing its own hours", "They drift apart when the schedule changes"),
    ("3", "One shared calendar", "Change it once, and everything that uses it follows"),
    ("check", "Schedule, time zone, holidays", "Open days and hours, and the days you are closed"),
]

WHERE = [
    ("1", "Queues", "Hold or overflow callers while closed"),
    ("2", "Inbound entry points", "Pick the closed action: voicemail, overflow or reject"),
    ("3", "Dialer profiles", "The calling window, in the contact's time zone"),
    ("4", "Automatic activity loads", "AI follow-up messages are only sent while open"),
]

RULES = [
    ("1", "To is exclusive", "09:00 to 17:00 closes at exactly 17:00"),
    ("2", "Same From and To", "Open for 24 hours"),
    ("3", "To earlier than From", "Runs past midnight, such as 22:00 to 06:00"),
    ("4", "Holidays and disabled calendars", "Invalid dates are ignored; a disabled calendar is always open"),
]

USES = [
    ("1", "Support hours", "After hours, callers reach voicemail"),
    ("2", "A calling window", "Never call a customer before breakfast"),
    ("3", "Holidays added once", "Every queue, menu and campaign is closed that day"),
    ("4", "Texts inside opening hours", "Nobody is texted at night"),
]


def slide(step_id, chapter_title, label):
    table = {"why-": (WHY, "One definition of open and closed"), "where-": (WHERE, "Four places a calendar is used"),
             "rule-": (RULES, "Rules worth knowing"), "use-": (USES, "What a calendar does for you")}
    for prefix, (cards, title) in table.items():
        if step_id.startswith(prefix):
            image = slide_base(chapter_title, label, title)
            number = int(step_id.split("-")[-1])
            bullets(image, cards, active=set(range(number)) if prefix == "why-" else {number - 1})
            return image
    raise SystemExit(f"No slide for {step_id}")
