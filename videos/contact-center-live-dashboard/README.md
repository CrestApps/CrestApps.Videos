# The live dashboard

**Feature:** `CrestApps.OrchardCore.ContactCenter (Supervision & Live Dashboard)` · **Length:** see `video.json`

Why a live dashboard, why a supervisor sees only the queues on their own entitlement record, how to read the summary, queue tiles, dialer banner and agent board, and (as slides, because they need an agent on a live call) listen, whisper, barge, take over, end, transfer, record, set an agent's state and message an agent. The video shows the admin only: no code or configuration files.

## Recording it again

The video was recorded on a **fresh site** with fake data only: made-up users (`jamie.chen`, `maria.lopez`, `sam.patel`), fake 555 numbers and made-up names. Telephony providers are unreachable: the site runs with `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9`, so no call or text can leave the machine. It needs no mask script.

1. Start from the site of the agent workspace video (three queues, `admin` signed in to two of them), so the board has one agent.
2. Narrate, then record `03-reading` and `04-board`, which save nothing.
