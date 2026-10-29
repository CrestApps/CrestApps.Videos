"""The slides of the Omnichannel CRM Setup video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Prospects are not customers yet", "A list, a scan or a web form should not fill the contact list"),
    ("2", "Teams need to see progress", "Which prospects qualified, and which deals are worth how much"),
    ("3", "Leads, accounts and opportunities", "Convert the leads that qualify, and group contacts and deals"),
    ("check", "Lists you control", "Statuses, stages and sources follow your way of selling"),
]

WHAT = [
    ("1", "Lead and account", "A prospect you can call and text, and the company or household of your contacts"),
    ("2", "Opportunity", "A deal with a stage, an amount and a close date"),
    ("3", "Statuses, stages and sources", "Where a lead is, where a deal is, and where a lead came from"),
    ("check", "A starter recipe", "Sets it all up, and every type can be renamed or extended"),
]

USE = [
    ("1", "Purchased lists", "Work them as leads and keep contacts for customers"),
    ("2", "Trade-show leads by source", "See which source converts best"),
    ("3", "Deals by stage", "A probability for the weighted pipeline"),
    ("4", "New sales and renewals", "Different opportunity types, each with its own stages"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Prospects are not customers yet")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("what-"):
        image = slide_base(chapter_title, label, "Four kinds of record and three lists")
        bullets(image, WHAT, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four typical uses")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
