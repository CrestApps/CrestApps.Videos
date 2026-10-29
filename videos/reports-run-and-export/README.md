# Run and export reports

**Feature:** `CrestApps.OrchardCore.Reports` with `CrestApps.OrchardCore.Reports.OpenXml` and `CrestApps.OrchardCore.Reports.Pdf` · **Length:** see `video.json`

Why reports exist, how to find a report under **Reports**, which roles see which reports, the date range (relative days,
calendar periods, rolling months, custom ranges), filters, export to CSV, Excel and PDF (with the real files shown), the
"too long" message of the agent time reports, and the "Capability not enabled" notice. The video shows the admin only:
no code or configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** (Blank recipe, site name Contoso, SQLite, admin user `admin`) with fake data
only, behind a dead proxy (`HTTP_PROXY=http://127.0.0.1:9`), so nothing leaves the machine.

1. Enable the features `CrestApps.OrchardCore.Reports`, `CrestApps.OrchardCore.Reports.OpenXml`,
   `CrestApps.OrchardCore.Reports.Pdf` and `CrestApps.OrchardCore.ContentTransfer`, and run the
   **Omnichannel CRM starter** recipe (Tools > Recipes). The Contact Center, Omnichannel and fake users
   (`jamie.chen`, `maria.lopez`, `sam.patel`, `alex.kim`, `priya.shah`, password `Password1!`) are as in the other
   Contact Center videos.
2. Stop the site and fill it with fake interactions, activities, contacts and leads:
   `python seed-reports-data.py <App_Data>/Sites/Default/OrchardCore.db`. The data is relative to the time you run
   it (the last 45 days), so run it the day you record.
3. Start the site, sign the recorder in (`Recorder setup`) and run `Recorder prepare`: it creates the **Report viewer**
   role (with **Access Admin Panel** and **View Contact Center reports**) and gives it to `jamie.chen`, whose sign-in it
   saves in `state-viewer.json` for the `03b-viewer` clip. Copy `App_Data` aside.
4. Narrate (`scripts/narrate.py videos/reports-run-and-export`) and record the clips in this order:
   `02-find`, `03-roles`, `03b-viewer`, `04-run`, `05-filters`, `06-export` (it saves the CSV, Excel and PDF files to
   `shots/downloads/`, which `slides.py` draws in the `06b-files` slides: record `06-export` before assembling),
   `07-limits`. For `07b-notice` turn off **Contact Center Call Recording** under Tools > Features first (it also
   turns off Contact Center Secure Data Capture), so **Recording coverage** shows the notice; the other clips need it on.
5. Assemble, then check the video for private data (`ocrcheck.py`).

A hiding rule keeps the soft phone out of the frame: a one-line style `.cc-agent-bar, #telephony-soft-phone { display: none }`
passed in `VIDEO_INIT_SCRIPT`.
