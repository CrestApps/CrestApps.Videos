# Categories and colors in Calendars

**YouTube:** not uploaded yet · **Feature:** `CrestApps.OrchardCore.Calendars` (Calendars)

How an event gets its color (its own color, its category, its kind of event, its calendar), making personal and shared categories, putting events in a category, filtering the calendar by category and deleting a category. Recorded as maria.lopez, and as admin for the shared category.

The title, description, tags and chapters YouTube shows are in `video.json`; `calendars-categories-and-colors.vtt` and `calendars-categories-and-colors.srt` are its captions.
Each task chapter is also assembled as a short video (`"short"` in the storyboard) for the documentation page about that task.

## Recording it again

Every Calendars video is recorded on the same fake demo site: a new site (Blank recipe, site name Contoso, time zone
America/New_York, administrator `admin` / `Password1!`) behind a dead proxy. `calendars-getting-started/recorder` seeds it
(`Recorder prepare`): the Calendars, Team Calendars and Resource Calendars features, the fake teams (roles Sales Team,
Support Team, Facilities, Managers) and people (jamie.chen, maria.lopez, sam.patel, alex.kim, priya.shah, dana.reyes),
and the Company calendar, a personal calendar for each person and two team calendars with events in the weeks of
2026-10-26 and 2026-11-02. The clips (`02-make`, `03-shared`, `04-use`, `05-filter`, `06-delete`) run in order; `03-shared` is recorded as admin (`state-admin.json`), the others as maria.lopez.

Sign in once for each person the clips are recorded as (`Recorder setup` with `VIDEO_USER_NAME`), and save the sign-in as
`state.json` (and `state-<name>.json` for another person) in this folder; they are git-ignored. Clips build on each other's
events, so record them in order from a clean copy of the seeded site. The scenes are in `recorder/`.
