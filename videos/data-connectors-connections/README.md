# Data connections

**Feature:** `CrestApps.OrchardCore.DataConnectors` with the HTTP API, Media Files, Elasticsearch and SQL Server connectors · **Length:** see `video.json`

Why connections exist, the four kinds, adding one (**Tools > Data Connectors > Connections**), **Save and test** (green and
red results), how secrets are kept (write-only, "Saved. Leave empty to keep it."), the **Test**, **Add data set** and
**Delete** actions, why a connection in use can't be deleted, and the Elasticsearch and SQL Server forms (shown, not saved).
The video shows the admin only: no code or configuration files.

## What is real and what is not

Everything is real except the systems: the HTTP API is `seed/fake-api.py` (made-up orders on `localhost:5555`), and the
files are `seed/customers.csv` and `seed/products.csv` (made-up people, `example.com` addresses). No SQL Server or
Elasticsearch is used: their forms are only opened and cancelled.

## Recording it again

1. Run the CrestApps.OrchardCore Cms.Web build behind a dead proxy on a fresh Blank site, with the features **Data Connectors**,
   **HTTP API / Elasticsearch / Media Files / SQL Server Data Connector** and **Data Pipelines** on, and copy
   `seed/*.csv` into the tenant's `Media/demo-data` folder. The site must allow CSV, JSON and JSON Lines uploads
   (the media `AllowedFileExtensions` setting: by default only Excel can be uploaded), or the library does not list the files.
2. Start `python seed/fake-api.py 5555`.
3. Start the site **without** `CrestApps:DataConnectors:Network:AllowPrivateNetworks`, then record `02-open`, `03-files`,
   `04-http` (the red check is the real one for an address on the site's own network).
4. Restart the site **with** that setting on, then record `05-fix`, run `Recorder prepare` (creates the Orders data set), and
   record `06-manage` and `07-kinds`.
