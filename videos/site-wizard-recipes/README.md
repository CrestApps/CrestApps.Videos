# Wizards, Recipes and Shared Resources

**Feature:** `CrestApps.OrchardCore.Wizard`, `CrestApps.OrchardCore.Recipes` and `CrestApps.OrchardCore.Resources` · **Length:** see `video.json`

Why a wizard, a recipe and shared resources exist, how to prepare a wizard (the Wizard part, the contained content
types and the completion policy), how to build its steps, what a visitor sees, the content items the wizard collects,
how to run a recipe from Tools, Recipes and how the JSON Import editor checks a pasted recipe, and what the shared
resources are. The video shows the admin and the site only: no code or configuration files. The title, description,
tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (an invented visitor, `priya.raman@example.com`), so it
needs no mask script. The recipes listed on Tools, Recipes are shown, never run.

1. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` and run it with `ORCHARD_APP_DATA`
   pointing at an empty folder outside both repositories, with `HTTP_PROXY`/`HTTPS_PROXY` set to
   `http://127.0.0.1:9`.
2. Set the site up with the **Blog** recipe, site name **Contoso**, SQLite, user `admin`.
3. Sign the recorder in and seed the site: `Recorder prepare` enables CrestApps Resources, CrestApps Recipes and
   Wizard, and imports `seed/registration.recipe.json` (the **Workshop Registration** host type and the **Visitor
   Details** and **Visit Preferences** step types):

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/site-wizard-recipes/recorder
   export VIDEO_ROOT=<repo>/videos/site-wizard-recipes VIDEO_BASE_URL=http://localhost:5376 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/site-wizard-recipes`) and record the clips in
   order: `03-type` adds the Wizard part that `04-steps` uses, `04-steps` saves the id of the wizard in
   `state-host-id.json` for `05-run`, and `06-results` shows what `05-run` collected. To record again, restore the copy
   first.
