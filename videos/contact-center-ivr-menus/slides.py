"""The slides of the IVR menus video."""
from brand import bullets, slide_base

WHY = [
    ("1", "One queue for everyone", "Agents spend the first minute sending callers to the right team"),
    ("2", "A menu lets callers choose", "Press 1 for sales, press 2 for support"),
    ("check", "A welcome message first", "Every caller knows they reached the right company"),
]

HOW = [
    ("1", "Prompt", "Spoken to the caller, or a recording from Voice media"),
    ("2", "Keys and actions", "A queue, an agent, a submenu, another menu, voicemail, an external number, or repeat"),
    ("3", "Tries and fallback", "How many mistakes a caller gets, and where they go when the tries run out"),
]

AI = [
    ("1", "An AI voice agent answers", "An AI profile that greets the caller and holds the conversation"),
    ("2", "Chosen on the Routing card", "Route to offers AI voice agent when the Telnyx AI Voice Agent feature is on"),
    ("3", "People stay in the loop", "The subject flow hands a caller to a person; closed lines go to voicemail"),
]

CALLERS = [
    ("1", "Welcome, then the main menu", "The welcome message plays once; a key moves the caller to their choice"),
    ("2", "Mistakes and dead ends", "A wrong key plays the menu again; an unreachable choice goes to the entry point target, then voicemail"),
]

USES = [
    ("1", "One number, two teams", "Press 1 for sales, press 2 for support"),
    ("2", "A clinic with a billing submenu", "Callers pick invoices or payments"),
    ("3", "A team that is often busy", "A key that leaves a voicemail"),
    ("4", "A greeting recorded by a person", "A Recorded prompt from Voice media"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Why a phone menu")
        bullets(image, WHY, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Prompts, keys and actions")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("ai-"):
        image = slide_base(chapter_title, label, "Let an AI answer the call")
        bullets(image, AI, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("cal-"):
        image = slide_base(chapter_title, label, "From the first ring to the queue")
        bullets(image, CALLERS, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where a phone menu helps")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
