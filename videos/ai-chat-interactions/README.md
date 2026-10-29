# AI Chat Interactions

**Feature:** `CrestApps.OrchardCore.AI.Chat.Interactions` · **Length:** 6:30

Why a chat interaction saves the setup of a profile, and how to use every screen: starting an interaction, the
Settings tab (model, persona, instructions, fine-tuning), the Capabilities tab (tools), reopening, copying, clearing
and deleting interactions, and the site settings behind the voice options. The video shows the admin only: no code or
configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only ("Contoso", made-up questions), and a real model behind
the chat so the answers are real. The model's connection comes from the demo site's own `App_Data/appsettings.json`
(never committed), so its name is masked: a mask script (kept outside the repository, set in `VIDEO_INIT_SCRIPT`)
replaces the names of the configured deployments on every page. The video never opens the connections or deployments
lists.

1. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` and run it with `ORCHARD_APP_DATA` pointing at
   an empty folder outside both repositories, on `http://localhost:5371`, with `HTTP_PROXY`/`HTTPS_PROXY` set to
   `http://127.0.0.1:9` and `NO_PROXY` listing only `localhost`, `127.0.0.1` and the model provider's host.
2. Put one connection and its deployments in that folder's `appsettings.json`, and set the site up with the **Blank**
   recipe, site name **Contoso**, SQLite, and the user `admin`.
3. Sign the recorder in and run `prepare`, which enables the features (AI Chat, AI Chat Interactions and the features
   of the AI chat video), chooses the default deployments, and hides the admin chat widget (it would cover the send
   button). Then create two interactions by hand, **Email to a customer** and **Repair checklist**, each with one
   question, so the list has entries to search and delete.

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/ai-chat-interactions/recorder
   export VIDEO_ROOT=<repo>/videos/ai-chat-interactions VIDEO_BASE_URL=http://localhost:5371 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password> VIDEO_INIT_SCRIPT=<mask.js>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/ai-chat-interactions`) and record the clips in
   order: `03-start` creates the **Draft a product description** interaction, `04-settings` gives it instructions, and
   `05-capabilities` ticks a tool. `06-manage` and `07-settings` save nothing.
