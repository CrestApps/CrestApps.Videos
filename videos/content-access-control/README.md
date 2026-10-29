# Content Access Control

**Feature:** `CrestApps.OrchardCore.ContentAccessControl` (with `CrestApps.OrchardCore.Roles`, the Enhanced Roles feature that provides the Role Picker part) · **Length:** see `video.json`

Why a page can be limited to some roles, how the Role Picker part and the Restrict content setting work together, how
to set a content type up, how to pick the roles on an item, and how to check the result by signing in as other people.
The video shows the admin and the site only: no code or configuration files. The title, description, tags and chapters
YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (invented people, `example.com` addresses), so it
needs no mask script.

1. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` and run it with `ORCHARD_APP_DATA`
   pointing at an empty folder outside both repositories, with `HTTP_PROXY`/`HTTPS_PROXY` set to
   `http://127.0.0.1:9`.
2. Set the site up with the **Blog** recipe, site name **Contoso**, SQLite, user `admin`.
3. Sign the recorder in, and seed the site: `Recorder prepare` enables Enhanced Roles and Content Access Control,
   imports `seed/handbook.recipe.json` (the **Supervisor** and **Support Agent** roles, a **Handbook Page** content
   type and a **Support hours** page), and creates the people **jordan.lee** (Supervisor) and **casey.morgan**
   (Support Agent), saving their sign-ins as `state-<name>.json`:

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/content-access-control/recorder
   export VIDEO_ROOT=<repo>/videos/content-access-control VIDEO_BASE_URL=http://localhost:5376 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/content-access-control`) and record the clips in
   order: `03-setup` adds the Role Picker part that `04-item` uses, and `04-item` creates the page that `05-check`
   opens. To record again, restore the copy first.
