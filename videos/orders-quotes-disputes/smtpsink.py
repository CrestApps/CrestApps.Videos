"""A tiny SMTP sink for the demo site: accepts every message on localhost:2525 and keeps it in a file. Nothing is sent anywhere."""
import os
import socketserver
import time

OUT = os.environ.get("VIDEO_MAIL_DIR", "mail")
os.makedirs(OUT, exist_ok=True)


class Handler(socketserver.StreamRequestHandler):
    def reply(self, text):
        self.wfile.write((text + "\r\n").encode())

    def handle(self):
        self.reply("220 sink ready")
        data_mode = False
        lines = []
        while True:
            raw = self.rfile.readline()
            if not raw:
                return
            line = raw.decode("utf-8", "replace").rstrip("\r\n")
            if data_mode:
                if line == ".":
                    name = os.path.join(OUT, f"{int(time.time() * 1000)}.eml")
                    with open(name, "w", encoding="utf-8") as f:
                        f.write("\n".join(lines))
                    data_mode = False
                    lines = []
                    self.reply("250 queued")
                else:
                    lines.append(line)
                continue
            upper = line.upper()
            if upper.startswith("EHLO"):
                self.wfile.write(b"250-sink\r\n250 8BITMIME\r\n")
            elif upper.startswith("HELO") or upper.startswith("MAIL") or upper.startswith("RCPT") or upper.startswith("RSET") or upper.startswith("NOOP"):
                self.reply("250 ok")
            elif upper.startswith("DATA"):
                self.reply("354 go ahead")
                data_mode = True
            elif upper.startswith("QUIT"):
                self.reply("221 bye")
                return
            else:
                self.reply("250 ok")


class Server(socketserver.ThreadingTCPServer):
    allow_reuse_address = True


with Server(("127.0.0.1", 2525), Handler) as server:
    server.serve_forever()
