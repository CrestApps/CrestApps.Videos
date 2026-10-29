# Markets

**Feature:** `CrestApps.OrchardCore.Markets` · **Length:** 5:20

Why markets exist, how a market, the storefront picker and the rules around a price fit together, and every screen: adding a market with its countries, currency, exchange rate, rounding, language and tax setting, the market picker, leaving a product out of a market, a shipping method in the market's currency, and a checkout with prices that include tax. The video shows the admin and the storefront only: no code or configuration files. The title, description,
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/markets/recorder
   export VIDEO_ROOT=<repo>/videos/markets VIDEO_BASE_URL=<the site> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<the password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   `prepare` turns on **Markets** and the test payment provider, and runs the **EU VAT standard rates** recipe (Admin > Configuration > Recipes). Run it on the site the Shipping video prepared (the shop with nine products and a zone *Everywhere* with Standard shipping and Express shipping). The market, its shipping method and the product left out are made on camera.
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/markets`) and record the clips in order:
   `03-market`, `04-picker`, `05-notsold`, `06-shipping`, `07-checkout`. Record them in this order on one copy of the site: `04-picker` and `05-notsold` need the market `03-market` adds, `05-notsold` leaves the Ceramic Mug out of it, and `07-checkout` needs the euro shipping method `06-shipping` adds (the guest ships a Trail Runner Shoe to Berlin). Restore the copy of the site before recording a clip again.
