## Producing videos

Use the `crestapps-video-producer` skill in `.agents/skills/crestapps-video-producer`. Each video is a folder in
`videos/<slug>/`; never commit an MP4, an audio file, a sign-in (`state.json`), a mask script with real names, or a
copy of a site's `App_Data`.

## Git Commit Rules

- Never add Claude as a commit author or co-author.
- Never add `Co-authored-by: Claude` or any Claude-related co-author trailer to commits.
- Commits must use only the configured human Git author.
