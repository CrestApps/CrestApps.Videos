# AI Connections and Deployments

**Feature:** `CrestApps.OrchardCore.AI.ConnectionManagement` and the provider features (`CrestApps.OrchardCore.OpenAI.Azure`, `CrestApps.OrchardCore.OpenAI`, `CrestApps.OrchardCore.Ollama`) · **Length:** 5:56

Why the AI provider is set up once, how connections, deployments and default models fit together, and how to add a
connection, add chat and embedding deployments, tick model capabilities (including reasoning effort), and choose the
default chat, utility and embedding models. The video shows the admin only: no code or configuration files. The title,
description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (a made-up Azure OpenAI address and key that is never
contacted), so it needs no mask script.

1. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` and run it with `ORCHARD_APP_DATA` pointing at
   an empty folder outside both repositories, on `http://localhost:5368`, with `HTTP_PROXY`/`HTTPS_PROXY` set to
   `http://127.0.0.1:9` so nothing reaches a provider.
2. Give that folder's `appsettings.json` one read-only connection and deployment under `OrchardCore:CrestApps:AI`
   (`contoso-shared`, titled "Contoso shared account", and `contoso-chat`), so the lists show the **Read-only** entries.
3. Set the site up with the **Blank** recipe, site name **Contoso**, SQLite, and a generated admin password.
4. Sign the recorder in and enable the provider features:

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/ai-connections-deployments/recorder
   export VIDEO_ROOT=<repo>/videos/ai-connections-deployments VIDEO_BASE_URL=http://localhost:5368 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<generated>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

5. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/ai-connections-deployments`) and record the
   clips in order: `03-connection` creates the connection that `04-deployment` uses, and `04-deployment` creates the
   deployments that `06-defaults` selects. `05-capabilities` saves nothing.
