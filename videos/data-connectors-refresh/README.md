# Refreshing imported data sets

**Feature:** `CrestApps.OrchardCore.DataConnectors` · **Length:** see `video.json`

Why import, creating an imported data set (**Refresh every (minutes)**, **Refresh reads**, **When the source's fields change**), the
first import, **Refresh now**, the refresh **History**, a failed refresh that keeps the last good rows, and the options for
SQL Server and Elasticsearch (**Only the rows changed since the last refresh**, **Changed rows have a larger**; shown, not saved).
The video shows the admin only: no code or configuration files.

## What is real and what is not

The HTTP API is `seed/fake-api.py` (made-up orders on `localhost:5555`). The clip `03-now` adds five orders to it with its
`/control` address, and `04-fail` makes it answer 500, so the failed refresh is a real one. The refresh task runs once a minute, so
the clips wait off camera (`OffCameraAsync`) for the list to show each result.

## Recording it again

1. Start from the site of the data sets video (a Blank site with the Data Connectors features, the four connections and no data
   sets, behind a dead proxy, allowed to reach its own network) and a fresh `python seed/fake-api.py 5555`.
2. Record `02-create`, `03-now`, `04-fail`, `05-more` in that order.
