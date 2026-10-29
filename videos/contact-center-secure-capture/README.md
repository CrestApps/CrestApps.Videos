# Contact Center Secure Data Capture: Let the Customer Type the Card Number

**Feature:** `CrestApps.OrchardCore.ContactCenter.SecureCapture` · **Length:** see `video.json`

Why secure capture, the settings, the agent sending the link from the workspace, the customer's secure page, what is kept, and what a production site needs.
The video shows the admin only: no code or configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The call is a fake live call made by `seed-live-call.py`; nothing reaches a phone provider. The happy path needs the Development
environment, because the built-in tokenizing sink is registered only there (see the video: production sites need a tokenization provider).
Only the standard test card (4242 4242 4242 4242, 12/30, 123) is typed.

1. Start from a demo site with the Contact Center, queues and agents (the snapshot of `../contact-center-reports/seed-reports.py`), the Contact
   Center Secure Data Capture feature, and the admin user. Stop it and run `python seed-live-call.py <App_Data> --reset --secure-capture off --hide-softphone`.
   It gives `admin` a live call from a fake caller (Casey Morgan, +1 212 555 0142) on the Support queue, and turns on Allow agents to pause recording.
2. Start the site with `ASPNETCORE_ENVIRONMENT=Development`, `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9` and `NO_PROXY=localhost,127.0.0.1`.
   Set `VIDEO_INIT_SCRIPT` to `secure-link-overlay.js`: the workspace shows the link with a native browser prompt that a headless recording never
   shows, and the script replaces just that prompt with a dialog of the same text.
3. Record `02-settings`, `03-agent` and `04-customer` in that order (03 writes the one-time link to `secure-link.txt`, which is not committed, and 04
   reads it; the capture link lasts five minutes by default).
4. Seed again with `--secure-capture on` on a site started in the Production environment and record `06-production`.

