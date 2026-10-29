# AI Outbound Calls

**Feature:** `CrestApps.OrchardCore.Telnyx.AiVoice`, `CrestApps.OrchardCore.Omnichannel.Voice` and `CrestApps.OrchardCore.AI.Chat` · **Length:** see `video.json`

How an AI voice agent places outbound calls: the AI profile from a phone template, the outbound subject with live agent handoff and disposition hints, the automatic load, the screening before each call, what the agent does during a call, the recording disclosure setting, and Review AI conversation. The video shows the admin only: no code or configuration files. The title, description, tags and
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/contact-center-ai-outbound-calls/recorder
   export VIDEO_ROOT=<repo>/videos/contact-center-ai-outbound-calls VIDEO_BASE_URL=http://localhost:5392
   <bin>/Recorder setup
   <bin>/Recorder prepare
   <bin>/Recorder record 02-profile 03-subject 04-load 07-disclosure 08-review
   ```

Seeding: the queue `Sales handoff` is created by `prepare`; a finished AI call with a made-up transcript is added to the site database with `seed-review.py` (run it with the site stopped, after `02-profile` has created the profile `Spring lead caller`). The contact and call are fake. `04-load` does not save the load, so no call is ever attempted. The behaviour of a live call is shown in slides.
