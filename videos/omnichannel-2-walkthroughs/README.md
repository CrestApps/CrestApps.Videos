# Omnichannel Management 2.0 Walkthroughs

**YouTube:** not uploaded · **Chapters:** 9 · **Kind:** a reel of silent screencasts

Short, silent walkthroughs of CrestApps Omnichannel Management 2.0 for Orchard Core, one screen per chapter: enabling the feature, automatic and manual inventory loads, a lead-generation subject, dispositions, inbound calls, completing activities, and reassigning them in bulk. These show version 2.0; later versions are covered by the Contact Center walkthroughs.

The title, description, tags and chapters YouTube shows are in `video.json`.

## Building it again

The screencasts were recorded for the documentation without narration. `reel.json` lists them by their path in
CrestApps.OrchardCore at commit `ef4a3177a`, the last one that has them, so nothing is recorded or narrated:

```bash
python .agents/skills/crestapps-video-producer/scripts/reel.py videos/omnichannel-2-walkthroughs
```

It downloads the screencasts into `clips/`, writes `omnichannel-2-walkthroughs.mp4` and `thumbnail.jpg`, the chapters into `video.json`,
and each screencast's `start` and `end` (whole seconds) into `reel.json`. A documentation page plays one screencast
with `<YouTube video="omnichannel-2-walkthroughs" title="..." start={start} end={end} />`.

To change a screencast, record it again, point its chapter at the new file, and publish the next version.
