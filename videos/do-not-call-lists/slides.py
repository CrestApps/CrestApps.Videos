"""The slides of the Do Not Call Lists video."""
from brand import bullets, slide_base

WHY = [
    ("1", "A call can cost more than it earns", "Fines for a single call to a listed number can be large"),
    ("2", "The numbers come from many places", "Your own list, monthly downloads, national registries"),
    ("3", "No agent can check them by hand", "A purchased file can hold hundreds of listed numbers"),
    ("check", "Keep them in one place", "The site checks at import and before every call"),
]

WHERE = [
    ("1", "Contact imports", "Checked against the lists you choose; some can be mandatory"),
    ("2", "The dialer", "A profile that respects do-not-call checks before each call"),
    ("3", "Calls dialed by hand", "Can be checked too, depending on how the site is set up"),
    ("check", "A registry that cannot answer", "Never counts as not listed: the row is skipped, the dialer tries later"),
]

FILE = [
    ("1", "One number on each row", "A single column; save just that column from a spreadsheet"),
    ("2", "A header row is optional", "Such as Phone Number: it is skipped, not counted as an error"),
    ("3", "Converted to the international form", "Numbers without a plus sign use the country you picked"),
    ("4", "Some rows are rejected", "Blank rows, several values, repeated or invalid numbers"),
]

USES = [
    ("1", "Your own suppression list", "Upload it, and customers who said no are never called"),
    ("2", "A team that buys leads", "Make the registries mandatory, so every file is cleaned on import"),
    ("3", "Calling in two countries", "One local list for each country, and both national registries"),
    ("4", "A monthly download", "Upload the new list, then delete the old one when it is completed"),
]


def slide(step_id, chapter_title, label):
    table = {"why-": (WHY, "One call can cost far more than it earns"), "where-": (WHERE, "Imports, the dialer, and calls by hand"),
             "file-": (FILE, "What the CSV file must look like"), "use-": (USES, "Staying on the right side of the rules")}
    for prefix, (cards, title) in table.items():
        if step_id.startswith(prefix):
            image = slide_base(chapter_title, label, title)
            number = int(step_id.split("-")[-1])
            bullets(image, cards, active=set(range(number)) if prefix == "why-" else {number - 1})
            return image
    raise SystemExit(f"No slide for {step_id}")
