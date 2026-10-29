"""The slides of the AI Profiles, Personas and Templates video."""
from brand import bullets, slide_base

WHY = [
    ("1", "An assistant is more than a model", "Instructions, a name and voice, knowledge, tools and limits"),
    ("2", "Scattered settings drift apart", "Every chat, widget and workflow would repeat them"),
    ("3", "Describe each job once", "A profile, a persona, and templates to start from"),
    ("check", "Change it in one place", "Everything that talks to the AI uses the same approved setup"),
]

HOW = [
    ("1", "Profile", "One assistant or task: instructions, model, knowledge, tools, limits"),
    ("2", "Persona", "Who it is: name, personality, language, voice, picture. Linked"),
    ("3", "Profile template", "Where to start: ready-made settings. Copied once"),
    ("4", "Prompt template", "A reusable piece of instructions, added to any profile"),
]

USES = [
    ("1", "Website assistant", "Start from a card, add your brand's persona"),
    ("2", "Standard assistants", "One profile template with the approved model and rules"),
    ("3", "One persona, many profiles", "Support and sales share the same character"),
    ("4", "Work in the background", "Utility profiles for workflows, agents for other assistants"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Describe each assistant once")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Four parts, each with one job")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Ways to use them")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
