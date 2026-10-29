# Bulk Import and Export

**Feature:** `CrestApps.OrchardCore.ContentTransfer` (with `CrestApps.OrchardCore.ContentTransfer.OpenXml` for Excel files) · **Length:** see `video.json`

Why bulk import and export exist, where to find them, how to choose which content types can be transferred, how to
prepare and upload a CSV or Excel file, how to follow an import (status, imported and rejected records, the error
file and the row actions), how to export with filters and download the file, who may use it, and some use cases. The
video shows the admin only: no code or configuration files. The title, description, tags and chapters YouTube shows
are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (invented customer names and `example.com` addresses),
so it needs no mask script.

1. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` and run it with `ORCHARD_APP_DATA`
   pointing at an empty folder outside both repositories, with `HTTP_PROXY`/`HTTPS_PROXY` set to
   `http://127.0.0.1:9`.
2. Set the site up with the **Blog** recipe, site name **Contoso**, SQLite, user `admin`.
3. Make the customers file: `python videos/content-transfer/make-data.py <folder>` writes `customers.csv` (600 rows,
   6 of them without the required title). Set `VIDEO_DATA` to that folder.
4. Sign the recorder in, and seed the site (enables Content Transfer and its OpenXml feature, and adds a **Customer**
   content type from `seed/customer-type.recipe.json`):

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/content-transfer/recorder
   export VIDEO_ROOT=<repo>/videos/content-transfer VIDEO_BASE_URL=http://localhost:5376 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password> VIDEO_DATA=<folder>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

5. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/content-transfer`) and record the clips in
   order: `05-import` uploads the file that `06-follow` shows, and `07-export` exports the customers it created. To
   record again, restore the copy first.
