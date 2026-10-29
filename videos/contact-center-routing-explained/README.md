# Routing Explained

**Feature:** `CrestApps.OrchardCore.ContactCenter.WorkDistribution` and `CrestApps.OrchardCore.ContactCenter.AgentEntitlements` · **Length:** see `video.json`

How a waiting call finds an agent: eligibility, skills and capacity, the routing strategies, sticky agents, entry point and queue priority, automatic priority, aging, declined and missed offers, overflow, and the two reports that explain a routing decision. The video shows the admin only: no code or configuration files. The title, description, tags and
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/contact-center-routing-explained/recorder
   export VIDEO_ROOT=<repo>/videos/contact-center-routing-explained VIDEO_BASE_URL=http://localhost:5392
   <bin>/Recorder setup
   <bin>/Recorder prepare
   <bin>/Recorder record 02-queue 03-skills 04-priority 04-aging 06-overflow 07-report
   ```

`prepare` creates the skills `Spanish` and `Billing`. No clip saves anything. Needs the Reports feature for `07-report`; the report rows come from the dialer work of the site, so record it after a dialer load has run for a few minutes. What happens in a live call is shown in slides.
