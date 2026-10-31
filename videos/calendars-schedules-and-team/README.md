# Schedules and team availability in Calendars

**YouTube:** not uploaded yet · **Feature:** `CrestApps.OrchardCore.Calendars` (Calendars, Team Calendars)

Whose schedule (one person, several people, everyone), how busy, tentative and free time are decided, and the Team Availability page with its list of times everyone shares. Recorded as maria.lopez, a member of the Sales Team who is also a manager.

The title, description, tags and chapters YouTube shows are in `video.json`; `calendars-schedules-and-team.vtt` and `calendars-schedules-and-team.srt` are its captions.
Each task chapter is also assembled as a short video (`"short"` in the storyboard) for the documentation page about that task.

## Recording it again

Every Calendars video is recorded on the same fake demo site: a new site (Blank recipe, site name Contoso, time zone
America/New_York, administrator `admin` / `Password1!`) behind a dead proxy. `calendars-getting-started/recorder` seeds it
(`Recorder prepare`): the Calendars, Team Calendars and Resource Calendars features, the fake teams (roles Sales Team,
Support Team, Facilities, Managers) and people (jamie.chen, maria.lopez, sam.patel, alex.kim, priya.shah, dana.reyes),
and the Company calendar, a personal calendar for each person and two team calendars with events in the weeks of
2026-10-26 and 2026-11-02. The clips (`01-schedule`, `03-team`) are recorded as maria.lopez. Run `Recorder prepare` of this video first (it adds one tentative event for sam.patel).

Sign in once for each person the clips are recorded as (`Recorder setup` with `VIDEO_USER_NAME`), and save the sign-in as
`state.json` (and `state-<name>.json` for another person) in this folder; they are git-ignored. Clips build on each other's
events, so record them in order from a clean copy of the seeded site. The scenes are in `recorder/`.
