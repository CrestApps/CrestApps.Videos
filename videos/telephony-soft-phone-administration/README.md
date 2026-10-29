# Soft Phone Administration: Provider, Diagnostics and Click to Dial

**Feature:** `CrestApps.OrchardCore.Telephony.SoftPhone` · **Length:** see `video.json`

The Soft Phone tab, the widget and its tabs, the diagnostics tab and click to dial.
The video shows the admin only: no code or configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The site is the Contact Center demo site (see `../contact-center-reports/README.md`) with the Telephony, Asterisk and Telnyx features and the
Asterisk provider set up with a fake address (`pbx.contoso.example`), so nothing reaches a phone provider and a call cannot connect.
Run it with `ORCHARD_APP_DATA` pointing at a folder outside both repositories, `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9` and
`NO_PROXY=localhost,127.0.0.1`, run `Recorder setup`, and record the clips named in `storyboard.json`.

The click to dial clip needs the CRM starter recipe (it adds the Contact content type); the first clip turns the diagnostics tab on and saves.
