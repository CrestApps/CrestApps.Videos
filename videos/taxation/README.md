# Taxation

**Feature:** `CrestApps.OrchardCore.Taxation` · **Length:** 6:35

Why taxation needs rules in one place, how tax categories, jurisdictions, rules, tables and the tax origin fit together, and how to add a category, a jurisdiction and a percentage rule, set where you sell from, classify products with the Taxation part, and see the tax in the checkout's order summary, plus tax tables, TaxJar and the EU VAT recipe. The video shows the admin and the storefront only: no code or configuration files. The title, description,
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/taxation/recorder
   export VIDEO_ROOT=<repo>/videos/taxation VIDEO_BASE_URL=<the site> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<the password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   `prepare` turns on the **TaxJar** feature. Run it on the site the Orders video prepared (the shop, a shipping zone **Everywhere** with the
   flat rates, and the Test payment provider). The categories, the jurisdiction, the rule and the product classification are made on camera.
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/taxation`) and record the clips in order:
   `03-categories`, `04-jurisdictions`, `05-rules`, `06-origin`, `07-classify`, `08-checkout`, `09-more`. `05-rules` can be slow to save, so the clip
   waits off camera; give the site a moment before `06-origin`. The rule uses **Any jurisdiction** and the category **Clothing**, and the Trail
   Runner Shoe is classified as Clothing, so the guest's order in `08-checkout` is charged 8.75%.
