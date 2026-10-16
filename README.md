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
| [Report Builder: Drag-and-Drop Reports Over Your Site's Data](videos/report-builder) | 10:32 | not uploaded | [CrestApps/CrestApps.OrchardCore#814](https://github.com/CrestApps/CrestApps.OrchardCore/pull/814) |
<!-- videos:end -->

This table is generated from each video's `video.json` by `python tools/youtube/youtube.py index`.

## What's here

| Path | What it is |
| --- | --- |
| `videos/<slug>/` | One video: `video.json` (YouTube title, description, tags, chapters, links, id), `storyboard.json` (narration), `slides.py`, `recorder/Scenes*.cs`, `durations.json`, captions, and a README on how to record it again |
| `.agents/skills/crestapps-video-producer/` | The skill: the video specification, the narration, recording and assembly scripts, and the shared recorder |
| `branding/` | The CrestApps logo and the Inter font every video uses |
| `tools/youtube/` | Uploads a video, pushes `video.json` changes, computes chapters, and writes the table above |
| `.github/workflows/youtube-metadata.yml` | Pushes a changed `video.json` to YouTube when it is merged |

## Making a video

Follow `.agents/skills/crestapps-video-producer/SKILL.md`. In short:

1. Copy `videos/ai-profile-builder/` to `videos/<slug>/` and write the storyboard and `video.json`.
2. Narrate (`scripts/narrate.py`), record the clips against a local site with fake data (the recorder), and assemble
   (`scripts/assemble.py`); then `youtube.py chapters`.
3. Check the video for private data, upload it (`youtube.py upload` or YouTube Studio), and embed it in the
   documentation with `<YouTube id="..." />`.
4. Commit the video's folder (never the MP4) and the index.

Changing a published video's title, description or chapters is an edit to its `video.json`; merging it updates YouTube.

## Requirements

Python 3.11+ (`pip install pillow pygments imageio-ffmpeg "edge-tts>=7.3.1"`, and `tools/youtube/requirements.txt`
to publish), the .NET 10 SDK for the recorder, and internet access for the voice.
