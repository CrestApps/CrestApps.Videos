# Placing and handling calls

**Feature:** `CrestApps.OrchardCore.Telephony (Soft Phone Extension)` · **Length:** see `video.json`

How to dial a number and an extension, and (as slides, because they need a live call) how to answer, hold, add a call, transfer and merge calls. The video shows the soft phone only: no code or configuration files, and no call is placed (the providers are unreachable on purpose). The soft phone itself, the settings and the extensions are in the `telephony` video.

## Recording it again

The video was recorded on a **fresh site** with fake data only: made-up users (`jamie.chen`, `maria.lopez`, `sam.patel`), fake 555 numbers and made-up names. Telephony providers are unreachable: the site runs with `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9`, so no call or text can leave the machine. It needs no mask script.

1. Start from a fresh site with the Contact Center and Telephony features enabled and a fake Asterisk provider (so the soft phone shows Ready).
2. Run the recorder's `prepare` command: it gives two fake users an extension each.
3. Narrate and record `02-dial` and `04-extension`; neither presses Call.
