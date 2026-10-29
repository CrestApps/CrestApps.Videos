"""The slides of the Agent-to-Agent (A2A) video."""
from brand import bullets, slide_base

WHY = [
    ("1", "One assistant for everything", "Its instructions grow, and its answers lose focus"),
    ("2", "A specialist has one job", "Its own instructions and tools, so it does the job well"),
    ("3", "A2A connects systems", "Agents on different systems find each other and work together"),
    ("check", "Offer and use agents", "Your agents for others, and the agents of others for you"),
]

HOW = [
    ("1", "Agent profile", "A profile of type Agent, with a description of what it does"),
    ("2", "A2A Host", "Offers every Agent profile to other systems"),
    ("3", "A2A Client", "Connections to the hosts of other systems"),
    ("check", "A feature each", "Only the menus of the features you turned on appear"),
]

HOST = [
    ("1", "Every Agent profile is offered", "With the Agent-to-Agent Host feature on"),
    ("2", "Found in the agent card", "Other systems discover the agents, and message them"),
    ("3", "Callers sign in", "No sign-in for testing, an API key, or OpenID Connect"),
    ("check", "A permission when asked", "Access the A2A Host"),
]

CASES = [
    ("1", "Specialists", "A legal, finance or warranty agent, with its own instructions"),
    ("2", "Partners", "A supplier or sister company offers an agent that you call"),
    ("3", "Reuse", "One agent serves many assistants, kept in one place"),
    ("4", "Care", "Describe each agent precisely, and give only the agents needed"),
]


def slide(step_id, chapter_title, label):
    kinds = {
        "why": ("Why agents", WHY, lambda n: set(range(n))),
        "how": ("Agents, a host and a client", HOW, lambda n: {n - 1}),
        "host": ("The A2A Host", HOST, lambda n: {n - 1}),
        "case": ("Where agents help", CASES, lambda n: {n - 1}),
    }
    prefix, _, number = step_id.partition("-")

    if prefix in kinds:
        heading, items, active = kinds[prefix]
        image = slide_base(chapter_title, label, heading)
        bullets(image, items, active=active(int(number)))
        return image

    raise SystemExit(f"No slide for {step_id}")
