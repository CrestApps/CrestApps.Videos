"""The slides of the Returns video."""
from brand import bullets, slide_base

WHY = [
    ("1", "A clear policy sells", "Shoppers hesitate when they cannot send it back"),
    ("2", "Email loses track", "What was asked, agreed and refunded"),
    ("3", "Buyers ask from their order", "You approve, receive, restock, then refund"),
    ("check", "Every step is emailed", "And written in the order's notes"),
]

HOW = [
    ("1", "Set the terms", "Window, reasons, exchanges, instructions"),
    ("2", "A buyer asks", "Money back, or something else instead"),
    ("3", "You answer", "Approve or decline, then mark received"),
    ("check", "Close it", "Refund, or send the replacement"),
]

KNOW = [
    ("1", "The window starts at payment", "Not at delivery"),
    ("2", "Returns start with the buyer", "Full refunds stay on the order"),
    ("3", "Sellable items go back in stock", "In the location you choose"),
    ("check", "Emails need email set up", "Without it, nobody is emailed"),
]

USE = [
    ("1", "Clothing", "Swap a size, no refund"),
    ("2", "Damaged on arrival", "Refund before it comes back"),
    ("3", "Out of window", "Decline with a clear reason"),
    ("4", "The holidays", "A longer window for a season"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Say yes to a return, on your terms")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Request, answer, receive, close")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("kno-"):
        image = slide_base(chapter_title, label, "Windows, stock and refunds")
        bullets(image, KNOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to use returns")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
