# Publishing to YouTube

`youtube.py` publishes the videos of this repository to the CrestApps YouTube channel through the YouTube Data API v3,
and keeps each video's YouTube metadata in its `video.json`.

```bash
pip install -r tools/youtube/requirements.txt
python tools/youtube/youtube.py auth                                   # once per machine
python tools/youtube/youtube.py upload <slug> --file <mp4> --captions <srt> --thumbnail <png>
python tools/youtube/youtube.py update <slug>                          # after editing video.json
python tools/youtube/youtube.py chapters <slug> --build videos/<slug>/build
python tools/youtube/youtube.py description <slug>                     # preview what YouTube gets
python tools/youtube/youtube.py index                                  # rewrite the table in README.md
```

## One-time setup

The API only acts for a Google account that signed in and granted access, so a person with access to the CrestApps
channel does this once:

1. In the [Google Cloud console](https://console.cloud.google.com/), create a project (for example "CrestApps Videos")
   and enable the **YouTube Data API v3**.
2. Under **Google Auth Platform**, configure the consent screen as **External**, add the scopes
   `youtube.upload` and `youtube.force-ssl`, and add the channel owner's account as a test user. Then **publish the
   app** ("In production"): while it stays in testing, its refresh tokens expire after 7 days. An unverified app in
   production still works for the accounts that sign in to it; Google shows them a warning first.
3. Create an **OAuth client ID** of type **Desktop app**, download its JSON, and save it as
   `tools/youtube/client_secret.json` (git-ignored), or point `YOUTUBE_CLIENT_SECRETS` at it.
4. Run `python tools/youtube/youtube.py auth`, sign in with the account that owns the CrestApps channel (pick the
   channel if it's a brand account), and allow access. The token is kept in `~/.crestapps-videos/youtube-token.json`,
   outside the repository.

## Updating metadata from GitHub

The **YouTube metadata** workflow (`.github/workflows/youtube-metadata.yml`) runs `youtube.py update` for every video
whose `video.json` changed in a push to `main`, so a corrected title or description reaches YouTube by merging a pull
request. It needs three repository secrets: `YOUTUBE_CLIENT_ID` and `YOUTUBE_CLIENT_SECRET` (from the OAuth client)
and `YOUTUBE_REFRESH_TOKEN` (the `refresh_token` in the token file `auth` wrote). Until they exist, the workflow skips
itself.

Uploading stays a local step: the MP4 is not in the repository, so a workflow would have nothing to upload.

## What the API allows, and what it doesn't

- **Uploads from an unaudited API project are locked to private.** YouTube restricts every video uploaded through
  `videos.insert` by a project that hasn't passed its [API compliance audit](https://support.google.com/youtube/contact/yt_api_form)
  to private viewing, and it can't be made public. Either request the audit once, which is free and takes a few weeks,
  or upload in YouTube Studio and use this tool for everything else (captions, thumbnail, metadata updates and the
  index), which isn't restricted. Record a Studio upload's id in `video.json` as `"youtube": { "id": "...", "url": "https://youtu.be/..." }`.
- **Quota**: 10,000 units a day by default. An upload costs 1,600, a caption 400, a thumbnail 50, an update 50.
- **Custom thumbnails** need a channel with phone verification or channel history; the tool reports when YouTube
  refuses one.
- **AI disclosure**: uploads declare synthetic media (`containsSyntheticMedia`), because the narration is a neural voice.
- **Chapters** come from the timestamps in the description; the first must be `0:00`, and there must be at least three.
