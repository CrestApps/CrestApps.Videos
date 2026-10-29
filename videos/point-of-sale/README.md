# Point of Sale

**Feature:** `CrestApps.OrchardCore.PointOfSale` · **Length:** see `video.json`

Why a store wants one system for its website and its counters, how registers, shifts and sales fit together, what to have
ready (stores, roles, sign-in), how to set up registers, what staff do in the till app during a day, how to read a shift
(sales, cash kept, card, the cash drawer and its difference), and how register sales show up as orders. The video shows the
admin only, with the till app's day told over slides: no code or configuration files. The title, description, tags and
chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (`example.com` addresses, made-up names and stores), so it needs
no mask script. Nothing reaches a payment or messaging provider; the card sales are recorded as taken on the counter terminal.

1. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` and run it with `ORCHARD_APP_DATA` pointing at
   an empty folder outside both repositories, on `http://localhost:5375`, with `HTTP_PROXY`/`HTTPS_PROXY` set to
   `http://127.0.0.1:9`.
2. Set the site up with the **Online store** recipe (site name **Contoso**, SQLite, `admin` / `Password1!`), delete the **Shop**
   listing the setup recipe made, and run the **Product listing: online store sample** recipe under **Tools > Recipes**.
3. Sign the recorder in and prepare the site:

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/point-of-sale/recorder
   export VIDEO_ROOT=<repo>/videos/point-of-sale VIDEO_BASE_URL=http://localhost:5375 VIDEO_USER_NAME=admin VIDEO_PASSWORD=Password1!
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   `prepare` turns on Point of Sale and Receipts, sets up the sign-in server and a `till-app` client (a till app signs staff in
   with the authorization code flow), adds the **Downtown store** and **Uptown store** locations, and adds **Downtown till 1**.
4. Run `python videos/point-of-sale/seed.py`. It signs in as `admin` the way a till app does, then opens a shift at
   **Downtown till 1**, rings up four sales (cash, card, a mix), records a bank run and a return, and closes the shift one
   dollar and 48 cents short; then it opens a second shift with two more sales. It needs `pip install requests` and the product
   ids it names (the sample store's *Contoso Minimal Watch*, *Northwind Wool Beanie*, *Contoso Baseball Cap* and *Contoso Tote Bag*):
   if the ids of your copy differ, change them at the top of the script.
5. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/point-of-sale`) and record the clips. `04-registers`
   adds the **Uptown till 1** register, so restore the copy before recording it again.
