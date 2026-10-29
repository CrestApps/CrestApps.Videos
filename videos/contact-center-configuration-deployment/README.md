# Contact Center Configuration and Deployment: Export a Plan and Replay It

**Feature:** `CrestApps.OrchardCore.ContactCenter` · **Length:** see `video.json`

The features and their dependencies, the recipe, exporting a plan and replaying it, voice media audio, and what does not travel.
The video shows the admin only: no code or configuration files. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The site is the Contact Center demo site of `../contact-center-reports/` (queues, agents, entitlements), plus the voice media clip
"Main hold music" that `Recorder prepare` adds from a short test tone (`ffmpeg -f lavfi -i sine=frequency=440:duration=4 tone.wav`, passed in
`VIDEO_AUDIO`; saving it shows the real warning that the provider did not accept a copy). Adding a CrestApps step to a deployment plan
needs the fix of CrestApps.OrchardCore pull request 1193 (the steps' `Category` and `Title` are not serialized); until it is released, record on a build that has it.

Record `02-features` to `06-voice` in order from the same snapshot: `04-export` downloads the plan to the temp folder (`cc-plan-demo.zip`, never
committed), `05-replay` renames a queue, deletes the voice media clip and imports the plan, `06-voice` uploads the audio again.
