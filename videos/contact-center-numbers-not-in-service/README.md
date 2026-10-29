# Numbers Not In Service

**Feature:** `CrestApps.OrchardCore.Omnichannel.Managements` · **Length:** see `video.json`

Why dead numbers matter, what the platform does when a call finds one (shown with slides, because calls never connect on the recording site), choosing the not-in-service disposition, where numbers are skipped, and managing the list: marking a number by hand, searching, Allow dialing, and the bulk action. The video shows the admin only: no code or configuration files. The title, description, tags and
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/contact-center-numbers-not-in-service/recorder
   export VIDEO_ROOT=<repo>/videos/contact-center-numbers-not-in-service VIDEO_BASE_URL=http://localhost:5385
   <bin>/Recorder setup
   <bin>/Recorder prepare
   <bin>/Recorder record 03-disposition 05-list
   ```

Record it after `contact-center-dispositions`: `03-disposition` opens the disposition `Number Not In Service` that video saved. `05-list` marks three fake numbers (+1 702 555 0123, 0188 and 0134) by hand, and allows dialing for `0188`.
