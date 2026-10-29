# Customers, Guests and Privacy

**Feature:** `CrestApps.OrchardCore.Customers` · **Length:** see `video.json`

Guest customers and merged accounts, how long guests' personal data is kept, Checkout details (your own fields at checkout), what the buyer sees, and staff's My Account page. The video shows the admin and the storefront only: no code or configuration files. The title, description, tags
and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (`example.com` addresses, made-up names), so it needs no mask
script. Nothing reaches a payment or messaging provider.

1. Start from the Buyer Account and Guest Orders video's site after its clips (a guest who made an account, so one customer is Merged), then run `Recorder setup`.
2. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/customers-guests-privacy`) and record the clips in order: `03-merge`, `04-retention`, `05-details`, `06-field`, `07-staff`. `05-details` adds a required Gift message field to Checkout details, which `06-field` uses, so restore the copy before recording them again.
