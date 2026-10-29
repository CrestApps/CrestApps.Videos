"""python seed.py: seeds the demo site's registers with two shifts: one closed (sales in cash and by card, a bank run, a return) and one open."""
import subprocess, sys, uuid
from pathlib import Path
import requests

BASE = "http://localhost:5375/api/commerce/v1/"
tok = subprocess.check_output([sys.executable, str(Path(__file__).with_name("till_token.py")), "admin"], text=True).strip()
H = {"Authorization": f"Bearer {tok}"}

BEANIE = "4rfejp993d52b6zb4fy8h6jxvv"
WATCH = "4bs2r0nmdqqcvhfhzjmnv2vwj3"
CAP = "4rgryp179mr10m6yvbt42xym8n"
TOTE = "497na17k1c9avgt0k4vchbkcxa"


def call(method, path, body=None, ok=(200, 201, 204)):
    h = dict(H)
    h["Idempotency-Key"] = str(uuid.uuid4())
    r = requests.request(method, BASE + path, json=body, headers=h, timeout=60)
    if r.status_code not in ok:
        print("FAILED", method, path, r.status_code, r.text[:400])
        sys.exit(1)
    return r.json() if r.text else {}


registers = call("GET", "pos/registers")
till = next(r for r in registers if r["name"] == "Downtown till 1")
print("register", till["id"])

# A closed shift.
shift = call("POST", f"pos/registers/{till['id']}/shifts", {"openingFloat": 100.00})
sid = shift["id"]


def sale(lines, buyer, pay):
    s = call("POST", "pos/sales", {"registerId": till["id"], "lines": lines, "customerEmail": buyer[1], "customerName": buyer[0]})
    p = call("POST", f"pos/sales/{s['checkout']['id']}/pay", pay)
    print("sale", s["orderNumber"], p["status"])
    return s


s1 = sale([{"productId": BEANIE, "quantity": 2}], ("Avery Brooks", "avery.brooks@example.com"), {"cash": 50.00})
s2 = sale([{"productId": WATCH, "quantity": 1}], ("Taylor Quinn", "taylor.quinn@example.com"), {"card": {"approvalCode": "483920"}})
s3 = sale([{"productId": CAP, "quantity": 1}, {"productId": TOTE, "quantity": 1}], ("Casey Rowe", "casey.rowe@example.com"), {"cash": 20.00, "card": {"approvalCode": "517204"}})
s4 = sale([{"productId": CAP, "quantity": 1}], ("Jordan Lane", "jordan.lane@example.com"), {"cash": 30.00})
call("POST", f"pos/shifts/{sid}/cash", {"amount": -50.00, "reason": "Banked at noon"})
ret = call("POST", "pos/returns", {"orderId": s4["orderId"], "registerId": till["id"], "reason": "Wrong color", "restock": True})
print("return", ret.get("refunded"), ret.get("cashBack"))
closed = call("GET", f"pos/shifts/{sid}")
print("expected", closed.get("expectedCash"))
exp = closed["expectedCash"]["amount"]
call("POST", f"pos/shifts/{sid}/close", {"countedCash": round(exp - 1.48, 2), "note": "Short 1.48, a coin dropped behind the till"})

# An open shift.
shift2 = call("POST", f"pos/registers/{till['id']}/shifts", {"openingFloat": 100.00})
sid2 = shift2["id"]
sale([{"productId": BEANIE, "quantity": 1}], ("Riley Moss", "riley.moss@example.com"), {"cash": 40.00})
sale([{"productId": WATCH, "quantity": 1}], ("Morgan Hale", "morgan.hale@example.com"), {"card": {"approvalCode": "902331"}})
print("done", sid, sid2)
