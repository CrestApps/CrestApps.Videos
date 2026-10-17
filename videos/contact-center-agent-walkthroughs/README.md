# Contact Center Agent Walkthroughs: Calls, Messages and Activities

**YouTube:** not uploaded · **Chapters:** 23 · **Kind:** a reel of silent screencasts

Short, silent walkthroughs of a day in the CrestApps Contact Center for Orchard Core, one screen per chapter: the agent workspace and soft phone, answering and placing calls, voicemail, contacts and activities, the messaging inbox, reviewing an AI conversation, and the supervisor's bulk actions, live dashboard and reports.

The title, description, tags and chapters YouTube shows are in `video.json`.

## Building it again

The screencasts were recorded for the documentation without narration. `reel.json` lists them by their path in
CrestApps.OrchardCore at commit `ef4a3177a`, the last one that has them, so nothing is recorded or narrated:

```bash
python .agents/skills/crestapps-video-producer/scripts/reel.py videos/contact-center-agent-walkthroughs
```

It downloads the screencasts into `clips/`, writes `contact-center-agent-walkthroughs.mp4` and `thumbnail.jpg`, the chapters into `video.json`,
and each screencast's `start` and `end` (whole seconds) into `reel.json`. A documentation page plays one screencast
with `<YouTube video="contact-center-agent-walkthroughs" title="..." start={start} end={end} />`.

To change a screencast, record it again, point its chapter at the new file, and publish the next version.
