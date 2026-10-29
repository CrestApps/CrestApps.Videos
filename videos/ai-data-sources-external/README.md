# External Data Sources

**Feature:** `CrestApps.OrchardCore.AI.DataSources` (with `...DataSources.Elasticsearch`, `...DataSources.AzureAI` and `...DataSources.PostgreSQL`) · **Length:** see `video.json`

Why a data source can read knowledge where it already lives, what every source shares (a connection, a destination
index and a field mapping), and how to create an **Elasticsearch** data source, ask a question that its remote index
answers, and fill in the **Azure AI Search** and **PostgreSQL** data source forms. The video shows the admin only: no
code or configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

It continues [`ai-data-sources`](../ai-data-sources), which covers the **Search Index Profile** data source, the
knowledge base index, the sync, the profile settings and the site-wide defaults.

## Recording it again

The video was recorded on the same **fresh site** as `ai-data-sources`, with fake data only: the remote index is a local
Elasticsearch index named `support-notes` with the four notes of `seed/support-notes.ndjson` (fields `id`, `title` and
`body`). It uses a **real model** (so the chat answer is real) and a **local Elasticsearch** (8.x, security off, port
9200), both outside this repository. Nothing about the model's account is shown: the demo site has a read-only
connection and three deployments titled `contoso-...`. The Azure AI Search and PostgreSQL clips only fill in the forms
(the endpoint is a made-up host, the connection string is left empty), and never save them.

1. Run a local Elasticsearch (single node, security off) on `http://localhost:9200`, with enough free disk space for it to
   allocate shards, and load the notes:

   ```bash
   curl -s -H 'Content-Type: application/x-ndjson' -XPOST 'localhost:9200/support-notes/_bulk?refresh=true' --data-binary @seed/support-notes.ndjson
   ```

2. Set the site up as for `ai-data-sources` (the **Blank** recipe, site name **Contoso**, a connection and its
   `contoso-...` deployments, the **Support assistant** profile with no tools or connections, **Enable preemptive
   retrieval-augmented generation (RAG)**). Create an **AI Knowledge Base Index** for Elasticsearch titled
   **Contoso knowledge** (index name `knowledge`, embedding deployment `contoso-embedding`) before recording, and no data
   source.
3. Sign the recorder in, enable the features, and record the clips in order (`03-elastic`, `04-ask`, `05-azure`,
   `06-postgres`): `04-ask` asks about the data source that `03-elastic` creates.

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/ai-data-sources-external/recorder
   export VIDEO_ROOT=<repo>/videos/ai-data-sources-external VIDEO_BASE_URL=http://localhost:<port> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   <bin>/Recorder record all
   ```

To record again, first delete the data source (and choose none on the profile), then wait for the passages to be gone
from the knowledge base index.
