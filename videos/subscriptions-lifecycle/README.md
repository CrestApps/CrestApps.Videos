# Subscriptions: the whole lifecycle

**Feature:** `CrestApps.OrchardCore.Subscriptions` (with Memberships) · **Length:** 4:33

Why a subscription needs a lifecycle, the statuses, a plan's billing and the roles it gives, pausing, resuming and canceling a subscription with its history, and what a subscriber sees under My Plans. The video shows the admin and the storefront only: no code or configuration files. The title, description,
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/subscriptions-lifecycle/recorder
   export VIDEO_ROOT=<repo>/videos/subscriptions-lifecycle VIDEO_BASE_URL=<the site> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<the password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   Restore the site the Subscriptions video prepared (the Commerce starter and Memberships sample recipes, a *Membership plan*, an *Annual Membership* and a customer account), then run `prepare` with `VIDEO_STAGE=buy`, which signs the customer `jamie` up to the Annual Membership with the test payment method.
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/subscriptions-lifecycle`) and record the clips in order:
   `03-plan`, `05-myplans`, `04-manage`. Record them in this order on one copy of the site: `05-myplans` shows the plan Active before `04-manage` pauses, resumes and cancels it. The Memberships and good-to-know chapters are narrated slides: a membership sold as a subscription does not appear under Commerce > Memberships, which lists products with the Membership part that are bought once.
