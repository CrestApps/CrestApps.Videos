# The CrestApps Report Builder

**YouTube:** not uploaded yet · **Length:** 10:32 · **Feature:** `CrestApps.OrchardCore.Reports.Builder` ([CrestApps.OrchardCore#814](https://github.com/CrestApps/CrestApps.OrchardCore/pull/814))

Why the Report Builder exists, how it is organized, and every task: building a report, columns and formulas, filters,
visuals, joins, kinds of reports, data sources, publishing and versions, reusable views, sharing, and adding a data
source in code. Each task chapter is also assembled as a short video (`"short"` in the storyboard).

The title, description, tags and chapters YouTube shows are in `video.json`; `report-builder.vtt` and
`report-builder.srt` are its captions.

## Publishing it

The file to publish is the one CrestApps.OrchardCore#814 carried before its docs moved to YouTube:
`src/CrestApps.Docs/static/img/docs/report-builder.mp4` at commit `5bbe98746`. To publish it:

```bash
curl -L -o report-builder.mp4 https://raw.githubusercontent.com/CrestApps/CrestApps.OrchardCore/5bbe98746b59062a231d0788e085ccf59c5e5ace/src/CrestApps.Docs/static/img/docs/report-builder.mp4
python tools/youtube/youtube.py release report-builder --file report-builder.mp4
```

The docs pages embed it with `<YouTube video="report-builder" />`, and each task page plays its chapter with `start`
and `end`; `sections.json` lists them, in whole seconds (each one is where that task's short video sits in this one,
matched caption by caption).

## Recording it again

It was recorded on a copy of a demo site's `App_Data` with Contact Center data, on `http://localhost:5330`, behind a
dead proxy. That copy still held real names in old records, so it was recorded with a mask script that replaced them;
the mask is not kept here. Prefer a fresh site with seeded fake data for the next take.

`seed-designs.json` holds the reports the later chapters start from. The scenes are in `recorder/`. A new take
changes where each chapter starts: update `sections.json` and the `start`/`end` of the embeds that `embeddedIn` lists.
