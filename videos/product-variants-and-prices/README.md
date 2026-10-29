# Product variants and prices

**Feature:** `CrestApps.OrchardCore.Products` (Product variants and Product prices) · **Length:** 4:28

Why a product that comes in versions is one item with variants, and how the Product variants part and its options, a product's values, SKUs and prices, what shoppers see, and the Product prices part fit together. The video shows the admin and the storefront only: no code or configuration files. The title, description,
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/product-variants-and-prices/recorder
   export VIDEO_ROOT=<repo>/videos/product-variants-and-prices VIDEO_BASE_URL=<the site> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<the password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   Nothing to prepare: the site is the one the Shipping video prepared (the shop with nine products). The **Product variants** and **Product prices** parts are added to the Product type on camera; the product edited is the *Canvas Backpack*, and the variants clip uses the item id `4swccm1c6r9fa4v1hjqgpfsn4h` of that site (read it from the Canvas Backpack's edit address on yours).
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/product-variants-and-prices`) and record the clips in order:
   `03-options`, `04-values`, `05-shop`, `06-prices`. Record them in this order on one copy of the site: each builds on the one before it. Restore the copy of the site before recording a clip again.
