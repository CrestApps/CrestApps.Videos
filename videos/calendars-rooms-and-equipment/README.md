# Rooms and equipment in Calendars

**YouTube:** not uploaded yet · **Feature:** `CrestApps.OrchardCore.Calendars` (Calendars, Resource Calendars)

Adding a Room and an Equipment calendar, booking a room by inviting it to an event, the overlap limit that refuses a second booking, and booking or closing a room on its own calendar. Recorded as admin (to add the calendars and book on the room), and as maria.lopez (to book by inviting the room).

The title, description, tags and chapters YouTube shows are in `video.json`; `calendars-rooms-and-equipment.vtt` and `calendars-rooms-and-equipment.srt` are its captions.
Each task chapter is also assembled as a short video (`"short"` in the storyboard) for the documentation page about that task.

## Recording it again

Every Calendars video is recorded on the same fake demo site: a new site (Blank recipe, site name Contoso, time zone
America/New_York, administrator `admin` / `Password1!`) behind a dead proxy. `calendars-getting-started/recorder` seeds it
(`Recorder prepare`): the Calendars, Team Calendars and Resource Calendars features, the fake teams (roles Sales Team,
Support Team, Facilities, Managers) and people (jamie.chen, maria.lopez, sam.patel, alex.kim, priya.shah, dana.reyes),
and the Company calendar, a personal calendar for each person and two team calendars with events in the weeks of
2026-10-26 and 2026-11-02. The clips (`01-create`, `02-book`, `03-direct`) run in order. `01-create` and `03-direct` are recorded as admin (`state-admin.json`), `02-book` as maria.lopez.

Sign in once for each person the clips are recorded as (`Recorder setup` with `VIDEO_USER_NAME`), and save the sign-in as
`state.json` (and `state-<name>.json` for another person) in this folder; they are git-ignored. Clips build on each other's
events, so record them in order from a clean copy of the seeded site. The scenes are in `recorder/`.
