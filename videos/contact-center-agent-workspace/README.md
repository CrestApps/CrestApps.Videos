# The agent workspace

**Feature:** `CrestApps.OrchardCore.ContactCenter (Agents, Real-Time, Agent Desktop)` · **Length:** see `video.json`

How an agent signs in to queues from the soft phone Work tab, sets presence, and reads My workspace and the docked agent bar. Offers, calls and wrap-up are in `contact-center-agent-calls`. The video shows the admin only: no code or configuration files.

## Recording it again

The video was recorded on a **fresh site** with fake data only: made-up users (`jamie.chen`, `maria.lopez`, `sam.patel`), fake 555 numbers and made-up names. Telephony providers are unreachable: the site runs with `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9`, so no call or text can leave the machine. It needs no mask script.

1. Start from a fresh site with the Contact Center and Telephony features enabled and run the recorder's `prepare` command: it adds three queues and gives the `admin` user entitlements to two of them (the agent in the video is `admin`).
2. Narrate and record the clips in order: `03-signin` signs `admin` in to two queues, then `04-presence`, `05-workspace` and `06-bar` show the signed-in state.
