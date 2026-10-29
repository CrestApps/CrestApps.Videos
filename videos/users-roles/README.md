# Users and Roles

**Feature:** `CrestApps.OrchardCore.Users` (User Display Name, User Avatar, Sign in with an email code) and `CrestApps.OrchardCore.Roles` (Enhanced Roles), on top of Orchard Core's Users and Roles · **Length:** see `video.json`

Why users and roles exist, how they fit together with names and avatars, how to read the user list, add a person,
create a role and choose its permissions, give a person a role, choose how names appear, set up avatars, and let
people sign in with an emailed code. The video shows the admin and the site only: no code or configuration files. The
title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only (invented people with `example.com` addresses), so it
needs no mask script. Nothing is sent: the "Email me a code" button is shown but never clicked.

1. Build CrestApps.OrchardCore's `src/Startup/CrestApps.OrchardCore.Cms.Web` and run it with `ORCHARD_APP_DATA`
   pointing at an empty folder outside both repositories, with `HTTP_PROXY`/`HTTPS_PROXY` set to
   `http://127.0.0.1:9`.
2. Set the site up with the **Blog** recipe, site name **Contoso**, SQLite, user `admin`.
3. Sign the recorder in and seed the site: `Recorder prepare` enables Enhanced Roles, User Display Name, User Avatar
   and Sign in with an email code, and creates the people **dana.whitfield** (Editor), **omar.haddad** (Author) and
   **lena.fischer** (Moderator):

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/users-roles/recorder
   export VIDEO_ROOT=<repo>/videos/users-roles VIDEO_BASE_URL=http://localhost:5376 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   ```

4. Copy the site's `App_Data` aside, narrate (`scripts/narrate.py videos/users-roles`) and record the clips in order:
   `06-assign` gives Dana the role that `05-roles` creates, and `08-avatars` finds Dana by the name that `07-names`
   sets. To record again, restore the copy first.
