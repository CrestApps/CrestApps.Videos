"""The slides of the Bulk Import and Export video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Content starts in spreadsheets", "Leads, records from an older system, price lists"),
    ("2", "Typing rows one by one is slow", "Every row is a chance for a mistake"),
    ("3", "Import creates or updates many items", "Every row is checked like a hand-saved item"),
    ("check", "Export downloads items to a file", "Fix them in Excel, share them, import them again"),
]

WHO = [
    ("1", "List content transfer entries", "Shows the Import menu and the Bulk Import page"),
    ("2", "Import and export from file", "Upload, templates, error files, pause and resume; exports"),
    ("3", "One content type at a time", "A role can import customers and nothing else"),
    ("check", "Shared storage", "Azure Blob Storage for sites on several servers"),
]

USE = [
    ("1", "Load a list in one upload", "Purchased or partner lists of leads, customers or products"),
    ("2", "Move records between systems", "Export from the old one, import into this one"),
    ("3", "Correct many records at once", "Export, fix the file in Excel, import to update the items"),
    ("4", "Share a list with an outsider", "An accountant or a partner with no access to the site"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "From a spreadsheet to your site, and back")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("who-"):
        image = slide_base(chapter_title, label, "Permissions control what each role sees")
        bullets(image, WHO, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to use bulk transfer")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
