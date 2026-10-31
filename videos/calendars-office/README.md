# Office Calendars

**YouTube:** not published yet · **Length:** 4:11 · **Feature:** `CrestApps.OrchardCore.Calendars.Office`

An Orchard Core office calendar shows who is in the office and which desks are booked, up to the number of desks. People mark the days they are in, book a desk and register visitors, and the calendar refuses a booking that would not fit.

Chapters:

- 0:00 Introduction
- 0:19 Why an office calendar
- 1:04 Set up an office
- 1:55 In the office, at a desk, or a visitor
- 3:00 Close the office
- 3:26 Good to know
- 3:59 Wrap-up

The title, description, tags and chapters YouTube shows are in `video.json`; `calendars-office.vtt` and `calendars-office.srt` are its captions.

## Recording it again

It was recorded on a **fresh local site** with fake data only (no mask script): CrestApps.OrchardCore `main`
(`src/Startup/CrestApps.OrchardCore.Cms.Web`), the Blog setup recipe, site name Contoso Studio, SQLite, behind a dead
proxy (`HTTP_PROXY`/`HTTPS_PROXY` = `http://127.0.0.1:9`). Enable the Calendars features and Workflows, import the seed calendars (a company calendar, a design team, a room Studio A and a three-desk Lisbon office) with the Calendars recipe step, and take a copy of `App_Data`.

Sign the recorder in, narrate, record every clip, then assemble:

```bash
python .agents/skills/crestapps-video-producer/scripts/narrate.py videos/calendars-office
Recorder setup && Recorder record all   # VIDEO_ROOT=videos/calendars-office, VIDEO_BASE_URL=<site>
python .agents/skills/crestapps-video-producer/scripts/assemble.py videos/calendars-office
```

Restore the site's `App_Data` from the copy made before the first take before every full take.
