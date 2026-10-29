# Model Context Protocol (MCP)

**Feature:** `CrestApps.OrchardCore.AI.Mcp` (MCP client), `.Stdio` (local client), `.Server` (MCP server), and the FTP and SFTP resource features · **Length:** 7:08

Why a common standard for AI tools helps, how the MCP client and server fit together, and how to add a connection to a
server on the web and to a local program, choose how a connection signs in, give an assistant the tools of a connection,
share the site's tools with the MCP Server settings, and share prompts. The video shows the admin only: no
code or configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (made-up server addresses and keys that are never
contacted), so it needs no mask script.

1. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` and run it with `ORCHARD_APP_DATA` pointing at
   an empty folder outside both repositories, with `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9` so nothing
   leaves the machine.
2. Set the site up with the **Blank** recipe, site name **Contoso**, SQLite, and an admin account.
3. Create one AI profile titled **Support assistant** (technical name `support-assistant`), so the `06-use` clip has a
   profile to give the connection to.
4. Sign the recorder in and enable the features (`Recorder prepare` enables the ones the clips need):

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/ai-mcp/recorder
   export VIDEO_ROOT=<repo>/videos/ai-mcp VIDEO_BASE_URL=http://localhost:<port> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

5. Narrate (`scripts/narrate.py videos/ai-mcp`) and record the clips in order: `03-connection` and `04-local` create the
   two connections that `06-use` lists, and `07-server` and `08-prompts` save what they show. To record
   one of those again, first delete what it created (the connections and the prompt) and clear the MCP Server
   settings, so the screens start empty.
