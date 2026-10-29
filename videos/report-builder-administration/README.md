# Report Builder administration

**Feature:** `CrestApps.OrchardCore.Reports.Builder` · **Length:** see `video.json`

For the person who looks after the Report Builder: the five permissions in a role (**Build reports and manage own custom
reports and views**, **View all custom reports**, **Manage all custom reports and views**, **Share custom reports publicly
and through share links** and **Manage report groups**), what a person without them sees and what changes after the build
permission is given, how to bring reports to a site with **Tools > Deployments > Package Import** (the file holds the
`ReportDesigns` step), what the limits mean on large data, and the usual problems with their fixes. The video shows the
admin only: no code or configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

The large-data and troubleshooting chapters are narrated slides (a limit or a problem cannot be shown on a small demo
site). The **Report Designs** step of a deployment plan (**Tools > Deployments > Plans**) is not shown: at the time of
recording, saving that step failed on the demo site with a serialization error, so the video imports a file instead.

## Recording it again

Start from the site of `videos/reports-run-and-export` (fresh Contoso site, the Reports features and the fake data of
`seed-reports-data.py` in that folder; behind a dead proxy). It needs a role named **Report viewer** with a user
**jamie.chen** in it.

1. Build the recorder with this folder's scenes and run `Recorder prepare`: it turns on the Report Builder and
   **Deployments**, imports `seed-reports.json` (**Call dashboard**) with **Tools > Deployments > JSON Import**, and signs
   in as **jamie.chen** to save a sign-in. Copy `App_Data` aside.
2. Record `01-roles`, `02-before`, `02b-grant`, `02c-after` and `03-deploy`. `02b-grant` gives the role the build
   permission, so restore the copy before recording `02-before` or `02b-grant` again. `02c-after` signs in again, because a
   person who is signed in when a role changes sees the change only after signing in again.
