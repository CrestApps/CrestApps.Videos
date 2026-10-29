# Store credit, loyalty points and referrals

**Feature:** `CrestApps.OrchardCore.Customers.StoreCredit`, `.Loyalty` and `.Referrals` · **Length:** 5:40

Why customers who come back are worth rewarding, and how giving store credit, the loyalty points program, spending both at checkout, and referral links with a friend's coupon and the customer's reward fit together. The video shows the admin and the storefront only: no code or configuration files. The title, description,
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/store-credit-loyalty-referrals/recorder
   export VIDEO_ROOT=<repo>/videos/store-credit-loyalty-referrals VIDEO_BASE_URL=<the site> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<the password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   `prepare` turns on **Store credit**, **Loyalty points** and **Referrals**, adds a customer with an account (user `ana`, made up for the recording, named *Ana Rivera*), a *FRIEND15* coupon with **Only the generated codes work** ticked, and one paid order of Ana's. Run it on the site the Shipping video prepared (the shop with nine products and a zone *Everywhere* with Standard shipping).
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/store-credit-loyalty-referrals`) and record the clips in order:
   `03-credit`, `04-refund`, `05-spend`, `06-points`, `07-spend`, `08-setup`, `09-refer`, `09-friend`, `09-reward`. Record them in this order on one copy of the site: they build on each other (the refund adds credit, the points program must be set up before points are spent, referrals before the friend buys). The signed-in clips sign Ana in off camera. Restore the copy of the site before recording a clip again.
