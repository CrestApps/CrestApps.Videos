# Dialer Profiles

**Feature:** `CrestApps.OrchardCore.ContactCenter.Dialer` and `CrestApps.OrchardCore.ContactCenter.Dialer.Paced` · **Length:** see `video.json`

Why a dialer profile exists, the four dialing modes (Preview, Power, Progressive, Predictive), where to find the profiles, and how to create one card by card: General, Dialing, Caller ID and Compliance. The abandoned-call settings, Predictive pacing and retries are in the `contact-center-dialer-pacing` video. The video shows the admin only: no code or configuration files. The title, description, tags and
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/contact-center-dialer-profiles/recorder
   export VIDEO_ROOT=<repo>/videos/contact-center-dialer-profiles VIDEO_BASE_URL=http://localhost:5385
   <bin>/Recorder setup
   <bin>/Recorder prepare
   <bin>/Recorder record 03-find 04-create
   ```

`prepare` creates the business hours calendar `Weekday calling hours` and the voice numbers `Main line` and `Campaign line` (555 numbers). `03-find` saves nothing; `04-create` saves the profile `Lead drive - power`.
