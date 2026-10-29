"""The slides of the Model Context Protocol video."""
from brand import bullets, slide_base

WHY = [
    ("1", "An assistant needs tools", "Look things up, calculate, read a company file"),
    ("2", "They live in many places", "Web services, ticket systems, shared drives, desktop apps"),
    ("3", "One link for each is slow", "Every AI app would need its own connection to each tool"),
    ("check", "One standard, MCP", "Plug in any tool server, and be used by any AI app"),
]

HOW = [
    ("1", "MCP client", "A connection brings an outside server's tools to your assistants"),
    ("2", "MCP server", "Your site shares the tools you pick with other AI apps"),
    ("3", "Prompts and resources", "Ready-made instructions, and files an app can read"),
    ("check", "A feature each", "Only the menus of the features you turned on appear"),
]

AUTH = [
    ("1", "Anonymous", "No sign-in: open or private-network servers only"),
    ("2", "API Key and Basic", "A key in a header, or a user name and password"),
    ("3", "OAuth 2.0", "A token from the Token Endpoint, by secret, private key or certificate"),
    ("4", "Custom Headers", "Any other headers the server asks for"),
]

CASES = [
    ("1", "Live facts", "Time-zone, weather or ticket services"),
    ("2", "Company tools", "The tools your technical team built, through one connection"),
    ("3", "Other AI apps", "Staff use the tools of your site from their favorite app"),
    ("4", "Shared wording", "Publish the prompts every team should use"),
]

SAFE = [
    ("1", "Permissions", "Manage MCP Connections, Prompts and Resources, plus the server settings"),
    ("2", "Tools really act", "Share only what you are comfortable having called"),
    ("3", "Sign in properly", "API key or OpenID Connect: Anonymous is for testing"),
    ("check", "Start small", "One connection, one assistant, then widen"),
]


def slide(step_id, chapter_title, label):
    kinds = {
        "why": ("Why a common standard", WHY, lambda n: set(range(n))),
        "how": ("A client, a server, and what they share", HOW, lambda n: {n - 1}),
        "auth": ("The authentication options", AUTH, lambda n: {n - 1}),
        "case": ("What teams connect", CASES, lambda n: {n - 1}),
        "safe": ("Permissions and good habits", SAFE, lambda n: {n - 1}),
    }
    prefix, _, number = step_id.partition("-")

    if prefix in kinds:
        heading, items, active = kinds[prefix]
        image = slide_base(chapter_title, label, heading)
        bullets(image, items, active=active(int(number)))
        return image

    raise SystemExit(f"No slide for {step_id}")
