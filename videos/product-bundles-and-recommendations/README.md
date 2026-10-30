# Product bundles and recommendations

**Feature:** `CrestApps.OrchardCore.Products.Bundles`, `.Grouped`, `.Recommendations` and `.Comparison` · **Length:** 5:15

Why kits, sets, suggestions and comparison grow the basket, and how to sell a bundle with its stock worked out from its parts, group products on one page, add upsells and cross-sells, and let shoppers compare products. The video shows the admin and the storefront only: no code or configuration files. The title, description,
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/product-bundles-and-recommendations/recorder
   export VIDEO_ROOT=<repo>/videos/product-bundles-and-recommendations VIDEO_BASE_URL=<the site> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<the password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   `prepare` turns on the four features, counts the Product type and gives the nine products their stock. Run it on the site the Inventory video prepared (the shop with nine products).
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/product-bundles-and-recommendations`) and record the clips in order:
   `03-bundle`, `04-grouped`, `05-recs`, `06-compare`. Record them in this order on one copy of the site: the clips add the kit and the set that later clips show. Restore the copy of the site before recording a clip again.
