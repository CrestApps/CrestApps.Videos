"""The slides of the Report subscriptions video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Every day", "For the day before, at the hour you choose"),
    ("2", "Every week", "For the 7 days before, on the weekday you choose"),
    ("3", "Every month", "For the month before, on a day from 1 to 28"),
    ("4", "A file as well", "The numbers and tables in the email, and CSV, Excel or PDF attached"),
]

BUILDER = [
    ("1", "Reports under Reports", "Provided by the modules of your site: subscribe to them"),
    ("2", "Reports from the Report Builder", "Cannot be subscribed to"),
    ("3", "Email one from a workflow", "The Email Report task sends a Report Builder report"),
]

USES = [
    ("1", "A weekly service review", "Queue service level, every Monday at 7, to the supervisors, as a PDF"),
    ("2", "A monthly bill check", "Queue usage for billing, on the 1st, to finance, as an Excel file"),
    ("3", "A daily pulse", "Call insights each morning for the day before, with no file"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Three schedules, one optional file")
        bullets(image, WHY, active={"why-1": {0, 1, 2}, "why-2": {0, 1, 2}, "why-3": {3}, "why-4": {0, 1, 2, 3}}[step_id])
        return image
    if step_id.startswith("bld-"):
        image = slide_base(chapter_title, label, "Two kinds of reports")
        bullets(image, BUILDER, active={"bld-1": {0, 1}, "bld-2": {1, 2}}[step_id])
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where subscriptions fit")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
