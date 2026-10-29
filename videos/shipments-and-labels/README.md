# Shipments and labels

**Feature:** `CrestApps.OrchardCore.Shipping` (with EasyPost) · **Length:** 4:52

Why shipments need a record, and how creating a shipment, shipping in parts, a parcel being packed, marked shipped, delivered or returned, EasyPost labels and the buyer's tracking page fit together. The video shows the admin and the storefront only: no code or configuration files. The title, description,
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/shipments-and-labels/recorder
   export VIDEO_ROOT=<repo>/videos/shipments-and-labels VIDEO_BASE_URL=<the site> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<the password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   `prepare` turns on the **EasyPost** feature (nothing is sent to EasyPost). Run it on the site the Shipping video prepared, which has the four fake orders of the Orders video: this video ships Ana Torres's order *261029-RAZ0B9*.
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/shipments-and-labels`) and record the clips in order:
   `03-create`, `04-follow`, `05-easypost`, `06-track`. Record them in this order on one copy of the site: `04-follow` ships the rest of what `03-create` left, and `06-track` shows both shipments. `06-track` rewrites the redirect of the *Track your order* form to the demo site's address, because the site's links carry the address it was set up with. Restore the copy of the site before recording a clip again.
