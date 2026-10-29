# Omnichannel Leads

**Feature:** `CrestApps.OrchardCore.Omnichannel.Crm` · **Length:** see `video.json`

Why leads are kept apart from contacts, the leads list and its search terms, adding and importing leads, calling and texting them, and converting a lead into a contact, an account and an opportunity. The video shows the admin and the site only: no code or configuration files. The title, description,
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/omnichannel-leads/recorder
   export VIDEO_ROOT=<repo>/videos/omnichannel-leads VIDEO_BASE_URL=http://localhost:5376 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/omnichannel-leads`) and record the clips in order. `prepare` runs the Omnichannel CRM starter recipe and loads ten leads from `make-data.py`'s file (set `VIDEO_DATA` to its folder); `06-convert` converts two of them.
