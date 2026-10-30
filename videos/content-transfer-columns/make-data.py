"""Writes the fake events file the Content Transfer Columns video uploads (all invented). Usage: python make-data.py <folder>"""
import csv
import sys
from pathlib import Path

HEADER = [
    "TitlePart_Title", "AliasPart_Alias", "AutoroutePart_Path", "Showcase_Location_Text", "Showcase_Seats_Value",
    "Showcase_EventDate_Value", "Showcase_StartsAt_Value", "Showcase_DoorsOpen_Value", "Showcase_Featured_Value",
    "Showcase_Speakers_Values", "Showcase_Website_Url", "Showcase_Website_Text",
]
ROWS = [
    ["Spring Open House", "spring-open-house", "events/spring-open-house", "Harbor Hall", "120", "2026-12-05", "2026-12-05T18:30:00", "17:30:00", "True", "Priya Shah|Marcus Lee", "https://example.com/open-house", "Event page"],
    ["Winter Workshop", "winter-workshop", "events/winter-workshop", "Pinecrest Studio", "40", "2027-01-16", "2027-01-16T10:00:00", "09:30:00", "False", "Elena Rossi", "https://example.com/workshop", "Workshop details"],
]

folder = Path(sys.argv[1] if len(sys.argv) > 1 else "data")
folder.mkdir(parents=True, exist_ok=True)
with open(folder / "events.csv", "w", newline="", encoding="utf-8") as handle:
    writer = csv.writer(handle)
    writer.writerow(HEADER)
    writer.writerows(ROWS)
print(len(ROWS), "events")
