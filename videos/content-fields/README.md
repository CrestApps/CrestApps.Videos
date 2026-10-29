# Phone and Address Fields

**Feature:** `CrestApps.OrchardCore.ContentFields` (Phone Field), `CrestApps.OrchardCore.ContentFields.Address` (Address Field) and the Addresses features (`CrestApps.OrchardCore.Addresses`, `.Countries`, `.GooglePlaces`) · **Length:** see `video.json`

Why contact details need fields that follow the country, how to add a Phone Field and an Address Field to a content
type, what an editor sees (the flag picker, the layout that follows the country, and the checks on the postal code),
how to manage the list of countries, and how address suggestions are set up. The video shows the admin and the site
only: no code or configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (an invented Toronto branch and a 555 phone number),
so it needs no mask script. The Google Places key typed on camera is a made-up string that is never contacted.

1. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` and run it with `ORCHARD_APP_DATA`
   pointing at an empty folder outside both repositories, with `HTTP_PROXY`/`HTTPS_PROXY` set to
   `http://127.0.0.1:9`.
2. Set the site up with the **Blog** recipe, site name **Contoso**, SQLite, user `admin`.
3. Sign the recorder in and seed the site: `Recorder prepare` enables CrestApps Content Fields, Address Field,
   Address Countries and Address Suggestions (Google Places), and imports `seed/branch-office.recipe.json` (a
   **Branch Office** content type with only a title and a URL):

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/content-fields/recorder
   export VIDEO_ROOT=<repo>/videos/content-fields VIDEO_BASE_URL=http://localhost:5376 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/content-fields`) and record the clips in
   order: `03-phone` and `04-address` add the fields that `05-enter` fills in. To record again, restore the copy
   first.
