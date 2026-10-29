# Report workflows

**Feature:** `CrestApps.OrchardCore.Reports.Builder` with `OrchardCore.Workflows` (and `OrchardCore.Workflows.Timers`, `OrchardCore.Email`) · **Length:** see `video.json`

Why workflows for reports, the report events (**Report Run**, **Report Exported**, **Report Published**) and how an event
is limited to some reports, the report tasks (**Run Report**, **Export Report**, **Email Report**, **Create Report Share
Link**) and their fields, two workflows run for real (publishing a report creates a share link, listed under **Tools >
Shared Links**; running the dashboard saves an Excel copy in the media library), and the **Email Report** task. The video
shows the admin only: no code or configuration files. The title, description, tags and chapters YouTube shows are in
`video.json`.

## What is real and what is not

The demo site has **no mail server**: the Email feature is on, but no provider is enabled. The **Email Report** workflow
(`Email the dashboard every morning`) is therefore shown, opened and explained, never run, and the narration says so. The
other two workflows run on camera.

## Recording it again

Start from the site of `videos/reports-run-and-export` (fresh Contoso site, the Reports features, the CRM starter recipe
and the fake data of `seed-reports-data.py` in that folder; behind a dead proxy).

1. Build the recorder with this folder's scenes and run `Recorder prepare`: it turns on the Report Builder, Workflows,
   Workflow timers and Email, and imports `seed-reports.json` (the **Call dashboard**) and `seed-workflows.json` (the three
   workflows) with **Tools > Deployments > JSON Import**. Copy `App_Data` aside.
2. Record `02-events`, `03-tasks`, `04-run`, `05-export` and `06-email`. `04-run` publishes the dashboard (a workflow
   instance and a share link appear) and `05-export` runs it (a file appears in the media library under `reports/runs`),
   so restore the copy before recording either again.
