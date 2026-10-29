# AI Memory

**Feature:** `CrestApps.OrchardCore.AI.Memory`, with `CrestApps.OrchardCore.AI.Memory.Elasticsearch` (or `.AzureAI`) · **Length:** 5:35

Why memory helps, how the index, the site settings and the profile fit together, and how to create the memory index,
choose it in the settings, turn memory on for a profile, chat with an assistant that remembers, and clear the saved
memory. The video shows the admin only: no code or configuration files. The title, description, tags and chapters
YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only. It uses a **real model** (so the chat answers are real)
and a **local Elasticsearch** (8.x, security off, port 9200), both outside this repository. Nothing about the model's
account is shown: the demo site has a read-only connection and three deployments titled `contoso-...`.

1. Run a local Elasticsearch (single node, security off) on `http://localhost:9200`.
2. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` and run it with `ORCHARD_APP_DATA` pointing at
   an empty folder outside both repositories. Put one connection and its chat and embedding deployments, named
   `contoso-...`, in that folder's `appsettings.json` (they show as **Read-only**), and let the site reach only the
   model's host (`NO_PROXY`), with `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9` for everything else.
3. Set the site up with the **Blank** recipe, site name **Contoso**, SQLite, and an admin account.
4. Choose the default chat, utility and embedding deployments in **Settings > Artificial Intelligence**, and create an AI
   profile titled **Support assistant** (technical name `support-assistant`).
5. Sign the recorder in, enable the features, narrate and record the clips in order (`03-index`, `04-settings`,
   `05-profile`, `06-chat`, `07-clear`): each one builds on the one before.

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/ai-memory/recorder
   export VIDEO_ROOT=<repo>/videos/ai-memory VIDEO_BASE_URL=http://localhost:<port> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   <bin>/Recorder record all
   ```

To record again, first delete the memory index (**Force Delete**), choose no index in **Settings > Artificial
Intelligence**, untick **Enable user memory** on the profile, and clear the saved memory of the signed-in user.
