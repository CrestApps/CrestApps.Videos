# Dialer Reports

**Feature:** `CrestApps.OrchardCore.Reports`, `CrestApps.OrchardCore.ContactCenter.Dialer.Paced` and `CrestApps.OrchardCore.ContactCenter.WorkDistribution` · **Length:** see `video.json`

The Dialer activity and health report (Dialer right now, Campaign queues, Dialer profiles, charts, suppressed attempts), the meaning of the dialer states, the Dialer decision log by reason, record and agent, the reasons a record was not dialed, the Dialer queue inventory, and the Last pacing decision of a Predictive profile. The video shows the admin only: no code or configuration files. The title, description, tags and
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/contact-center-dialer-reports/recorder
   export VIDEO_ROOT=<repo>/videos/contact-center-dialer-reports VIDEO_BASE_URL=http://localhost:5392
   <bin>/Recorder setup
   <bin>/Recorder prepare
   <bin>/Recorder record 02-health 03-log 04-inventory
   ```

`prepare` loads a dialer batch (`Spring leads - power dial`) with the Power profile and nobody signed in, so the pacing task records real cycles and routing decisions (No agents available). Record a few minutes after `prepare`. Nothing else is seeded.
