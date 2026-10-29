"""Writes the fake contacts file that the Omnichannel Contacts video seeds the site with (names are invented)."""
import csv
import sys
from pathlib import Path

ROWS = [
    ("Jamie", "Chen", "Office Manager", "jamie.chen@example.com", "415 555 0101", "America/Los_Angeles"),
    ("Priya", "Patel", "Buyer", "priya.patel@example.com", "212 555 0102", "America/New_York"),
    ("Marcus", "Brooks", "Owner", "marcus.brooks@example.com", "312 555 0103", "America/Chicago"),
    ("Elena", "Rossi", "Facilities Lead", "elena.rossi@example.com", "303 555 0104", "America/Denver"),
    ("Tomas", "Novak", "Accountant", "tomas.novak@example.com", "206 555 0105", "America/Los_Angeles"),
    ("Aisha", "Khan", "Operations Director", "aisha.khan@example.com", "617 555 0106", "America/New_York"),
    ("Noah", "Fischer", "Store Manager", "noah.fischer@example.com", "702 555 0107", "America/Los_Angeles"),
    ("Sofia", "Silva", "Project Lead", "sofia.silva@example.com", "512 555 0108", "America/Chicago"),
]

folder = Path(sys.argv[1] if len(sys.argv) > 1 else "data")
folder.mkdir(parents=True, exist_ok=True)
with open(folder / "contacts.csv", "w", newline="", encoding="utf-8") as handle:
    writer = csv.writer(handle)
    writer.writerow(["Contact_FirstName_Text", "Contact_LastName_Text", "Contact_JobTitle_Text", "ContactMethods_Email", "ContactMethods_CellPhone", "TimeZoneId"])
    writer.writerows(ROWS)
print(len(ROWS), "contacts")
