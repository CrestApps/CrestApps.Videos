# Headless API

**Feature:** `CrestApps.OrchardCore.Orders.Api` · **Length:** see `video.json`

Why a commerce API, turning on the API features, OpenAPI and its explorers, the OpenID sign-in server, a public application for buyers and a confidential application for a server, a CORS policy, and who can do what. The client secret typed in the video is a throwaway value for a demo application. The video shows the admin and the storefront only: no code or configuration files. The title, description, tags
and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (`example.com` addresses, made-up names), so it needs no mask
script. Nothing reaches a payment or messaging provider.

1. Start from the Buyer Account and Guest Orders video's site (its steps 1 and 2) and run `Recorder setup`, then `Recorder prepare` (turns on the API features, OpenAPI with Swagger UI and ReDoc UI, the three OpenID features and CORS; the Inventory API and Scalar UI are left off for the video to turn on).
2. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/headless-api`) and record the clips in order: `03-features`, `04-explorers`, `05-server`, `06-apps`, `07-cors`. `05-server` must come before `06-apps` (the client credentials flow is only offered once the server allows it), so restore the copy before recording them again.
