# Run an Outbound Calling Campaign

**Feature:** `CrestApps.OrchardCore.ContactCenter.Dialer`, `...Dialer.Paced`, `CrestApps.OrchardCore.Omnichannel.Managements` and the Contact Center agent features · **Length:** see `video.json`

The use case from the user manual: who it is for, the features and roles, the set-up order, creating the subject and its flow, the campaign, calling hours, dialer profile and entitlements, loading a list of contacts and reading the load's report, what agents, supervisors and managers do, and how to check it works. Calls never connect, so the live parts are slides. The video shows the admin only: no code or configuration files. The title, description, tags and
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/use-case-outbound-calling-campaign/recorder
   export VIDEO_ROOT=<repo>/videos/use-case-outbound-calling-campaign VIDEO_BASE_URL=http://localhost:5385
   <bin>/Recorder setup
   <bin>/Recorder prepare
   <bin>/Recorder record 02-before 04-subject 05-rules 06-load 08-watch
   ```

Record it after `contact-center-dialer-profiles`, `contact-center-campaigns`, `contact-center-dispositions` and `contact-center-numbers-not-in-service`: `05-rules` and `06-load` show what those videos saved (the profile `Lead drive - power`, the campaigns, the dispositions, two numbers not in service). `prepare` creates a `Contact` type and three fake contacts (Morgan Lee, whose only number is on the not-in-service list, Casey Brooks and Taylor Reed); run it once. `04-subject` creates the subject `Spring lead drive`, and `06-load` creates a dialer load and loads it: two of the three contacts load, and one is skipped because its only number is not in service. Run `VIDEO_WARM=1 Recorder prepare` after a restart of the site, and before `04-subject`, to warm the pages up so the take is not slowed down (it creates and deletes a throwaway type).
