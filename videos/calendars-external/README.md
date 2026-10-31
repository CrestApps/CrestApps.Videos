# External Calendars

**YouTube:** not published yet · **Length:** 3:30 · **Feature:** `CrestApps.OrchardCore.Calendars.External`

With External Calendars on, an Orchard Core calendar can follow a secret iCalendar (.ics) address and show its events read only, kept up to date by the site. This video shows subscribing, the refresh interval, what the events look like, and how addresses on the local network are refused.

Chapters:

- 0:00 Introduction
- 0:19 Why external calendars
- 1:05 Subscribe a calendar
- 1:54 What happens next
- 2:52 Good to know
- 3:21 Wrap-up

The title, description, tags and chapters YouTube shows are in `video.json`; `calendars-external.vtt` and `calendars-external.srt` are its captions.

## Recording it again

It was recorded on a **fresh local site** with fake data only (no mask script): CrestApps.OrchardCore `main`
(`src/Startup/CrestApps.OrchardCore.Cms.Web`), the Blog setup recipe, site name Contoso Studio, SQLite, behind a dead
proxy (`HTTP_PROXY`/`HTTPS_PROXY` = `http://127.0.0.1:9`). Enable the Calendars features and take a copy of `App_Data`. This video needs the site to reach the internet (start it without the dead proxy): it subscribes to `feed/partner-events.ics` in this folder through its raw GitHub address (set in `recorder/Scenes.cs`; after the video is merged use the `main` address). `03-result` waits off camera for the site's background read (every five minutes) of both calendars.

Sign the recorder in, narrate, record every clip, then assemble:

```bash
python .agents/skills/crestapps-video-producer/scripts/narrate.py videos/calendars-external
Recorder setup && Recorder record all   # VIDEO_ROOT=videos/calendars-external, VIDEO_BASE_URL=<site>
python .agents/skills/crestapps-video-producer/scripts/assemble.py videos/calendars-external
```

Restore the site's `App_Data` from the copy made before the first take before every full take.
