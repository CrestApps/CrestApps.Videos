# Connected data in reports and pipelines

**Feature:** `CrestApps.OrchardCore.DataConnectors` with `CrestApps.OrchardCore.Reports.Builder` and `OrchardCore.DataPipelines` · **Length:** see `video.json`

Where connected data sets appear (**Connected data (live)** and **Connected data (imported)** in the Report Builder's **Add data set**
and the **Read a data source** step of a data pipeline), a report on a live data set, an imported data set's card, and the
permissions in **Access Control > Roles**: **Read every connected data set** and **Read the connected data set - name**, the same
for live and imported data. The video shows the admin only: no code or configuration files.

## What is real and what is not

The HTTP API is `seed/fake-api.py` (made-up orders on `localhost:5555`, dated in the 18 days that end about a week ago, so a
report's default period shows them), and the files are `seed/customers.csv` and `seed/products.csv`.

## Recording it again

1. Start from the site of the data sets video (the four connections, behind a dead proxy, allowed to reach its own network),
   run `python seed/fake-api.py 5555`, then `Recorder prepare`: it creates the live data sets Orders and Customers, the imported
   data set Imported orders and the role Analyst (the first import runs within a minute, before the report clip).
2. Record `02-report`, `03-pipeline`, `04-roles`.
