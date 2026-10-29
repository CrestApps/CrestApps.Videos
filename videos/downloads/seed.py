"""python seed.py: sells the two sample ebooks at a register, to the customers Jamie Chen and Riley Park, so there are paid orders with
downloads. It signs in as admin the way a till app does (the Point of Sale video's site: a register, a till-app client and the Customers API)."""
import subprocess, sys, uuid
from pathlib import Path

import requests

BASE = "http://localhost:5375/api/commerce/v1/"
tok = subprocess.check_output([sys.executable, str(Path(__file__).with_name("till_token.py")), "admin"], text=True).strip()
H = {"Authorization": f"Bearer {tok}"}

TRAIL_GUIDE = "4dgtlprod0trailguide00000a"
CAMP_RECIPES = "4dgtlprod0camprecipes0000a"


def call(method, path, body=None, ok=(200, 201, 204)):
    h = dict(H)
    h["Idempotency-Key"] = str(uuid.uuid4())
    r = requests.request(method, BASE + path, json=body, headers=h, timeout=60)
    if r.status_code not in ok:
        print("FAILED", method, path, r.status_code, r.text[:400])
        sys.exit(1)
    return r.json() if r.text else {}


def customer(q):
    found = call("GET", f"customers?q={q}")
    items = found if isinstance(found, list) else found.get("items", [])
    return items[0]["id"]


register = next(r for r in call("GET", "pos/registers") if r["name"] == "Downtown till 1")

if not register.get("openShiftId"):
    call("POST", f"pos/registers/{register['id']}/shifts", {"openingFloat": 100.00})

for buyer, lines in (("jamie", [TRAIL_GUIDE, CAMP_RECIPES]), ("riley", [TRAIL_GUIDE])):
    sale = call("POST", "pos/sales", {"registerId": register["id"], "lines": [{"productId": p, "quantity": 1} for p in lines], "customerId": customer(buyer)})
    paid = call("POST", f"pos/sales/{sale['checkout']['id']}/pay", {"cash": 100.00})
    print(buyer, sale["orderNumber"], paid["status"])
