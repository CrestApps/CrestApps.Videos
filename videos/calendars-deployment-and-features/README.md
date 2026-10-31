# Calendars: Features and Deployment

**YouTube:** not published yet · **Length:** 4:05 · **Feature:** `CrestApps.OrchardCore.Calendars`

A guide to the Orchard Core calendar features (Calendars, Team, Resource, Office, Reminders, Invitations, Feeds, External, API), how to copy calendars, events and categories to another site with a deployment plan, what the import does and does not do, and what to check when something does not work.

Chapters:

- 0:00 Introduction
- 0:16 Which feature to turn on
- 1:52 Copy calendars to another site
- 2:35 What the import does
- 3:12 Troubleshooting
- 3:56 Wrap-up

The title, description, tags and chapters YouTube shows are in `video.json`; `calendars-deployment-and-features.vtt` and `calendars-deployment-and-features.srt` are its captions.

## Recording it again

It was recorded on a **fresh local site** with fake data only (no mask script): CrestApps.OrchardCore `main`
(`src/Startup/CrestApps.OrchardCore.Cms.Web`), the Blog setup recipe, site name Contoso Studio, SQLite, behind a dead
proxy (`HTTP_PROXY`/`HTTPS_PROXY` = `http://127.0.0.1:9`). Enable the Calendars features (not Calendars API), import the seed calendars with the Calendars recipe step and take a copy of `App_Data`. `03-plan` creates the deployment plan on camera, so restore the copy before every take.

Sign the recorder in, narrate, record every clip, then assemble:

```bash
python .agents/skills/crestapps-video-producer/scripts/narrate.py videos/calendars-deployment-and-features
Recorder setup && Recorder record all   # VIDEO_ROOT=videos/calendars-deployment-and-features, VIDEO_BASE_URL=<site>
python .agents/skills/crestapps-video-producer/scripts/assemble.py videos/calendars-deployment-and-features
```

Restore the site's `App_Data` from the copy made before the first take before every full take.
