"""Gives the seeded contacts of the Phone Number Verification video their verification results (fake data only).

The demo site never reaches a verification service, so the results a service would return are written straight into the
site's database. Run it with the site stopped:

    python seed-verification.py <App_Data>/Sites/Default/OrchardCore.db
"""
import json
import sqlite3
import sys
from datetime import datetime, timedelta, timezone

NOW = datetime.now(timezone.utc)

# phone number -> (status, line type, carrier, failed attempts, last error, days since the last attempt)
PROFILES = {
    "+12125550101": ("Verified", "Mobile", "Contoso Wireless", 0, None, 12),
    "+12125550102": ("Verified", "Landline", "Contoso Telecom", 0, None, 12),
    "+12125550103": ("Verified", "Voip", "Fabrikam Voice", 0, None, 30),
    "+12125550104": ("Invalid", "Unknown", None, 0, None, 3),
    "+12125550105": ("Failed", "Unknown", None, 1, "The verification service could not be reached.", 0),
    "+12125550106": ("Failed", "Unknown", None, 3, "The verification service could not be reached.", 0),
    "+12125550108": ("Verified", "Mobile", "Contoso Wireless", 0, None, 45),
}
STATUS = {"Unverified": 0, "Verified": 1, "Invalid": 2, "Failed": 3}
LINE = {"Unknown": 0, "Mobile": 1, "Landline": 2, "Voip": 3}


def iso(dt):
    return dt.strftime("%Y-%m-%dT%H:%M:%S.0000000Z")


def part_for(number, profile):
    status, line, carrier, failed, error, days = profile
    last = NOW - timedelta(days=days, minutes=7)
    verified = status in ("Verified", "Invalid")
    result = {
        "PhoneNumber": number,
        "NormalizedPhoneNumber": number,
        "NationalFormat": "(212) 555-" + number[-4:],
        "IsValid": status == "Verified",
        "IsReachable": status == "Verified",
        "IsMobile": line == "Mobile",
        "IsLandline": line == "Landline",
        "IsVoip": line == "Voip",
        "CountryCode": "US",
        "CountryName": "United States",
        "Region": "New York",
        "City": "New York",
        "Carrier": carrier,
        "LineType": LINE[line],
        "LineStatus": "Active" if status == "Verified" else None,
        "VerificationProvider": "Veriphone",
        "VerificationDateUtc": iso(last),
        "Status": STATUS[status],
    }
    return {
        "PhoneNumber": number,
        "NormalizedPhoneNumber": number,
        "LastVerifiedUtc": iso(last) if verified else None,
        "LastVerifiedByUserId": None,
        "VerificationProvider": "Veriphone" if verified else None,
        "VerificationStatus": status,
        "VerificationResultJson": json.dumps(result) if verified else None,
        "VerificationAttemptCount": failed if status == "Failed" else 1,
        "FailedAttemptCount": failed,
        "LastError": error,
        "LastAttemptUtc": iso(last),
        "NextVerificationDueUtc": iso(last + timedelta(days=365)) if status == "Verified" else (iso(NOW) if status == "Failed" else None),
    }, result


def main(path):
    con = sqlite3.connect(path)
    cur = con.cursor()
    rows = cur.execute("select d.Id, d.Content from Document d join ContentItemIndex i on i.DocumentId = d.Id where i.ContentType = 'Contact'").fetchall()

    for doc_id, content in rows:
        item = json.loads(content)
        number = item["PhoneNumberVerificationPart"]["PhoneNumber"]
        profile = PROFILES.get(number)

        if profile is None:
            continue

        part, result = part_for(number, profile)
        item["PhoneNumberVerificationPart"] = part

        for sub in item["ContactMethods"]["ContentItems"]:
            if "PhoneNumberVerificationPart" in sub:
                sub["PhoneNumberVerificationPart"] = part

        cur.execute("update Document set Content = ? where Id = ?", (json.dumps(item), doc_id))
        status, line, carrier, failed, error, days = profile
        cur.execute(
            "update PhoneNumberVerificationPartIndex set IsVerified = ?, VerificationStatus = ?, VerificationProvider = ?, LastVerifiedUtc = ?, NextVerificationDueUtc = ?, FailedAttemptCount = ?, LastAttemptUtc = ?, CountryCode = ?, Carrier = ?, IsMobile = ?, IsLandline = ?, IsVoip = ?, LineStatus = ? where DocumentId = ?",
            (
                int(status == "Verified"),
                STATUS[status],
                part["VerificationProvider"],
                part["LastVerifiedUtc"],
                part["NextVerificationDueUtc"],
                failed,
                part["LastAttemptUtc"],
                "US" if status in ("Verified", "Invalid") else None,
                carrier,
                int(line == "Mobile"),
                int(line == "Landline"),
                int(line == "Voip"),
                result["LineStatus"],
                doc_id,
            ),
        )

    con.commit()
    con.close()


main(sys.argv[1])
