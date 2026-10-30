"""The slides of the Omnichannel Reports video."""
from brand import bullets, slide_base

WHY = [
    ("1", "A list is not a picture", "Your own to-do list does not show how much work is open, or who is behind"),
    ("2", "Any slice you need", "By source, channel, campaign, person, address or period"),
    ("3", "Worked out from the records", "A total always matches the activities, leads and deals behind it"),
    ("check", "From first lead to closed deal", "Activities, campaigns, dispositions, leads and opportunities"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "What the reports are for")
        number = int(step_id.split("-")[-1])
        bullets(image, WHY, active={0} if number == 1 else {1, 2, 3})
        return image
    raise SystemExit(f"No slide for {step_id}")
