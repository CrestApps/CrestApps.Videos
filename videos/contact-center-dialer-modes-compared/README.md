# Dialer Modes Compared

**Feature:** `CrestApps.OrchardCore.ContactCenter.Dialer` and `CrestApps.OrchardCore.ContactCenter.Dialer.Paced` · **Length:** see `video.json`

Preview, Power, Progressive and Predictive side by side: who presses Dial, whether an agent is held before the customer's phone rings, what can go wrong, what each mode shows in the profile form, and which to choose. It builds on `contact-center-dialer-profiles` and `contact-center-dialer-pacing`. The video shows the admin only: no code or configuration files. The title, description, tags and
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/contact-center-dialer-modes-compared/recorder
   export VIDEO_ROOT=<repo>/videos/contact-center-dialer-modes-compared VIDEO_BASE_URL=http://localhost:5392
   <bin>/Recorder setup
   <bin>/Recorder prepare
   <bin>/Recorder record 02-preview 03-power 04-progressive 05-predictive
   ```

Every clip opens the Add Dialer Profile form and saves nothing, so the clips can be recorded in any order on a site that has the Contact Center Outbound Dialer and Paced Dialing features. What agents see during a live call is shown in slides.
