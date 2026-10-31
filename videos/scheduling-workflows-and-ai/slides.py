"""The slides of the Scheduling: workflows, AI and moving it video."""
from brand import bullets, slide_base

RULES = [
    ("1", "Only what customers can book", "Appointment types with Customers can book it themselves"),
    ("2", "Find and book", "List appointment types, find open times, and book one"),
    ("3", "Move or cancel with proof", "The code from the manage link, or the email or phone number booked with"),
    ("check", "Staff keep the controls", "Staff who can manage appointments may use every appointment type"),
]

CHECKS = [
    ("1", "No times are offered", "The type needs resources that take appointments, hours that cover the day, and room within the limits"),
    ("2", "Something is busy", "A blocked time or event on the resource's calendar, or on the staff member's own"),
    ("3", "Check the type and resource", "Can be booked, Takes appointments, and the minimum notice"),
]

PAGE = [
    ("1", "The booking page isn't found", "Can be booked, Customers can book it themselves, and a Booking page address"),
    ("2", "The feature is on", "Scheduling Self-Service must be enabled in Features"),
]

SHIFTS = [
    ("1", "A staff member with shifts is never offered", "Choose their Shift calendar"),
    ("2", "The shifts name them", "Each Shift event on that calendar must list the staff member as an attendee"),
]


def slide(step_id, chapter_title, label):
    if step_id == "ai-3":
        image = slide_base(chapter_title, label, "What the assistant can do")
        bullets(image, RULES)
        return image
    if step_id == "trouble-1":
        image = slide_base(chapter_title, label, "No times are offered")
        bullets(image, CHECKS)
        return image
    if step_id == "trouble-2":
        image = slide_base(chapter_title, label, "The booking page isn't found")
        bullets(image, PAGE)
        return image
    if step_id == "trouble-3":
        image = slide_base(chapter_title, label, "Staff on shifts")
        bullets(image, SHIFTS)
        return image
    raise SystemExit(f"No slide for {step_id}")
