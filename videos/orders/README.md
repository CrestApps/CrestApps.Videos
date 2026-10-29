# Orders

**Feature:** `CrestApps.OrchardCore.Orders` (with Returns and Gift Cards) and `CrestApps.OrchardCore.Receipts` · **Length:** 7:03

Why an order is the one record of a sale, how the order list, the order page, Create order, Returns, Gift cards and the Orders, Emails, Receipts and Returns settings fit together, and how to find, inspect, refund, cancel and create orders, handle returns, issue a gift card and set up the order emails. The video shows the admin and the storefront only: no code or configuration files. The title, description,
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/orders/recorder
   export VIDEO_ROOT=<repo>/videos/orders VIDEO_BASE_URL=<the site> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<the password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   `prepare` turns on the **Test payment** provider, **Returns** and **Gift cards**, adds a shipping zone **Everywhere** with the flat rates
   **Standard shipping** ($6) and **Express shipping** ($15), turns on **Bank transfer**, and places four orders through the storefront as
   the fake buyers Jamie Chen (`customer@example.com`), Priya Shah (bank transfer, so it reads **Payment due**), Marcus Lee and Ana Torres.
   Run it on the site the Products video prepared.
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/orders`) and record the clips in order:
   `03-list`, `04-order`, `05-manage`, `06-create`, `07-returns`, `08-gift`, `09-settings`. `05-manage` refunds part of Marcus Lee's order and
   cancels Priya Shah's, `06-create` saves a draft order for a made-up buyer, and `08-gift` issues a gift card, so restore the copy of the site
   before recording a clip again.
