# AI Data Sources

**Feature:** `CrestApps.OrchardCore.AI.DataSources` (brought by `CrestApps.OrchardCore.AI.DataSources.Elasticsearch`) · **Length:** 5:26

Why large and changing content needs a data source, how the source index, the knowledge base index and the data source
fit together, and how to index your content, create the knowledge base index and a **Search Index Profile** data source,
sync it, choose it on a profile, ask a question and read the sources, and set the site-wide defaults. The video shows the
admin only: no code or configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only: the content is the four **Article** items of
`seed/contoso-articles-recipe.json` (paste it in **Tools > Deployments > JSON Import**). It uses a **real model** (so the
chat answer is real) and a **local Elasticsearch** (8.x, security off, port 9200), both outside this repository. Nothing
about the model's account is shown: the demo site has a read-only connection and three deployments titled `contoso-...`.

1. Run a local Elasticsearch (single node, security off) on `http://localhost:9200`, with enough free disk space for it to
   allocate shards.
2. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` and run it with `ORCHARD_APP_DATA` pointing at
   an empty folder outside both repositories. Put one connection and its chat and embedding deployments, named
   `contoso-...`, in that folder's `appsettings.json`, and let the site reach only the model's host (`NO_PROXY`), with
   `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9` for everything else.
3. Set the site up with the **Blank** recipe, site name **Contoso**, SQLite, and an admin account. Import the articles
   recipe. Choose the default chat, utility and embedding deployments in **Settings > Artificial Intelligence**, turn on
   **Enable preemptive retrieval-augmented generation (RAG)** under **Default Orchestrator**, and create an AI profile
   titled **Support assistant** (technical name `support-assistant`) with no tools or connections.
4. Sign the recorder in, enable the features, narrate and record the clips in order (`03-index`, `04-kb`,
   `05-datasource`, `06-sync`, `07-use`, `08-ask`, `09-defaults`): each one builds on the one before.

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/ai-data-sources/recorder
   export VIDEO_ROOT=<repo>/videos/ai-data-sources VIDEO_BASE_URL=http://localhost:<port> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   <bin>/Recorder record all
   ```

To record again, first delete the data source, then the two indexes (**Force Delete**), and choose no data source on the
profile. Wait for each index to be gone from Elasticsearch before recording the clips again.
