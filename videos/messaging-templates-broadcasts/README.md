# Messaging: Templates and Broadcasts

**Feature:** `CrestApps.OrchardCore.Omnichannel.Messaging` · **Length:** 5:32

Message templates (canned responses) and how an agent inserts one into a reply, then broadcasts: the form, the status
that goes from **Queued** to **Completed**, the sent and failed counts, the retries, what a broadcast skips (quiet hours,
do-not-call lists), and the STOP, START and HELP keywords. The video shows the admin only: no code or configuration files.

## Recording it again

It continues the setup of `videos/messaging-inbox/` (read its README): a fresh Contoso site, a made-up Twilio provider
and number, the **Support** queue and a text entry point, customers' texts simulated with a signed request to the site's
own `/api/twilio/webhook/sms`. In addition:

1. Run the site in the **Development** environment and set **Settings > Communication > SMS** to the **Log** provider,
   which only writes a message to the log, so replies and broadcasts are "sent" without a real provider.
2. Run the **Omnichannel CRM starter** recipe (**Configuration > Recipes**) for the **Contact** type, and add five
   contacts with made-up numbers `+14155550131` to `+14155550135` (a **Phone Number** in **Contact methods**).
3. Simulate the texts: `+14155550135` sends `STOP`, then `START`, then `STOP` again; `+14155550133` asks when the
   shop opens.
4. Before the broadcast clip, queue a first broadcast and note the second of the minute at which it completes. A queued
   broadcast is sent by a sweep that runs at the same second of every minute, and `04-broadcast` waits for
   `VIDEO_SWEEP_SECOND` so that the list shows **Completed** without a long wait. Record `02-template`, `03-insert`,
   `04-broadcast` and `06-stop` in that order.
