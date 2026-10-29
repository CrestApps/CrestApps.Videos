# Report subscriptions

**Feature:** `CrestApps.OrchardCore.Reports.Subscriptions` (needs `CrestApps.OrchardCore.Reports` and `OrchardCore.Email`) · **Length:** see `video.json`

Why a scheduled report, the three schedules (every day for the day before, every week for the 7 days before, every month
for the month before), turning the feature and the **Manage report subscriptions** permission on, the email
prerequisite (**Settings > Email** says *No email providers are currently enabled* until a provider is enabled and
configured), adding a subscription, **Save and send now**, how a failed send is shown, pausing, searching and deleting,
and why Report Builder reports cannot be subscribed to (a workflow's Email Report task sends them). The video shows the
admin only: no code or configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

## What is real and what is not

The demo site has **no mail server**, so no email is ever sent. The video does not fake one: the SMTP provider is
configured with a made-up host (`mail.contoso.example`), **Save and send now** fails, and the screens show the real
error (*The report wasn't emailed: ... No such host is known.* on the subscription's page, and *Last run didn't go out* on
its row in the list). The email's content is described in the narration and the slides, never shown.

## Recording it again

1. Start from the site of `videos/reports-run-and-export` (fresh Contoso site, the Reports features, the CRM starter
   recipe, the fake data of `seed-reports-data.py` in that folder, and its **Report viewer** role). Nothing else is
   prepared: `02-features` turns on Report Subscriptions (and Email with it) on camera, `03-email` turns on the SMTP provider
   and saves its settings. Run the site behind a dead proxy so nothing leaves the machine.
2. Record the clips in order, from the site as it was: `02-features`, `02b-roles`, `03-email`, then copy `App_Data`
   aside, then `04-add`, `05-send`, `06-manage` (they create and change the two subscriptions, so restore the copy
   before recording them again).
