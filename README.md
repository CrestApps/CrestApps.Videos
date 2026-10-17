# CrestApps Videos

The source of every narrated CrestApps video: the storyboards, slides, recorder scenes and YouTube metadata, and the
skill that turns them into a video with one look and one voice.

The videos themselves are published on the CrestApps YouTube channel and embedded from there in the documentation.
**No video file is ever committed**, here or in a product repository, so cloning a repository never downloads videos.
Everything kept here is what's needed to make a video again, or to change how it appears on YouTube.

## Videos

<!-- videos:start -->
| Video | Length | YouTube | Source |
| --- | --- | --- | --- |
| [AI Profile Builder: Train Your AI Assistant by Role-Play and Feedback](videos/ai-profile-builder) | 4:40 | [https://youtu.be/UOKvEY-kID4](https://youtu.be/UOKvEY-kID4) | [CrestApps/CrestApps.OrchardCore#910](https://github.com/CrestApps/CrestApps.OrchardCore/pull/910) |
| [AI Walkthroughs: Providers, Chat, Knowledge, Agents and Widgets](videos/ai-walkthroughs) | 11:34 | [https://youtu.be/rgGJJM3-u2c](https://youtu.be/rgGJJM3-u2c) | CrestApps/CrestApps.OrchardCore |
| [CrestApps Commerce: Payments, Subscriptions, Payment Plans and Invoices](videos/commerce) | 10:30 | [https://youtu.be/YQHVdIucYgI](https://youtu.be/YQHVdIucYgI) | CrestApps/CrestApps.OrchardCore |
| [Contact Center Agent Walkthroughs: Calls, Messages and Activities](videos/contact-center-agent-walkthroughs) | 13:57 | [https://youtu.be/B6T-s_80exw](https://youtu.be/B6T-s_80exw) | CrestApps/CrestApps.OrchardCore |
| [Contact Center Setup Walkthroughs: Numbers, Queues, IVR and Campaigns](videos/contact-center-setup-walkthroughs) | 23:17 | [https://youtu.be/nMQ4AC7VtWA](https://youtu.be/nMQ4AC7VtWA) | CrestApps/CrestApps.OrchardCore |
| [Omnichannel Management 2.0 Walkthroughs](videos/omnichannel-2-walkthroughs) | 6:43 | [https://youtu.be/YPLb7Wf9f-4](https://youtu.be/YPLb7Wf9f-4) | CrestApps/CrestApps.OrchardCore |
| [Report Builder: Drag-and-Drop Reports Over Your Site's Data](videos/report-builder) | 10:32 | not uploaded | [CrestApps/CrestApps.OrchardCore#814](https://github.com/CrestApps/CrestApps.OrchardCore/pull/814) |
| [Site Administration Walkthroughs: Fields, Roles, Users and Imports](videos/site-administration-walkthroughs) | 5:14 | [https://youtu.be/f1en3LF024s](https://youtu.be/f1en3LF024s) | CrestApps/CrestApps.OrchardCore |
| [Tenant Hierarchy: Parent Tenants That Create and Open Their Own Sites](videos/tenant-hierarchy) | 8:53 | [https://youtu.be/eL-gcGqRWxw](https://youtu.be/eL-gcGqRWxw) | [CrestApps/CrestApps.OrchardCore#818](https://github.com/CrestApps/CrestApps.OrchardCore/pull/818) |
<!-- videos:end -->

This table is generated from each video's `video.json` by `python tools/youtube/youtube.py index`.

## What's here

| Path | What it is |
| --- | --- |
| `videos/<slug>/` | One video: `video.json` (YouTube title, description, tags, chapters, links, id), `storyboard.json` (narration), `slides.py`, `recorder/Scenes*.cs`, `durations.json`, captions, and a README on how to record it again |
| `videos/*-walkthroughs/` | Reels: the documentation's silent screencasts joined into one video per area, built by `scripts/reel.py` from `reel.json` |
| `.agents/skills/crestapps-video-producer/` | The skill: the video specification, the narration, recording and assembly scripts, and the shared recorder |
| `branding/` | The CrestApps logo and the Inter font every video uses |
| `tools/youtube/` | Uploads a video, pushes `video.json` changes, computes chapters, and writes the table above |
| `.github/workflows/publish-video.yml` | Publishing a release uploads its video and opens the docs pull request |
| `.github/workflows/youtube-metadata.yml` | Pushes a changed `video.json` to YouTube when it is merged |
| `CONTRIBUTING.md` | How to submit a video, how it is published, and the one-time setup |

## Making a video

**To submit a video, read [CONTRIBUTING.md](CONTRIBUTING.md)**: what a pull request holds, how a video is published
to YouTube, how the documentation pull request follows, and the one-time setup of the secrets.

Follow `.agents/skills/crestapps-video-producer/SKILL.md`. In short:

1. Copy `videos/ai-profile-builder/` to `videos/<slug>/` and write the storyboard and `video.json`.
2. Narrate (`scripts/narrate.py`), record the clips against a local site with fake data (the recorder), and assemble
   (`scripts/assemble.py`); then `youtube.py chapters`.
3. Check the video for private data, and merge its folder (never the MP4) to `main`.
4. `python tools/youtube/youtube.py release <slug> --file <mp4>` creates a draft release with the MP4 attached.
   Watch it there, then **publish the release**: the **Publish video** workflow uploads it to YouTube with its
   metadata, captions, thumbnail and playlists, records the YouTube id here, and opens a pull request in
   CrestApps.OrchardCore that links it on every docs page with `<YouTube video="<slug>" />`.

A video's versions are its releases (`<slug>/v1`, `<slug>/v2`, ...): a re-recording is a new release, and the previous
version on YouTube becomes unlisted. Changing a published video's title, description or chapters is an edit to its
`video.json`; merging it updates YouTube. See `tools/youtube/README.md` for the secrets these workflows need.

## Requirements

Python 3.11+ (`pip install pillow pygments imageio-ffmpeg "edge-tts>=7.3.1"`, and `tools/youtube/requirements.txt`
to publish), the .NET 10 SDK for the recorder, and internet access for the voice.
