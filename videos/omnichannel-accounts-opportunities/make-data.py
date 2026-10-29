"""Writes the fake accounts, contacts and opportunities the Accounts and Opportunities video seeds the site with (all invented)."""
import csv
import sys
from pathlib import Path

ACCOUNTS = ["Riverbend Dental", "Summit Fitness Group", "Harbor Cafe Co", "Pinecrest Realty"]
CONTACTS = [
    ("Jamie", "Chen", "Office Manager", "jamie.chen@example.com", "415 555 0101", "America/Los_Angeles", "Riverbend Dental"),
    ("Tomas", "Novak", "Accountant", "tomas.novak@example.com", "206 555 0105", "America/Los_Angeles", "Riverbend Dental"),
    ("Priya", "Patel", "Buyer", "priya.patel@example.com", "212 555 0102", "America/New_York", "Summit Fitness Group"),
    ("Aisha", "Khan", "Operations Director", "aisha.khan@example.com", "617 555 0106", "America/New_York", "Summit Fitness Group"),
    ("Marcus", "Brooks", "Owner", "marcus.brooks@example.com", "312 555 0103", "America/Chicago", "Harbor Cafe Co"),
    ("Sofia", "Silva", "Project Lead", "sofia.silva@example.com", "512 555 0108", "America/Chicago", "Harbor Cafe Co"),
    ("Elena", "Rossi", "Facilities Lead", "elena.rossi@example.com", "303 555 0104", "America/Denver", "Pinecrest Realty"),
    ("Noah", "Fischer", "Store Manager", "noah.fischer@example.com", "702 555 0107", "America/Los_Angeles", "Pinecrest Realty"),
]
OPPORTUNITIES = [
    ("Riverbend dental chairs", 18500, "2026-12-15", "Riverbend Dental"),
    ("Summit gym equipment", 42000, "2027-01-20", "Summit Fitness Group"),
    ("Harbor espresso machines", 9800, "2026-11-30", "Harbor Cafe Co"),
    ("Pinecrest signage", 6400, "2026-12-05", "Pinecrest Realty"),
    ("Summit towel service", 3200, "2027-02-10", "Summit Fitness Group"),
]

folder = Path(sys.argv[1] if len(sys.argv) > 1 else "data")
folder.mkdir(parents=True, exist_ok=True)


def write(name, header, rows):
    with open(folder / name, "w", newline="", encoding="utf-8") as handle:
        writer = csv.writer(handle)
        writer.writerow(header)
        writer.writerows(rows)


write("accounts.csv", ["TitlePart_Title"], [[a] for a in ACCOUNTS])
write("account-contacts.csv", ["Contact_FirstName_Text", "Contact_LastName_Text", "Contact_JobTitle_Text", "ContactMethods_Email", "ContactMethods_CellPhone", "TimeZoneId", "Account"], CONTACTS)
write("opportunities.csv", ["TitlePart_Title", "OpportunityPart_Amount_Value", "OpportunityPart_CloseDate_Value", "Account"], OPPORTUNITIES)
print(len(ACCOUNTS), "accounts,", len(CONTACTS), "contacts,", len(OPPORTUNITIES), "opportunities")
