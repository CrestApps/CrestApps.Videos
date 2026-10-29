"""The slides of the Bookings video."""
from brand import bullets, slide_base

WHY = [
    ("1", "A date is part of the sale", "Each date holds a limited number of people"),
    ("2", "Prices follow the booking", "Per person or per room, higher in high season"),
    ("3", "Deposits and cancellations", "Pay a share now, know what canceling costs"),
    ("check", "One catalog, cart and checkout", "The same store, with dates"),
]

HOW = [
    ("1", "The Booking part", "A date with seats, nights, or a time"),
    ("2", "A capacity for each date", "A full date cannot be bought"),
    ("3", "Rules on the product", "Deposit, cancellation, requests, travelers"),
    ("4", "A list and a calendar", "Confirm, complete or cancel each booking"),
]

USES = [
    ("1", "A tour operator", "Departures with seats, a deposit, a cancellation policy"),
    ("2", "A hotel or a rental", "Rooms by the night, a weekend price, a minimum stay"),
    ("3", "A clinic or a studio", "Appointments by the hour in your opening hours"),
    ("4", "A guide who vets guests", "Requests you confirm first"),
]


def slide(step_id, chapter_title, label):
    n = int(step_id.split("-")[-1])
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Selling a date")
        bullets(image, WHY, active=set(range(n)))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Dates, capacity and rules")
        bullets(image, HOW, active={n - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where it fits")
        bullets(image, USES, active={n - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
