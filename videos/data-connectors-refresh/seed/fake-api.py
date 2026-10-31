"""A fake sales API for the Data Connectors videos: made-up orders, no real data, no credentials.

python fake-api.py [port]   (default 5555)

GET /                         the API's home (what "Save and test" calls)
GET /orders?page=1&limit=25   {"data": {"items": [...]}, "page": n}, one page at a time (empty after the last page)
GET /orders?status=open       only the open orders
GET /orders.csv               the same rows as CSV
GET /control?extra=N&fail=0|1 adds N new orders and changes a few; fail=1 makes /orders answer 500
"""
import csv, io, json, random, sys
from datetime import datetime, timedelta, timezone
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from urllib.parse import parse_qs, urlparse

CUSTOMERS = ["Northwind Traders", "Contoso Retail", "Fabrikam Supply", "Adventure Works", "Tailspin Toys",
             "Wide World Importers", "Litware Foods", "Proseware Labs"]
REGIONS = ["East", "West", "North", "South"]
STATUSES = ["open", "shipped", "delivered", "cancelled"]
BASE = datetime(2026, 9, 1, 9, 0, tzinfo=timezone.utc)
STATE = {"extra": 0, "fail": False, "rev": 0}


def order(i):
    rnd = random.Random(i)
    ordered = BASE + timedelta(hours=7 * i)
    modified = ordered + timedelta(hours=rnd.randint(1, 30))
    status = STATUSES[rnd.choice([0, 0, 1, 1, 1, 2, 2, 3])]
    total = round(rnd.uniform(20, 900), 2)
    if STATE["rev"] and i % 7 == 0:
        status, total = "shipped", round(total + 10, 2)
        modified = BASE + timedelta(days=40, hours=i)
    return {"orderId": 1000 + i, "customer": CUSTOMERS[rnd.randrange(len(CUSTOMERS))], "region": REGIONS[rnd.randrange(4)],
            "status": status, "total": total, "orderedOn": ordered.strftime("%Y-%m-%dT%H:%M:%SZ"),
            "modifiedOn": modified.strftime("%Y-%m-%dT%H:%M:%SZ")}


def all_orders():
    return [order(i) for i in range(60 + STATE["extra"])]


class Handler(BaseHTTPRequestHandler):
    def log_message(self, *args):
        pass

    def send(self, code, body, kind="application/json"):
        data = body.encode()
        self.send_response(code)
        self.send_header("Content-Type", kind)
        self.send_header("Content-Length", str(len(data)))
        self.end_headers()
        self.wfile.write(data)

    def do_GET(self):
        url = urlparse(self.path)
        q = {k: v[0] for k, v in parse_qs(url.query).items()}
        if url.path == "/control":
            STATE["extra"] += int(q.get("extra", 0))
            if "extra" in q:
                STATE["rev"] = 1
            STATE["fail"] = q.get("fail") == "1"
            return self.send(200, json.dumps(STATE))
        if url.path in ("/", ""):
            return self.send(200, json.dumps({"name": "Demo sales API", "ok": True}))
        if url.path in ("/orders", "/orders.csv"):
            if STATE["fail"]:
                return self.send(500, json.dumps({"error": "The orders service is down."}))
            rows = all_orders()
            if "status" in q:
                rows = [r for r in rows if r["status"] == q["status"]]
            if url.path == "/orders.csv":
                out = io.StringIO()
                w = csv.DictWriter(out, fieldnames=list(rows[0]))
                w.writeheader(); w.writerows(rows)
                return self.send(200, out.getvalue(), "text/csv")
            size, page = int(q.get("size") or q.get("limit") or 25), int(q.get("page", 1))
            return self.send(200, json.dumps({"data": {"items": rows[(page - 1) * size: page * size]}, "page": page}))
        self.send(404, json.dumps({"error": "Not found"}))


if __name__ == "__main__":
    port = int(sys.argv[1]) if len(sys.argv) > 1 else 5555
    ThreadingHTTPServer(("127.0.0.1", port), Handler).serve_forever()
