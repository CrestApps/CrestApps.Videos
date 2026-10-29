# The soft phone apps

**Feature:** `CrestApps.OrchardCore.Telephony (Soft Phone Extension)` · **Length:** see `video.json`

Why phone apps, where each runs, the administrator's steps on the site (the Telephony Soft Phone Extension feature, the Use the telephony soft phone permission and the site domain), the soft phone page that the apps open, and (as slides, because the apps are installed on the user's computer) how to set up the browser extension and the Windows app. The video shows the admin only: no code or configuration files.

## Recording it again

The video was recorded on a **fresh site** with fake data only: made-up users (`jamie.chen`, `maria.lopez`, `sam.patel`), fake 555 numbers and made-up names. Telephony providers are unreachable: the site runs with `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9`, so no call or text can leave the machine. It needs no mask script.

1. Start from a fresh site with the Contact Center and Telephony features enabled and a fake Asterisk provider.
2. Narrate and record `03-admin` (saves nothing) and `04-page`.
