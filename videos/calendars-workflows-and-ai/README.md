# Calendars in Workflows and AI

**YouTube:** not published yet · **Length:** 4:42 · **Feature:** `CrestApps.OrchardCore.Calendars`

The Orchard Core calendar adds events and tasks to the workflow editor, and six calendar tools to AI profiles. Workflows can react to calendar changes and create, move, find and cancel events, and AI assistants can read your calendar, find free time and book for you, with your access.

Chapters:

- 0:00 Introduction
- 0:21 Why connect them
- 1:01 Calendar activities in the workflow editor
- 2:09 Run it
- 2:43 The AI assistant
- 3:59 Good to know
- 4:30 Wrap-up

The title, description, tags and chapters YouTube shows are in `video.json`; `calendars-workflows-and-ai.vtt` and `calendars-workflows-and-ai.srt` are its captions.

## Recording it again

It was recorded on a **fresh local site** with fake data only (no mask script): CrestApps.OrchardCore `main`
(`src/Startup/CrestApps.OrchardCore.Cms.Web`), the Blog setup recipe, site name Contoso Studio, SQLite, behind a dead
proxy (`HTTP_PROXY`/`HTTPS_PROXY` = `http://127.0.0.1:9`). Enable the Calendars features, Workflows, and the AI features (`CrestApps.OrchardCore.AI`, `.AI.ConnectionManagement`, `.OpenAI.Azure`, `.AI.Chat`, `.AI.Agent`, `.AI.Personas`); add a made-up Azure OpenAI connection and a `gpt-4.1-mini` deployment (nothing contacts them) and an AI profile **Calendar assistant**; import `seed-workflow.json` with Import/Export, Import JSON (the workflow *Plan cover for time off*). The chat conversation is a slide: no AI provider is called, so the tool list is the real UI and the conversation is an illustration.

Sign the recorder in, narrate, record every clip, then assemble:

```bash
python .agents/skills/crestapps-video-producer/scripts/narrate.py videos/calendars-workflows-and-ai
Recorder setup && Recorder record all   # VIDEO_ROOT=videos/calendars-workflows-and-ai, VIDEO_BASE_URL=<site>
python .agents/skills/crestapps-video-producer/scripts/assemble.py videos/calendars-workflows-and-ai
```

Restore the site's `App_Data` from the copy made before the first take before every full take.
