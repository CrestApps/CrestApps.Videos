# Point of Sale: A Day at the Till

**Feature:** `CrestApps.OrchardCore.PointOfSale` · **Length:** see `video.json`

The till app and what the site gives you, a day at the till step by step, the register and the open shift, closing the shift (expected, counted and the difference), and the sales and returns as orders. No till app ships in this solution: the till steps are narrated slides, and the screens shown are the ones the site has. The video shows the admin and the storefront only: no code or configuration files. The title, description, tags
and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (`example.com` addresses, made-up names), so it needs no mask
script. Nothing reaches a payment or messaging provider.

1. Start from the Point of Sale video's site (its README: the demo store, a register, and a till day rung up through the commerce API with its seed script: an open shift, a closed shift with a counted difference, register sales and a return), then run `Recorder setup`.
2. Narrate (`scripts/narrate.py videos/point-of-sale-till`) and record the clips: `03-open`, `04-closed`, `05-sales`. They only read the site, so they can be recorded in any order and again.
