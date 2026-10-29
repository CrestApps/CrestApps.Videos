"""The slides of the Invoices and Receipts video."""
from brand import bullets, slide_base

WHY = [
    ("1", "What do I owe, and by when", "Clear invoices for customers who pay later"),
    ("2", "Pay without a password", "Even from a forwarded email"),
    ("3", "Proof with a number", "A receipt for every payment"),
    ("check", "Your business on every one", "Name, address and footer"),
]

HOW = [
    ("1", "An invoice number", "INV-1001, from the first notice"),
    ("2", "A pay link", "No sign-in, works for 60 days"),
    ("3", "A receipt after every payment", "R-1001, and refund notices"),
    ("4", "One set of branding", "They all print cleanly"),
]

MORE = [
    ("1", "Refund notices", "The customer is told the amount"),
    ("2", "Test payment badge", "On receipts from a provider's test mode"),
    ("3", "Base url", "Set it before you rely on pay links"),
]

USES = [
    ("1", "A service business", "An invoice with a pay link"),
    ("2", "Slow payers", "Reminders carry the same link"),
    ("3", "A bookkeeper", "A numbered receipt to file"),
    ("4", "A shop", "Customers print their own documents"),
]


def slide(step_id, chapter_title, label):
    n = int(step_id.split("-")[-1])
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Why invoices and receipts")
        bullets(image, WHY, active=set(range(n)))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Numbers, links and notices")
        bullets(image, HOW, active={n - 1})
        return image
    if step_id.startswith("more-"):
        image = slide_base(chapter_title, label, "Good to know")
        bullets(image, MORE, active={n - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where it fits")
        bullets(image, USES, active={n - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
