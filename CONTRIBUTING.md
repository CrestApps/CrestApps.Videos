# Contributing a video

Every CrestApps video goes through this repository: you submit its **source** by pull request, a maintainer
publishes it, and GitHub Actions uploads it to the CrestApps YouTube channel and opens the pull request that shows it
in the documentation. Nobody uploads to YouTube by hand, and **no video file is ever committed**, here or in a product
repository.

```
 you                          a maintainer                 GitHub Actions
 ───                          ────────────                 ──────────────
 1. pull request: the    ──►  2. merge it                ─┐
    video's folder            3. draft release + MP4      │
                              4. watch it, publish it  ──►  5. upload to YouTube with video.json
                                                            6. commit the YouTube id here
                                                            7. open the docs pull request in
                                                               CrestApps.OrchardCore  ──►  8. merge it: the
                                                                                             video shows on
                                                                                             the docs pages
```

## 1. Make the video

Follow the skill in `.agents/skills/crestapps-video-producer/SKILL.md` (an AI coding agent picks it up when asked
for a video). It holds the one look every video shares: 1080p, the CrestApps cards and frame, the Inter font, the same
neural voice, captions and chapters. Don't change it for one video; change the skill so every later video follows.

- A **narrated video** has a storyboard, recorder scenes and narration; start from a copy of `videos/ai-profile-builder/`.
- A **reel** joins existing silent screencasts into one video with a chapter each (`scripts/reel.py`); see
  `videos/ai-walkthroughs/`.

Record a copy of a demo site with fake data only: never real people, phone numbers, email addresses, accounts or
tokens. Check the result as the skill says (OCR at one frame per second) before submitting.

## 2. Submit it

Open a pull request with the video's folder, `videos/<slug>/`. The slug is the video's permanent name: the
documentation embeds the video by it, so pick a short, lowercase, hyphenated name (`report-builder`) and don't
rename it later.

| File | Required | What it is |
| --- | --- | --- |
| `video.json` | yes | What YouTube shows: the title (at most 100 characters, `<Feature>: <benefit> \| CrestApps for Orchard Core`), the description, tags, hashtags, chapters, links to the documentation, playlists, `privacyStatus`, and `embeddedIn` (the docs pages that show it). Leave `youtube` empty: the workflow fills it in. |
| `thumbnail.jpg` | yes | The YouTube thumbnail: the opening card, as JPEG |
| `README.md` | yes | What the video covers, and how to make it again |
| `<slug>.srt`, `<slug>.vtt` | narrated videos | The captions (uploaded to YouTube with the video) |
| `storyboard.json`, `slides.py`, `recorder/`, `durations.json` | narrated videos | Everything needed to make it again |
| `reel.json` | reels | The screencasts, by their path in a source repository at a fixed commit |

Never commit the MP4, audio files, `build/`, `clips/`, sign-ins (`state.json`) or a copy of a site's `App_Data`:
`.gitignore` keeps them out, so don't force them in.

**Chapters**: YouTube shows chapters only when there are at least three, the first at `0:00`, and every one is at
least 10 seconds long. If one is shorter, it shows none.

In the pull request, attach a link to the MP4 (a shared drive, or a draft release a maintainer makes for you) so the
reviewer can watch it.

### Pull request checklist

- [ ] `videos/<slug>/` only, with no MP4 or audio file
- [ ] The video was recorded on fake data and checked for private data
- [ ] `video.json`: title of 100 characters or fewer, description, chapters (each 10 seconds or more), links, `embeddedIn`
- [ ] `thumbnail.jpg`, and captions for a narrated video
- [ ] `python tools/youtube/youtube.py description <slug>` prints what YouTube will show, without an error
- [ ] `python tools/youtube/youtube.py index` updated the table in `README.md`

## 3. Publishing (maintainers)

Publishing a GitHub release is what sends a video to YouTube.

1. Merge the video's pull request.
2. Create the draft release of its next version, with the MP4 attached:

   ```bash
   python tools/youtube/youtube.py release <slug> --file <path to the MP4>
   ```

   It tags `<slug>/v1` (then `v2`, ...) on `main`. A draft is visible only to this repository's members: watch the
   video on the release page.
3. **Publish the release.** The **Publish video** workflow then uploads the MP4 with everything in `video.json`
   (title, description with chapters and links, tags, captions, thumbnail, playlists), records the YouTube id in
   `video.json` and the README table, adds the YouTube link to the release, and opens the documentation pull request.

Publish at most about **four videos a day**: each upload costs about 2,150 of the YouTube API's 10,000 daily units.
A release that fails on the quota can be run again the next day from **Actions > Publish video > Run workflow**, with
its tag.

- **A new recording** of a published video is its next version (`youtube.py release <slug>` makes `<slug>/v2`). It is
  a new YouTube video, because YouTube can't replace a video's file; the previous one becomes unlisted, so its links
  keep working, and the documentation pull request points the docs at the new one.
- **A new title, description, tags or chapters** need no release: change `video.json` and merge, and the **YouTube
  metadata** workflow updates the video.

## 4. The documentation pull request

The documentation never stores videos. A docs page embeds a video by its slug, and reads its YouTube id from
`src/CrestApps.Docs/src/data/videos.json` in CrestApps.OrchardCore:

```mdx
<YouTube video="report-builder" title="Video overview of the Report Builder" />
<YouTube video="report-builder" title="Video of building a report" start={99} end={146} />
```

`start` and `end` (whole seconds) play one chapter of a longer video, for the section of a page about that task.

- **The feature's own pull request** in CrestApps.OrchardCore adds the embeds. Until the video is published, a slug
  that isn't in `videos.json` renders nothing, so the embeds can go in first.
- **The publish workflow's pull request** (`Link the <title> video (vN)`, from a `videos/<slug>-vN` branch) adds or
  changes only the slug's line in `videos.json`, and lists the pages that embed it. Review that those pages show the
  right video, and merge it.

If the workflow couldn't open it (no `DOCS_REPOSITORY_TOKEN`), its log says which line to add to `videos.json`; open
that pull request by hand.

## One-time setup (maintainers)

The workflows need four repository secrets: three that let them upload to the CrestApps YouTube channel, and one that
lets them open pull requests in CrestApps.OrchardCore. `tools/youtube/README.md` explains each step in detail.

1. **Google Cloud**: create a project, enable the **YouTube Data API v3**, configure the OAuth consent screen
   (External, scopes `youtube.upload` and `youtube.force-ssl`, the channel owner as a test user), and **publish the
   app** to production so its refresh token doesn't expire after 7 days.
2. **OAuth client**: create an OAuth client ID of type **Desktop app**, and save its JSON as
   `tools/youtube/client_secret.json` (git-ignored).
3. **Sign in once**, as the account that owns the CrestApps channel:

   ```bash
   pip install -r tools/youtube/requirements.txt
   python tools/youtube/youtube.py auth
   ```

4. **Store the secrets** in this repository (from the client JSON and the token file `auth` wrote, in
   `~/.crestapps-videos/youtube-token.json`):

   ```bash
   gh secret set YOUTUBE_CLIENT_ID --repo CrestApps/CrestApps.Videos          # the client_id
   gh secret set YOUTUBE_CLIENT_SECRET --repo CrestApps/CrestApps.Videos      # the client_secret
   gh secret set YOUTUBE_REFRESH_TOKEN --repo CrestApps/CrestApps.Videos      # the refresh_token
   ```

   Each command asks for the value, so it never lands in your shell history.
5. **The documentation token**: create a fine-grained personal access token (or a GitHub App token) with access to
   **CrestApps/CrestApps.OrchardCore** only, and **Contents** and **Pull requests** set to read and write. Store it:

   ```bash
   gh secret set DOCS_REPOSITORY_TOKEN --repo CrestApps/CrestApps.Videos
   ```

6. **Public uploads**: YouTube keeps every video uploaded through the API by an unaudited project **private**, and a
   private video doesn't play in the documentation. Request the free
   [API compliance audit](https://support.google.com/youtube/contact/yt_api_form) once for the Google Cloud project.
   Until it is approved, upload in YouTube Studio instead, and record the id in `video.json`
   (`"youtube": {"id": "...", "url": "https://youtu.be/...", "version": "v1"}`), then add it to the docs' `videos.json`.
