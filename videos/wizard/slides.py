"""The slides of the Wizard video."""
from brand import bullets, slide_base

WHY = [
    ("1", "One step at a time", "A progress bar and answers kept between steps"),
    ("2", "Steps are content types", "Each step has its own fields"),
    ("check", "Resume, control, complete", "Visitors continue later; you choose who and what is kept"),
]

EVENTS = [
    ("1", "Wizard Started, Step Displayed", "A session begins, and each step is shown"),
    ("2", "Wizard Completed, Wizard Failed", "A session finishes or fails"),
    ("3", "Session details in every event", "Session, wizard type, current step and status"),
]

USE = [
    ("1", "Registrations", "Events and workshops"),
    ("2", "Onboarding", "New customers, one step at a time"),
    ("3", "Surveys", "Too long for one page"),
    ("4", "Intake forms", "That visitors finish later"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Guide visitors through a form")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("events-"):
        image = slide_base(chapter_title, label, "Four workflow events")
        bullets(image, EVENTS, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four typical uses")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
