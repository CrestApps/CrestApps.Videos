"""The slides of the Stripe: wallets, saved cards and disputes video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Shoppers expect cards and wallets", "Apple Pay and Google Pay in a touch"),
    ("2", "What happens after matters too", "Refunds, failed payments, disputes"),
    ("3", "Stripe takes the payment", "And tells your store what happens"),
    ("check", "This demo never contacts Stripe", "It shows what you and buyers see"),
]

HOW = [
    ("1", "Enter your keys", "Test keys first"),
    ("2", "Connect", "A webhook, and wallets for your domain"),
    ("3", "Buyers pay", "A wallet, a card, a saved card"),
    ("check", "Stripe reports", "Disputes and risky payments show on the order"),
]

CONNECT = [
    ("1", "A webhook is created", "Stripe reports payments, refunds, disputes"),
    ("2", "The domain is registered", "For Apple Pay and Google Pay"),
    ("3", "Disconnect clears the keys", "Keys from the application settings stay"),
    ("check", "Not public yet?", "Forward events with the Stripe command line tool"),
]

DISPUTES = [
    ("1", "Disputed", "A bank opened a dispute on the payment"),
    ("2", "Risky", "Stripe thinks it may be fraud"),
    ("3", "Workflow events", "Tell your team when it happens"),
    ("check", "Answer in Stripe", "Send your evidence from the dashboard"),
]

USE = [
    ("1", "Go live in steps", "Test keys, a test order, then live keys"),
    ("2", "A mobile shopper", "A wallet payment in a second"),
    ("3", "A returning customer", "A saved card, one click"),
    ("4", "A chargeback", "The order says Disputed"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Cards, wallets and disputes")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Keys, Connect and the webhook")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("con-"):
        image = slide_base(chapter_title, label, "Webhook, wallets and Disconnect")
        bullets(image, CONNECT, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("dsp-"):
        image = slide_base(chapter_title, label, "What Stripe tells your store")
        bullets(image, DISPUTES, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to use Stripe")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
