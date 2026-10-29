"""The slides of the Phone and Address Fields video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Contact details are everywhere", "A branch phone, a venue address, a billing address"),
    ("2", "A text box accepts anything", "Phone numbers typed in every style cannot be dialled"),
    ("3", "Every country writes addresses differently", "State and ZIP code, province and postal code, post town and postcode"),
    ("check", "Fields that follow the country", "They check what is typed and keep one consistent shape"),
]

WHAT = [
    ("1", "Phone Field", "Country flag picker, formatted as you type, stored internationally"),
    ("2", "Address Field", "Laid out and checked the way the chosen country writes addresses"),
    ("3", "Address Countries", "The countries your site serves, which you can rename, add or remove"),
    ("4", "Address suggestions", "Optional: Google Places suggests addresses as a street is typed"),
]

USE = [
    ("1", "Branch, office and venue pages", "A clickable phone number and a correct address"),
    ("2", "Contacts and customers", "Phone numbers that can be dialled or texted"),
    ("3", "Billing and shipping addresses", "Each one follows its country's rules"),
    ("4", "Sites that serve some countries", "Offer exactly those countries"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "A text box accepts anything")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("what-"):
        image = slide_base(chapter_title, label, "Two fields, a country list, optional suggestions")
        bullets(image, WHAT, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four typical uses")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
