# Installment Plans

**Feature:** `CrestApps.OrchardCore.Subscriptions.Installments` · **Length:** see `video.json`

Why installment plans and how a plan works, the plan settings, creating a plan (customer, amounts, schedule, collection), managing and canceling a plan, taking the card, Charge now, declined cards and statuses, and customers choosing installments at checkout. The card steps are narrated slides: they need a payment provider that keeps a card, and the demo site has none. The video shows the admin and the storefront only: no code or configuration files. The title, description, tags
and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (`example.com` addresses, made-up names), so it needs no mask
script. Nothing reaches a payment or messaging provider.

1. Start from the Buyer Account and Guest Orders video's site (its steps 1 and 2) and run `Recorder setup`, then `Recorder prepare` (turns on the Installments feature).
2. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/installment-plans`) and record the clips in order: `03-settings`, `04-new`, `05-manage`. `04-new` creates the plan that `05-manage` opens and cancels, so restore the copy before recording them again.
