"""The slides of the Report workflows video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Reports nobody opens", "A morning email, a file for a manager, a link for a colleague"),
    ("2", "An event or a timer, then tasks", "Report Published, Report Run, Report Exported, or a Timer"),
    ("3", "Two routes to email", "Subscriptions for reports under Reports, workflows for Report Builder reports"),
]

USES = [
    ("1", "A morning report", "A timer, then Email Report with the dashboard as an Excel file"),
    ("2", "A handover", "When a report is published, create a share link for the people who need it"),
    ("3", "An archive", "When a report is run, export a copy to the media library"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Reports that act on their own")
        bullets(image, WHY, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where workflows fit")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
