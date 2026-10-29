# Contact Center Roles and Permissions: Agent, Supervisor and Who Sees What

**Feature:** `CrestApps.OrchardCore.ContactCenter` · **Length:** see `video.json`

Why roles, creating the Agent and Supervisor roles, what each gets by default, the soft phone permission, giving roles to users, the agent entitlement record a supervisor needs, and the permissions behind recordings, voicemail and reports.
The video shows the admin only: no code or configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

1. Run `src/Startup/CrestApps.OrchardCore.Cms.Web` of CrestApps.OrchardCore with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9` and `NO_PROXY=localhost,127.0.0.1`. Set it up
   with the **Blank** recipe, site name **Contoso**, SQLite and the admin user `admin`. Enable the Contact Center features, the
   Telephony soft phone, and create the fake users `jamie.chen`, `maria.lopez`, `sam.patel`, `alex.kim`, `priya.shah` and `taylor.brooks`.
2. The site needs queues (Support, Billing, New sales, Renewals) and an agent entitlement record for the five agents (no record for
   `taylor.brooks` or `admin`), and it must not have an Agent or Supervisor role yet. `../contact-center-reports/seed-reports.py` makes the
   queues and entitlements.
3. `Recorder setup`, then record `02-roles` to `07-gated` in order: the clips create the roles, tick the soft phone on the Agent role,
   give `taylor.brooks` both roles and an entitlement record. Start again from the same snapshot to record a clip again.

