# Dispositions

**Feature:** `CrestApps.OrchardCore.Omnichannel.Managements` · **Length:** see `video.json`

Why dispositions exist, how to create them (name, description, outcome, lead status), what cannot change once saved, the outcomes the platform applies by itself, and which disposition is used when a call ends before an agent joins. The video shows the admin only: no code or configuration files. The title, description, tags and
chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only: made-up users (`jamie.chen`, `maria.lopez`, `sam.patel`),
fake 555 numbers, and a made-up Asterisk address that is never contacted, so it needs no mask script. Calls never
leave the machine, and no call connects, so the video shows the screens as they are and uses slides for what happens
in a live call.

1. Run CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` with `ORCHARD_APP_DATA` pointing at an
   empty folder outside both repositories, with `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9` and
   `NO_PROXY=localhost,127.0.0.1`, so nothing reaches a phone provider.
2. Set the site up with the **Blank** recipe, site name **Contoso**, SQLite and the admin user `admin`, then enable the
   Contact Center, Omnichannel, Telephony and DNC features.
3. Build the recorder with this video's scenes, sign in, seed the site, and record the clips in order:

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/contact-center-dispositions/recorder
   export VIDEO_ROOT=<repo>/videos/contact-center-dispositions VIDEO_BASE_URL=http://localhost:5385
   <bin>/Recorder setup
   <bin>/Recorder prepare
   <bin>/Recorder record 02-create 03-edit 04b-outcomes
   ```

No `prepare` is needed on the standard fake site. `02-create` saves `Test Drive Booked`, `Call Back Later` and `Not Interested`, `03-edit` saves nothing, and `04b-outcomes` saves `Number Not In Service` (outcome Number not in service) and `No Answer` (outcome No answer).
