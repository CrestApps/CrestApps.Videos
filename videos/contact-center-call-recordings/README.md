# Call Recordings

**Feature:** `CrestApps.OrchardCore.ContactCenter.Recording` · **Length:** see `video.json`

Why call recordings, who sees which calls, which calls are recorded, how to search them, the call page and its transcript,
and how to erase a recording. The video shows the admin only: no code or configuration files. The title, description, tags
and chapters YouTube shows are in `video.json`.

## Recording it again

The demo site never places a call (its phone providers are unreachable), so it holds no recordings: the clip shows the
search bar and the empty list as they are, and the call page, the transcript and erasing are shown on slides. Record it
on a **fresh site** with fake data only, so it needs no mask script.

1. Run `src/Startup/CrestApps.OrchardCore.Cms.Web` of CrestApps.OrchardCore with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, with `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9` and `NO_PROXY=localhost,127.0.0.1`. Set it
   up with the **Blank** recipe, site name **Contoso**, SQLite and the admin user `admin`, and enable the Contact Center
   features (Call Recording, Voice, Agents).
2. `Recorder setup`, then record `04-search`; it needs no seed data and saves nothing.
