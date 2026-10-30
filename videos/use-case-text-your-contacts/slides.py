"""The slides of the Use case: Text Your Contacts video (slides only)."""
from brand import bullets, slide_base

CHAPTERS = {'who': ('Two-way texting with customers', [('A shared inbox', 'Or one agent, depending on the number'), ('Agents work the conversations', 'Claim, reply, transfer, close'), ('Supervisors broadcast', 'Each reply is its own conversation')]), 'need': ('Provider, features and permissions', [('An SMS provider', 'Twilio or Telnyx, with numbers that can text'), ('The features', 'SMS Messaging Channel and the rest'), ('The permissions', 'Use the workspace; send group messages')]), 'setup': ('Provider, contacts, numbers, entry points', [('Connect the provider', 'Settings, Communication, SMS'), ('Import your contacts', 'Mark Do not SMS where needed'), ('Add your texting numbers', 'Omnichannel Addresses'), ('Route the incoming texts', 'A Text messages entry point per number')]), 'work': ('Templates, inbox, broadcasts', [('Write templates', 'Messaging, Templates'), ('Train agents on the inbox', 'Filter, claim, reply, transfer, close'), ('Send broadcasts', 'Messaging, Broadcasts')]), 'chk': ('From your own phone', [('Text your number', 'It lands in the inbox'), ('Reply, and start one', 'Claim, answer, then a new conversation'), ('Text STOP', 'Then START')]), 'tip': ('Good to know', [('Unassigned texts', 'A number with no entry point still receives texts'), ('Quiet hours', 'A banner warns; it does not block'), ('Broadcasts', 'Cannot be cancelled once queued')])}


def slide(step_id, chapter_title, label):
    prefix, number = step_id.rsplit("-", 1)
    subtitle, items = CHAPTERS[prefix]
    image = slide_base(chapter_title, label, subtitle)
    bullets(image, [(str(i + 1), t, s) for i, (t, s) in enumerate(items)], active={int(number) - 1})
    return image
