# Run an Inbound Call Center

**Feature:** `CrestApps.OrchardCore.ContactCenter.Queues`, `...EntryPoints`, `...InboundVoice`, `...Agents` and `...Supervision` · **Length:** see `video.json`

The use case from the user manual: who it is for, the features, roles and provider, the set-up order, listing numbers and opening hours, creating a queue, routing a number with an inbound entry point, giving agents their queues, and what agents and supervisors do. Calls never connect, so the live parts are slides. The video shows the admin only: no code or configuration files. The title, description, tags and
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/use-case-inbound-call-center/recorder
   export VIDEO_ROOT=<repo>/videos/use-case-inbound-call-center VIDEO_BASE_URL=http://localhost:5385
   <bin>/Recorder setup
   <bin>/Recorder prepare
   <bin>/Recorder record 02-before 04-numbers 05-queue 06-route 07-agents
   ```

Record it after `contact-center-dialer-profiles` and `contact-center-campaigns`: `prepare` of `contact-center-dialer-profiles` creates the calendar `Weekday calling hours` and the numbers `Main line` and `Campaign line` that `04-numbers`, `05-queue` and `06-route` use, and `contact-center-campaigns` creates the entitlement of `jamie.chen` that `07-agents` edits. `05-queue` saves the queue `Sales`, `06-route` saves the entry point `Main line to Sales`, and `07-agents` adds `Sales` to the entitlement. Open the three create pages once before recording (as GET requests), so that the first take is not slowed down by a cold site.
