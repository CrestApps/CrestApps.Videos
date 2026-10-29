"""The slides of the MCP Resources video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Apps work from what they see", "Resources give them files and data to read"),
    ("2", "Pasting goes out of date", "Documents copied into a chat are never updated"),
    ("3", "The app asks for the file", "It reads the current version when it needs it"),
    ("check", "Part of the MCP server", "Managed beside the prompts of the server"),
]

CASES = [
    ("1", "Policies and price lists", "Publish once, and every app reads the current version"),
    ("2", "A partner's file server", "Read the files over SFTP without copying them"),
    ("3", "Media library documents", "Brochures and manuals your editors keep up to date"),
    ("4", "Share with care", "An app can read whatever a resource points to"),
]


def slide(step_id, chapter_title, label):
    kinds = {
        "why": ("Why resources", WHY, lambda n: set(range(n))),
        "case": ("What teams share", CASES, lambda n: {n - 1}),
    }
    prefix, _, number = step_id.partition("-")

    if prefix in kinds:
        heading, items, active = kinds[prefix]
        image = slide_base(chapter_title, label, heading)
        bullets(image, items, active=active(int(number)))
        return image

    raise SystemExit(f"No slide for {step_id}")
