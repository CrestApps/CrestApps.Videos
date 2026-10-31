"""The slides of the Data connectors deployment and permissions video."""
from brand import bullets, slide_base

PERMS = [
    ("1", "Manage data connections", "Create, edit, test and delete connections: security critical"),
    ("2", "Manage connected data sets", "Create, edit, preview and delete data sets"),
    ("3", "Refresh imported data sets", "Run Refresh now: included in the manage permission"),
    ("4", "Read connected data sets", "Read every one, or one by name, in reports and pipelines"),
]

OTHER = [
    ("1", "Matched, then created", "Same identifier or technical name: updated. The others: created"),
    ("2", "Secrets are not copied", "Type each one again, then Save and test"),
    ("3", "No imported rows", "Each imported data set runs its first import within a minute"),
    ("4", "Same features, same reach", "The connector features on, and the systems reachable"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("perms-"):
        image = slide_base(chapter_title, label, "What each permission allows")
        bullets(image, PERMS, active={"perms-1": {0}, "perms-2": {1, 2}, "perms-3": {3}}[step_id])
        return image
    if step_id.startswith("other-"):
        image = slide_base(chapter_title, label, "After the plan is imported")
        bullets(image, OTHER, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
