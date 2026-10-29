# Report Builder formulas

**Feature:** `CrestApps.OrchardCore.Reports.Builder` · **Length:** see `video.json`

Why formulas, adding a calculated field (**Label**, **Name**, **Formula**, the **Fields** and **Functions** lists, **Check**,
**Apply**, dragging it onto **Columns**), five worked formulas (minutes from seconds, a Long or Short label, a name or
a fallback, the age of a call in days, and an answer rate worked out once per queue) and what **Check** says when a
formula mixes an aggregate with a row-level field. The video shows the admin only: no code or configuration files. The
title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

Start from the site of `videos/reports-run-and-export` (fresh Contoso site, the Reports features, the CRM starter recipe
and the fake data of `seed-reports-data.py` in that folder; behind a dead proxy).

1. Build the recorder with this folder's scenes and run `Recorder prepare`: it turns the Report Builder on and imports
   `seed-reports.json` (**Calls with formulas**, one row for each call, and **Queues with formulas**, calls for each
   queue) with **Tools > Deployments > JSON Import**. Copy `App_Data` aside.
2. Record `02-add`, `03-labels` and `04-groups`. Each adds calculated fields to a report as an unpublished draft, so restore
   the copy before recording a clip again, and between the clips of `calls-with-formulas`.
