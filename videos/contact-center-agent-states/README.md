# Agent states

**Feature:** `CrestApps.OrchardCore.ContactCenter (Agents)` · **Length:** see `video.json`

Why agent states exist, how reason codes map to presence states, how to create a reason code, what agents see in the presence menu, and how to retire a reason code. The video shows the admin only: no code or configuration files.

## Recording it again

The video was recorded on a **fresh site** with fake data only: made-up users (`jamie.chen`, `maria.lopez`, `sam.patel`), fake 555 numbers and made-up names. Telephony providers are unreachable: the site runs with `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9`, so no call or text can leave the machine. It needs no mask script.

1. Start from a fresh site with the Contact Center features enabled. No seed data is needed: the site comes with seven reason codes.
2. Narrate and record the clips in order: `03-create` adds `Customer visit`, `04-menu` shows the presence menu, `05-disable` disables `Customer visit`.
