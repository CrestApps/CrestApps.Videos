# Subscriptions

**Feature:** `CrestApps.OrchardCore.Subscriptions` and `CrestApps.OrchardCore.Memberships` · **Length:** 5:23

Why subscriptions and memberships exist, how plans, the Plans page, sign-up, Agreements, the subscription settings and Memberships fit together, and how to read a plan type and a plan's billing fields, sign up as a guest, manage a subscription, and set the Membership part. The video shows the admin and the storefront only: no code or configuration files. The title, description,
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
   dotnet build -c Release -p:VideoScenes=<repo>/videos/subscriptions/recorder
   export VIDEO_ROOT=<repo>/videos/subscriptions VIDEO_BASE_URL=<the site> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<the password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   `prepare` turns on **Subscriptions**, **Installment plans**, **Memberships** and the **Test payment** provider, runs the **Commerce starter** and
   **Memberships sample** recipes (a **Membership** plan, an **Annual Membership**, a **Plans** page at `/plans`, a **Member** role), adds the
   customer account `jamie` (`jamie.chen@example.com`) who signs up to **Annual Membership**, and turns on **Allow guest sign-up**. Run it on the
   site the Orders video prepared. (`VIDEO_STAGE=members`, `buy` and `guest` run the parts on their own.)
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/subscriptions`) and record the clips in order:
   `03-plan`, `04-signup`, `05-agreements`, `06-settings`, `07-memberships`. `03-plan` gives the Membership plan a 7 day trial and `04-signup`
   signs a guest up to it, so restore the copy of the site before recording a clip again.
