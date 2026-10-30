# AI in Workflows

**Feature:** `OrchardCore.Workflows` with `CrestApps.OrchardCore.AI` · **Length:** see `video.json`

Building a workflow in **Design > Workflows**: the **AI Chat Session Closed** event, the **AI Completion using Profile**
task (**Profiles**, **Result property name**, **Prompt template**), the outcomes **Done**, **Drew Blank** and **Failed**,
connecting the steps on the canvas, **AI Completion using Direct Config**, and the four chat events. The video shows the
admin only: no code or configuration files.

## Recording it again

A fresh Contoso site with the AI features of `videos/ai-chat/` (a made-up chat profile **Contoso website assistant**
backed by a model deployment that is never shown), with **Workflows** enabled (enable it before recording and restart
the site: enabling a feature reloads the tenant). Record `02-create` to `06-direct` in order. `05-connect` drags the
task away from the event with the mouse and drags from the event's **Done** endpoint to the task; nothing runs, so no
model call is made.
