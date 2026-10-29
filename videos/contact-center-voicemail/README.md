# Voicemail

**Feature:** `CrestApps.OrchardCore.ContactCenter.InboundVoice` with Agents and the Telephony soft phone · **Length:** see `video.json`

Why voicemail, where a message goes, recording or uploading a greeting, listening in the soft phone, whose inbox a message
goes to, and working a shared voicemail box. The video shows the admin only: no code or configuration files. The title,
description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The demo site never takes a call (its phone providers are unreachable), so the Voicemail tab and the shared box are empty
and the video shows them as they are; claiming, calling back and marking done are on slides. A four-second test tone made
with ffmpeg stands in for the greeting (`ffmpeg -f lavfi -i sine=frequency=440:duration=4 greeting.mp3`, kept outside the
repository and passed in `VIDEO_AUDIO`). Saving it shows the real warning that the provider did not accept a copy.

1. Run `src/Startup/CrestApps.OrchardCore.Cms.Web` of CrestApps.OrchardCore with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, with `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9` and `NO_PROXY=localhost,127.0.0.1`. Set it
   up with the **Blank** recipe, site name **Contoso**, SQLite and the admin user `admin`, and enable the Contact Center
   (Inbound Voice, Agents) and Telephony soft phone features.
2. `Recorder setup`, then record `03-greeting` (saves the greeting), `04-listen` and `06-shared`.
