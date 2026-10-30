# Webhooks

**Feature:** `CrestApps.OrchardCore.Commerce.Webhooks` · **Length:** see `video.json`

Why webhooks, adding one, the events, what the receiver checks, the deliveries log (Send a test event, Send again and the retry schedule), a new secret, and removing a webhook. The receiver's address does not exist, so every delivery waits to be retried: that is what the log shows. The signing secret shown belongs to a throwaway demo webhook. The video shows the admin and the storefront only: no code or configuration files. The title, description, tags
and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (`example.com` addresses, made-up names), so it needs no mask
script. Nothing reaches a payment or messaging provider.

1. Start from the Buyer Account and Guest Orders video's site (its steps 1 and 2) and run `Recorder setup`, then `Recorder prepare` (turns on Commerce Webhooks). Run the site without a proxy, so the missing receiver address shows *No such host is known* rather than the proxy.
2. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/webhooks`) and record the clips in order: `03-add` (saves the webhook's id), `06-deliveries` (cancels the order that is waiting for payment, so an order.canceled event is raised) and `08-secret`. Restore the copy before recording them again.
