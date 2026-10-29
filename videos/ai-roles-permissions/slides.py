"""The slides of the AI Roles and Permissions video."""
from brand import bullets, slide_base

WHO = [
    ('1', 'Builders', 'Profiles, deployments, knowledge, tools'),
    ('2', 'Analysts', 'Chat reports and exports'),
    ('3', 'Staff', 'Chat with the assistants of their work'),
    ('4', 'Anonymous visitors', 'Only a public assistant'),
]

TR = [
    ('1', 'Admin works, others do not', 'A permission problem'),
    ('2', 'Check the role', 'The right permission, in the right group'),
    ('3', 'Tools and public chats', 'Tool permissions and the Anonymous role'),
]


def slide(step_id, chapter_title, label):
    kinds = {
        'who': ('Four kinds of people', WHO, lambda n: set(range(n))),
        'tr': ('A short checklist', TR, lambda n: {n - 1}),
    }
    prefix, _, number = step_id.partition("-")

    if prefix in kinds:
        heading, items, active = kinds[prefix]
        image = slide_base(chapter_title, label, heading)
        bullets(image, items, active=active(int(number)))
        return image

    raise SystemExit(f"No slide for {step_id}")
