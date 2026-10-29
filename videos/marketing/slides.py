"""The slides of the Marketing video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Your orders know your customers", "Who bought recently, who has lapsed, who buys again"),
    ("2", "Each deserves a different message", "A thank you, an offer to come back, an early look"),
    ("3", "Only who said yes", "And a way to unsubscribe in one click"),
    ("check", "Segments and campaigns", "Built from your paid orders"),
]

HOW = [
    ("1", "A segment", "Customers who meet rules about their orders"),
    ("2", "A campaign", "An email for a segment, sent now or at a time"),
    ("3", "Consent", "Only customers who accept marketing get it"),
    ("4", "Unsubscribe", "A link in every email, confirmed on a page"),
]

USES = [
    ("1", "A win-back campaign", "Customers who have not ordered in 90 days"),
    ("2", "A welcome note", "Customers who first ordered in the last 30 days"),
    ("3", "A loyalty thank-you", "Repeat buyers, or customers who spent 500 or more"),
    ("4", "A local promotion", "Customers whose last order went to one country"),
]


def slide(step_id, chapter_title, label):
    n = int(step_id.split("-")[-1])
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Market to people who bought")
        bullets(image, WHY, active=set(range(n)))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Segments and campaigns")
        bullets(image, HOW, active={n - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where it fits")
        bullets(image, USES, active={n - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
