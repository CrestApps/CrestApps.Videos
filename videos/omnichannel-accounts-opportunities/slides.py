"""The slides of the Omnichannel Accounts and Opportunities video."""
from brand import bullets, slide_base

WHY = [
    ("1", "One company, several people", "Looking at each person alone hides the relationship"),
    ("2", "An account groups them", "Its contacts and its opportunities hang under it"),
    ("check", "An opportunity is a deal", "A stage, an amount, a close date and a primary contact"),
]

REPORTS = [
    ("1", "Where to find them", "Reports, CRM and Campaigns, with View Omnichannel reports"),
    ("2", "Lead funnel", "Leads by current status, with the share converted"),
    ("3", "Lead conversion by source and list", "Leads, conversions, rate and average time to convert"),
    ("4", "Opportunity pipeline", "Open deals by stage, weighted amount, and the win rate"),
]

USE = [
    ("1", "A company on one page", "Every person and every deal"),
    ("2", "The whole conversation", "Activities across all the contacts of an account"),
    ("3", "Deals by stage and amount", "What is closing this month"),
    ("4", "Pipeline and source reports", "How well each lead source converts"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "One company, many people and deals")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("reports-"):
        image = slide_base(chapter_title, label, "Three reports under Reports")
        bullets(image, REPORTS, active={1: {0}, 2: {1, 2}, 3: {3}}[int(step_id[-1])])
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four typical uses")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
