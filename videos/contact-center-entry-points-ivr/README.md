# Inbound entry points

**Feature:** `CrestApps.OrchardCore.ContactCenter (Inbound Entry Points)` · **Length:** see `video.json`

Why a number needs an entry point, how numbers, routing, hours and voicemail fit together, and how to add numbers, create a call entry point, set closed hours and voicemail, make a personal line, create a text entry point and clone an entry point. The IVR menu has its own video (`contact-center-ivr-menus`). The video shows the admin only: no code or configuration files.

## Recording it again

The video was recorded on a **fresh site** with fake data only: made-up users (`jamie.chen`, `maria.lopez`, `sam.patel`), fake 555 numbers and made-up names. Telephony providers are unreachable: the site runs with `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9`, so no call or text can leave the machine. It needs no mask script.

1. Start from a fresh site with the Contact Center, Omnichannel and Telephony features enabled and a fake Asterisk provider.
2. Run the recorder's `prepare` command (`recorder/Scenes.cs`): it adds four fake numbers, a weekday calendar, three queues, a voice media item and three agents with entitlements.
3. Narrate (`scripts/narrate.py`) and record the clips in order: `03-numbers` to `09-clone`. `04-create` to `08-text` build up the entry points that the later clips edit.
