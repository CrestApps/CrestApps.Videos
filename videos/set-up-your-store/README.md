# Set up your store

**Feature:** `CrestApps.OrchardCore.Commerce` (the setup guide) · **Length:** 5:00

Why a new store needs a guide, and how the steps fit together from a brand new Online store site: your store's details and currency, payments, shipping, tax, a first product and a test order, staff roles and the Storefront theme. The video shows the admin and the storefront only: no code or configuration files. The title, description,
tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (made-up products, the customer `customer@example.com`,
no payment provider contacted), so it needs no mask script.

1. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` and run it with `ORCHARD_APP_DATA` pointing
   at an empty folder outside both repositories, with `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9` and
   `NO_PROXY=localhost,127.0.0.1`, so nothing reaches a provider.
2. Set the site up with the **Online store** recipe, site name **Contoso**, SQLite, user `admin`.
3. Sign the recorder in and seed the site:

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/set-up-your-store/recorder
   export VIDEO_ROOT=<repo>/videos/set-up-your-store VIDEO_BASE_URL=<the site> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<the password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   This video needs **no `prepare`**: it starts from a brand new site. Set it up with the **Online store** recipe (site name *Contoso*, SQLite, user `admin`) and copy its `App_Data` aside before recording. Every step of the guide is done on camera.
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/set-up-your-store`) and record the clips in order:
   `03-guide`, `04-details`, `05-payments`, `06-ship`, `07-first`, `08-roles`, `09-theme`. Record them in this order on one copy of the new site: each step of the guide builds on the one before it (the product made in `07-first` is what the test order buys, with the bank transfer step from `05-payments`). Restore the copy of the site before recording a clip again.
