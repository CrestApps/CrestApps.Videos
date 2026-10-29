"""The slides of the Outbound Calling Campaign video."""
from brand import bullets, slide_base

WHO = [
    ("1", "Managers, agents and supervisors", "Lead drives, renewals, appointment reminders"),
    ("2", "Contacts become call activities", "For one campaign that agents sign in to"),
    ("3", "Every call is checked first", "Opt-outs, registries, calling window, dead numbers"),
    ("check", "Outcomes drive the follow-up", "The subject flow retries, and reports measure"),
]


ORDER = [
    ("1", "The data and the outcomes", "Contacts, subject, dispositions, subject flow"),
    ("2", "The rules", "Campaign, business hours calendar, dialer profile"),
    ("3", "The people", "The campaign on each agent's entitlements"),
    ("4", "The work", "Load a list, and agents dial it"),
]

DIAL = [
    ("1", "Sign in and be Available", "The Work tab of the soft phone"),
    ("2", "Preview", "The agent reviews each record, then clicks Dial or Skip"),
    ("3", "Power and Progressive", "The dialer places the call, and the record opens"),
    ("4", "Complete with a disposition", "That runs the subject flow: retry or follow-up"),
]

REPORTS = [
    ("1", "Campaign summary and Subject inventory", "How far the campaign got"),
    ("2", "Disposition breakdown and Agent productivity", "How calls ended, and who worked them"),
    ("3", "Dialer call detail", "The numbers behind the counts"),
]

TIPS = [
    ("1", "Check it with a small list", "A test agent, Available, and a Preview offer or a Power call"),
    ("2", "Complete with Call back", "Check that the flow scheduled the next attempt"),
]


def slide(step_id, chapter_title, label):
    table = {
        "who-": ("Who it is for", WHO),
        "order-": ("Four groups of steps", ORDER),
        "dial-": ("What agents do", DIAL),
        "tip-": ("Before the first real list", TIPS),
    }
    for prefix, (heading, items) in table.items():
        if step_id.startswith(prefix):
            image = slide_base(chapter_title, label, heading)
            n = int(step_id.split("-")[-1])
            bullets(image, items, active={2, 3} if (prefix == "who-" and n == 3) else {min(n, len(items)) - 1})
            return image
    raise SystemExit(f"No slide for {step_id}")
