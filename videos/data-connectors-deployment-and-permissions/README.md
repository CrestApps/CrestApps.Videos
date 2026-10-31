# Data connectors: deployment and permissions

**Feature:** `CrestApps.OrchardCore.DataConnectors` with `OrchardCore.Deployment` · **Length:** see `video.json`

The five permissions (**Manage data connections and their credentials**, **Manage connected data sets**, **Refresh imported data sets**,
**Read every connected data set**, **Read the connected data set - name**) and how **Manage connected data sets** includes the refresh
permission (the role is saved, then opened again: the Effective column shows it), a deployment plan with **Data Connections** and
**Connected Data Sets** (category **Data**), **Execute**, **Package Import**, what the other site gets and does not get, and what to check
when a data set is missing or a refresh fails. The video shows the admin only: no code or configuration files.

## What is real and what is not

The plan is only built, never executed, and no second site is shown: the other site's part is narrated on slides. The data
(`seed/`) is made up: a fake API on `localhost:5555` and two CSV files.

## Recording it again

1. Start from the site of the reports and pipelines video: the four connections, the data sets Orders, Customers and Imported orders
   (wait for its first import to succeed), and the empty role Analyst (`Recorder prepare` creates them), behind a dead proxy and
   allowed to reach its own network, with `python seed/fake-api.py 5555` running.
2. Record `02-roles`, `03-plan`, `05-check`. `02-roles` saves the role Analyst with a permission, so restore the copy of the site
   before recording it again.
