"""Writes the fake leads file that the Omnichannel Leads video seeds the site with (people and companies are invented)."""
import csv
import sys
from pathlib import Path

ROWS = [
    ("Aaron", "Blake", "Owner", "Open - Not Contacted", "Trade show", "Spring Import", "Riverbend Dental", "Hot", "aaron.blake@example.com", "415 555 0201", "America/Los_Angeles"),
    ("Priya", "Patel", "Buyer", "Working - Contacted", "Web form", "Spring Import", "Patel Home Supply", "Warm", "priya.patel@example.com", "646 555 0199", "America/New_York"),
    ("Grace", "Okafor", "Manager", "Open - Not Contacted", "Purchased list", "Spring Import", "Summit Fitness", "Cold", "grace.okafor@example.com", "213 555 0203", "America/Los_Angeles"),
    ("Liam", "Murphy", "Director", "Nurturing", "Referral", "Spring Import", "Harbor Cafe", "Warm", "liam.murphy@example.com", "617 555 0204", "America/New_York"),
    ("Mei", "Tanaka", "Founder", "Open - Not Contacted", "Trade show", "Spring Import", "Pinecrest Realty", "Hot", "mei.tanaka@example.com", "206 555 0205", "America/Los_Angeles"),
    ("Omar", "Haddad", "Purchasing", "Working - Contacted", "Cold call", "Spring Import", "Lakeview Hardware", "Warm", "omar.haddad@example.com", "312 555 0206", "America/Chicago"),
    ("Hannah", "Larsen", "Owner", "Open - Not Contacted", "Purchased list", "Spring Import", "Maple Street Bakery", "Cold", "hannah.larsen@example.com", "303 555 0207", "America/Denver"),
    ("Diego", "Ortega", "Operations", "Nurturing", "Web form", "Spring Import", "Cobalt Logistics", "Warm", "diego.ortega@example.com", "713 555 0208", "America/Chicago"),
    ("Zoe", "Bennett", "Partner", "Open - Not Contacted", "Trade show", "Spring Import", "Granite Insurance", "Hot", "zoe.bennett@example.com", "404 555 0209", "America/New_York"),
    ("Kenji", "Sato", "Manager", "Working - Contacted", "Referral", "Spring Import", "Bluebird Travel", "Hot", "kenji.sato@example.com", "602 555 0210", "America/Phoenix"),
]

folder = Path(sys.argv[1] if len(sys.argv) > 1 else "data")
folder.mkdir(parents=True, exist_ok=True)
with open(folder / "leads.csv", "w", newline="", encoding="utf-8") as handle:
    writer = csv.writer(handle)
    writer.writerow(["Lead_FirstName_Text", "Lead_LastName_Text", "Lead_JobTitle_Text", "LeadStatus", "LeadSource", "LeadList", "Company", "Rating", "ContactMethods_Email", "ContactMethods_CellPhone", "TimeZoneId"])
    writer.writerows(ROWS)
print(len(ROWS), "leads")
