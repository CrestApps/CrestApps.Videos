# Queues

**Feature:** `CrestApps.OrchardCore.ContactCenter (Work Distribution)` · **Length:** see `video.json`

Why queues, how groups, queues and their cards fit together, and how to create a queue group and a queue, choose how the next agent is picked, set service levels and skills, hours, overflow and limits, what callers hear while they wait and callbacks, and clone a queue. The video shows the admin only: no code or configuration files.

## Recording it again

The video was recorded on a **fresh site** with fake data only: made-up users (`jamie.chen`, `maria.lopez`, `sam.patel`), fake 555 numbers and made-up names. Telephony providers are unreachable: the site runs with `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9`, so no call or text can leave the machine. It needs no mask script.

1. Start from a fresh site with the Contact Center features enabled and run the recorder's `prepare` command: it adds two skills, a weekday calendar, a `General help` queue and a hold music recording.
2. Narrate and record the clips in order: `03-create` creates the `Support` queue, then `04-routing` to `07-waiting` each edit it and save; `08-clone` saves nothing.
