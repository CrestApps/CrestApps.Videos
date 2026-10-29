# Buyer Account and Guest Orders

**Feature:** `CrestApps.OrchardCore.Orders` · **Length:** see `video.json`

Why buyers need a way back to their orders, the buyer's account (orders, invoices, payment methods, wishlist), checking out as a guest and the order link, Track your order and Find my order, and turning a guest order into an account. The video shows the admin and the storefront only: no code or configuration files. The title, description, tags
and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (`example.com` addresses, made-up names), so it needs no mask
script. Nothing reaches a payment or messaging provider.

1. Make the Company Approvals video's store (its steps 1 and 2), then run `Recorder setup` and `Recorder prepare` (turns on Wishlists, SMTP and Registration, signs jamie in, and gives Jamie one paid order bought by bank transfer).
2. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/buyer-account-guest-orders`) and record the clips in order: `03-account`, `04-guest`, `05-find`, `06-register`. `04-guest` writes the guest's order number to `guest-order.txt`, `05-find` reads it and the email sink's newest message, and `06-register` creates the account, so restore the copy before recording any of them again.
