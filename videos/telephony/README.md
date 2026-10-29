# Telephony

**Feature:** `CrestApps.OrchardCore.Telephony` with `CrestApps.OrchardCore.Telephony.SoftPhone`, `CrestApps.OrchardCore.Telephony.SoftPhone.Extension`, `CrestApps.OrchardCore.Telnyx` and `CrestApps.OrchardCore.Asterisk` (and `CrestApps.OrchardCore.Telephony.Azure`, which has no screen) · **Length:** see `video.json`

Why a phone in the browser, how providers, the default provider, the soft phone and extensions fit together, and how to
turn the features on, connect Asterisk and Telnyx, choose the default provider and the soft phone options, give people
extensions, use the soft phone (keypad, extension dialing, recent calls, voicemail) and give roles the permissions. The
video shows the admin only: no code or configuration files. The title, description, tags and chapters YouTube shows are
in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only: made-up users (`jamie.chen`, `maria.lopez`, `sam.patel`),
fake 555 numbers, and a made-up Asterisk address, password and Telnyx key that are never contacted, so it needs no mask
script. Calls never leave the machine.

1. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` and run it with `ORCHARD_APP_DATA` pointing at
   an empty folder outside both repositories, on `http://localhost:5373`, with `HTTP_PROXY`/`HTTPS_PROXY` set to
   `http://127.0.0.1:9` and `NO_PROXY=localhost,127.0.0.1`, so nothing reaches a phone provider.
2. Set the site up with the **Blank** recipe, site name **Contoso**, SQLite, and the admin user `admin`.
3. Sign the recorder in, enable the features and create the fake users:

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/telephony/recorder
   export VIDEO_ROOT=<repo>/videos/telephony VIDEO_BASE_URL=http://localhost:5373 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/telephony`) and record the clips in order:
   `04-asterisk` saves the Asterisk settings (which makes Asterisk the default provider), `05-telnyx` saves a Telnyx key,
   `06-defaults` saves the soft phone settings, `07-extensions` creates two extensions that `08-softphone` then looks up
   by name, and `09-permissions` saves nothing. `03-features` saves nothing either.
