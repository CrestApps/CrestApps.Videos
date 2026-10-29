# Product Reviews

**Feature:** `CrestApps.OrchardCore.Reviews` · **Length:** see `video.json`

Why a store shows reviews, how writing, approving and showing them fit together, what shoppers see on a product page, how a
customer writes a review, how to approve, reject and delete reviews, the settings (approval, only buyers can review, asking
buyers for reviews by email), and how listings sort and filter by customer rating. The video shows the admin and the
storefront only: no code or configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (`example.com` addresses, made-up names), so it needs no mask
script. Nothing reaches a payment or messaging provider.

1. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` and run it with `ORCHARD_APP_DATA` pointing at
   an empty folder outside both repositories, on `http://localhost:5375`, with `HTTP_PROXY`/`HTTPS_PROXY` set to
   `http://127.0.0.1:9`.
2. Set the site up with the **Online store** recipe (site name **Contoso**, SQLite, `admin` / `Password1!`), delete the **Shop**
   listing the setup recipe made, and run the **Product listing: online store sample** recipe under **Tools > Recipes**.
3. The fake customers are the ones the Customers and Companies video makes (`jamie`, `riley`, `morgan`, `dana`, all with the demo
   password): run that video's `prepare` first, or add the four users under **Access Control > Users**.
4. Sign the recorder in and prepare the site:

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/reviews/recorder
   export VIDEO_ROOT=<repo>/videos/reviews VIDEO_BASE_URL=http://localhost:5375 VIDEO_USER_NAME=admin VIDEO_PASSWORD=Password1!
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   `prepare` lets any signed-in customer review, saves a sign-in for each fake customer (`state-<user>.json`, git-ignored), sends
   five reviews, and approves three of them.
5. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/reviews`) and record the clips. `04-write` sends the
   review that `05-approve` approves, so record them in that order, with `07-listing` before the settings are put back. Then run
   `VIDEO_PREPARE_STAGE=2 <bin>/Recorder prepare` (it turns **Only buyers can review** on again) and record `06-settings`.
