# IVR menus

**Feature:** `CrestApps.OrchardCore.ContactCenter (Inbound Entry Points, Inbound Voice)` · **Length:** see `video.json`

The welcome message and the IVR menu of an inbound entry point: why a phone menu, how prompts, keys and actions work, how to build a menu with submenus, what the tries and the fallback do, what the builder reports as problems, and (as slides, because it needs a feature that this site does not enable) how an AI voice agent can answer instead. The video shows the admin only: no code or configuration files. The entry points themselves are in `contact-center-entry-points-ivr`.

## Recording it again

The video was recorded on a **fresh site** with fake data only: made-up users (`jamie.chen`, `maria.lopez`, `sam.patel`), fake 555 numbers and made-up names. Telephony providers are unreachable: the site runs with `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9`, so no call or text can leave the machine. It needs no mask script.

1. Start from the site of the entry points video (numbers, queues, agents, voice media) and run the recorder's `prepare` command: it adds the `Main line` entry point.
2. Narrate and record the clips in order: `03-welcome`, `04-build`, `05-submenus`, `06-tries` (each one edits `Main line` and saves it).
