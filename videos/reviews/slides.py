"""The slides of the Product Reviews video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Shoppers look for proof", "What other buyers say is the best proof"),
    ("2", "Reviews can be a risk", "Spam, abuse, or a complaint about something you do not sell"),
    ("3", "Rated, written, approved", "One to five stars from a customer, approved by you"),
    ("check", "Ratings follow the product", "Stars on its page, and sorting and filtering in listings"),
]

HOW = [
    ("1", "A customer writes", "Stars, a title, a few words and up to three photos"),
    ("2", "Verified purchase", "Marked when the customer bought the product"),
    ("3", "You approve", "A new or changed review waits under Commerce, Reviews"),
    ("4", "It shows", "On the product page, and in its average rating"),
]

USES = [
    ("1", "Many similar products", "Shoppers sort by customer rating"),
    ("2", "Honest feedback", "Only buyers can review: every review comes from a customer"),
    ("3", "A busy store", "Ask buyers for reviews by email, a couple of weeks after they pay"),
    ("4", "A brand that guards its name", "Keep approval on, and reject spam and abuse"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Proof from other buyers")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Write, approve, show")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where it fits")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
