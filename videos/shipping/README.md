# Shipping

**Feature:** `CrestApps.OrchardCore.Shipping` (with the EasyPost carrier provider) · **Length:** 5:33

Why shipping needs zones and methods, how zones, methods, the shipping settings, Try rates and shipments fit together, and how to add a zone and a flat rate and a free shipping method, set product weights and the shipping settings, add a pickup location, look at the ship-from and EasyPost settings, check rates, and ship an order. The video shows the admin and the storefront only: no code or configuration files. The title, description,
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/shipping/recorder
   export VIDEO_ROOT=<repo>/videos/shipping VIDEO_BASE_URL=<the site> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<the password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   `prepare` turns on the **EasyPost** carrier provider (no account or key is used). Run it on the site the Orders video prepared (the shop,
   four fake orders, and a shipping zone **Everywhere** with **Standard shipping** and **Express shipping** flat rates).
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/shipping`) and record the clips in order:
   `03-zones`, `04-methods`, `05-settings`, `06-pickup`, `07-try`, `08-ship`. `03-zones` adds a **United States** zone, `04-methods` gives it a
   Standard shipping rate and a Free shipping method, and `08-ship` records a shipment on Marcus Lee's order, so restore the copy of the site
   before recording a clip again.
