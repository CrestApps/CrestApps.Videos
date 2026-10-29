# Time Zones

**Feature:** `CrestApps.OrchardCore.TimeZones` (with Orchard Core's `OrchardCore.Users.TimeZone` for a time zone per person) · **Length:** see `video.json`

Why friendly time zone names help, what a time zone map is and which menus follow the list, how to browse, add and
change maps, how to set the site's default time zone, and how a person can have their own. The video shows the admin
only: no code or configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (one invented person, `maria.santos`), so it needs no
mask script.

1. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` and run it with `ORCHARD_APP_DATA`
   pointing at an empty folder outside both repositories, with `HTTP_PROXY`/`HTTPS_PROXY` set to
   `http://127.0.0.1:9`.
2. Set the site up with the **Blog** recipe, site name **Contoso**, SQLite, user `admin`.
3. Sign the recorder in and seed the site: `Recorder prepare` enables Time Zones and User Time Zone, and creates the
   person **maria.santos** (Editor):

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/time-zones/recorder
   export VIDEO_ROOT=<repo>/videos/time-zones VIDEO_BASE_URL=http://localhost:5376 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/time-zones`) and record the clips in order:
   `04-site` selects the map that `03-list` adds. To record again, restore the copy first.
