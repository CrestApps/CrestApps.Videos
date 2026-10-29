# AI Web Crawlers

**Feature:** `CrestApps.OrchardCore.AI.WebCrawlers`, with a data source feature such as `CrestApps.OrchardCore.AI.DataSources.Elasticsearch` · **Length:** 5:15

Why copying web pages by hand goes stale, how a knowledge base index, a **Web** data source and a web crawler fit
together, and how to create each one, set the crawler's limits, URL patterns and schedule, read the website now, give an
assistant the data source, and ask a question that the pages answer, with a link to the page. The video shows the admin
only: no code or configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only: the website the crawler reads is the three pages and
sitemap of `seed/helpsite`, served by a throwaway local web server on `http://localhost:55391` (for example
`python -m http.server 55391 --bind 127.0.0.1` in that folder). It uses a **real model** (so the chat answer is real)
and a **local Elasticsearch** (8.x, security off, port 9200), both outside this repository. Nothing about the model's
account is shown: the demo site has a read-only connection and three deployments titled `contoso-...`.

1. Run a local Elasticsearch (single node, security off) on `http://localhost:9200`, with enough free disk space for
   it to allocate shards (or turn its disk threshold off for the demo).
2. Serve `seed/helpsite` on `http://localhost:55391`.
3. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` and run it with `ORCHARD_APP_DATA` pointing at
   an empty folder outside both repositories. Put one connection and its chat and embedding deployments, named
   `contoso-...`, in that folder's `appsettings.json`, and let the site reach only the model's host and `localhost`
   (`NO_PROXY`), with `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9` for everything else.
4. Set the site up with the **Blank** recipe, site name **Contoso**, SQLite, and an admin account. Choose the default chat,
   utility and embedding deployments in **Settings > Artificial Intelligence**, and create an AI profile titled **Support
   assistant** (technical name `support-assistant`) with no tools or connections.
5. Sign the recorder in, enable the features, narrate and record the clips in order (`03-index`, `04-datasource`,
   `05-crawler`, `06-sync`, `07-use`, `08-ask`): each one builds on the one before.

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/ai-web-crawlers/recorder
   export VIDEO_ROOT=<repo>/videos/ai-web-crawlers VIDEO_BASE_URL=http://localhost:<port> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   <bin>/Recorder record all
   ```

To record again, first delete the web crawler, the data source and the knowledge base index (**Force Delete**, and
delete its Elasticsearch index if it is still there), and choose no data source on the profile.
