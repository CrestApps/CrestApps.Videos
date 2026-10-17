# Tenant Hierarchy: Parent Tenants That Create and Open Their Own Sites

**YouTube:** not uploaded · **Length:** 8:53

The CrestApps Tenant Hierarchy lets one Orchard Core tenant create and run its own child tenants, and lets its people open them without signing in again.

The title, description, tags and chapters YouTube shows are in `video.json`; `tenant-hierarchy.vtt` and `tenant-hierarchy.srt`
are its captions.

## Where the video comes from

This video was made before this repository existed, with an earlier version of the `crestapps-video-producer` skill,
and its storyboard and recorder scenes weren't kept. The published file is the one the documentation used to carry:
`src/CrestApps.Docs/static/img/docs/tenant-hierarchy.mp4` in CrestApps.OrchardCore at commit `ef4a3177a`. To publish it:

```bash
curl -L -o tenant-hierarchy.mp4 https://raw.githubusercontent.com/CrestApps/CrestApps.OrchardCore/ef4a3177a444a0232c400cc1082b158d1fc2bb29/src/CrestApps.Docs/static/img/docs/tenant-hierarchy.mp4
python tools/youtube/youtube.py release tenant-hierarchy --file tenant-hierarchy.mp4
```

The documentation used to show each section as its own short video. It now embeds this one, and plays a section with
`start` and `end`; `sections.json` lists them, in whole seconds.

To make it again, write a storyboard for it in this folder and follow the skill: it is then the next version.
