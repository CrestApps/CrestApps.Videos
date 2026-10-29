# Contact Center Troubleshooting

**Feature:** `CrestApps.OrchardCore.ContactCenter` and the features of its pages · **Length:** see `video.json`

The troubleshooting page of the user manual: how to use it, a missing menu or button (permissions, features, site settings), empty pages (entitlements, filters, date ranges), a phone that does not ring, no or poor audio, texts and the AI, the dialer that is not calling, wrap-up, contacts and loads, and what to send your administrator. The screens where problems are diagnosed are recorded; the checklists are slides. The video shows the admin only: no code or configuration files. The title, description, tags and
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/contact-center-troubleshooting/recorder
   export VIDEO_ROOT=<repo>/videos/contact-center-troubleshooting VIDEO_BASE_URL=http://localhost:5385
   <bin>/Recorder setup
   <bin>/Recorder prepare
   <bin>/Recorder record 02-menu 03-empty 04-ring 05b-diag 07-dialer
   ```

Record it last, on the site of the other contact center videos: `02-menu` opens the permissions of the Editor role and the Contact Center and Telephony settings, and it turns on the soft phone's Diagnostics tab and saves the Telephony settings, which `05b-diag` then shows. `03-empty` and `07-dialer` show the live dashboard of the administrator, who has no agent entitlement record, so it is empty.
