# Realtime Voice

**Feature:** `CrestApps.OrchardCore.AI.Chat` with a realtime deployment (a provider's speech-to-speech model, or a **Cascaded Realtime** deployment) · **Length:** see `video.json`

How to set up a spoken conversation with an AI assistant: the speech deployments, the **Cascaded Realtime** deployment that
chains a speech-to-text deployment, a chat deployment and a text-to-speech deployment, the site defaults in **Settings >
Artificial Intelligence**, the **Chat mode**, **Conversation deployment** and **Voice** of a profile, and where the
**Start Conversation** button appears in the chat. The video shows the admin only: no code or configuration files, and no
live conversation (it needs a microphone and a live speech service, which the demo site does not have). The title,
description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on the same **fresh site** as the other AI videos, with fake data only. The two speech
deployments (`contoso-transcribe` with **Speech to text** and **Realtime**, and `contoso-voice` with **Text to speech**,
both of the **Azure AI Services** provider) point at a made-up endpoint with a made-up key: nothing is ever sent to them.
The **Support assistant** profile is a chat profile saved once, so that **Chat mode** is shown (it needs a speech model on
the site).

1. Set the site up as for `ai-knowledge`, with the `contoso-...` chat deployment.
2. Create the two speech deployments (**Artificial Intelligence > Deployments > Add Deployment > Azure AI Services**),
   with the fake endpoint `https://contoso-speech.cognitiveservices.azure.com` and an API key.
3. Sign the recorder in, enable the features, and record the clips in order (`02-speech`, `03-cascade`, `04-defaults`,
   `05-profile`, `06-chat`). `02-speech` only fills in a form and leaves without saving; `03-cascade` creates
   `contoso-conversation`, which `05-profile` chooses.

   ```bash
   cd .agents/skills/crestapps-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/ai-realtime-voice/recorder
   export VIDEO_ROOT=<repo>/videos/ai-realtime-voice VIDEO_BASE_URL=http://localhost:<port> VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   <bin>/Recorder prepare
   <bin>/Recorder record all
   ```

To record again, delete `contoso-conversation` and set the profile's **Chat mode** back to **Text only**.
