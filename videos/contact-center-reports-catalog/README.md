# The Contact Center Report Catalog: Which Report Answers Which Question

**Feature:** `CrestApps.OrchardCore.Reports` · **Length:** see `video.json`

The eight categories of the Contact Center reports, with a real report from each, and which report answers which question.
The video shows the admin only: no code or configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The reports need history, so the demo site is filled with fake calls, queues and agent time by `seed-reports.py` (in `videos/contact-center-reports/`)
(about 2,500 interactions over 40 days, five fake agents, four queues in two queue groups; every number is invented).

1. Run `src/Startup/CrestApps.OrchardCore.Cms.Web` of CrestApps.OrchardCore with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9` and `NO_PROXY=localhost,127.0.0.1`. Set it up
   with the **Blank** recipe, site name **Contoso**, SQLite and the admin user `admin`. Enable the Contact Center features, the
   Reports, Reports (OpenXml) and Reports (PDF) features, and create the users `jamie.chen`, `maria.lopez`, `sam.patel`, `alex.kim`
   and `priya.shah`.
2. Stop the site and run `python seed-reports.py <App_Data folder>`; start the site again. Dates are relative to the day the script
   runs, so seed again before recording.
3. `Recorder setup`, then record the clips named in `storyboard.json`. The reports are opened with a custom date range of the last
   seven days (the page's own Last 7 Days quick range needs a click and **Show**).

