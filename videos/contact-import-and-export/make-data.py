"""Writes the fake contacts the omnichannel videos load (all invented). Usage: python make-data.py <folder>"""
import csv
import sys
from pathlib import Path

CONTACTS = [
    ("Liam", "Parker", "Customer", "liam.parker@example.com", "415 555 0121", "America/Los_Angeles"),
    ("Olivia", "Nguyen", "Customer", "olivia.nguyen@example.com", "206 555 0122", "America/Los_Angeles"),
    ("Ethan", "Walker", "Customer", "ethan.walker@example.com", "212 555 0123", "America/New_York"),
    ("Mia", "Hernandez", "Customer", "mia.hernandez@example.com", "617 555 0124", "America/New_York"),
    ("Lucas", "Kim", "Customer", "lucas.kim@example.com", "312 555 0125", "America/Chicago"),
    ("Ava", "Morales", "Customer", "ava.morales@example.com", "512 555 0126", "America/Chicago"),
    ("Noah", "Bennett", "Customer", "noah.bennett@example.com", "303 555 0127", "America/Denver"),
    ("Emma", "Rivera", "Customer", "emma.rivera@example.com", "702 555 0128", "America/Los_Angeles"),
]

folder = Path(sys.argv[1] if len(sys.argv) > 1 else "data")
folder.mkdir(parents=True, exist_ok=True)
with open(folder / "omni-contacts.csv", "w", newline="", encoding="utf-8") as handle:
    writer = csv.writer(handle)
    writer.writerow(["Contact_FirstName_Text", "Contact_LastName_Text", "Contact_JobTitle_Text", "ContactMethods_Email", "ContactMethods_CellPhone", "TimeZoneId"])
    writer.writerows(CONTACTS)
print(len(CONTACTS), "contacts")

# The file the Contact Import and Export video uploads: two new people and three whose phone number is already used.
IMPORT_ROWS = [
    ("Grace", "Okafor", "Customer", "grace.okafor@example.com", "415 555 0121", "America/Los_Angeles"),
    ("Henry", "Zhao", "Customer", "henry.zhao@example.com", "646 555 0140", "America/New_York"),
    ("Iris", "Tanaka", "Customer", "iris.tanaka@example.com", "646 555 0140", "America/New_York"),
    ("Jonas", "Weber", "Customer", "jonas.weber@example.com", "303 555 0127", "America/Denver"),
    ("Keira", "Doyle", "Customer", "keira.doyle@example.com", "214 555 0150", "America/Chicago"),
]
with open(folder / "new-contacts.csv", "w", newline="", encoding="utf-8") as handle:
    writer = csv.writer(handle)
    writer.writerow(["Contact_FirstName_Text", "Contact_LastName_Text", "Contact_JobTitle_Text", "ContactMethods_Email", "ContactMethods_CellPhone", "TimeZoneId"])
    writer.writerows(IMPORT_ROWS)
print(len(IMPORT_ROWS), "contacts to import")
