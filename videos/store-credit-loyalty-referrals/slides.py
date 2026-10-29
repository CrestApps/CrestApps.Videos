"""The slides of the Store credit, loyalty points and referrals video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Keeping a customer costs less", "Than winning a new one"),
    ("2", "Credit, points and referrals", "Money owed, earned rewards, bringing a friend"),
    ("3", "Paid at checkout, after tax", "Like a gift card"),
    ("check", "For signed in buyers", "They belong to a customer's account"),
]

HOW = [
    ("1", "You give or take", "On the customer's page, with a reason"),
    ("2", "Buyers spend them", "A box at checkout for each"),
    ("3", "Buyers follow them", "Store credit, Rewards, Refer a friend"),
    ("check", "Settings set the rules", "Rewards and Referrals"),
]

USE = [
    ("1", "A make-good", "Credit after a late delivery"),
    ("2", "Refund as credit", "The money stays in your store"),
    ("3", "A rewards program", "Points on every order"),
    ("4", "Growth through friends", "A coupon for the friend, credit for the customer"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Credit, points and referrals")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Customers, checkout and the buyer's account")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to use rewards")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
