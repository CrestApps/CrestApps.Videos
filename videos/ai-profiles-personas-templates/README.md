# AI Profiles, Personas and Templates

**Feature:** `CrestApps.OrchardCore.AI` (profiles, personas, templates) with `CrestApps.OrchardCore.AI.Personas`, `CrestApps.OrchardCore.AI.Prompting` and `CrestApps.OrchardCore.AI.Chat` · **Length:** 10:51

Why each assistant is described once and its parts are reused, and how to use every screen: starting a profile from the
New AI Profile picker, the cards and tabs of the profile editor, profile types, cloning and the list, personas, profile
templates, and reusable system prompt templates. The video shows the admin only: no code or configuration files. The
title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (a made-up Azure OpenAI address and key that is never
contacted, a "Contoso" site, and made-up names), so it needs no mask script.

1. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` and run it with `ORCHARD_APP_DATA` pointing at
   an empty folder outside both repositories, on `http://localhost:5371`, with `HTTP_PROXY`/`HTTPS_PROXY` set to
   `http://127.0.0.1:9` so nothing reaches a provider.
2. Set the site up with the **Blank** recipe, site name **Contoso**, SQLite, and the user `admin`.
3. Sign the recorder in and run `prepare`, which enables the features (AI Connection Management, OpenAI Azure, AI Chat,
   AI Personas, AI Agent, and AI Prompt Templates as a dependency), adds one made-up connection and deployment, and
   chooses the default chat and utility deployments:

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/ai-profiles-personas-templates/recorder
   export VIDEO_ROOT=<repo>/videos/ai-profiles-personas-templates VIDEO_BASE_URL=http://localhost:5371 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/ai-profiles-personas-templates`) and record the
   clips in order, because each one builds on the last: `03-start` creates the profile that `04-editor` edits,
   `05-manage` adds a utility profile, `06-personas` creates the persona and chooses it in the profile, `07-templates`
   creates the profile template, and `08-prompts` creates the system prompt template and adds it to the profile.
