# Calendar Reminders

**YouTube:** not published yet · **Length:** 3:02 · **Feature:** `CrestApps.OrchardCore.Calendars.Reminders`

With Calendar Reminders on, an Orchard Core calendar event can remind its owner and invited users on screen before it starts. The reminder stays on every admin page until it is dismissed, and workflows can act at the same moments.

Chapters:

- 0:00 Introduction
- 0:17 Why reminders
- 0:54 Set a reminder
- 1:39 The reminder on screen
- 2:14 Workflows
- 2:52 Wrap-up

The title, description, tags and chapters YouTube shows are in `video.json`; `calendars-reminders.vtt` and `calendars-reminders.srt` are its captions.

## Recording it again

It was recorded on a **fresh local site** with fake data only (no mask script): CrestApps.OrchardCore `main`
(`src/Startup/CrestApps.OrchardCore.Cms.Web`), the Blog setup recipe, site name Contoso Studio, SQLite, behind a dead
proxy (`HTTP_PROXY`/`HTTPS_PROXY` = `http://127.0.0.1:9`). Enable the Calendars features, import the seed calendars with the Calendars recipe step and take a copy of `App_Data`. The clips are recorded in order: the reminder is created in `02-set` and shown by `03-show`, which waits off camera for the next once-a-minute check.

Sign the recorder in, narrate, record every clip, then assemble:

```bash
python .agents/skills/crestapps-video-producer/scripts/narrate.py videos/calendars-reminders
Recorder setup && Recorder record all   # VIDEO_ROOT=videos/calendars-reminders, VIDEO_BASE_URL=<site>
python .agents/skills/crestapps-video-producer/scripts/assemble.py videos/calendars-reminders
```

Restore the site's `App_Data` from the copy made before the first take before every full take.
