# The CrestApps Report Builder

**YouTube:** not uploaded yet · **Length:** 10:32 · **Feature:** `CrestApps.OrchardCore.Reports.Builder` ([CrestApps.OrchardCore#814](https://github.com/CrestApps/CrestApps.OrchardCore/pull/814))

Why the Report Builder exists, how it is organized, and every task: building a report, columns and formulas, filters,
visuals, joins, kinds of reports, data sources, publishing and versions, reusable views, sharing, and adding a data
source in code. Each task chapter is also assembled as a short video (`"short"` in the storyboard).

## Recording it again

It was recorded on a copy of a demo site's `App_Data` with Contact Center data, on `http://localhost:5330`, behind a
dead proxy. That copy still held real names in old records, so it was recorded with a mask script that replaced them;
the mask is not kept here. Prefer a fresh site with seeded fake data for the next take.

`seed-designs.json` holds the reports the later chapters start from. The scenes are in `recorder/`.

When it is uploaded, run `python tools/youtube/youtube.py upload report-builder --file <mp4> --captions <srt>`, or
record the id in `video.json` after a YouTube Studio upload, then swap the MP4 embeds in
CrestApps.OrchardCore#814's docs pages for `<YouTube id="..." />`.
