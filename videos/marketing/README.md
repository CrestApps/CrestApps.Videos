# Marketing

**Feature:** `CrestApps.OrchardCore.Marketing` · **Length:** see `video.json`

Why your orders are the best audience, segments (the rules about what customers bought), campaigns (write, test, send now or
later, and how many were sent, skipped and failed), consent and the one-click unsubscribe, and the ready-made automations that
email for you. The video shows the admin and the storefront only: no code or configuration files. The title, description, tags
and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (`example.com` addresses, made-up names). No email leaves the
machine: the site's SMTP settings point at a local sink that writes each message to a file, which the `05-consent` clip reads to
find the unsubscribe link of the test email the `04-campaigns` clip sent.

1. Make the site the Point of Sale video describes (its steps 1 to 4: the **Online store** recipe and the sample store), and the
   fake customers the Customers and Companies video makes, with a few paid orders (see the Point of Sale and Reviews videos).
2. Start a local SMTP sink on port 2525 that saves each message to a folder, and set `MailFolder` in
   `recorder/Scenes.Clips.cs` to that folder.
3. Sign the recorder in, then prepare the site:

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/marketing/recorder
   export VIDEO_ROOT=<repo>/videos/marketing VIDEO_BASE_URL=http://localhost:5375 VIDEO_USER_NAME=admin VIDEO_PASSWORD=Password1!
   <bin>/Recorder setup
   <bin>/Recorder prepare     # turns on Marketing and SMTP, ticks Accepts marketing on the customers, adds two segments
   ```
4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/marketing`) and record the clips. `04-campaigns` sends a
   campaign and `05-consent` unsubscribes a customer, so restore the copy before recording either again (record `04-campaigns`
   with a long timeout: it waits for the campaign to finish sending).
