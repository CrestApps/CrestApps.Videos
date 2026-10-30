# Messaging: Setting Up Text Messages

**Feature:** `CrestApps.OrchardCore.Omnichannel.Messaging.Sms` · **Length:** 5:59

The administrator's setup of the messaging workspace: **SMS Messaging Channel** in **Tools > Features**, the SMS provider
and its **Webhook URL** (**Settings > Communication > SMS**), a test text (**Tools > Testing > SMS Test**), a number as an
**Omnichannel Address** (**Used for: Text messages (SMS)**, **Provider**, **Agents who text from this number**), the text
**entry point** (**Route to**, **Target queue**, **Queue distribution**, **Auto-reply**, **Closed auto-reply**), the role's
messaging permissions, and the **Base url** for pictures. The video shows the admin only: no code or configuration files.

## Recording it again

It needs the site of `videos/messaging-templates-broadcasts/` (read its README): a fresh Contoso site with the made-up
Twilio provider and number `+14155550100`, the **Support** queue and the **Support texts** entry point, the **Messaging
agent** role, run in the **Development** environment with the **Log** SMS provider as the default. Nothing is changed by
the clips except the **SMS Test** page, which only writes the test text to the log. Record `02-features` to `08-base`.
