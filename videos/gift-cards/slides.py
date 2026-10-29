"""The slides of the Gift cards video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Gift cards are cash up front", "And a way to win a new customer"),
    ("2", "By hand, balances cannot be trusted", "Spreadsheets, and cards spent twice"),
    ("3", "A code with money on it", "Typed at checkout, the rest stays on the card"),
    ("check", "Sell them, or issue them", "For example after a late delivery"),
]

HOW = [
    ("1", "Sell one", "A gift card product; an order issues a card"),
    ("2", "Issue one by hand", "Amount, recipient, your own code, expiry"),
    ("3", "Spend one", "A second way to pay at checkout"),
    ("check", "Follow one", "A balance and an activity list for every card"),
]

REFUNDS = [
    ("1", "Refunds go back to the card", "Wallet first, then the gift card"),
    ("2", "A bought card is turned off", "When its order is canceled, if nothing was spent"),
    ("3", "Money is held while paying", "Two buyers cannot spend the same money"),
    ("check", "Safe codes", "No look-alike characters, and tries are limited"),
]

USE = [
    ("1", "Holiday cards", "Amounts you choose, such as 25, 50 and 100"),
    ("2", "A make-good", "A card for a late delivery"),
    ("3", "A physical card", "Your own printed codes"),
    ("4", "Prizes and rewards", "With an expiry date where allowed"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Money on a code")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Sold, issued, spent and followed")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("ref-"):
        image = slide_base(chapter_title, label, "What happens to the money")
        bullets(image, REFUNDS, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to use gift cards")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
