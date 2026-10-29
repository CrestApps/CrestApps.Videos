# AI Documents

**Feature:** `CrestApps.OrchardCore.AI.Documents` (brought by `...Documents.Profiles`, `...Documents.ChatSessions`, `...Documents.Elasticsearch`, `...Documents.OpenXml` and `...Documents.Pdf`) · **Length:** see `video.json`

How a person attaches a document to a chat and gets an answer from it, how the assistant can read the charts inside a
PDF document, and the settings that control it: the **Documents** section of **Settings > Artificial
Intelligence**, the **Default vision deployment**, and the **Knowledge** tab of a profile. The video shows the admin
only: no code or configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

It builds on [`ai-knowledge`](../ai-knowledge), which creates the documents index and uploads documents to a profile.

## Recording it again

The video was recorded on the same **fresh site** as `ai-knowledge`, with fake data only: the files that are attached are
in `seed/` (`return-policy.md`, and `annual-sales-review.pdf`, a report whose only sales numbers are in its chart, which
has a caption, "Figure 1", so that the site describes it; `make_docs.py` writes both again). It uses a **real model** (so the chat answers are real, and a vision-capable
deployment describes the chart) and a **local Elasticsearch** (8.x, security off, port 9200), both outside this
repository. Nothing about the model's account is shown: the demo site has a read-only connection and three deployments
titled `contoso-...`.

1. Run a local Elasticsearch (single node, security off) on `http://localhost:9200`.
2. Set the site up as for `ai-knowledge`: a connection with `contoso-chat`, `contoso-embedding` and `contoso-vision`
   deployments, the **Support assistant** profile, and an **AI Documents** index for Elasticsearch titled **Contoso
   documents** chosen as the **Index profile** in **Settings > Artificial Intelligence**. Choose `contoso-vision` as the
   **Default vision deployment**, leave **Describe figures in uploaded documents** on, and leave the profile with
   **Allow session document uploads** off and **Describe figures in uploaded documents** on **Use site default**.
3. Sign the recorder in, enable the features, and record the clips in order (`03-settings`, `04-profile`, `05-attach`,
   `06-figures`): `04-profile` allows the uploads that the next two clips use.

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/ai-documents/recorder
   export VIDEO_ROOT=<repo>/videos/ai-documents VIDEO_BASE_URL=http://localhost:<port> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   <bin>/Recorder record all
   ```
