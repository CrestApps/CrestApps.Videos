# AI Walkthroughs: Providers, Chat, Knowledge, Agents and Widgets

**YouTube:** not uploaded · **Chapters:** 16 · **Kind:** a reel of silent screencasts

Short, silent walkthroughs of the CrestApps AI modules for Orchard Core, one screen per chapter: connecting a provider, chat profiles, chat interactions and documents, templates, knowledge from your content, tools and agents, MCP, chat widgets, workflows and analytics.

The title, description, tags and chapters YouTube shows are in `video.json`.

## Building it again

The screencasts were recorded for the documentation without narration. `reel.json` lists them by their path in
CrestApps.OrchardCore at commit `ef4a3177a`, the last one that has them, so nothing is recorded or narrated:

```bash
python .agents/skills/crestapps-video-producer/scripts/reel.py videos/ai-walkthroughs
```

It downloads the screencasts into `clips/`, writes `ai-walkthroughs.mp4` and `thumbnail.jpg`, the chapters into `video.json`,
and each screencast's `start` and `end` (whole seconds) into `reel.json`. A documentation page plays one screencast
with `<YouTube video="ai-walkthroughs" title="..." start={start} end={end} />`.

To change a screencast, record it again, point its chapter at the new file, and publish the next version.
