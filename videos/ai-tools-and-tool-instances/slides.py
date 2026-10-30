"""The slides of the AI Tools and Tool Instances video."""
from brand import bullets, slide_base

WHAT = [
    ("1", "A tool is an action", "The AI decides when to use it"),
    ("2", "Built-in tools", "From Orchard Core AI Agent, they act on the site"),
    ("3", "Tool instances", "Tools you set up yourself, each with a permission"),
]

WHO = [
    ("1", "Two permissions", "Using a profile, and using its tools"),
    ("2", "Left out quietly", "The assistant answers without the tool"),
    ("3", "Prefer single tools", "Access any AI tool is for administrators"),
]

KINDS = [
    ("1", "HTTP API Request, documentation search", "A web service, or a documentation site"),
    ("2", "Website search, data source search", "A site's own search, or one of your data sources"),
    ("3", "A data source search does not restrict answers", "Use the Knowledge tab for that"),
    ("4", "Share with other AI apps", "Pick them in the MCP Server settings"),
]

USES = [
    ("1", "A shop", "An order lookup instance"),
    ("2", "A software company", "A documentation search"),
    ("3", "An administrator", "Only the tools its editors need"),
]


def slide(step_id, chapter_title, label):
    table = {
        "what-": ("Tools let an assistant act", WHAT),
        "who-": ("Who can use a tool", WHO),
        "kin-": ("Kinds of tool instances", KINDS),
        "use-": ("Ways to use it", USES),
    }
    for prefix, (subtitle, items) in table.items():
        if step_id.startswith(prefix):
            image = slide_base(chapter_title, label, subtitle)
            bullets(image, items, active={int(step_id[-1]) - 1})
            return image
    raise SystemExit(f"No slide for {step_id}")
