# Site Administration Walkthroughs: Fields, Roles, Users and Imports

**YouTube:** not uploaded · **Chapters:** 9 · **Kind:** a reel of silent screencasts

Short, silent walkthroughs of the CrestApps site administration features for Orchard Core, one screen per chapter: the phone field, role pickers and restricted content, user display names, time zones, bulk import and export, and Do Not Call lists.

The title, description, tags and chapters YouTube shows are in `video.json`.

## Building it again

The screencasts were recorded for the documentation without narration. `reel.json` lists them by their path in
CrestApps.OrchardCore at commit `ef4a3177a`, the last one that has them, so nothing is recorded or narrated:

```bash
python .agents/skills/crestapps-video-producer/scripts/reel.py videos/site-administration-walkthroughs
```

It downloads the screencasts into `clips/`, writes `site-administration-walkthroughs.mp4` and `thumbnail.jpg`, the chapters into `video.json`,
and each screencast's `start` and `end` (whole seconds) into `reel.json`. A documentation page plays one screencast
with `<YouTube video="site-administration-walkthroughs" title="..." start={start} end={end} />`.

To change a screencast, record it again, point its chapter at the new file, and publish the next version.
