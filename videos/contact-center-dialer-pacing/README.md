# Dialer Pacing

**Feature:** `CrestApps.OrchardCore.ContactCenter.Dialer.Paced` · **Length:** see `video.json`

The abandoned-call settings of a dialer profile (the cap, the message, the measured rate), Predictive pacing (one call per reserved agent, or over-dial), the checks the dialer runs on every record, how a call that ends early is dispositioned, and retries. It continues `contact-center-dialer-profiles`. The video shows the admin only: no code or configuration files. The title, description, tags and
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/contact-center-dialer-pacing/recorder
   export VIDEO_ROOT=<repo>/videos/contact-center-dialer-pacing VIDEO_BASE_URL=http://localhost:5385
   <bin>/Recorder setup
   <bin>/Recorder prepare
   <bin>/Recorder record 02-abandoned 03-predictive
   ```

Record it after `contact-center-dialer-profiles`: `02-abandoned` opens the profile `Lead drive - power` that video saved, and turns on the cap and the message. `03-predictive` creates `Lead drive - predictive`: its first save shows the validation errors on purpose, and the clip then turns on the cap and the message and saves again. The what-happens, retries and live-call chapters are slides.
