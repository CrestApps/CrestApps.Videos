"""The slides of the AI in Workflows video."""
from brand import bullets, slide_base

WHAT = [
    ("1", "Two AI tasks", "Using a profile, or with the model set up in the task"),
    ("2", "Four chat events", "Closed, field extracted, all fields, post-processed"),
    ("3", "Three outcomes", "Done, Drew Blank, Failed"),
]

EVENTS = [
    ("1", "Session Closed", "A chat ends: a goodbye, or the inactivity time"),
    ("2", "Field Extracted", "One data extraction field has a value"),
    ("3", "All Fields Extracted", "Every field has a value, once per chat"),
    ("4", "Post-Processed", "A closed chat was analyzed by its post-session tasks"),
]

USES = [
    ("1", "New leads to sales", "Email the extracted name, email and question"),
    ("2", "Summarize closed chats", "Save or send the post-session summary"),
    ("3", "Tag incoming requests", "One word: Sales, Support or Billing"),
]


def slide(step_id, chapter_title, label):
    table = {
        "wf-": ("What the AI adds", WHAT),
        "evs-": ("When each event starts", EVENTS),
        "use-": ("Ways to use it", USES),
    }
    for prefix, (subtitle, items) in table.items():
        if step_id.startswith(prefix):
            image = slide_base(chapter_title, label, subtitle)
            bullets(image, items, active={int(step_id[-1]) - 1})
            return image
    raise SystemExit(f"No slide for {step_id}")
