"""Publishes the videos of this repository to the CrestApps YouTube channel, and keeps them in step with video.json.

Each video lives in videos/<slug>/ with a video.json that holds what YouTube shows: the title, the description,
tags, chapters and links. This tool turns that into a YouTube upload or update, and records the video's YouTube id
back into video.json and the README index. The MP4 is never committed; it is passed to `upload` from wherever the
video was assembled.

Commands:
  python tools/youtube/youtube.py auth                         sign in once, in a browser, and keep the token
  python tools/youtube/youtube.py upload <slug> --file <mp4>   upload, with captions and thumbnail when given
         [--captions <srt>] [--thumbnail <png|jpg>] [--privacy private|unlisted|public]
  python tools/youtube/youtube.py update <slug>... | --all     push video.json's title, description, tags to YouTube
  python tools/youtube/youtube.py chapters <slug> --build <dir> compute chapter times from an assembled build
  python tools/youtube/youtube.py description <slug>           print the description exactly as YouTube gets it
  python tools/youtube/youtube.py index                        rewrite the video table in README.md

Authentication (see tools/youtube/README.md):
  Locally, `auth` runs the OAuth flow with the client in YOUTUBE_CLIENT_SECRETS (default: tools/youtube/client_secret.json,
  which is git-ignored) and stores the token in ~/.crestapps-videos/youtube-token.json, outside the repository.
  In CI, YOUTUBE_CLIENT_ID, YOUTUBE_CLIENT_SECRET and YOUTUBE_REFRESH_TOKEN are used instead.
"""
import argparse
import json
import os
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
VIDEOS = ROOT / "videos"
README = ROOT / "README.md"
TOKEN = Path(os.environ.get("YOUTUBE_TOKEN") or Path.home() / ".crestapps-videos" / "youtube-token.json")
CLIENT_SECRETS = Path(os.environ.get("YOUTUBE_CLIENT_SECRETS") or Path(__file__).with_name("client_secret.json"))
SCOPES = ["https://www.googleapis.com/auth/youtube.upload", "https://www.googleapis.com/auth/youtube.force-ssl"]
INDEX_START, INDEX_END = "<!-- videos:start -->", "<!-- videos:end -->"


# ----- video.json -----

def load(slug):
    path = VIDEOS / slug / "video.json"
    if not path.exists():
        raise SystemExit(f"No video named '{slug}': {path} does not exist.")
    return json.loads(path.read_text(encoding="utf-8"))


def save(slug, video):
    path = VIDEOS / slug / "video.json"
    path.write_text(json.dumps(video, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


def slugs():
    return sorted(path.parent.name for path in VIDEOS.glob("*/video.json"))


def description(video):
    """The description YouTube shows: the text, then the chapters, the links and the hashtags."""
    parts = [video["description"].strip()]

    if video.get("chapters"):
        parts.append("Chapters:\n" + "\n".join(f'{c["time"]} {c["title"]}' for c in video["chapters"]))

    if video.get("links"):
        parts.append("\n".join(f'{link["label"]}: {link["url"]}' for link in video["links"]))

    if video.get("hashtags"):
        parts.append(" ".join("#" + tag for tag in video["hashtags"]))

    text = "\n\n".join(parts)

    if len(text) > 5000:
        raise SystemExit(f"The description is {len(text)} characters; YouTube allows 5000.")

    return text


def snippet(video):
    if len(video["title"]) > 100:
        raise SystemExit(f'The title is {len(video["title"])} characters; YouTube allows 100.')

    return {
        "title": video["title"],
        "description": description(video),
        "tags": video.get("tags", []),
        "categoryId": video.get("categoryId", "28"),
        "defaultLanguage": "en",
        "defaultAudioLanguage": "en",
    }


# ----- YouTube -----

def credentials(interactive=False):
    from google.auth.transport.requests import Request
    from google.oauth2.credentials import Credentials

    if os.environ.get("YOUTUBE_REFRESH_TOKEN"):
        creds = Credentials(
            None,
            refresh_token=os.environ["YOUTUBE_REFRESH_TOKEN"],
            client_id=os.environ["YOUTUBE_CLIENT_ID"],
            client_secret=os.environ["YOUTUBE_CLIENT_SECRET"],
            token_uri="https://oauth2.googleapis.com/token",
            scopes=SCOPES,
        )
        creds.refresh(Request())
        return creds

    creds = Credentials.from_authorized_user_file(str(TOKEN), SCOPES) if TOKEN.exists() else None

    if creds and creds.expired and creds.refresh_token:
        creds.refresh(Request())
    elif not creds or not creds.valid:
        if not interactive:
            raise SystemExit("Not signed in to YouTube: run `python tools/youtube/youtube.py auth` first.")
        from google_auth_oauthlib.flow import InstalledAppFlow

        if not CLIENT_SECRETS.exists():
            raise SystemExit(f"No OAuth client at {CLIENT_SECRETS}. See tools/youtube/README.md.")
        creds = InstalledAppFlow.from_client_secrets_file(str(CLIENT_SECRETS), SCOPES).run_local_server(port=0)

    TOKEN.parent.mkdir(parents=True, exist_ok=True)
    TOKEN.write_text(creds.to_json(), encoding="utf-8")
    return creds


def service():
    from googleapiclient.discovery import build

    return build("youtube", "v3", credentials=credentials(), cache_discovery=False)


def cmd_auth(args):
    creds = credentials(interactive=True)
    from googleapiclient.discovery import build

    channel = build("youtube", "v3", credentials=creds, cache_discovery=False).channels().list(part="snippet", mine=True).execute()
    title = channel["items"][0]["snippet"]["title"] if channel.get("items") else "(no channel)"
    print(f"Signed in. Uploads go to the channel: {title}")
    print(f"Token saved to {TOKEN}. For CI, store its refresh_token as the YOUTUBE_REFRESH_TOKEN secret.")


def cmd_upload(args):
    from googleapiclient.http import MediaFileUpload

    video = load(args.slug)

    if video.get("youtube", {}).get("id") and not args.force:
        raise SystemExit(f'{args.slug} is already on YouTube ({video["youtube"]["url"]}). Use `update`, or --force to upload a new copy.')

    youtube = service()
    body = {
        "snippet": snippet(video),
        "status": {
            "privacyStatus": args.privacy or video.get("privacyStatus", "private"),
            "selfDeclaredMadeForKids": False,
            "containsSyntheticMedia": True,
        },
    }
    request = youtube.videos().insert(part="snippet,status", body=body, media_body=MediaFileUpload(args.file, chunksize=8 * 1024 * 1024, resumable=True))

    response = None
    while response is None:
        status, response = request.next_chunk()
        if status:
            print(f"  uploaded {int(status.progress() * 100)}%")

    video_id = response["id"]
    video["youtube"] = {"id": video_id, "url": f"https://youtu.be/{video_id}"}
    save(args.slug, video)
    print(f"Uploaded: https://youtu.be/{video_id}")

    if args.captions:
        youtube.captions().insert(
            part="snippet",
            body={"snippet": {"videoId": video_id, "language": "en", "name": "English", "isDraft": False}},
            media_body=MediaFileUpload(args.captions, mimetype="application/octet-stream"),
        ).execute()
        print("  captions uploaded")

    if args.thumbnail:
        try:
            youtube.thumbnails().set(videoId=video_id, media_body=MediaFileUpload(args.thumbnail)).execute()
            print("  thumbnail set")
        except Exception as error:  # Custom thumbnails need a verified channel.
            print(f"  thumbnail not set: {error}")

    cmd_index(args)


def cmd_update(args):
    youtube = service()
    targets = slugs() if args.all else args.slugs

    for slug in targets:
        video = load(slug)
        video_id = video.get("youtube", {}).get("id")

        if not video_id:
            print(f"{slug}: not on YouTube yet, skipped")
            continue

        youtube.videos().update(part="snippet", body={"id": video_id, "snippet": snippet(video)}).execute()
        print(f"{slug}: updated https://youtu.be/{video_id}")


def cmd_chapters(args):
    """Reads the assembled segments of a build (build/<clip>.mp4, in storyboard order) and writes the chapter times."""
    import imageio_ffmpeg

    video = load(args.slug)
    storyboard = json.loads((VIDEOS / args.slug / "storyboard.json").read_text(encoding="utf-8"))
    build = Path(args.build)
    ffmpeg = imageio_ffmpeg.get_ffmpeg_exe()

    def length(path):
        output = subprocess.run([ffmpeg, "-i", str(path)], capture_output=True, text=True).stderr
        hours, minutes, seconds = re.search(r"Duration: (\d+):(\d+):([\d.]+)", output).groups()
        return int(hours) * 3600 + int(minutes) * 60 + float(seconds)

    def stamp(seconds):
        seconds = int(seconds)
        return f"{seconds // 3600}:{seconds % 3600 // 60:02d}:{seconds % 60:02d}" if seconds >= 3600 else f"{seconds // 60}:{seconds % 60:02d}"

    chapters, elapsed = [], 0.0

    for chapter in storyboard["chapters"]:
        title = {"00": "Introduction", "99": "Wrap-up"}.get(chapter["id"], chapter["title"])
        chapters.append({"time": stamp(elapsed), "title": title})

        if not all(clip["kind"] == "card" for clip in chapter["clips"]):
            elapsed += length(build / f'card-{chapter["id"]}.mp4')

        for clip in chapter["clips"]:
            if clip["kind"] == "slide":
                elapsed += sum(length(build / f'slide-{step["id"]}.mp4') for step in clip["steps"])
            else:
                elapsed += length(build / f'{clip["id"]}.mp4')

    master = build / "master.mp4"
    if master.exists() and abs(length(master) - elapsed) > 1:
        raise SystemExit(f"The segments add up to {elapsed:.1f}s but master.mp4 is {length(master):.1f}s: check the build.")

    video["chapters"] = chapters
    video["length"] = stamp(elapsed)
    save(args.slug, video)
    print("\n".join(f'{c["time"]} {c["title"]}' for c in chapters))


def cmd_description(args):
    print(description(load(args.slug)))


def cmd_index(args):
    rows = ["| Video | Length | YouTube | Source |", "| --- | --- | --- | --- |"]

    for slug in slugs():
        video = load(slug)
        youtube = video.get("youtube", {}).get("url")
        source = video.get("source", {})
        pr = f'[{source["repository"]}#{source["pullRequest"]}](https://github.com/{source["repository"]}/pull/{source["pullRequest"]})' if source.get("pullRequest") else source.get("repository", "")
        rows.append(f'| [{video["title"].split(" | ")[0]}](videos/{slug}) | {video.get("length", "")} | {f"[{youtube}]({youtube})" if youtube else "not uploaded"} | {pr} |')

    text = README.read_text(encoding="utf-8")
    start, end = text.index(INDEX_START) + len(INDEX_START), text.index(INDEX_END)
    README.write_text(text[:start] + "\n" + "\n".join(rows) + "\n" + text[end:], encoding="utf-8")
    print(f"README.md: {len(rows) - 2} videos")


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    commands = parser.add_subparsers(dest="command", required=True)

    commands.add_parser("auth").set_defaults(run=cmd_auth)

    upload = commands.add_parser("upload")
    upload.add_argument("slug")
    upload.add_argument("--file", required=True)
    upload.add_argument("--captions")
    upload.add_argument("--thumbnail")
    upload.add_argument("--privacy", choices=["private", "unlisted", "public"])
    upload.add_argument("--force", action="store_true")
    upload.set_defaults(run=cmd_upload)

    update = commands.add_parser("update")
    update.add_argument("slugs", nargs="*")
    update.add_argument("--all", action="store_true")
    update.set_defaults(run=cmd_update)

    chapters = commands.add_parser("chapters")
    chapters.add_argument("slug")
    chapters.add_argument("--build", required=True)
    chapters.set_defaults(run=cmd_chapters)

    describe = commands.add_parser("description")
    describe.add_argument("slug")
    describe.set_defaults(run=cmd_description)

    commands.add_parser("index").set_defaults(run=cmd_index)

    args = parser.parse_args()
    args.run(args)


if __name__ == "__main__":
    sys.exit(main())
