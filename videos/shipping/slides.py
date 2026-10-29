"""The slides of the Shipping video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Where sales are lost", "A surprise price, or no option that fits"),
    ("2", "By hand does not scale", "Quoting every order and writing tracking emails"),
    ("3", "Methods that fit", "Each buyer sees what suits their address and order"),
    ("check", "Shipped and recorded", "Shipments with tracking, and the buyer is told"),
]

HOW = [
    ("1", "Zones", "A place you ship to"),
    ("2", "Methods", "Flat, table, free, local delivery, carrier rates"),
    ("3", "Settings", "Weight unit, pickup, ship-from, carrier account"),
    ("check", "Try rates and shipments", "Check the setup, then record each shipment"),
]

USE = [
    ("1", "Domestic and abroad", "A flat rate at home, a higher rate elsewhere"),
    ("2", "Free over a subtotal", "A free method that starts at a spend"),
    ("3", "Heavy goods", "A table rate that looks the price up by weight"),
    ("4", "Click and collect", "A pickup location, and local delivery"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "The right options at the right price")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Zones, methods, settings and shipments")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to use shipping")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
