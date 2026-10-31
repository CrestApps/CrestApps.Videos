# Feeds and Subscriptions

**YouTube:** not published yet · **Length:** 3:36 · **Feature:** `CrestApps.OrchardCore.Calendars.Feeds`

Calendar Feeds let Outlook, Google Calendar and phones follow an Orchard Core calendar through a private subscription address, and let you export a calendar as an .ics file or import one. Re-importing a file updates events by their identifier instead of adding them twice.

Chapters:

- 0:00 Introduction
- 0:19 Why feeds
- 0:56 The subscription address
- 1:59 Export and import
- 2:55 Good to know
- 3:26 Wrap-up

The title, description, tags and chapters YouTube shows are in `video.json`; `calendars-feeds-and-subscriptions.vtt` and `calendars-feeds-and-subscriptions.srt` are its captions.

## Recording it again

It was recorded on a **fresh local site** with fake data only (no mask script): CrestApps.OrchardCore `main`
(`src/Startup/CrestApps.OrchardCore.Cms.Web`), the Blog setup recipe, site name Contoso Studio, SQLite, behind a dead
proxy (`HTTP_PROXY`/`HTTPS_PROXY` = `http://127.0.0.1:9`). Enable the Calendars features, import the seed calendars with the Calendars recipe step and take a copy of `App_Data`. `05-import` imports two small made-up `.ics` files (`partner-events.ics`, three events with UID, LAST-MODIFIED and SEQUENCE; the second moves one event and raises its SEQUENCE); the recorder's `ImportFolder` points at them. The importer counts an event as unchanged only when its SEQUENCE is the same and its LAST-MODIFIED is not newer than the stored event.

Sign the recorder in, narrate, record every clip, then assemble:

```bash
python .agents/skills/crestapps-video-producer/scripts/narrate.py videos/calendars-feeds-and-subscriptions
Recorder setup && Recorder record all   # VIDEO_ROOT=videos/calendars-feeds-and-subscriptions, VIDEO_BASE_URL=<site>
python .agents/skills/crestapps-video-producer/scripts/assemble.py videos/calendars-feeds-and-subscriptions
```

Restore the site's `App_Data` from the copy made before the first take before every full take.
