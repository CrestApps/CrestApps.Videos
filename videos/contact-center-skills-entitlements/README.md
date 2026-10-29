# Skills and entitlements

**Feature:** `CrestApps.OrchardCore.ContactCenter (Work Distribution, Agent Entitlements)` · **Length:** see `video.json`

Why skills and entitlements exist, how they fit together, and how to create a skill, give an agent queues (with a priority and a delay), campaigns and skills (with a proficiency), read the entitlements list and give supervisors a record. The video shows the admin only: no code or configuration files.

## Recording it again

The video was recorded on a **fresh site** with fake data only: made-up users (`jamie.chen`, `maria.lopez`, `sam.patel`), fake 555 numbers and made-up names. Telephony providers are unreachable: the site runs with `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9`, so no call or text can leave the machine. It needs no mask script.

1. Start from a fresh site with the Contact Center features enabled and run the recorder's `prepare` command: it adds two skills, three queues and the entitlements of one agent.
2. Narrate and record the clips in order: `03-skill` creates a skill, `04-queues` fills the form without saving, `05-skills` fills it again silently and saves it, `06-list` saves nothing.
