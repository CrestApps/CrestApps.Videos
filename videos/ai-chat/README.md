# AI Chat

**Feature:** `CrestApps.OrchardCore.AI.Chat` (chat page, admin chat widget, website chat widget) with `CrestApps.OrchardCore.AI.Chat.Claude` and `CrestApps.OrchardCore.AI.Chat.Copilot` · **Length:** 8:05

Why every assistant needs a chat, and how to use every screen: opening a chat and asking follow-up questions, finding
and managing past chats, the admin chat widget, the website chat widget on a layer, the chat engines (default
orchestrator, Claude and GitHub Copilot), and the protections of a public assistant. The video shows the admin only:
no code or configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only ("Contoso", a made-up bicycle shop, made-up questions),
and a real model behind the chat so the answers are real. The model's connection comes from the demo site's own
`App_Data/appsettings.json` (never committed), so its name is masked: a mask script (kept outside the repository, set in
`VIDEO_INIT_SCRIPT`) replaces the names of the configured deployments on every page. The video never opens the
connections or deployments lists.

1. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` and run it with `ORCHARD_APP_DATA` pointing at
   an empty folder outside both repositories, on `http://localhost:5371`, with `HTTP_PROXY`/`HTTPS_PROXY` set to
   `http://127.0.0.1:9` and `NO_PROXY` listing only `localhost`, `127.0.0.1` and the model provider's host.
2. Put one connection and its deployments in that folder's `appsettings.json`, and set the site up with the **Blank**
   recipe, site name **Contoso**, SQLite, and the user `admin`.
3. Sign the recorder in and run `prepare`, which enables the features (AI Chat, the admin widget, chat analytics, the
   Claude and Copilot orchestrators, personas, widgets and layers), chooses the default deployments, makes **The Theme**
   current with a **Footer** zone and an **Always** layer (a Boolean rule set to true), creates the **Contoso website
   assistant** profile (from the Website assistant starting point, with the shop's facts as its instructions), and asks
   it two questions so the history has entries:

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/ai-chat/recorder
   export VIDEO_ROOT=<repo>/videos/ai-chat VIDEO_BASE_URL=http://localhost:5371 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password> VIDEO_INIT_SCRIPT=<mask.js>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/ai-chat`) and record the clips in order:
   `05-widget` chooses the admin widget's profile, and `06-website` adds the website widget. `03-chat`, `04-history`,
   `07-engines` and `08-protect` save nothing that a later clip needs.
