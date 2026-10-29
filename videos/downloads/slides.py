"""The slides of the Digital Downloads video."""
from brand import bullets, slide_base

WHY = [
    ("1", "A file can be copied", "Anyone who finds a public address downloads it for free"),
    ("2", "Delivery should be automatic", "The buyer pays, and the file is theirs"),
    ("3", "You need control afterwards", "Send a link again, stop a refunded order's delivery"),
    ("check", "Private files, expiring links", "A set number of downloads, for a set number of days"),
]

HOW = [
    ("1", "Upload on the product", "Kept privately, never in the media library"),
    ("2", "The buyer pays", "No shipping address for digital products only"),
    ("3", "The buyer downloads", "From the order page, or the confirmation email"),
    ("4", "You manage", "Take back, give back, copy the buyer's link"),
]

USES = [
    ("1", "An author selling ebooks", "Each buyer has the file the moment they pay"),
    ("2", "Software", "Links that expire after a set number of days"),
    ("3", "A course with several files", "Every file on one product"),
    ("4", "A lost email", "Copy the buyer's link, or give the downloads back"),
]


def slide(step_id, chapter_title, label):
    n = int(step_id.split("-")[-1])
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Deliver a file without giving it away")
        bullets(image, WHY, active=set(range(n)))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Upload, pay, download")
        bullets(image, HOW, active={n - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where it fits")
        bullets(image, USES, active={n - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
