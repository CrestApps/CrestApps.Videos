# Business Hours Calendars

**Feature:** `CrestApps.OrchardCore.ContactCenter.BusinessHours` · **Length:** see `video.json`

Why a shared calendar of opening hours, where a calendar is used (queues, inbound entry points, dialer profiles, automatic
activity loads), how to create one with a weekly schedule, a time zone and holidays, the rules worth knowing, and use cases.
The video shows the admin only: no code or configuration files. The title, description, tags and chapters YouTube shows
are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (a calendar named Support hours), so it needs no mask script.

1. Run `src/Startup/CrestApps.OrchardCore.Cms.Web` of CrestApps.OrchardCore with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, with `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9` and `NO_PROXY=localhost,127.0.0.1`. Set it up
   with the **Blank** recipe, site name **Contoso**, SQLite and the admin user `admin`, and enable the Contact Center
   Business Hours feature.
2. `Recorder setup`, then record `03-create`; it needs no seed data and creates the calendar.
