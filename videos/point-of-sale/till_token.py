"""usage: pos_token.py <user> - prints an access token of the till-app client for the user (authorization code + PKCE)."""
import base64, hashlib, os, re, secrets, sys, urllib.parse
import requests

BASE = "http://localhost:5375"
user = sys.argv[1] if len(sys.argv) > 1 else "admin"
s = requests.Session()
r = s.get(BASE + "/Login")
tok = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', r.text).group(1)
r = s.post(BASE + "/Login", data={"LoginForm.UserName": user, "LoginForm.Password": "Password1!", "__RequestVerificationToken": tok}, allow_redirects=False)
assert r.status_code in (302, 200), r.status_code

verifier = secrets.token_urlsafe(48)
challenge = base64.urlsafe_b64encode(hashlib.sha256(verifier.encode()).digest()).rstrip(b"=").decode()
params = {"client_id": "till-app", "response_type": "code", "redirect_uri": "http://localhost:9/callback", "scope": "openid profile roles",
          "code_challenge": challenge, "code_challenge_method": "S256", "state": "x"}
r = s.get(BASE + "/connect/authorize", params=params, allow_redirects=False)
loc = r.headers.get("Location", "")
for _ in range(6):
    if "code=" in loc or not loc:
        break
    r = s.get(urllib.parse.urljoin(BASE, loc), allow_redirects=False)
    loc = r.headers.get("Location", "")
if "code=" not in loc:
    print("no code", r.status_code, loc, r.text[:300], file=sys.stderr)
    sys.exit(1)
code = urllib.parse.parse_qs(urllib.parse.urlparse(loc).query)["code"][0]
r = requests.post(BASE + "/connect/token", data={"grant_type": "authorization_code", "code": code, "redirect_uri": "http://localhost:9/callback",
                                                  "client_id": "till-app", "code_verifier": verifier})
if r.status_code != 200:
    print("token", r.status_code, r.text[:300], file=sys.stderr)
    sys.exit(1)
print(r.json()["access_token"])
