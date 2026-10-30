# Orders, quotes and disputes

**Feature:** `CrestApps.OrchardCore.Orders` · **Length:** 5:24

Why some orders are not an ordinary checkout, and how to take quote requests and answer them, add items to a paid order, offer an order bump and a recovery email, export orders, and what a dispute and a risky payment look like. The video shows the admin and the storefront only: no code or configuration files. The title, description,
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/orders-quotes-disputes/recorder
   export VIDEO_ROOT=<repo>/videos/orders-quotes-disputes VIDEO_BASE_URL=<the site> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<the password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   `prepare` turns on the SMTP email provider and points it at a mail catcher on this computer (`smtpsink.py`, port 2525: it accepts every message and keeps it in a folder, so the demo store can send a quote and nothing leaves it), and adds a buyer with an account. Start the catcher before recording. Run it on the site the Orders video prepared (a shop with fake orders).
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/orders-quotes-disputes`) and record the clips in order:
   `03-settings`, `04-ask`, `05-quote`, `05b-accept`, `06-add`, `07-export`. Record them in this order on one copy of the site: the quote is asked for, priced, sent and paid in turn. Restore the copy of the site before recording a clip again. The dispute and risky payment chapter is a narrated slide, because both come from Stripe.
