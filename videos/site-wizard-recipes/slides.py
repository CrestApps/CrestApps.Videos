"""The slides of the Wizards, Recipes and Shared Resources video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Long forms put visitors off", "Steps with progress shown get more of them to the end"),
    ("2", "Setting up by hand is slow", "Every new site ends up slightly different"),
    ("3", "The same widgets again and again", "A date range menu or a phone box should behave the same everywhere"),
    ("check", "Build once, reuse", "The Wizard, Recipes and shared resources answer one problem each"),
]

WHAT = [
    ("1", "Wizard", "Asks for information one step at a time and creates content items from the answers"),
    ("2", "Recipe", "A ready-made setup you run from the admin"),
    ("3", "Shared resources", "Common scripts and styles that other CrestApps screens use"),
    ("check", "Built and run in the admin", "The shared resources only need to be turned on"),
]

RES = [
    ("1", "Date range menu", "Presets such as Last 7 Days and This Month, or a custom range"),
    ("2", "Phone box with flags", "Shared by the Phone Field and other phone screens"),
    ("3", "List and picker helpers", "Select all, collapsible panels, date pickers, item selectors"),
    ("check", "One place, one look", "Every screen looks and behaves the same"),
]

USE = [
    ("1", "Registrations", "Event or workshop details in a few short steps"),
    ("2", "Customer onboarding", "Contact details first, then preferences"),
    ("3", "The same setup for every site", "Content types, roles and sample pages in one recipe"),
    ("4", "Training and demo sites", "Run the recipe again for a fresh copy"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Long forms, repeated setups and repeated widgets")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("what-"):
        image = slide_base(chapter_title, label, "A wizard, a recipe and a shared resource")
        bullets(image, WHAT, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("res-"):
        image = slide_base(chapter_title, label, "Small building blocks used everywhere")
        bullets(image, RES, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four typical uses")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
