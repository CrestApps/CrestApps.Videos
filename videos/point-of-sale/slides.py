"""The slides of the Point of Sale video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Two systems drift apart", "Products, prices and stock differ online and at the counter"),
    ("2", "Cash needs care", "Count the drawer, explain a shortage, tell a sale from a refund"),
    ("3", "One store, two ways to sell", "The same products, prices, coupons, gift cards and stock"),
    ("check", "Every sale is an order", "In your orders, your stock and your reports"),
]

HOW = [
    ("1", "Register", "A till at one of your stores"),
    ("2", "Shift", "Opened with the cash in the drawer"),
    ("3", "Sales", "Rung up in the till app, paid in cash, by card, or both"),
    ("4", "Close", "Count the drawer, and see the difference"),
]

BEFORE = [
    ("1", "Stores", "A location for each store, under Settings, Commerce, Inventory"),
    ("2", "Roles", "Sell at a register, and Manage registers and shifts"),
    ("3", "Sign-in", "Staff sign in to the till app with their own accounts"),
    ("4", "Optional", "Receipts, and Stripe card readers"),
]

DAY = [
    ("1", "Open a shift", "Pick a register, and enter the float"),
    ("2", "Ring up a sale", "Add the products, name the buyer, add a code or gift card"),
    ("3", "Take the payment", "Cash, card, or both, then the receipt"),
    ("4", "Cash, returns and closing", "Record cash moved, take a return, count the drawer"),
]

USES = [
    ("1", "A shop and a counter", "One catalog and one stock"),
    ("2", "Several stores", "A register at each, with sales by store"),
    ("3", "Several staff at a till", "Every shift and sale says who made it"),
    ("4", "A stall or a pop-up", "A phone or tablet, cash and a card reader"),
]

# Which card of the day slide each step lights.
DAY_CARD = {1: 0, 2: 1, 3: 1, 4: 2, 5: 2, 6: 3, 7: 3, 8: 3}


def slide(step_id, chapter_title, label):
    n = int(step_id.split("-")[-1])
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Counter and website, together")
        bullets(image, WHY, active=set(range(n)))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Registers, shifts and sales")
        bullets(image, HOW, active={n - 1})
        return image
    if step_id.startswith("before-"):
        image = slide_base(chapter_title, label, "What to have ready")
        bullets(image, BEFORE, active={n - 1})
        return image
    if step_id.startswith("day-"):
        image = slide_base(chapter_title, label, "A shift in the till app")
        bullets(image, DAY, active={DAY_CARD[n]})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where it fits")
        bullets(image, USES, active={n - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
