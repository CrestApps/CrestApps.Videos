# The CrestApps Report Builder

**YouTube:** [v1](https://youtu.be/5KTFADbxswA) (v2 not uploaded yet) · **Length:** 13:01 · **Feature:** `CrestApps.OrchardCore.Reports.Builder` ([CrestApps.OrchardCore#814](https://github.com/CrestApps/CrestApps.OrchardCore/pull/814))

Why the Report Builder exists, how it is organized, and every task: building a report, columns and formulas, filters,
visuals, joining data sets and choosing which rows a join keeps, kinds of reports, data sources, publishing and
versions, a reusable view with a report built on it, sharing with people, roles and share links, and adding a data
source in code. Each task chapter is also assembled as a short video (`"short"` in the storyboard).

The title, description, tags and chapters YouTube shows are in `video.json`; `report-builder.vtt` and
`report-builder.srt` are its captions.

## Publishing it

Assemble it (see "Recording it again"), then create the draft release of its next version with the result:

```bash
python tools/youtube/youtube.py release report-builder --file videos/report-builder/build/master.mp4
```

A new version is a new YouTube video, and the previous one becomes unlisted. The docs pages embed it with
`<YouTube video="report-builder" />`, and each task page plays its chapter with `start` and `end`; `sections.json`
lists them, in whole seconds: each chapter starts where its chapter card starts, and ends a second after the next one
starts. When the version is published, update the `start` and `end` of the embeds that `embeddedIn` lists from
`sections.json`.

## Recording it again

It was recorded on a copy of a demo site's `App_Data` with Contact Center data, on `http://localhost:5331`, behind a
dead proxy, running the build of CrestApps.OrchardCore#814. That copy still held real names in old records, so it was
recorded with a mask script (`VIDEO_INIT_SCRIPT`) that replaced them and hid the agent bar; the mask is not kept here.
Prefer a fresh site with seeded fake data for the next take.

`seed-designs.json` holds the reports the later chapters start from. The scenes are in `recorder/`. Record every clip
in one take from a clean copy (`Recorder record all`): the clips build on each other's reports, and the views clip is
recorded before the data sources clip, which shows the view it saves. A new take changes where each chapter starts:
update `sections.json` and the `start`/`end` of the embeds that `embeddedIn` lists.
