"""Builds the fake documents of the AI Documents video: a Word report with a chart, a PDF price sheet."""
import io
import sys
import zipfile
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

out = Path(sys.argv[1])
out.mkdir(parents=True, exist_ok=True)


def font(size, bold=False):
    for name in ("arialbd.ttf" if bold else "arial.ttf", "segoeuib.ttf" if bold else "segoeui.ttf"):
        try:
            return ImageFont.truetype("C:/Windows/Fonts/" + name, size)
        except OSError:
            pass
    return ImageFont.load_default()


# the chart: unit sales by quarter. The numbers exist only in the picture.
W, H = 1200, 700
img = Image.new("RGB", (W, H), "white")
d = ImageDraw.Draw(img)
d.text((60, 30), "Contoso unit sales by quarter", fill="#1f2937", font=font(40, True))
data = [("Q1", 420), ("Q2", 560), ("Q3", 910), ("Q4", 640)]
base, top, left = 620, 130, 120
d.line((left, base, W - 60, base), fill="#6b7280", width=3)
d.line((left, top, left, base), fill="#6b7280", width=3)
for i, (q, v) in enumerate(data):
    x = left + 80 + i * 250
    h = int((v / 1000) * (base - top))
    d.rectangle((x, base - h, x + 150, base), fill="#2563eb" if q != "Q3" else "#f59e0b")
    d.text((x + 35, base - h - 48), str(v), fill="#111827", font=font(36, True))
    d.text((x + 50, base + 12), q, fill="#111827", font=font(36))
buf = io.BytesIO()
img.save(buf, "PNG")
png = buf.getvalue()

content_types = """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Default Extension="png" ContentType="image/png"/><Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/></Types>"""
rels = """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/></Relationships>"""
doc_rels = """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId10" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/image" Target="media/chart.png"/></Relationships>"""

cx, cy = 5486400, int(5486400 * H / W)


def para(text, bold=False):
    b = "<w:rPr><w:b/><w:sz w:val=\"36\"/></w:rPr>" if bold else ""
    return f"<w:p><w:r>{b}<w:t xml:space=\"preserve\">{text}</w:t></w:r></w:p>"


drawing = f"""<w:p><w:r><w:drawing><wp:inline distT="0" distB="0" distL="0" distR="0"><wp:extent cx="{cx}" cy="{cy}"/><wp:docPr id="1" name="Chart" descr=""/><a:graphic xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"><a:graphicData uri="http://schemas.openxmlformats.org/drawingml/2006/picture"><pic:pic xmlns:pic="http://schemas.openxmlformats.org/drawingml/2006/picture"><pic:nvPicPr><pic:cNvPr id="0" name="chart.png"/><pic:cNvPicPr/></pic:nvPicPr><pic:blipFill><a:blip r:embed="rId10"/><a:stretch><a:fillRect/></a:stretch></pic:blipFill><pic:spPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="{cx}" cy="{cy}"/></a:xfrm><a:prstGeom prst="rect"><a:avLst/></a:prstGeom></pic:spPr></pic:pic></a:graphicData></a:graphic></wp:inline></w:drawing></w:r></w:p>"""
body = (
    para("Contoso Annual Sales Review", True)
    + para("This review covers the unit sales of the Contoso router for last year. The team is happy with the year and plans to keep the same pricing.")
    + para("The chart below shows the unit sales of each quarter.")
    + drawing
    + para("Support requests stayed flat during the year.")
)
document = f"""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships" xmlns:wp="http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing"><w:body>{body}</w:body></w:document>"""

with zipfile.ZipFile(out / "annual-sales-review.docx", "w", zipfile.ZIP_DEFLATED) as z:
    z.writestr("[Content_Types].xml", content_types)
    z.writestr("_rels/.rels", rels)
    z.writestr("word/document.xml", document)
    z.writestr("word/_rels/document.xml.rels", doc_rels)
    z.writestr("word/media/chart.png", png)

# a text file the user could attach: a short policy
(out / "return-policy.md").write_text(
    "# Contoso return policy\n\nYou can return most items within 45 days of delivery. Refunds reach the original payment method within 5 business days after the warehouse receives the item. Opened software cannot be returned.\n",
    encoding="utf-8",
)
print("ok", [p.name for p in out.iterdir()])


# the same report as a PDF: real text, and the chart as a picture
def make_pdf(path):
    jpg = io.BytesIO()
    img.convert("RGB").save(jpg, "JPEG", quality=92)
    jpg = jpg.getvalue()
    iw, ih = 480, int(480 * H / W)
    lines = [
        (72, 720, 20, "Contoso Annual Sales Review"),
        (72, 690, 11, "This review covers the unit sales of the Contoso router for last year."),
        (72, 674, 11, "The team is happy with the year and plans to keep the same pricing."),
        (72, 650, 11, "Figure 1 shows the unit sales of each quarter."),
        (72, 340, 10, "Figure 1: Unit sales by quarter"),
        (72, 250, 11, "Support requests stayed flat during the year."),
    ]
    text = "".join(f"BT /F1 {s} Tf {x} {y} Td ({t}) Tj ET\n" for x, y, s, t in lines)
    content = text + f"q {iw} 0 0 {ih} 72 {640 - ih} cm /Im1 Do Q\n"
    objs = [
        "<< /Type /Catalog /Pages 2 0 R >>",
        "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
        "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> /XObject << /Im1 6 0 R >> >> >>",
        None,
        "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
        None,
    ]
    out = bytearray(b"%PDF-1.4\n")
    offsets = []

    def add(body):
        offsets.append(len(out))
        out.extend(f"{len(offsets)} 0 obj\n".encode() + body + b"\nendobj\n")

    for i, o in enumerate(objs):
        if i == 3:
            c = content.encode("latin-1")
            add(f"<< /Length {len(c)} >>\nstream\n".encode() + c + b"\nendstream")
        elif i == 5:
            add(f"<< /Type /XObject /Subtype /Image /Width {W} /Height {H} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length {len(jpg)} >>\nstream\n".encode() + jpg + b"\nendstream")
        else:
            add(o.encode())
    xref = len(out)
    out.extend(f"xref\n0 {len(offsets) + 1}\n0000000000 65535 f \n".encode())
    for off in offsets:
        out.extend(f"{off:010d} 00000 n \n".encode())
    out.extend(f"trailer\n<< /Size {len(offsets) + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n".encode())
    Path(path).write_bytes(bytes(out))


make_pdf(out / "annual-sales-review.pdf")
print("pdf ok")
