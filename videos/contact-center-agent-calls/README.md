# Offers, calls and wrap-up

**Feature:** `CrestApps.OrchardCore.ContactCenter (Agents, Real-Time, Voice)` · **Length:** see `video.json`

What an agent does from the offer to the outcome: accept or decline, calls from the dialer, protecting card details, and wrap-up. A live call cannot be placed on the recording site (its providers are unreachable on purpose), so every screen that appears only during a call is described on a slide with the words it shows; only `06-recent` is a recording (the empty Recent activity, Recent and Voicemail screens). The video shows the admin only: no code or configuration files.

## Recording it again

The video was recorded on a **fresh site** with fake data only: made-up users (`jamie.chen`, `maria.lopez`, `sam.patel`), fake 555 numbers and made-up names. Telephony providers are unreachable: the site runs with `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9`, so no call or text can leave the machine. It needs no mask script.

1. Start from the site of the agent workspace video (three queues, `admin` signed in to two of them).
2. Narrate, then record `06-recent`, which saves nothing.
