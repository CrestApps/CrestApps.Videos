"""The slides of the AI Chat Interactions video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Some questions are one-off", "Try a model, draft a paragraph, test instructions"),
    ("2", "No setup first", "Model, persona, instructions and tools sit beside the chat"),
    ("3", "Saved as you go", "There is no Save button to forget"),
]

HOW = [
    ("1", "The list", "Your own interactions, each a saved conversation"),
    ("2", "Settings tab", "Model, persona, instructions, fine-tuning"),
    ("3", "Capabilities tab", "Tools, agents and connections, in this interaction only"),
    ("4", "Site settings", "Voice options for every interaction, set by an administrator"),
]

USES = [
    ("1", "Try a model", "Change the Chat deployment between messages"),
    ("2", "Draft text", "Instructions for tone and length, then the first version"),
    ("3", "Test instructions", "Refine them here, then copy them into an AI profile"),
    ("4", "A personal assistant", "Tick a few tools, and let the AI look things up"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Talk to a model right away")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "One page, three places")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Ways to use it")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
