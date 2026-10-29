"""The slides of the Users and Roles video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Not everyone should change everything", "An editor writes pages, a supervisor manages a team, a visitor reads"),
    ("2", "Individual permissions do not scale", "A role is a named set that you give to many people at once"),
    ("3", "Sign-in names are hard to read", "People expect to see Dana Whitfield, with a picture"),
    ("check", "Simple access, recognizable names", "Users and roles give you both"),
]

HOW = [
    ("1", "User", "One person's account: user name, email and password"),
    ("2", "Role", "A named set of permissions; a person can hold several"),
    ("3", "Names and avatars", "Settings choose how each person's name and picture appear"),
    ("check", "Roles on content", "An item can be limited to the people who hold a picked role"),
]

USE = [
    ("1", "A role for each job", "Agent, Supervisor and Manager, each with the right access"),
    ("2", "Onboarding a new team member", "One account and one role"),
    ("3", "Full names everywhere", "Author badges and user pickers show people, not sign-in names"),
    ("4", "Passwordless sign-in", "Occasional users sign in with an emailed code"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Access you can manage, and names people recognize")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Users, roles, permissions and names")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Four typical uses")
        bullets(image, USE, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
