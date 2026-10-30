# Product import and feeds

**Feature:** `CrestApps.OrchardCore.ContentTransfer` and `CrestApps.OrchardCore.Products.Feeds` · **Length:** 4:40

Why products move by spreadsheet, and how Bulk Export, Bulk Import with its template and SKU matching, product feeds and the Product Feed part fit together, with the move from Shopify or WooCommerce explained. The video shows the admin and the storefront only: no code or configuration files. The title, description,
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/product-import-and-feeds/recorder
   export VIDEO_ROOT=<repo>/videos/product-import-and-feeds VIDEO_BASE_URL=<the site> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<the password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   `prepare` turns on **Content Transfer** and **Product Feeds**. Run it on the site the Shipping video prepared (the shop with nine products). `new-products.csv` is the file `04-import` uploads: three new products and a fourth row that repeats a SKU, which the import skips.
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/product-import-and-feeds`) and record the clips in order:
   `03-export`, `04-import`, `05-feeds`. Record them in this order on one copy of the site: the feed counts the products the import adds. Restore the copy of the site before recording a clip again.
