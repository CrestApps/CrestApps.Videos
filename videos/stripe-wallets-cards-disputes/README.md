# Stripe: wallets, saved cards and disputes

**Feature:** `CrestApps.OrchardCore.Payments.Stripe` · **Length:** 4:37

Why a store connects Stripe, and how the keys, Connect and the webhook, Apple Pay and Google Pay, saved cards under My account and Disputed and Risky orders fit together. The demo never contacts Stripe: the keys typed are made up, and Connect is shown but not clicked. The video shows the admin and the storefront only: no code or configuration files. The title, description,
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/stripe-wallets-cards-disputes/recorder
   export VIDEO_ROOT=<repo>/videos/stripe-wallets-cards-disputes VIDEO_BASE_URL=<the site> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<the password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   Nothing to prepare: the site is the one the Shipping video prepared, which already has the Stripe feature on.
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/stripe-wallets-cards-disputes`) and record the clips in order:
   `03-keys`, `05-cards`. The other chapters are narrated slides, because the wallet buttons, the disputes and the risk badges come from Stripe and a public domain.
