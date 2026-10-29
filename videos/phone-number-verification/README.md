# Phone Number Verification

**Feature:** `CrestApps.OrchardCore.PhoneNumbers.Verifications` with a provider feature (`...Verifications.Veriphone`, `.AbstractApi`, `.Twilio`) · **Length:** see `video.json`

Why phone numbers are verified, when a number is checked, how to set up a provider (AbstractAPI, Twilio, Veriphone) and
the general settings, how to read the status icons next to a number, how to work the Phone Verifications Queue, how to
read the report, and use cases. The video shows the admin only: no code or configuration files. The title, description,
tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only: eight made-up contacts (`seed-contacts.csv`, 212-555-01xx
numbers) and a made-up Veriphone key. The verification services are never contacted (`HTTP_PROXY`/`HTTPS_PROXY` point at
`http://127.0.0.1:9`), so the results a service would return are written into the database by `seed-verification.py`.

1. Run `src/Startup/CrestApps.OrchardCore.Cms.Web` of CrestApps.OrchardCore with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, the dead proxy above and `NO_PROXY=localhost,127.0.0.1`. Set it up with the **Blank**
   recipe, site name **Contoso**, SQLite and the admin user `admin`. Enable the Contact Center, Omnichannel Management
   and the three Phone Number Verification provider features.
2. `Recorder setup`, then `Recorder prepare`: it enables Content Transfer, runs the **Omnichannel CRM starter** recipe and
   imports `seed-contacts.csv`.
3. Copy the site's `App_Data` aside (call it A) and record `03-provider` from A: it enables Veriphone and saves the
   general settings. Enable the **Report Builder** feature (it brings the report pages), stop the site, copy `App_Data`
   again (B), then run `python seed-verification.py <App_Data>/Sites/Default/OrchardCore.db` with the site stopped.
4. Start the site on the seeded copy and record `04-icons`, `05-queue` and `06-report`. `05-queue` re-checks a number, which
   takes about twenty seconds with the service unreachable: that wait is cut out of the clip.
