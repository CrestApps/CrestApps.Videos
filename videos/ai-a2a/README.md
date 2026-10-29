# Agent-to-Agent (A2A)

**Feature:** `CrestApps.OrchardCore.AI.A2A` (client) and `CrestApps.OrchardCore.AI.A2A.Host`, with `CrestApps.OrchardCore.AI.Agent` · **Length:** 5:16

Why specialist agents help, how Agent profiles, the A2A Host and the A2A Client fit together, and how to create an Agent
profile, how the host offers it, how to add an A2A connection, give an assistant the connection, and ask a specialist a
question. The video shows the admin only: no code or configuration files. The title, description, tags and chapters
YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only. It uses a **real model** (so the chat answer is real),
outside this repository. Nothing about the model's account is shown: the demo site has a read-only connection and three
deployments titled `contoso-...`. The "other system" is the same site: its A2A host answers on this machine.

1. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` and run it with `ORCHARD_APP_DATA` pointing at
   an empty folder outside both repositories. Put one connection and its chat and embedding deployments, named
   `contoso-...`, in that folder's `appsettings.json`, and let the site reach only the model's host and `localhost`
   (`NO_PROXY`), with `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9` for everything else.
2. In that same `appsettings.json`, configure the A2A host for the demo (technical team work, never shown in the video):
   `OrchardCore:CrestApps:AI:A2AHost` with `AuthenticationType` `None`, `RequireAccessPermission` `false` and
   `ExposeAgentsAsSkill` `true`. The A2A client reads the skills of an agent card, which only that mode lists.
3. Set the site up with the **Blank** recipe, site name **Contoso**, SQLite, and an admin account. Choose the default chat
   and utility deployments in **Settings > Artificial Intelligence**, and create an AI profile titled **Support
   assistant** (technical name `support-assistant`) with no tools or connections.
4. Sign the recorder in, enable the features, narrate and record the clips in order (`03-agent`, `05-connection`,
   `06-use`, `07-ask`): each one builds on the one before. The connection's endpoint ends with `/a2a`, where the host
   answers.

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/ai-a2a/recorder
   export VIDEO_ROOT=<repo>/videos/ai-a2a VIDEO_BASE_URL=http://localhost:<port> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   <bin>/Recorder record all
   ```

To record again, first delete the **Warranty Agent** profile and the A2A connection, and untick them on the profile.
