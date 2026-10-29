"""Writes the fake customer files the Content Transfer video imports (names are invented)."""
import csv
import sys
from pathlib import Path

FIRST = ["Jamie", "Priya", "Marcus", "Elena", "Tomas", "Aisha", "Noah", "Sofia", "Liam", "Mei", "Omar", "Hannah", "Diego", "Grace", "Kenji", "Olivia", "Ravi", "Zoe", "Ethan", "Lucia"]
LAST = ["Chen", "Patel", "Brooks", "Rossi", "Novak", "Khan", "Fischer", "Silva", "Walsh", "Tanaka", "Haddad", "Larsen", "Ortega", "Bennett", "Sato", "Murphy", "Iyer", "Kowalski", "Reed", "Costa"]
CITIES = ["Austin", "Denver", "Seattle", "Boston", "Phoenix", "Portland", "Chicago", "Atlanta"]
PLANS = ["Basic", "Plus", "Premium"]


def main(folder, count=600, bad_every=97):
    folder = Path(folder)
    folder.mkdir(parents=True, exist_ok=True)
    rows = []
    for i in range(count):
        first, last = FIRST[i % len(FIRST)], LAST[(i * 7 + i // len(FIRST)) % len(LAST)]
        title = f"{first} {last}"
        if i % bad_every == bad_every - 1:
            title = ""  # a row the import rejects: the title is required
        rows.append({
            "TitlePart_Title": title,
            "Customer_Email_Text": f"{first.lower()}.{last.lower()}{i}@example.com",
            "Customer_City_Text": CITIES[i % len(CITIES)],
            "Customer_Plan_Text": PLANS[i % len(PLANS)],
        })
    with open(folder / "customers.csv", "w", newline="", encoding="utf-8") as handle:
        writer = csv.DictWriter(handle, fieldnames=list(rows[0]))
        writer.writeheader()
        writer.writerows(rows)
    print(f"{len(rows)} rows, {sum(1 for r in rows if not r['TitlePart_Title'])} without a title")


if __name__ == "__main__":
    main(sys.argv[1] if len(sys.argv) > 1 else "data")
