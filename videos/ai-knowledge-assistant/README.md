# Build an AI Knowledge Assistant

**Use case:** the user-manual page *Build an AI Knowledge Assistant* · **Length:** see `video.json`

The outcome of the knowledge videos in one walk-through: upload a document to a profile, choose a data source of the
site's help articles, turn on **Restrict answers to retrieved data only**, and test three questions: one that the
document answers, one that an article answers (with its numbered source), and one that the knowledge does not cover
(the assistant says that it cannot find it). The video shows the admin only: no code or configuration files. The title,
description, tags and chapters YouTube shows are in `video.json`.

It builds on [`ai-knowledge`](../ai-knowledge) and [`ai-data-sources`](../ai-data-sources), which create the indexes and
the data source; the slides say that they are ready.

## Recording it again

The video was recorded on the same **fresh site** as `ai-data-sources`, with fake data only: the four **Article** items
of that video's `seed/contoso-articles-recipe.json`, and `seed/return-policy.md`. It uses a **real model** (so the chat
answers are real) and a **local Elasticsearch** (8.x, security off, port 9200), both outside this repository. Nothing
about the model's account is shown: the demo site has a read-only connection and three deployments titled `contoso-...`.

1. Set the site up as for `ai-data-sources` and `ai-knowledge`: the **Articles** content index, the **Contoso knowledge**
   AI Knowledge Base index, the **Contoso documents** AI Documents index (chosen in **Settings > Artificial
   Intelligence**), a **Help articles** data source (synced), the **Support assistant** profile with no documents and no
   data source, and **Enable preemptive retrieval-augmented generation (RAG)**.
2. Sign the recorder in, enable the features, and record the clips in order (`03-upload`, `04-connect`, `05-test`).

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/ai-knowledge-assistant/recorder
   export VIDEO_ROOT=<repo>/videos/ai-knowledge-assistant VIDEO_BASE_URL=http://localhost:<port> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   <bin>/Recorder record all
   ```

To record again, remove the document and choose no data source on the profile.
