# The CrestApps AI Profile Builder

**YouTube:** https://youtu.be/UOKvEY-kID4 · **Length:** 4:40 · **Feature:** `CrestApps.OrchardCore.AI.Chat.Interactions.ProfileBuilder` ([CrestApps.OrchardCore#910](https://github.com/CrestApps/CrestApps.OrchardCore/pull/910))

Why the Profile Builder exists, how it works, and how to start a session from an AI profile, role-play and give
feedback, use revisions and replay, and save the result. The title, description, tags and chapters YouTube shows are
in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only, so it needs no mask script.

1. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` (a branch with the Profile Builder) and
   run it with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories, on `https://localhost:5368`,
   with `HTTP_PROXY`/`HTTPS_PROXY` set to `http://127.0.0.1:9` and `NO_PROXY` set to your AI provider's host. Give the
   host `App_Data/appsettings.json` an `OrchardCore:CrestApps:AI` section with an Azure OpenAI connection and a
   `gpt-4.1-mini` chat deployment.
2. Set the site up with the **Blank** recipe, site name **Contoso**, SQLite, and a generated admin password.
3. Sign the recorder in and seed the site:

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/ai-profile-builder/recorder
   export VIDEO_ROOT=<repo>/videos/ai-profile-builder VIDEO_BASE_URL=https://localhost:5368 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<generated>
   <bin>/Recorder setup
   <bin>/Recorder prepare     # enables the features, sets the default deployments, creates "Contoso Dental (SMS)"
   ```

   `prepare` creates the AI profile with a price list in its instructions, so the first reply quotes a price, which is
   the behavior the video corrects. Stop the site and copy its `App_Data` aside: restore that copy before every full take.
4. Narrate and record: `scripts/narrate.py videos/ai-profile-builder`, then `Recorder record all`. The clips depend on
   each other (03 creates the session that 04 to 06 use), so record them in order.

The model's replies change from take to take. Check the contact sheets: the first reply must quote a price, and the
regenerated one must leave it to the front desk.
