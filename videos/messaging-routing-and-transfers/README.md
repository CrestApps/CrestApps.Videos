# Messaging: Routing and Transfers

**Feature:** `CrestApps.OrchardCore.Omnichannel.Messaging` with `CrestApps.OrchardCore.Omnichannel.Messaging.RoutedDistribution` · **Length:** 4:01

How a queue hands out conversations (shared pool or routed distribution, the **Available** switch, the five-minute pickup
and two re-routes before the shared pool), and how an agent transfers a conversation to a person, with a note, or back to
a queue. The video shows the admin only: no code or configuration files.

## Recording it again

It continues the setup of `videos/messaging-inbox/` (read its README): a fresh Contoso site, a made-up Twilio provider
and number, the **Support** queue and a text entry point, no provider ever reached, and customers' texts simulated with a
signed request to the site's own `/api/twilio/webhook/sms`. In addition:

1. Create a role **Messaging agent** with the permissions **Use the messaging workspace** and **View your own and your
   queues' unclaimed messaging conversations**, give it to the users `jamie.chen` and `maria.lopez`, and add an agent
   entitlement for each with the **Support** queue (the transfer dialog lists only people who are agents).
2. Simulate two new customers' texts (`+14155550116`, `+14155550117`) and record `03-modes`, `04-person` and `05-queue`
   in order: they claim and transfer those two conversations.
