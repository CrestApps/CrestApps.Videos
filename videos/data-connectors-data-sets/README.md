# Connected data sets

**Feature:** `CrestApps.OrchardCore.DataConnectors` · **Length:** see `video.json`

What a data set is, adding one from a media library file and from a web API (resource, Live access, the connector's own
settings, **Discover fields**, **Show**, **Display name**, **Type**, **Identifier**, **Default date field**), **Preview**, **Edit**
(technical name locked), and the custom query of SQL Server and the query of Elasticsearch (forms shown, not saved).
Refreshing imported data sets is the next video. The video shows the admin only: no code or configuration files.

## What is real and what is not

The HTTP API is `seed/fake-api.py` (made-up orders on `localhost:5555`) and the files are `seed/customers.csv` and
`seed/products.csv` (made-up people with `example.com` addresses). The SQL Server and Elasticsearch connections point at
made-up hosts and are only opened and cancelled.

## Recording it again

1. Start from a fresh Blank site with the Data Connectors features and Data Pipelines on, run behind a dead proxy and with the
   site allowed to reach its own network (`CrestApps:DataConnectors:Network:AllowPrivateNetworks`), the seed CSV files in
   the tenant's `Media/demo-data` folder, and CSV, JSON and JSON Lines allowed as media file types.
2. Start `python seed/fake-api.py 5555`, then `Recorder prepare` (creates the four connections).
3. Record `02-file`, `03-preview`, `04-api`, `05-others` in that order (each builds on the data sets before it).
