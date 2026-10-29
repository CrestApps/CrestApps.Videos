"""The slides of the Shipments and labels video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Paid is not finished", "Pack it, send it, tell the buyer"),
    ("2", "Parts and parcels are hard to track", "In a spreadsheet"),
    ("3", "A shipment for each parcel", "Items, carrier, tracking number"),
    ("check", "Buyers are told", "And with EasyPost, labels are one click"),
]

HOW = [
    ("1", "Shipments on the order", "Commerce, Orders"),
    ("2", "Being packed or Shipped", "Items, carrier and tracking number"),
    ("3", "Delivered or Returned", "Returned items can be sent again"),
    ("check", "Remove a shipment being packed", "Its items go back"),
]

LABELS = [
    ("1", "Carrier rates at checkout", "A Carrier rates method on a zone"),
    ("2", "Buy label", "On a shipment being packed"),
    ("3", "Tracking updates", "The carrier's word, and Delivered by itself"),
]

KNOW = [
    ("1", "Workflow events", "Created, Shipped, Delivered, Returned"),
    ("2", "The shipped email", "On by default"),
    ("3", "A packing slip for each shipment", "Only its items"),
    ("check", "The Shipping times report", "Waiting and delivery times"),
]

USE = [
    ("1", "An order in parts", "Ship what is in stock now"),
    ("2", "A packing station", "Being packed, slips, mark shipped"),
    ("3", "Labels from the order", "Buy and print with EasyPost"),
    ("4", "A return in the post", "Mark it returned, send again"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Know what left and what is left")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "From a paid order to a delivered parcel")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("lab-"):
        image = slide_base(chapter_title, label, "Carrier rates and labels")
        bullets(image, LABELS, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("kno-"):
        image = slide_base(chapter_title, label, "Workflows, emails and packing slips")
        bullets(image, KNOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four ways to use shipments")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
