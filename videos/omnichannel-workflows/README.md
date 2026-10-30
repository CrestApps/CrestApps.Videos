# Omnichannel Workflows

**Feature:** `CrestApps.OrchardCore.ContactCenter` with `OrchardCore.Workflows`, plus the Omnichannel CRM and SMS messaging features · **Length:** see `video.json`

Workflows for the contact center and the CRM: creating a workflow, the Contact Center Event and its Event type list, the
tasks of the Contact Center category (Place Call or Send Message, Set Agent Presence, Find Agent Numbers, Enqueue
Activity, Hand Off to Live Agent, Schedule Callback, Schedule Dialer Retry, Start and Stop Call Recording, Send Text
Message), the CRM event Lead Converted and task Convert Lead, how each task ends, and the runs of a workflow. The video
shows the admin only: no code or configuration files. The title, description, tags and chapters YouTube shows are in
`video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only: the made-up admin user, fake 555 numbers and a made-up
Asterisk address that is never contacted, so it needs no mask script. No call or text ever leaves the machine, so the
workflow that needs a real dialer call is built but not run: the Runs chapter uses the event Agent presence changed,
which the soft phone raises.

1. Run CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` with `ORCHARD_APP_DATA` pointing at an empty
   folder outside both repositories, with `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9` and
   `NO_PROXY=localhost,127.0.0.1`, so nothing reaches a phone provider.
2. Set the site up with the **Blank** recipe, site name **Contoso**, SQLite and the admin user `admin`, then enable the
   Contact Center, Omnichannel (management, messaging, SMS, voice, CRM), Telephony features and **Workflows**
   (`OrchardCore.Workflows`), and run the recipe `OmnichannelCrmStarter`. Restart the site after enabling features.
3. Build the recorder with this video's scenes, sign in, seed the site, and record the clips in order:

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/omnichannel-workflows/recorder
   export VIDEO_ROOT=<repo>/videos/omnichannel-workflows VIDEO_BASE_URL=http://localhost:5396
   <bin>/Recorder setup
   <bin>/Recorder prepare
   <bin>/Recorder record 02-create 03-event 04-picker 04-sms 05-forms 06-crm 08-runs
   ```

`prepare` creates the workflow **Look up agent numbers** (the event Agent presence changed, then Find Agent Numbers),
the one the Runs chapter opens. The clips build **Text customers the dialer could not reach** on camera, so record
`02-create`, `03-event`, `04-picker` and `04-sms` in that order, from a site that does not have it yet. The settings
tour (`05-forms`) and the CRM clip only open the forms and cancel. `08-runs` sets the presence of the signed-in admin to
Available in the soft phone, which starts the runs it lists. The two task forms Place Call or Send Message and Hand Off
to Live Agent need the fix that makes their view models non-sealed (they showed only a Title field before it).
