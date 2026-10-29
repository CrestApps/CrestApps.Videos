# Report widgets on the dashboard

**Feature:** `CrestApps.OrchardCore.Reports` and `OrchardCore.AdminDashboard` (the Custom Report widget needs `CrestApps.OrchardCore.Reports.Builder`) · **Length:** see `video.json`

Why a report on the admin dashboard, turning on the Admin Dashboard, adding a **Report** widget (position, width, height,
title, report, period, what to show, rows per table, keep for), what a tile shows (period, when it was made, *Open the
full report*), the **Custom Report** widget for a Report Builder report, and what a person sees who may open the dashboard
but not a report (*You can't view the Lead funnel report.*). The video shows the admin only: no code or configuration
files. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

Start from the site of `videos/reports-run-and-export` (fresh Contoso site, the Reports features, the CRM starter recipe,
the fake data of `seed-reports-data.py` in that folder and its **Report viewer** role; behind a dead proxy).

1. Build and run the recorder with this folder's scenes. `Recorder prepare` turns the Report Builder on and imports
   `seed-reports.json` (six ready-made Report Builder reports) with **Tools > Deployments > JSON Import**.
2. Record `02-features`, which turns the Admin Dashboard on. Run `Recorder prepare` again: it lets the **Report viewer**
   role open the dashboard, adds a **Lead funnel** widget (position 5, width 2, last 90 days, figures and tables) as an
   administrator, and signs `jamie.chen` in (saved in `state-viewer.json`). Copy `App_Data` aside.
3. Record `03-report`, `04-dashboard`, `05-custom` and `06-viewer` in this order (they add widgets; restore the copy
   before recording them again).
