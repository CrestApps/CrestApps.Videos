# Inventory

**Feature:** `CrestApps.OrchardCore.Inventory` (with Stock alerts and Purchase orders) · **Length:** 6:22

Why inventory needs counted stock, and how the Inventory part, the stock list, received / adjusted / counted changes with their history, CSV import and export, a product's stock settings, the inventory settings and locations, purchase orders and the shop's stock labels fit together. The video shows the admin and the storefront only: no code or configuration files. The title, description,
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/inventory/recorder
   export VIDEO_ROOT=<repo>/videos/inventory VIDEO_BASE_URL=<the site> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<the password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   `prepare` turns on **Stock alerts** and **Purchase orders**. Run it on the site the Orders video prepared (the shop with nine products and four
   fake orders). The **Inventory** part, the counts and the location are made on camera. `stock-counts.csv` is the file `05-import` imports.
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/inventory`) and record the clips in order:
   `03-count`, `04-stock`, `05-import`, `06-product`, `07-locations`, `08-purchase`, `09-shop`. `04-stock` receives, adjusts and counts the Ceramic Mug,
   `05-import` sets every product's count from `stock-counts.csv`, `08-purchase` receives a purchase order, and `09-shop` first counts the
   Ceramic Mug back to 4 and the Wool Beanie to 0 (off camera) so the shop shows a low and an out of stock product. Restore the copy of the site
   before recording a clip again.
