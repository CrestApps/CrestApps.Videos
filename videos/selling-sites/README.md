# Selling Sites

**Feature:** `CrestApps.OrchardCore.Subscriptions.Tenants` · **Length:** see `video.json`

Why sell sites, setting up a plan with Tenant Onboarding, the database for new sites, the buyer signing up (the Your new site step), My Sites, Site provisioning with Retry now, and the statuses. The subscriber pays with Pay Later because the demo site has no card provider. The video shows the admin and the storefront only: no code or configuration files. The title, description, tags
and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (`example.com` addresses, made-up names), so it needs no mask
script. Nothing reaches a payment or messaging provider.

1. Start from the Buyer Account and Guest Orders video's site (its steps 1 and 2) and run `Recorder setup`, then `Recorder prepare` (turns on Subscriptions, Multi-Tenancy and Subscriptions - Sites, turns on Pay Later, runs the Commerce starter recipe for its Plans page, and signs jamie in).
2. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/selling-sites`) and record the clips in order: `03-plan` (adds the Tenant Onboarding part and picks the Blog recipe), `04-settings`, `05-buy` (signs Jamie up for the plan). The site creates a waiting job on its own every minute, so copy the site's `App_Data` aside again right after `05-buy`, and record `06-sites`, `07-provisioning` (which clicks Retry now on the waiting job) and `08-ready` from that copy straight away, restoring it and trying again if the job was already created.
