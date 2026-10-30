# Omnichannel Automated SMS

**Feature:** `CrestApps.OrchardCore.Omnichannel.Sms` · **Length:** see `video.json`

Why automate texts, the AI profile, the automatic activity load with its reply delay, cadence and business hours, loading it, what stops a conversation, and routing a text number to an AI agent. The video shows the admin and the site only: no code or configuration files. The title, description,
tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (invented people with `example.com` addresses and 555
numbers), so it needs no mask script. Nothing reaches a provider: the site runs with `HTTP_PROXY`/`HTTPS_PROXY` set to
`http://127.0.0.1:9`.

1. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` and run it with `ORCHARD_APP_DATA`
   pointing at an empty folder outside both repositories.
2. Set the site up with the **Blog** recipe, site name **Contoso**, SQLite, user `admin`.
3. Sign the recorder in and seed the site:

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/omnichannel-automated-sms/recorder
   export VIDEO_ROOT=<repo>/videos/omnichannel-automated-sms VIDEO_BASE_URL=http://localhost:5376 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/omnichannel-automated-sms`) and record the clips in order. `prepare` runs the Omnichannel CRM starter recipe, loads contacts from `make-data.py`'s file (set `VIDEO_DATA` to its folder), imports `seed/`, turns on the AI features, sets the log SMS provider (run the site in Development; nothing is sent), and adds a placeholder AI connection and deployment that point nowhere, plus a business hours calendar and a cadence.
