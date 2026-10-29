"""The slides of the AI Knowledge video."""
from brand import bullets, slide_base

WHY = [
    ("1", "A general model guesses", "It does not know your prices, policies or help articles"),
    ("2", "Knowledge fixes that", "The assistant looks up passages that match each question"),
    ("3", "Sources for site content", "Numbered statements, and a numbered list under the answer"),
    ("check", "Search by meaning", "A question finds the right passage even when the wording differs"),
]

CHOOSE = [
    ("1", "Documents on a profile", "A few files, kept current by uploading new versions"),
    ("2", "Attachments in a chat", "Files a person wants to ask about, for that chat only"),
    ("3", "A data source", "Large or changing content that the site keeps in step"),
    ("4", "A file source or a web crawler", "A folder of files, or a public website, read on a schedule"),
]

MORE = [
    ("1", "Data source", "A knowledge base from a search index or a database"),
    ("2", "File source", "A folder on the server, an FTP site or an SFTP site"),
    ("3", "Web crawler", "A public website, page by page, with links back"),
    ("4", "Documentation search tool", "A public documentation site, searched live when needed"),
]

WELL = [
    ("1", "Preemptive retrieval", "Search the knowledge before every answer, or only when needed"),
    ("2", "Restrict answers", "Answer only from retrieved data, and say so when nothing matches"),
    ("3", "Check the index", "Has indexing finished, and does the passage hold the answer?"),
    ("4", "Tune the search", "A lower Strictness, more Retrieved documents, clear instructions"),
]

CASES = [
    ("1", "Support", "Answers from policies and help articles, with a link to the source"),
    ("2", "Sales", "A price list or product sheet in every conversation"),
    ("3", "Staff", "A handbook assistant for leave, travel and tools"),
    ("4", "Review", "With data sources, people check the numbered sources"),
]


def slide(step_id, chapter_title, label):
    kinds = {
        "why": ("Why knowledge", WHY, lambda n: set(range(n))),
        "choose": ("Four ways to add knowledge", CHOOSE, lambda n: {n - 1}),
        "more": ("More sources of knowledge", MORE, lambda n: {n - 1}),
        "well": ("Keep answers on your content", WELL, lambda n: {n - 1}),
        "case": ("Where knowledge helps", CASES, lambda n: {n - 1}),
    }
    prefix, _, number = step_id.partition("-")

    if prefix in kinds:
        heading, items, active = kinds[prefix]
        image = slide_base(chapter_title, label, heading)
        bullets(image, items, active=active(int(number)))
        return image

    raise SystemExit(f"No slide for {step_id}")
