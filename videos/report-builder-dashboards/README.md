# Report Builder dashboards

**Feature:** `CrestApps.OrchardCore.Reports.Builder` · **Length:** see `video.json`

Why a dashboard needs several queries, using one (click an agent, a queue or a day, Ctrl-click, Clear selection), several
queries and **Add query**, containers (title, width, height, the visual's **Container** setting), **When clicked** (**Pick
the value of**, **Filter these visuals**) and the Excel and PDF export that keeps the containers. The video shows the
admin only: no code or configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

Start from the site of `videos/reports-run-and-export` (fresh Contoso site, the Reports features, the CRM starter recipe
and the fake data of `seed-reports-data.py` in that folder; behind a dead proxy).

1. Build the recorder with this folder's scenes and run `Recorder prepare`: it turns the Report Builder on and imports
   `seed-reports.json` (the **Call dashboard**: a main query and three more, four containers, click actions) with
   **Tools > Deployments > JSON Import**. Copy `App_Data` aside.
2. Record `02-run`, `03-queries`, `04-containers`, `05-clicks` and `06-export` in this order. `03-queries` adds a query
   and removes it again, and `04-containers` adds a container and removes it, so each leaves an unpublished draft behind
   that the later clips show: nothing is ever published. `06-export` saves the Excel and PDF files to
   `shots/downloads/`, which `slides.py` draws in the `06b-files` slides.
