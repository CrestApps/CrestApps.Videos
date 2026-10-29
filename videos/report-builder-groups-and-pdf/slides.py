"""The slides of the Report groups and PDF export video."""
from pathlib import Path

from PIL import Image, ImageDraw

from brand import BORDER, CODE_BACKGROUND, MARGIN, MUTED, PANEL, TEXT, W, bullets, font, mono, slide_base

DOWNLOADS = Path(__file__).resolve().parent / "shots" / "downloads"

WHY = [
    ("1", "A folder for reports", "Sales can hold Regional, which holds the regional reports"),
    ("2", "In the admin menu", "Each group is an item under Reports: groups first, then reports"),
    ("3", "Never empty", "A group shows only when it holds a report you may open"),
]

USES = [
    ("1", "A team menu", "Service, Sales and Finance: each team opens its own reports"),
    ("2", "A monthly pack", "A PDF of the dashboard for a manager who does not open the site"),
    ("3", "An upgrade", "Categories become top-level groups with the same names"),
]


def _window(image, box, title):
    x0, y0, x1, y1 = box
    draw = ImageDraw.Draw(image)
    draw.rounded_rectangle(box, radius=14, fill=CODE_BACKGROUND, outline=BORDER, width=2)
    draw.rounded_rectangle((x0, y0, x1, y0 + 52), radius=14, fill=PANEL)
    draw.rectangle((x0, y0 + 30, x1, y0 + 52), fill=PANEL)
    for index, color in enumerate([(237, 106, 94), (245, 191, 79), (98, 197, 84)]):
        draw.ellipse((x0 + 22 + index * 26, y0 + 19, x0 + 36 + index * 26, y0 + 33), fill=color)
    draw.text((x0 + 112, y0 + 26), title, font=font(21, "SemiBold"), fill=MUTED, anchor="lm")
    return draw


def _find(extension):
    files = sorted(DOWNLOADS.glob(f"*{extension}"))
    if not files:
        raise SystemExit(f"No downloaded {extension} file in {DOWNLOADS}")
    return files[0]


def _pdf_slide(image):
    import fitz

    path = _find(".pdf")
    document = fitz.open(path)
    page = document[0]
    box_height = 1020 - 240
    zoom = box_height / page.rect.height * 2
    pixmap = page.get_pixmap(matrix=fitz.Matrix(zoom, zoom))
    page_image = Image.frombytes("RGB", (pixmap.width, pixmap.height), pixmap.samples)
    page_image = page_image.resize((int(page_image.width / 2), int(page_image.height / 2)))
    x = (W - page_image.width) // 2
    ImageDraw.Draw(image).rounded_rectangle((x - 6, 234, x + page_image.width + 6, 1026), radius=8, fill=BORDER)
    image.paste(page_image, (x, 240))


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Reports in folders")
        bullets(image, WHY, active={"why-1": {0}, "why-2": {0, 1}, "why-3": {2}}[step_id])
        return image
    if step_id == "file-1":
        image = slide_base(chapter_title, label, "The PDF")
        _pdf_slide(image)
        return image
    if step_id.startswith("use-"):
        image = slide_base(chapter_title, label, "Where groups fit")
        bullets(image, USES, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
