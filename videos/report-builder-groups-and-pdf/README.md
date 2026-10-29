# Report groups and PDF export

**Feature:** `CrestApps.OrchardCore.Reports.Builder` (and `CrestApps.OrchardCore.Reports.Pdf` for the PDF) · **Length:** see `video.json`

Why report groups, **Reports > Report Groups** (**Add Group**, **Name**, **Parent group**, **Display order**, **Add a group
inside**), a report's **Settings** tab (**Group**, **Show in the admin menu**, **Let people the report is shared with
export it**), the report in the admin menu inside its nested group, and **Export PDF**. The video shows the admin only: no
code or configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

Start from the site of `videos/reports-run-and-export` (fresh Contoso site, the Reports features, the CRM starter recipe
and the fake data of `seed-reports-data.py` in that folder; behind a dead proxy).

1. Build the recorder with this folder's scenes and run `Recorder prepare`: it turns the Report Builder on and imports
   `seed-reports.json` (the **Call dashboard**, in a *Contact Center* group) with **Tools > Deployments > JSON Import**.
2. Record `02-groups` (creates the **Service** group and **Regional** inside it), copy `App_Data` aside, then `03-settings`
   (publishes the dashboard into *Service / Regional*, in the admin menu) and `04-export` (saves the PDF to
   `shots/downloads/`, which `slides.py` draws in the `04b-files` slide). Restore the copy to record `03-settings` again.
