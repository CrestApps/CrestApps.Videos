# Do Not Call Lists

**Feature:** `CrestApps.OrchardCore.DncRegistry` with `.Local`, `.UsaFtc` and `.CanadaDncl` · **Length:** see `video.json`

Why do-not-call lists exist, where they are checked (imports, the dialer, calls dialed by hand), how to upload a local list
and prepare the CSV file, how to follow and manage lists, how to make every import check them, what people see on the
import screen, how to connect the USA FTC and Canada LNNTE-DNCL registries, and use cases. The video shows the admin only:
no code or configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only: `seed-dnc-list.csv` (212-555-01xx numbers, with one invalid
row and one repeated number, so the list finishes with two rejected records) and `seed-import-contacts.csv` (six fake
contacts, two of them on that list). The registries are never contacted (made-up organization ID, account number and keys;
`HTTP_PROXY`/`HTTPS_PROXY` point at `http://127.0.0.1:9`).

1. Run `src/Startup/CrestApps.OrchardCore.Cms.Web` of CrestApps.OrchardCore with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, the dead proxy above and `NO_PROXY=localhost,127.0.0.1`. Set it up with the **Blank** recipe,
   site name **Contoso**, SQLite and the admin user `admin`. Enable the Contact Center, Omnichannel Management and the DNC
   Registry features (Local, USA FTC, Canada LNNTE-DNCL).
2. `Recorder setup`, then `Recorder prepare`: it enables Content Transfer and runs the **Omnichannel CRM starter** recipe.
3. Record in order, because each clip starts from the state the previous one left: `03-upload` (uploads the list),
   `05-manage`, `06-enforce` (enforces the Local list), `07-import` (imports the six contacts: the two numbers on the list are
   left out) and `08-registries`. `01`, `02`, `04` and `09` are slides.
