"""The slides of the AI Chat video."""
from brand import bullets, slide_base

WHY = [
    ("1", "People need a place to talk", "Staff want answers in their work, visitors want them on the site"),
    ("2", "A chat remembers", "Follow-up questions make sense, and the history is kept"),
    ("3", "It works where people are", "In the admin, in a corner of every page, on the public site"),
    ("check", "Built on your AI profiles", "An assistant behaves the same everywhere"),
]

HOW = [
    ("1", "Chat page", "One page per chat profile, with your own history"),
    ("2", "Admin chat widget", "Floats on every admin page, one profile chosen in the settings"),
    ("3", "Website chat widget", "A widget on a layer, for your public pages"),
    ("4", "Chat engines", "The orchestrator behind a chat: default, Claude or GitHub Copilot"),
]

USES = [
    ("1", "Internal helper", "A policy assistant one click away in the admin menu"),
    ("2", "Website assistant", "Answers about hours, orders and repairs at any time"),
    ("3", "Admin assistant", "Always there, with tools to look things up on the site"),
    ("4", "Your preferred engine", "Claude or GitHub Copilot for one profile, default for the rest"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "A chat for every assistant")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Four parts, each with one job")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Ways to use it")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
