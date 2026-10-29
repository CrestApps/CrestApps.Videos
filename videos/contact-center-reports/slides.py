"""The slides of the Contact Center Reports video."""
from brand import bullets, slide_base

WHY = [
    ("1", "A number, not an impression", "How many calls, how many missed, how long callers waited"),
    ("2", "Any slice you need", "The whole center, one queue or one agent, over any period"),
    ("check", "Worked out from the records", "A total always matches the calls and states behind it"),
]

NEEDS = [
    ("1", "View Contact Center reports", "Supervisor and Administrator roles have it; CRM reports need View Omnichannel reports"),
    ("2", "Reports and Contact Center Work Distribution", "The two features that bring the Contact Center reports"),
    ("3", "Reports (OpenXml) and Reports (PDF)", "Add Export Excel and Export PDF to the Export menu"),
]

MORE = [
    ("1", "Reports > Subscriptions", "Email any report every day, week or month"),
    ("2", "Report Builder reports", "Cannot be subscribed to: a workflow with the Email Report task sends them"),
]

USES = [
    ("1", "Monday review", "Executive dashboard for Last Week, then export the PDF"),
    ("2", "Staffing plan", "Hour-of-day performance for Last 30 Days"),
    ("3", "Payroll", "Reconciled payroll timecard, one month at a time, as Excel"),
    ("4", "Queue tuning", "Check a queue's service level before changing its overflow"),
]


def slide(step_id, chapter_title, label):
    table = {"why-": (WHY, "Why reports"), "needs-": (NEEDS, "What a report needs"),
             "more-": (MORE, "Beyond the page"), "use-": (USES, "Reports at work")}
    for prefix, (cards, title) in table.items():
        if step_id.startswith(prefix):
            image = slide_base(chapter_title, label, title)
            number = int(step_id.split("-")[-1])
            if prefix == "needs-":
                active = {0} if number == 1 else {1, 2}
            elif prefix == "more-":
                active = {0, 1}
            elif prefix in ("why-",):
                active = set(range(number))
            else:
                active = {number - 1}
            bullets(image, cards, active=active)
            return image
    raise SystemExit(f"No slide for {step_id}")
