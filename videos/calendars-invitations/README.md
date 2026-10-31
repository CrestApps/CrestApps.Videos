# Calendar Invitations

**YouTube:** not published yet · **Length:** 4:15 · **Feature:** `CrestApps.OrchardCore.Calendars.Invitations`

With Calendar Invitations on, Orchard Core emails the people invited to a calendar event: an invitation with a calendar file and Accept, Maybe and Decline links, then updates and cancellations. Guests answer from the email without signing in.

Chapters:

- 0:00 Introduction
- 0:19 What is sent
- 1:03 Set up email
- 1:49 Invite people
- 2:26 Answer from the email
- 2:57 Changes and cancellations
- 3:37 Good to know
- 4:04 Wrap-up

The title, description, tags and chapters YouTube shows are in `video.json`; `calendars-invitations.vtt` and `calendars-invitations.srt` are its captions.

## Recording it again

It was recorded on a **fresh local site** with fake data only (no mask script): CrestApps.OrchardCore `main`
(`src/Startup/CrestApps.OrchardCore.Cms.Web`), the Blog setup recipe, site name Contoso Studio, SQLite, behind a dead
proxy (`HTTP_PROXY`/`HTTPS_PROXY` = `http://127.0.0.1:9`). Enable the Calendars features, import the seed calendars with the Calendars recipe step and take a copy of `App_Data`. Record in order: `02-mail` (SMTP with a pickup directory, and the Base url), `03-invite`, then build the message pages with the helper that renders the site's `.eml` files (`Emails` folder under `App_Data/Sites/Default`) as web pages, `04-answer`, `05-change`, build the update and cancellation pages, and `05-mails`. The message pages are opened from `file:///` paths set in `Scenes.cs`; the pickup folder never sends mail.

Sign the recorder in, narrate, record every clip, then assemble:

```bash
python .agents/skills/crestapps-video-producer/scripts/narrate.py videos/calendars-invitations
Recorder setup && Recorder record all   # VIDEO_ROOT=videos/calendars-invitations, VIDEO_BASE_URL=<site>
python .agents/skills/crestapps-video-producer/scripts/assemble.py videos/calendars-invitations
```

Restore the site's `App_Data` from the copy made before the first take before every full take.
