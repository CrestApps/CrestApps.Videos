# Carts and Checkout

**Feature:** `CrestApps.OrchardCore.Carts`, `CrestApps.OrchardCore.Checkout` · **Length:** 5:48

Why a cart and a checkout exist, how the cart, the Contact, Shipping and Payment steps and the order confirmation fit together, and how to use the mini cart, the cart page, each checkout step, a promotion code, and the checkout and customer settings. The video shows the admin and the storefront only: no code or configuration files. The title, description,
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/carts-checkout/recorder
   export VIDEO_ROOT=<repo>/videos/carts-checkout VIDEO_BASE_URL=<the site> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<the password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   `prepare` turns on the **Test payment** provider (`CrestApps.OrchardCore.Checkout.TestProvider`) and **Checkout details**, adds a
   shipping zone **Everywhere** (rest of the world) with two flat rates, **Standard shipping** ($6) and **Express shipping** ($15),
   and a 10% promotion code **WELCOME10**. Run it on the site that the Products video prepared (the shop with its nine products).
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/carts-checkout`) and record the clips in order:
   `03-cart`, `04-contact`, `05-shipping`, `06-payment`, `07-settings`. The storefront clips start from an empty browser (a guest);
   `04-contact`, `05-shipping` and `06-payment` first put a cart at their step off camera. `06-payment` pays with **Test payment**
   as the guest **Jamie Chen** (`customer@example.com`), so no payment provider is contacted.
