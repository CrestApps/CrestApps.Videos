"""The slides of the AI File Sources video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Uploads go stale", "Handbooks, price sheets and exports change, and nobody re-uploads"),
    ("2", "The files already have a home", "A folder on the server, an FTP site, or an SFTP site"),
    ("3", "A schedule reads the folder", "Each file becomes passages the assistant can search"),
    ("check", "Only the changes", "New, changed and removed files are processed"),
]

HOW = [
    ("1", "Knowledge base index", "Stores the passages, searched by meaning"),
    ("2", "Files data source", "The target: the knowledge the assistant answers from"),
    ("3", "File source", "The reader: File system, FTP or SFTP connector"),
    ("check", "A profile uses it", "Answers come from the files, with sources"),
]


def slide(step_id, chapter_title, label):
    kinds = {
        "why": ("Knowledge that follows the folder", WHY, lambda n: set(range(n))),
        "how": ("An index, a data source, a file source", HOW, lambda n: {n - 1}),
    }
    prefix, _, number = step_id.partition("-")

    if prefix in kinds:
        heading, items, active = kinds[prefix]
        image = slide_base(chapter_title, label, heading)
        bullets(image, items, active=active(int(number)))
        return image

    raise SystemExit(f"No slide for {step_id}")
