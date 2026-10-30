# Omnichannel Reports: Which Report Answers Which Question

**Features:** `CrestApps.OrchardCore.Reports` with `CrestApps.OrchardCore.Omnichannel.Managements` and `CrestApps.OrchardCore.Omnichannel.Crm` · **Length:** see `video.json`

Where the omnichannel reports are, their shared filters and the permission they need, then every group of reports and the
question it answers: volume and completion (Activity summary, Campaign performance, Disposition breakdown), backlog and age,
workload grouped by source, channel, kind, contact type, assignment, urgency and attempts, people, campaign mixes, customer
workload and address usage, Dialer call detail, AI handoff and containment, Lead funnel, Lead conversion and Opportunity
pipeline. The last part shows the Omnichannel and Messaging data sets in a Report Builder design. The video shows the admin
only: no code or configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** (Blank recipe, site name Contoso, SQLite, admin user `admin`) with fake data
only, behind a dead proxy (`HTTP_PROXY=http://127.0.0.1:9`), so nothing leaves the machine.

1. Enable the Contact Center and Omnichannel features (management, messaging, SMS, voice, CRM), Reports, Reports (OpenXml),
   Reports (PDF), Report Builder (it brings Data Pipelines Core) and run the **Omnichannel CRM starter** recipe (Tools >
   Recipes). Create the users `jamie.chen`, `maria.lopez`, `sam.patel`, `alex.kim` and `priya.shah`. Restart the site after
   enabling features.
2. Stop the site and fill it, in this order, with fake contacts, leads, activities, campaigns, dispositions and calls:
   `python ../reports-run-and-export/seed-reports-data.py <App_Data>/Sites/Default/OrchardCore.db`, then
   `python seed-omnichannel-reports.py <App_Data>/Sites/Default/OrchardCore.db`. The second script spreads the activities over
   four campaigns, gives them real sources, addresses and creators, adds AI conversations (contained and handed off), converts
   more leads, adds 26 sales opportunities in every stage and 28 messaging conversations. Everything is relative to the day
   you run it (the last 45 days), so run it the day you record. Start the site again and copy `App_Data` aside.
3. A hiding rule keeps the soft phone out of the frame: `VIDEO_INIT_SCRIPT` points to a one-line script that adds the style
   `.cc-agent-bar, #telephony-soft-phone { display: none }`.
4. Narrate (`scripts/narrate.py videos/omnichannel-reports`), then `Recorder setup` and record the clips of `storyboard.json`
   (`02-where`, `02b-permission`, `03-volume`, `04-backlog`, `05-groupings`, `06-people`, `07-campaigns`, `08-dialer`,
   `09-leads`, `10-builder`, `11-uses`). `10-builder` creates a draft report: restore `App_Data` before recording it again.
5. Assemble, then check the video for private data (`ocrcheck.py`).

The reports are opened on the last 90 days with the page's own **Last 90 Days** range.
