# Product search indexes

**Feature:** `CrestApps.OrchardCore.Products.Lucene` · **Length:** 3:43

Why a listing can search an index instead of the database, and the three steps: turning on a product search feature, creating the index under Search, Indexes, and choosing it on the listing, ending with what visitors see. The video shows the admin and the storefront only: no code or configuration files. The title, description,
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/product-search-indexes/recorder
   export VIDEO_ROOT=<repo>/videos/product-search-indexes VIDEO_BASE_URL=<the site> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<the password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   `prepare` removes any index left from an earlier video and turns **Product Search (Lucene)** off (run it with `VIDEO_STAGE=on` to turn it on again). Run it on the site the Shipping video prepared (the shop with nine products and its *Shop* listing, whose item id `4jzxbym2j0f3psgb8s1jh63xcs` is in the recorder).
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/product-search-indexes`) and record the clips in order:
   `03-feature`, `04-index`, `05-listing`, `06-visitors`. Record them in this order on one copy of the site: each builds on the one before it. Restore the copy of the site before recording a clip again.
