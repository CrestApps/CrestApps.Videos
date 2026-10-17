# CrestApps Commerce: Payments, Subscriptions, Payment Plans and Invoices

**YouTube:** not uploaded · **Length:** 10:30

The CrestApps Commerce modules let an Orchard Core site take payments: sell subscriptions online, set up payment plans for your customers, send invoices and receipts, and keep track of every payment.

The title, description, tags and chapters YouTube shows are in `video.json`; `commerce.vtt` and `commerce.srt`
are its captions.

## Where the video comes from

This video was made before this repository existed, with an earlier version of the `crestapps-video-producer` skill,
and its storyboard and recorder scenes weren't kept. The published file is the one the documentation used to carry:
`src/CrestApps.Docs/static/img/docs/commerce.mp4` in CrestApps.OrchardCore at commit `ef4a3177a`. To publish it:

```bash
curl -L -o commerce.mp4 https://raw.githubusercontent.com/CrestApps/CrestApps.OrchardCore/ef4a3177a444a0232c400cc1082b158d1fc2bb29/src/CrestApps.Docs/static/img/docs/commerce.mp4
python tools/youtube/youtube.py release commerce --file commerce.mp4
```

The documentation used to show each section as its own short video. It now embeds this one, and plays a section with
`start` and `end`; `sections.json` lists them, in whole seconds.

To make it again, write a storyboard for it in this folder and follow the skill: it is then the next version.
