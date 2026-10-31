"""mailview.py <name> <to> <subject-prefix>: renders the newest matching .eml of the demo site as a web page (mailview/<name>.html)."""
import sys, email, html, glob, os
from email import policy
name, to, subject = sys.argv[1:4]
files = sorted(glob.glob("C:/cavideo/cal2/App_Data/Sites/Default/Emails/*.eml"), key=os.path.getmtime, reverse=True)
for f in files:
    m = email.message_from_file(open(f, encoding="utf-8"), policy=policy.default)
    if m["To"] == to and str(m["Subject"]).startswith(subject):
        break
else:
    raise SystemExit("no such mail")
body = m.get_body(preferencelist=("html",)).get_content()
atts = [p.get_filename() for p in m.iter_attachments()]
page = f"""<!doctype html><html><head><meta charset="utf-8"><title>{html.escape(str(m['Subject']))}</title>
<style>body{{font-family:Segoe UI,Arial,sans-serif;background:#eef2f6;margin:0;padding:48px}}
.mail{{max-width:820px;margin:0 auto;background:#fff;border:1px solid #d5dde5;border-radius:10px;padding:28px 36px;box-shadow:0 2px 10px #0001}}
h1{{font-size:26px;margin:0 0 14px}}.meta{{color:#55616d;font-size:15px;line-height:1.7;border-bottom:1px solid #e3e8ed;padding-bottom:14px;margin-bottom:18px}}
.body{{font-size:20px;line-height:1.5}}.body a{{color:#1971c2;font-weight:600}}
.att{{margin-top:22px;border-top:1px solid #e3e8ed;padding-top:12px;color:#55616d;font-size:15px}}</style></head>
<body><div class="mail"><h1>{html.escape(str(m['Subject']))}</h1>
<div class="meta"><b>From:</b> {html.escape(str(m['From']))}<br><b>To:</b> {html.escape(str(m['To']))}<br><b>Date:</b> {html.escape(str(m['Date']))}</div>
<div class="body">{body}</div>
<div class="att"><b>Attachment:</b> {", ".join(html.escape(a) for a in atts)}</div></div></body></html>"""
os.makedirs("C:/cavideo/cal2/mailview", exist_ok=True)
open(f"C:/cavideo/cal2/mailview/{name}.html", "w", encoding="utf-8").write(page)
print("ok", name, m["Subject"])
