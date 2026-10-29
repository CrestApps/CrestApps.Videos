# Purchase orders and stock alerts

**Feature:** `CrestApps.OrchardCore.Inventory.PurchaseOrders` and `.StockAlerts` · **Length:** 4:33

Why stock needs ordering as well as counting, and how a purchase order from low stock, its costs, Mark as ordered, partial and full receipt, cancellation, back in stock requests and low stock notifications fit together. The video shows the admin and the storefront only: no code or configuration files. The title, description,
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/purchase-orders-and-stock-alerts/recorder
   export VIDEO_ROOT=<repo>/videos/purchase-orders-and-stock-alerts VIDEO_BASE_URL=<the site> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<the password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   `prepare` turns on **Back-in-Stock Alerts** and **Purchase Orders**, adds the **Inventory** part to the Product type, counts every product (Ceramic Mug 4, Canvas Backpack 3, Wool Beanie 0, the rest 9 to 30) and adds a location *Main warehouse*. Run it on the site the Orders video prepared (the shop with nine products).
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/purchase-orders-and-stock-alerts`) and record the clips in order:
   `03-start`, `04-order`, `05-request`, `05b-waiting`, `06-receive`, `07-cancel`, `08-alerts`. Record them in this order on one copy of the site: `04-order` also makes a second order off camera (*Harbor Goods*) for `07-cancel`, `05-request` asks to be told about the sold out Wool Beanie before `06-receive` brings it back, and `06-receive` receives the *Northwind Supply* order. Restore the copy of the site before recording a clip again.
