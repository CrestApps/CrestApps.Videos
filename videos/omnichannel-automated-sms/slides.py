"""The slides of the Omnichannel Automated SMS video."""
from brand import bullets, slide_base

WHY = [
    ("1", "The AI starts the conversation", "It sends the opening text and answers each reply"),
    ("2", "You set the pace", "Reply delay, follow-up cadence and business hours"),
    ("check", "You stay in control", "Opt-outs stop it, and a live agent can take over"),
]

RULES = [
    ("1", "Opt-out keywords", "STOP cancels the activity and marks the contact Do not SMS"),
    ("2", "No-response timeout", "A quiet customer fails the activity after the subject's time"),
    ("3", "Reply delay", "The load's delay wins over the subject's response delay"),
]

END = [
    ("1", "Disposition and summary", "Chosen by the AI, then the subject flow runs"),
    ("2", "Review AI conversation", "The read-only transcript, under Completed Activities"),
    ("3", "Live agent handoff", "When the subject allows it and the customer asks"),
]

USE = [
    ("1", "Qualify new leads", "By text, as soon as they arrive"),
    ("2", "Confirm and care", "Appointments and customer questions"),
    ("3", "Follow up", "Only while the showroom is open"),
    ("4", "Answer a sales line", "Hand over only the customers who are ready"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "An AI profile works the activity")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("rules-"):
        image = slide_base(chapter_title, label, "What stops or paces a conversation")
        bullets(image, RULES, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("end-"):
        image = slide_base(chapter_title, label, "After the last text")
        bullets(image, END, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four typical uses")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
