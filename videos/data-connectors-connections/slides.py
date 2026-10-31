"""The slides of the Data connections video."""
from brand import bullets, slide_base

SOURCES = [
    ("1", "A sales web API", "Orders, customers, anything the API answers"),
    ("2", "Files someone exports", "CSV, JSON, JSON Lines or Excel in the media library"),
    ("3", "A search index", "Elasticsearch or OpenSearch"),
    ("4", "A database", "Tables, views and read-only queries of SQL Server"),
]

KINDS = [
    ("1", "SQL Server", "Server, database and how to sign in"),
    ("2", "HTTP API", "Base URL and one of six ways to authenticate"),
    ("3", "Elasticsearch", "Cluster URL and authentication"),
    ("4", "Media library files", "A folder of the media library"),
]


def slide(step_id, chapter_title, label):
    if step_id in ("why-1", "why-2"):
        image = slide_base(chapter_title, label, "Data that lives somewhere else")
        bullets(image, SOURCES, active={"why-1": {0, 1, 2, 3}, "why-2": {0, 1, 2, 3}}[step_id])
        return image
    if step_id in ("why-3", "why-4"):
        image = slide_base(chapter_title, label, "Four kinds of connection")
        bullets(image, KINDS, active={0, 1, 2, 3})
        return image
    raise SystemExit(f"No slide for {step_id}")
