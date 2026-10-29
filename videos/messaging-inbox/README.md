# The Messaging Inbox

**Feature:** `CrestApps.OrchardCore.Omnichannel.Messaging` with `CrestApps.OrchardCore.Omnichannel.Messaging.Sms` · **Length:** 5:43

Why a shared inbox beats one person's phone, and how to use every part of it: the list, the conversation, the composer
and the Customer card, searching and filtering, claiming and replying, closing, marking spam and reopening, favorites
and attachments, the badges and notices when new work arrives, and starting a conversation. The video shows the admin
only: no code or configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only ("Contoso", made-up 555 numbers and texts). No SMS
provider is ever reached: the site runs with `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9`, a Twilio provider
configured with made-up credentials, and customers' texts are simulated by posting a Twilio-shaped, correctly signed
request (signed with the made-up auth token) to the site's own `/api/twilio/webhook/sms` address. A mask script (kept
outside the repository, set in `VIDEO_INIT_SCRIPT`) removes the notice about a text the provider could not send.

1. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` and run it with `ORCHARD_APP_DATA` pointing at
   an empty folder outside both repositories, on `http://localhost:5371`, with the dead proxy above.
2. Set the site up with the **Blank** recipe, site name **Contoso**, SQLite, the user `admin`, and the features of the
   Contact Center and Omnichannel Messaging (SMS Messaging Channel brings the rest).
3. Under **Settings > Communication > SMS** enable the **Twilio** provider with a valid but made-up number such as
   `+14155550100` (an invalid format is refused), a made-up account SID and token, and choose it as the default provider.
4. Add an **Omnichannel Address** with that number used for **Text messages (SMS)**, a queue named **Support**, a text
   **Inbound entry point** that routes the number to the queue (shared pool), and give the `admin` agent entitlement the
   queue. Open **Messaging > Inbox** once.
5. Simulate four customers' texts (`+14155550111` to `+14155550114`), then record the clips in order with the recorder
   (`dotnet build -p:VideoScenes=<repo>/videos/messaging-inbox/recorder`, then `Recorder setup` and `Recorder record <clip>`):
   `03-open`, `04-reply` (claims and answers one conversation), `05-close`, `06-favorites`, `07-notice` (simulates one more
   text while recording) and `08-new`. Restore the site's `App_Data` before a full take.
