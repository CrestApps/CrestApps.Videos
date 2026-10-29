# Gift cards

**Feature:** `CrestApps.OrchardCore.Orders.GiftCards` · **Length:** 6:21

Why gift cards need tracked balances, and how selling one as a product, issuing one by hand, spending one at checkout and following every change in its activity fit together, ending with the list, its filters and what a refund does. The video shows the admin and the storefront only: no code or configuration files. The title, description,
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/gift-cards/recorder
   export VIDEO_ROOT=<repo>/videos/gift-cards VIDEO_BASE_URL=<the site> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<the password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   `prepare` turns on the test payment provider and the **Gift cards** feature. Run it on the site the Shipping video prepared (the shop with nine products and a zone *Everywhere* with Standard shipping and Express shipping). The gift card product, the Shop listing change and the cards are made on camera.
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/gift-cards`) and record the clips in order:
   `03-sell`, `04-issue`, `05-card`, `06-buy`, `07-pay`, `08-list`. Record them in this order on one copy of the site: `05-card` opens the card `04-issue` makes (code `CONTOSO25`), `06-buy` needs the product `03-sell` adds to the Shop listing, `07-pay` spends the `CONTOSO25` card on a Canvas Backpack and `08-list` shows both cards. Restore the copy of the site before recording a clip again.
