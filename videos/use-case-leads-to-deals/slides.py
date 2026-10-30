"""The slides of the Use case: Turn Leads into Deals video (slides only)."""
from brand import bullets, slide_base

CHAPTERS = {'who': ('Prospects stay apart from customers', [('Leads stay apart', 'Your contact list holds only real customers'), ('Worked like contacts', 'Agents and the dialer call and text them'), ('Converted when they qualify', 'A contact, an account, an opportunity'), ('Measured', 'Funnel, conversion by source, pipeline')]), 'need': ('Features, permissions and channels', [('The features', 'Omnichannel CRM, Content Transfer, Reports'), ('The permissions', 'Convert leads, statuses and stages'), ('A way to reach them', 'Calling or texting')]), 'setup': ('Turn it on, review the lists, import', [('Turn it on', 'The CRM starter recipe, in Tools, Recipes'), ('Review your lists', 'Lead statuses, sources, opportunity stages'), ('Import your leads', 'Content, Import, with the lead type'), ('Work the leads', 'Interaction Center, Leads')]), 'work': ('Call, text, convert, measure', [('Call and text them', 'An activity load with the lead type'), ('Move them along', 'Set lead status on each flow action'), ('Convert the ones that qualify', 'Convert Lead, with an opportunity'), ('Work the deals', 'Opportunities and Accounts'), ('Measure', 'Lead funnel, conversion, pipeline')]), 'chk': ('A small test list', [('Import a small list', 'Two or three leads, one with your number'), ('Convert one', 'With Create an opportunity ticked'), ('Run the funnel', 'The Lead funnel report for today')]), 'tip': ('Good to know', [('A converted lead is read-only', 'Its history moves to the contact'), ('A number in two places', 'Inbound calls and texts match the contact'), ('Act on a conversion', 'A workflow on Lead Converted')])}


def slide(step_id, chapter_title, label):
    prefix, number = step_id.rsplit("-", 1)
    subtitle, items = CHAPTERS[prefix]
    image = slide_base(chapter_title, label, subtitle)
    bullets(image, [(str(i + 1), t, s) for i, (t, s) in enumerate(items)], active={int(number) - 1})
    return image
