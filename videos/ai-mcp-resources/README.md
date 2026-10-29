# MCP Resources

**Feature:** `CrestApps.OrchardCore.AI.Mcp.Server` with the FTP and SFTP resource features (`CrestApps.OrchardCore.AI.Mcp.Resources.Ftp`, `.Sftp`) · **Length:** 4:32

Why resources keep AI apps working from current files, the resource types, and how to add a File, a Media, an SFTP and
an FTP resource. The video shows the admin only: no code or configuration files. The title, description, tags and
chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (made-up file server addresses and passwords that are
never contacted), so it needs no mask script.

1. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` and run it with `ORCHARD_APP_DATA` pointing at
   an empty folder outside both repositories, with `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9`.
2. Set the site up with the **Blank** recipe, site name **Contoso**, SQLite, and an admin account.
3. Sign the recorder in and enable the features (`Recorder prepare` enables them):

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/ai-mcp-resources/recorder
   export VIDEO_ROOT=<repo>/videos/ai-mcp-resources VIDEO_BASE_URL=http://localhost:<port> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

4. Narrate (`scripts/narrate.py videos/ai-mcp-resources`) and record the clips in order (`02-types`, `03-file`,
   `04-media`, `05-remote`): each one adds a resource, so start from an empty **Resources** list. To record again,
   delete the resources first.
