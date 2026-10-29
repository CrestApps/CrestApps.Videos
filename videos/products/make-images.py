"""Makes fake product pictures (plain colored cards with the product name) for the demo store.

Usage: python make-images.py <the site's App_Data>/Sites/Default/Media/products
"""
import os
import sys
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

FONT = str(Path(__file__).resolve().parents[2] / "branding" / "fonts" / "Inter-Bold.ttf")
OUT = sys.argv[1]
os.makedirs(OUT, exist_ok=True)

ITEMS = [
    ("trail-runner", "Trail Runner", (42, 129, 187)),
    ("ceramic-mug", "Ceramic Mug", (234, 164, 41)),
    ("canvas-backpack", "Canvas Backpack", (96, 130, 99)),
    ("wool-beanie", "Wool Beanie", (170, 82, 82)),
    ("water-bottle", "Water Bottle", (60, 160, 160)),
    ("desk-lamp", "Desk Lamp", (120, 100, 170)),
    ("notebook", "Notebook", (190, 120, 60)),
    ("travel-hoodie", "Travel Hoodie", (70, 90, 120)),
    ("leather-wallet", "Leather Wallet", (140, 90, 60)),
    ("earbuds", "Wireless Earbuds", (80, 80, 90)),
    ("gift-card", "Gift Card", (200, 70, 110)),
    ("desk-organizer", "Desk Organizer", (90, 120, 150)),
    ("starter-kit", "Starter Kit", (50, 110, 80)),
]

font = ImageFont.truetype(FONT, 64)
for slug, name, color in ITEMS:
    w, h = 1000, 750
    img = Image.new("RGB", (w, h), color)
    d = ImageDraw.Draw(img)
    # a lighter band and a darker disc give each card some depth
    light = tuple(min(255, c + 40) for c in color)
    dark = tuple(max(0, c - 40) for c in color)
    d.ellipse((w * 0.62, -h * 0.25, w * 1.25, h * 0.55), fill=light)
    d.ellipse((-w * 0.2, h * 0.55, w * 0.38, h * 1.25), fill=dark)
    d.rounded_rectangle((w * 0.33, h * 0.28, w * 0.67, h * 0.62), radius=40, fill=(255, 255, 255))
    tw = d.textlength(name, font=font)
    d.text(((w - tw) / 2, h * 0.74), name, font=font, fill=(255, 255, 255))
    img.save(os.path.join(OUT, slug + ".jpg"), quality=88)
print("ok", len(ITEMS))
