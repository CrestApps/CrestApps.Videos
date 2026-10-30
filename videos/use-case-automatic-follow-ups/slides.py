"""The slides of the Use case: Follow Up Automatically video (slides only)."""
from brand import bullets, slide_base

CHAPTERS = {'who': ('Conversations that do not die', [('The AI texts your contacts', 'And holds the conversation'), ('A cadence follows up', 'After the silence you chose'), ('Only in business hours', "In the customer's own time zone"), ('Never nagging', 'A reply or STOP ends it')]), 'need': ('What cadences follow', [('Automated conversations only', 'From an automatic activity load'), ('The features', 'SMS Omnichannel Automation'), ('The permissions', 'Cadences, business hours, loads, flows')]), 'setup': ('Profile, outcomes, number, hours', [('Create the AI profile', 'A Text messaging starting point'), ('Set up the subject', 'An outbound subject, with outcomes'), ('Add the number and the hours', 'Text messages ticked; a business hours calendar')]), 'work': ('Steps, load, review', [('Build the cadence', 'Interaction Center, Management, Cadences'), ('Load the conversations', 'An automatic activity load'), ('Review the results', 'Review AI conversation')]), 'chk': ('With your own phone', [('A test contact', 'Your own number, a short first step'), ('Stay silent', 'The follow-up arrives'), ('Reply, or text STOP', 'No more follow-ups')]), 'tip': ('Good to know', [('The number of steps', 'The most follow-ups anyone gets'), ('Counted from the last message', 'Not from the start'), ('AI guidance', 'Steer the tone')])}


def slide(step_id, chapter_title, label):
    prefix, number = step_id.rsplit("-", 1)
    subtitle, items = CHAPTERS[prefix]
    image = slide_base(chapter_title, label, subtitle)
    bullets(image, [(str(i + 1), t, s) for i, (t, s) in enumerate(items)], active={int(number) - 1})
    return image
