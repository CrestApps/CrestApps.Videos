# AI Voice Agent for Inbound Calls

**Feature:** `CrestApps.OrchardCore.Telnyx.AiVoice`, `CrestApps.OrchardCore.ContactCenter.EntryPoints` and `CrestApps.OrchardCore.ContactCenter.Voice.Media` · **Length:** see `video.json`

Routing a number to an AI voice agent from an inbound entry point, open and closed hours, the inbound subject (AI profile, goal, handoff), turn-based versus live audio conversations and the name clash between the Voice Media library and the Contact Center Voice Media feature, what callers experience, and reviewing the call. The video shows the admin only: no code or configuration files. The title, description, tags and
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/contact-center-ai-voice-agent-inbound/recorder
   export VIDEO_ROOT=<repo>/videos/contact-center-ai-voice-agent-inbound VIDEO_BASE_URL=http://localhost:5392
   <bin>/Recorder setup
   <bin>/Recorder prepare
   <bin>/Recorder record 02-entry 03-hours 04-subject 05-voice 07-review
   ```

Build on the state of `contact-center-ai-outbound-calls` (the AI profile `Spring lead caller` and the finished call seeded with `seed-review.py` of that video). `prepare` creates the profile `Front desk answerer`. `02-entry` saves the entry point `Front desk line`; the other clips save nothing. The behaviour of a live call is shown in slides.
