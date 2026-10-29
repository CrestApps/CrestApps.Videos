"""The slides of the Campaigns video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Work needs a name", "A report can tell a lead drive from renewal reminders"),
    ("2", "Agents sign in to a campaign", "That is how they receive its dialer calls"),
    ("3", "Groups roll campaigns up", "Several campaigns, one line in a report"),
    ("check", "Only groups and reports", "The channel, number and outcomes come from elsewhere"),
]

WHERE = [
    ("1", "Activity loads", "The campaign is stamped on every activity the load creates"),
    ("2", "Dialer loads", "Required: the line of work agents sign in to"),
    ("3", "Agent entitlements", "Allowed campaigns lists what an agent may sign in to"),
    ("4", "Reports", "The campaign and the campaign group are filters"),
]

FLOW = [
    ("1", "Create the campaign", "And add it to the entitlements of the agents who work it"),
    ("2", "Load the work", "A dialer load creates an activity for each contact"),
    ("3", "Agents sign in and dial", "In the soft phone, the dialer offers or places the calls"),
    ("4", "Report the results", "By campaign and by campaign group"),
]

USES = [
    ("1", "One campaign for each push", "A lead drive, a renewal round, an appointment reminder"),
    ("2", "A group for each season or region", "One report line for all of them"),
    ("3", "Calls and texts share a campaign", "Their results are reported together"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Name the work, and roll it up")
        bullets(image, WHY, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("flow-"):
        image = slide_base(chapter_title, label, "From creation to the report")
        bullets(image, FLOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("where-"):
        image = slide_base(chapter_title, label, "Where a campaign is used")
        bullets(image, WHERE, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "How teams use campaigns")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
