# Coupons and discounts

**Feature:** `CrestApps.OrchardCore.Checkout` (Coupons) · **Length:** 6:37

Why discounts need limits, and how codes, automatic discounts, buy X get Y, dates and usage limits, minimums, combining rules, discount links and unique single-use codes work, ending with what the buyer sees at checkout and a coupon's recent uses. The video shows the admin and the storefront only: no code or configuration files. The title, description,
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/coupons-and-discounts/recorder
   export VIDEO_ROOT=<repo>/videos/coupons-and-discounts VIDEO_BASE_URL=<the site> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<the password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   `prepare` turns on the test payment provider. Run it on the site the Shipping video prepared (the shop with nine products and a zone *Everywhere* with Standard shipping and Express shipping). The coupons are made on camera.
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/coupons-and-discounts`) and record the clips in order:
   `03-code`, `04-rules`, `05-auto`, `06-share`, `07-checkout`, `08-list`. Record them in this order on one copy of the site: `06-share` opens the WELCOME10 coupon `03-code` makes, `07-checkout` needs the automatic *Summer sale* from `05-auto` (three notebooks, with WELCOME10 giving way to it) and `08-list` shows the Summer sale's recent use from that order. Restore the copy of the site before recording a clip again.
