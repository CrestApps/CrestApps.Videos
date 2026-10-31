# Repeating events and attendees in Calendars

**YouTube:** not uploaded yet · **Feature:** `CrestApps.OrchardCore.Calendars` (Calendars)

Making an event repeat and ending the series, changing or cancelling one occurrence or the rest of the series, inviting users and guests, answering an invitation, and status, busy or free, and visibility. It is recorded as maria.lopez, and as jamie.chen for the clip that answers an invitation.

The title, description, tags and chapters YouTube shows are in `video.json`; `calendars-events-recurrence-attendees.vtt` and `calendars-events-recurrence-attendees.srt` are its captions.
Each task chapter is also assembled as a short video (`"short"` in the storyboard) for the documentation page about that task.

## Recording it again

Every Calendars video is recorded on the same fake demo site: a new site (Blank recipe, site name Contoso, time zone
America/New_York, administrator `admin` / `Password1!`) behind a dead proxy. `calendars-getting-started/recorder` seeds it
(`Recorder prepare`): the Calendars, Team Calendars and Resource Calendars features, the fake teams (roles Sales Team,
Support Team, Facilities, Managers) and people (jamie.chen, maria.lopez, sam.patel, alex.kim, priya.shah, dana.reyes),
and the Company calendar, a personal calendar for each person and two team calendars with events in the weeks of
2026-10-26 and 2026-11-02. The clips (`01-repeat`, `02-series`, `03-invite`, `04-respond`, `05-status`) are recorded as maria.lopez, except `04-respond`, which is recorded as jamie.chen (`state-jamie.json`).

Sign in once for each person the clips are recorded as (`Recorder setup` with `VIDEO_USER_NAME`), and save the sign-in as
`state.json` (and `state-<name>.json` for another person) in this folder; they are git-ignored. Clips build on each other's
events, so record them in order from a clean copy of the seeded site. The scenes are in `recorder/`.
