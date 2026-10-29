# AI File Sources

**Feature:** `CrestApps.OrchardCore.AI.FileSources`, with `.Ftp` and `.Sftp` and a data source feature such as `CrestApps.OrchardCore.AI.DataSources.Elasticsearch` · **Length:** 6:08

Why uploads go stale, how a knowledge base index, a **Files** data source and a file source fit together, and how to
create each one, read the folder now, read the last run, set up the FTP and SFTP connectors, give an assistant the data
source, and ask a question that the files answer, with sources. The video shows the admin only: no code or
configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only: the three files of `seed/handbooks` are copied into
the tenant's `file-sources/handbooks` folder (`App_Data/Sites/Default/file-sources/handbooks` of the site). It uses a
**real model** (so the chat answer is real) and a **local Elasticsearch** (8.x, security off, port 9200), both outside
this repository. Nothing about the model's account is shown: the demo site has a read-only connection and three
deployments titled `contoso-...`.

1. Run a local Elasticsearch (single node, security off) on `http://localhost:9200`.
2. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` and run it with `ORCHARD_APP_DATA` pointing at
   an empty folder outside both repositories. Put one connection and its chat and embedding deployments, named
   `contoso-...`, in that folder's `appsettings.json`, and let the site reach only the model's host (`NO_PROXY`), with
   `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9` for everything else.
3. Set the site up with the **Blank** recipe, site name **Contoso**, SQLite, and an admin account. Choose the default chat,
   utility and embedding deployments in **Settings > Artificial Intelligence**, and create an AI profile titled **Support
   assistant** (technical name `support-assistant`) with no tools or connections. Copy `seed/handbooks` into the
   site's `file-sources` folder.
4. Sign the recorder in, enable the features, narrate and record the clips in order (`03-index`, `04-datasource`,
   `05-source`, `06-run`, `07-remote`, `08-use`, `09-ask`): each one builds on the one before.

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/ai-file-sources/recorder
   export VIDEO_ROOT=<repo>/videos/ai-file-sources VIDEO_BASE_URL=http://localhost:<port> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   <bin>/Recorder record all
   ```

To record again, first delete the file source, the data source and the knowledge base index (**Force Delete**, and delete
its Elasticsearch index if it is still there), and choose no data source on the profile.
