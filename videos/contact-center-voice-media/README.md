# Voice Media

**Feature:** `CrestApps.OrchardCore.ContactCenter` with Telephony (for the master copy) · **Length:** see `video.json`

Why a voice media library, adding a clip, the badges, replacing, protecting and re-syncing a clip, where a clip is used,
deleting a clip, and use cases. The video shows the admin only: no code or configuration files. The title, description,
tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only: a four-second test tone made with ffmpeg stands in for the
hold music (`ffmpeg -f lavfi -i sine=frequency=440:duration=4 hold-music.mp3`, kept outside the repository and passed in
`VIDEO_AUDIO`). The phone provider is never reached (`HTTP_PROXY`/`HTTPS_PROXY` point at `http://127.0.0.1:9`), so saving
a clip shows the real warning that the provider did not accept a copy, and Re-sync media shows the provider's error.

1. Run `src/Startup/CrestApps.OrchardCore.Cms.Web` of CrestApps.OrchardCore with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, the dead proxy above and `NO_PROXY=localhost,127.0.0.1`. Set it up with the **Blank** recipe, site
   name **Contoso**, SQLite and the admin user `admin`, and enable the Contact Center and Telephony features with a
   phone provider (Telnyx or Asterisk).
2. Record in order: `02-add` creates the clip, `04-manage` edits and re-syncs it, `06-delete` deletes it (so the site must
   hold the clip again before it is re-recorded).
