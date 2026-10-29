# Commerce Automation

**Feature:** `CrestApps.OrchardCore.Orders` · **Length:** see `video.json`

Why automate, the three ready-made recipes (thank-you email, canceled order alert, low stock alert), the workflow editor with its commerce and Stripe events, turning a workflow on and testing it by canceling an order, and workflows or webhooks. The video shows the admin and the storefront only: no code or configuration files. The title, description, tags
and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (`example.com` addresses, made-up names), so it needs no mask
script. Nothing reaches a payment or messaging provider.

1. Start from the Buyer Account and Guest Orders video's site after its first prepare (an order still awaiting payment, and the SMTP settings that point at a local email sink), then run `Recorder setup`.
2. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/commerce-automation`) and record the clips in order: `03-recipes`, `04-editor`, `05-fire`. Each one builds on the one before (the recipes add the workflow, the editor changes it, `05-fire` turns it on and cancels the awaiting order), so restore the copy before recording them again.
