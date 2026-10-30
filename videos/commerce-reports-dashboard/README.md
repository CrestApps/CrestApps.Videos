# Commerce reports and dashboard

**Feature:** `CrestApps.OrchardCore.Reports` (with `.OpenXml`, `.Pdf` and `.Builder`) and `OrchardCore.AdminDashboard` · **Length:** 6:07

Why store reports matter, where each report comes from, how to run one with the period picker and export it, a tour of the order, stock and shipping reports, the dashboard widgets, and the ready-made reports to adapt in the Report Builder. The video shows the admin and the storefront only: no code or configuration files. The title, description,
tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (made-up products, the customer `customer@example.com`,
no payment provider contacted), so it needs no mask script.

1. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` and run it with `ORCHARD_APP_DATA` pointing
   at an empty folder outside both repositories, with `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9` and
   `NO_PROXY=localhost,127.0.0.1`, so nothing reaches a provider.
2. Set the site up with the **Online store** recipe, site name **Contoso**, SQLite, user `admin`.
3. Sign the recorder in and seed the site:

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/commerce-reports-dashboard/recorder
   export VIDEO_ROOT=<repo>/videos/commerce-reports-dashboard VIDEO_BASE_URL=<the site> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<the password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

   `prepare` turns on Reports with its Excel and PDF exports, the Report Builder and the Admin Dashboard. Run it on the site the Orders video prepared (a shop with fake orders paid today).
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/commerce-reports-dashboard`) and record the clips in order:
   `03-run`, `04-tour`, `05-dashboard`, `06-adapt`. Record them in this order on one copy of the site: the dashboard clip adds widgets and the adapt clip runs a recipe. Restore the copy of the site before recording a clip again.
