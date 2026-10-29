"""The slides of the Where Call Recordings Are Stored video."""
from brand import bullets, slide_base

WHERE = [
    ("1", "A folder of the site, encrypted", "Recordings and the master copy of voice media clips, encrypted before they are written"),
    ("2", "Downloaded from the provider", "The provider records; the platform fetches the finished recording into its own store"),
]

KNOW = [
    ("1", "Deleting a voice media clip", "Removes its master copy and every provider copy, in every environment that shares the store"),
    ("2", "Deleting a recording", "Erases the file; a legal hold blocks it; only administrators can by default"),
]

# step-id prefix -> (cards, heading, how the cards light up: "all" up to the step number, "one" the card of the step, or a fixed set per step id)
TABLE = {"where-": (WHERE, "Where they live", "all"), "know-": (KNOW, "Good to know", "one")}


def slide(step_id, chapter_title, label):
    for prefix, (cards, heading, active) in TABLE.items():
        if step_id.startswith(prefix):
            image = slide_base(chapter_title, label, heading)
            number = int(step_id.split("-")[-1])
            lit = active.get(step_id) if isinstance(active, dict) else (set(range(number)) if active == "all" else {number - 1})
            bullets(image, cards, active=lit)
            return image
    raise SystemExit(f"No slide for {step_id}")
