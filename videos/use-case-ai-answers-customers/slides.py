"""The slides of the Use case: Let the AI Answer Your Customers video (slides only)."""
from brand import bullets, slide_base

CHAPTERS = {'who': ('An AI on the front line', [('Answers every text or call', 'Around the clock, to a number you choose'), ('Or reaches out', 'It texts or calls the contacts you load'), ('Picks an outcome', 'A disposition, a summary, your subject flow'), ('Hands over to a person', 'When asked, qualified or frustrated')]), 'need': ('AI, channels and queues', [('An AI connection', 'A provider and a chat deployment'), ('The channel features', 'SMS Omnichannel Automation, or Telnyx AI Voice Agent'), ('Entry points and queues', 'To answer numbers, and to hand over')]), 'setup': ('Profile, knowledge, number, outcomes', [('Create the AI profile', 'Pick a starting point, name your business'), ('Teach it about your company', 'Documents or data sources'), ('List the number', 'Voice calls or Text messages ticked'), ('Set the outcomes and the hand-off', 'An inbound automated subject')]), 'work': ('Entry point, outreach, review', [('Staff the hand-off queue', 'Calls ring an agent; texts reach the inbox'), ('Point the number at the AI', 'Route to: AI agent or AI voice agent'), ('Or let the AI reach out', 'An automatic activity load, with a cadence'), ('Review', 'Review AI conversation, and the hand-off report')]), 'chk': ('Text or call your own number', [('Talk to it', 'It greets you, or replies after its pause'), ('Ask for a person', 'A conversation, or a ring'), ('Read it afterwards', 'Review AI conversation')]), 'tip': ('Good to know', [('A closed entry point', 'No AI'), ('People first', 'An open conversation, or a recent one'), ('Start small', 'A test number and a few colleagues')])}


def slide(step_id, chapter_title, label):
    prefix, number = step_id.rsplit("-", 1)
    subtitle, items = CHAPTERS[prefix]
    image = slide_base(chapter_title, label, subtitle)
    bullets(image, [(str(i + 1), t, s) for i, (t, s) in enumerate(items)], active={int(number) - 1})
    return image
