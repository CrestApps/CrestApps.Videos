# Contact Center Settings

**Feature:** `CrestApps.OrchardCore.ContactCenter` with Call Recording, Voice and Secure Data Capture (each tab names its feature) · **Length:** see `video.json`

Why the rules are site-wide, the Default numbers tab, Recording governance (consent, disclosure, retention, legal hold,
pausing), who says the disclosure on each kind of call, External transfer destinations, and Secure Data Capture. The video
shows the admin only: no code or configuration files. The title, description, tags and chapters YouTube shows are in
`video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (two made-up Omnichannel Addresses, 212-555-01xx numbers,
created by `Recorder prepare`), so it needs no mask script.

1. Run `src/Startup/CrestApps.OrchardCore.Cms.Web` of CrestApps.OrchardCore with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, with `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9` and `NO_PROXY=localhost,127.0.0.1`.
   Set it up with the **Blank** recipe, site name **Contoso**, SQLite and the admin user `admin`, and enable the Contact
   Center features (Call Recording, Voice, Secure Data Capture) and Omnichannel Management.
2. `Recorder setup`, then `Recorder prepare`.
3. Record `02-numbers`, `03-recording`, `05-transfers` and `06-secure` in order (each saves the form it edits).
   Adding a transfer destination only saves with the fix of CrestApps.OrchardCore pull request 1146.
