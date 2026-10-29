# Products

**Feature:** `CrestApps.OrchardCore.Products` and its Listing, Comparison, Bundles, Grouped, Recommendations, Feeds and Lucene features · **Length:** 8:47

How products keep their price with their content, and how to add a currency and a product, set sale prices, quantity limits, volume and role discounts and add-ons, choose product types and sales modes, build product listing pages, compare, recommend and bundle products, publish product feeds, and use a product search index. The video shows the admin and the storefront only: no code or configuration files. The title, description,
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/products/recorder
   export VIDEO_ROOT=<repo>/videos/products VIDEO_BASE_URL=<the site> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<the password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   `prepare` turns on the product features, sets the site's base URL, creates a Lucene product search index, a **Wholesale**
   role and nine fake products with placeholder pictures (`python make-images.py <App_Data>/Sites/Default/Media/products` makes them) in the media
   library). Run it with `VIDEO_STAGE=extras` before the `08-extras` clip: it turns on **Product Bundles**, **Grouped
   Products** and **Product Recommendations**, whose sections the product editor shows above its tabs.
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/products`) and record the clips in order:
   `03-currencies`, `04-product`, `05-pricing`, `06-type`, `07-listing`, `09-feeds`, then `VIDEO_STAGE=extras prepare`,
   then `08-extras`. `04-product` creates the product the shop then shows, and `05-pricing` changes the **Ceramic Mug**.
