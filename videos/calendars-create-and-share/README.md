# Create and share a calendar

**YouTube:** not uploaded yet · **Feature:** `CrestApps.OrchardCore.Calendars` (Calendars)

Creating a General calendar, its visibility and its four sharing levels, what a person shared at each level sees (details, or only busy time), and the calendar permissions of a role. The calendar is created as admin; the clip that shows what people see is recorded as jamie.chen and as alex.kim.

The title, description, tags and chapters YouTube shows are in `video.json`; `calendars-create-and-share.vtt` and `calendars-create-and-share.srt` are its captions.
Each task chapter is also assembled as a short video (`"short"` in the storyboard) for the documentation page about that task.

## Recording it again

Every Calendars video is recorded on the same fake demo site: a new site (Blank recipe, site name Contoso, time zone
America/New_York, administrator `admin` / `Password1!`) behind a dead proxy. `calendars-getting-started/recorder` seeds it
(`Recorder prepare`): the Calendars, Team Calendars and Resource Calendars features, the fake teams (roles Sales Team,
Support Team, Facilities, Managers) and people (jamie.chen, maria.lopez, sam.patel, alex.kim, priya.shah, dana.reyes),
and the Company calendar, a personal calendar for each person and two team calendars with events in the weeks of
2026-10-26 and 2026-11-02. The clips (`01-create`, `02-jamie`, `03-alex`, `04-permissions`) run in order. `01-create` is recorded as admin and creates the Project Falcon calendar; `Recorder prepare` (with the clip's `PrepareAsync`) then adds its events, and `02-jamie` and `03-alex` are recorded as those people (`state-jamie.json`, `state-alex.json`); `04-permissions` is recorded as admin.

Sign in once for each person the clips are recorded as (`Recorder setup` with `VIDEO_USER_NAME`), and save the sign-in as
`state.json` (and `state-<name>.json` for another person) in this folder; they are git-ignored. Clips build on each other's
events, so record them in order from a clean copy of the seeded site. The scenes are in `recorder/`.
