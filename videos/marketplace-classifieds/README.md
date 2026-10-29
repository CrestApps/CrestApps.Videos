# Marketplace and Classifieds

**Feature:** `CrestApps.OrchardCore.Marketplace` (with Marketplace payouts with Stripe Connect) and `CrestApps.OrchardCore.Classifieds` · **Length:** see `video.json`

Why a site opens up to other sellers, how vendors, commission, hold and payouts work, the settings, adding a vendor and
choosing it on a product, a vendor's sales and what is held, ready and paid out, what the people who run a vendor see, how to
let visitors post a kind of listing, posting, approving and rejecting listings, renewing and upgrading, and searching by
words and distance. The video shows the admin and the storefront only: no code or configuration files. The title,
description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (`example.com` addresses, made-up names and places), so it
needs no mask script. Nothing reaches a payment or messaging provider (no payout account is set up, as the video says).

1. Make the site the Point of Sale video describes (its steps 1 to 4: the **Online store** recipe, the sample store, the
   registers and the till-app sign-in), and the fake customers the Customers and Companies video makes. The marketplace's
   sales are register sales, paid in cash.
2. Sign the recorder in, then prepare the site in four passes:

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/marketplace-classifieds/recorder
   export VIDEO_ROOT=<repo>/videos/marketplace-classifieds VIDEO_BASE_URL=http://localhost:5375 VIDEO_USER_NAME=admin VIDEO_PASSWORD=Password1!
   <bin>/Recorder setup
   <bin>/Recorder prepare                                   # the Marketplace and Classifieds sample recipes, and the Customers and Orders APIs
   VIDEO_PREPARE_STAGE=signin <bin>/Recorder prepare        # saves a sign-in for jamie, riley and morgan (state-<user>.json, git-ignored)
   VIDEO_PREPARE_STAGE=seed <bin>/Recorder prepare          # the vendor Clayworks Studio, its mug, four posted listings, two approved
   python videos/marketplace-classifieds/seed.py            # two register sales of the mug, for the vendor's earnings (pip install requests)
   ```

   The piano lessons are approved for five days only, so that their owner is offered **Renew**.
3. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/marketplace-classifieds`) and record the clips in
   order. `04-vendor` adds a vendor, `05-detail` releases a sale that `06-portal` then shows as ready, `08-post` posts the
   listing that `09-approve` approves, and `09-approve` rejects one: restore the copy before recording again.
