"""Writes the fake event file with two rows without a title that the Content Transfer Recovery video uploads (all invented). Usage: python make-data.py <folder>"""
import csv
import sys
from pathlib import Path

HEADER = ["TitlePart_Title", "Showcase_Location_Text", "Showcase_Seats_Value", "Showcase_EventDate_Value", "Showcase_Featured_Value"]
ROWS = [
    ["Autumn Open House", "Harbor Hall", "80", "2026-11-14", "True"],
    ["", "Pinecrest Studio", "40", "2026-11-21", "False"],
    ["Spring Tasting", "Riverbend Cafe", "60", "2027-03-12", "True"],
    ["", "Summit Park", "300", "2026-12-12", "False"],
    ["Winter Gala", "Harbor Hall", "150", "2027-12-04", "True"],
    ["Harvest Fair", "Pinecrest Fields", "500", "2026-10-30", "True"],
]

folder = Path(sys.argv[1] if len(sys.argv) > 1 else "data")
folder.mkdir(parents=True, exist_ok=True)
with open(folder / "bad-events.csv", "w", newline="", encoding="utf-8") as handle:
    writer = csv.writer(handle)
    writer.writerow(HEADER)
    writer.writerows(ROWS)
print(len(ROWS), "rows, 2 of them without a title")
