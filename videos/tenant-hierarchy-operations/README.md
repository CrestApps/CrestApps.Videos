# Tenant Hierarchy Operations

**Feature:** `CrestApps.OrchardCore.TenantHierarchy` · **Length:** see `video.json`

Why operations matter, the client list and its state filter, fixing a failed setup, bulk actions, favorites and signing out, and the platform's orphans, moves and Make ordinary. The video shows the admin and the site only: no code or configuration files. The title, description,
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/tenant-hierarchy-operations/recorder
   export VIDEO_ROOT=<repo>/videos/tenant-hierarchy-operations VIDEO_BASE_URL=http://localhost:5376 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/tenant-hierarchy-operations`) and record the clips in order. `prepare` needs the site served over https (a parent tenant's cookies are secure only) and tenants at `*.localhost` addresses. It turns on Tenants and the Tenant Hierarchy platform, sets up four tenants (the firm, lakeside, northridge, emptyco), makes them parents, and adds six clients to the firm and one to lakeside from the firm's own screens. Then `seed/make-states.py`, run while the site is stopped, marks one client as failed, removes lakeside (leaving its client an orphan) and makes one client an ordinary tenant.
