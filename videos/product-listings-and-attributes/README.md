# Product listings and attributes

**Feature:** `CrestApps.OrchardCore.Products` (Product Listing) · **Length:** 5:11

Why listings need filters, sorts and search, and how the Product listing settings of a field, the listing page's filters, sorts, layout and paging, and what visitors see fit together. The video shows the admin and the storefront only: no code or configuration files. The title, description,
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/product-listings-and-attributes/recorder
   export VIDEO_ROOT=<repo>/videos/product-listings-and-attributes VIDEO_BASE_URL=<the site> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<the password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   Nothing to prepare: the site is the one the Shipping video prepared (the shop with nine products and its *Shop* listing). The *Brand* field is added to the Product type on camera; `04-value` gives the other products a brand off camera, and the listing and product item ids in the recorder (`4jzxbym2j0f3psgb8s1jh63xcs`, `4swccm1c6r9fa4v1hjqgpfsn4h`) are those of that site.
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/product-listings-and-attributes`) and record the clips in order:
   `03-field`, `04-value`, `05-listing`, `06-visitors`. Record them in this order on one copy of the site: each builds on the one before it. Restore the copy of the site before recording a clip again.
