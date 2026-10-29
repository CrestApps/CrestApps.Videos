# Returns and exchanges

**Feature:** `CrestApps.OrchardCore.Orders.Returns` · **Length:** 5:00

Why returns need a clear flow, and how the settings, a buyer's request from their order, and approving, receiving, refunding or exchanging a return fit together, with the buyer's orders made through the shop's own pages. The video shows the admin and the storefront only: no code or configuration files. The title, description,
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/returns/recorder
   export VIDEO_ROOT=<repo>/videos/returns VIDEO_BASE_URL=<the site> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<the password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   `prepare` turns on **Returns** and the test payment provider. Run it on the site the Shipping video prepared (the shop with nine products and a zone *Everywhere* with Standard shipping). The settings are made on camera; the guest buyers' orders and requests are made with the shop's own pages (`04-ask` on camera, the exchange request in `06-exchange` off camera).
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/returns`) and record the clips in order:
   `03-settings`, `04-ask`, `05-handle`, `06-exchange`. Record them in this order on one copy of the site: `05-handle` handles the return `04-ask` makes, and `06-exchange` makes its own exchange request and approves it. Restore the copy of the site before recording a clip again.
