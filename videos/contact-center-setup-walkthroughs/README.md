# Contact Center Setup Walkthroughs: Numbers, Queues, IVR and Campaigns

**YouTube:** not uploaded · **Chapters:** 28 · **Kind:** a reel of silent screencasts

Short, silent walkthroughs of setting up the CrestApps Contact Center for Orchard Core, one screen per chapter: phone and SMS providers, numbers and extensions, business hours, queues, skills, entry points and IVR menus, subjects and their flows, campaigns, dialer profiles, cadences, activity loads and workflows.

The title, description, tags and chapters YouTube shows are in `video.json`.

## Building it again

The screencasts were recorded for the documentation without narration. `reel.json` lists them by their path in
CrestApps.OrchardCore at commit `ef4a3177a`, the last one that has them, so nothing is recorded or narrated:

```bash
python .agents/skills/crestapps-video-producer/scripts/reel.py videos/contact-center-setup-walkthroughs
```

It downloads the screencasts into `clips/`, writes `contact-center-setup-walkthroughs.mp4` and `thumbnail.jpg`, the chapters into `video.json`,
and each screencast's `start` and `end` (whole seconds) into `reel.json`. A documentation page plays one screencast
with `<YouTube video="contact-center-setup-walkthroughs" title="..." start={start} end={end} />`.

To change a screencast, record it again, point its chapter at the new file, and publish the next version.
