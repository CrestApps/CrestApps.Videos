# Memberships

**Feature:** `CrestApps.OrchardCore.Memberships` · **Length:** see `video.json`

Why memberships, selling a membership (the Memberships sample, a role and the days), keeping content for members with the Role Picker, buying a membership and what a member sees, and managing and ending memberships. The video shows the admin and the storefront only: no code or configuration files. The title, description, tags
and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (`example.com` addresses, made-up names), so it needs no mask
script. Nothing reaches a payment or messaging provider.

1. Start from the Buyer Account and Guest Orders video's site (its steps 1 and 2) and run `Recorder setup`, then `Recorder prepare` (turns on SMTP and signs jamie in).
2. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/memberships`) and record the clips in order: `03-sell` (runs the Memberships sample recipe), `04-members`, `04-guest`, `05-buy`, `06-member`, `07-manage`, `08-after`. `05-buy` places Jamie's order, `06-member` records its bank transfer and signs Jamie in again, and `07-manage` ends the membership, so restore the copy before recording them again.
