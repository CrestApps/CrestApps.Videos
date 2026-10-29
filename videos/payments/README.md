# Payments

**Feature:** `CrestApps.OrchardCore.Transactions`, `CrestApps.OrchardCore.Stripe`, `CrestApps.OrchardCore.PayLater` and `CrestApps.OrchardCore.Commerce.Webhooks` · **Length:** 6:44

Why payments need one record, and how Stripe, the offline methods, transactions, reminders, payments, refunds and webhooks fit together, and how to connect Stripe (with made-up test keys that are never saved), set up Pay Later, bank transfer and cash on delivery, manage a transaction and record a payment, set the reminders, refund a payment, and add a webhook. The video shows the admin and the storefront only: no code or configuration files. The title, description,
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/payments/recorder
   export VIDEO_ROOT=<repo>/videos/payments VIDEO_BASE_URL=<the site> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<the password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   `prepare` turns on **Commerce.Webhooks**, the transaction **Notification** and **Receipts** features. Run it on the site the Orders video
   prepared (four fake orders, one of them awaiting a bank transfer): the clips use the **Transactions** balance of that order.
   No real Stripe key is used: the clip types the obviously made-up `pk_test_demo_not_a_real_key` and does not save it.
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/payments`) and record the clips in order:
   `03-stripe`, `04-offline`, `05-transactions`, `06-reminders`, `07-payments`, `08-webhooks`. `04-offline` saves a cash on delivery note,
   `05-transactions` records the bank transfer, `07-payments` refunds $20 of a test payment and `08-webhooks` adds a webhook, so restore the
   copy of the site before recording a clip again. The webhook's signing secret is generated for the demo and hidden on camera.
