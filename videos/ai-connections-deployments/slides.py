"""The slides of the AI Connections and Deployments video."""
from brand import bullets, slide_base

WHY = [
    ("1", "Every AI feature needs a model", "Chat, search, voice and pictures all run at an AI provider"),
    ("2", "Different jobs, different models", "Chat, embedding, vision and speech models are not interchangeable"),
    ("3", "Keys copied everywhere are fragile", "Changing one would mean editing every profile by hand"),
    ("check", "Set it up once, pick by name", "A change in one place reaches every feature"),
]

HOW = [
    ("1", "Connection", "The address and the key of one AI provider account"),
    ("2", "Deployment", "One model used through a connection, with its capabilities"),
    ("3", "Default deployments", "The model each kind of job uses when none is chosen"),
    ("check", "Managed for you", "Managed by your technical team, and shown as Read-only"),
]

JOBS = [
    ("1", "Chat and workflows", "Text conversation"),
    ("2", "Search over your content", "Text embedding, plus a chat model"),
    ("3", "Pictures and speech", "Image input or output, Speech to text, Text to speech"),
    ("4", "Spoken conversations", "Realtime, or a Cascaded Realtime deployment"),
]


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        image = slide_base(chapter_title, label, "Set the provider up once")
        bullets(image, WHY, active=set(range(int(step_id[-1]))))
        return image
    if step_id.startswith("how-"):
        image = slide_base(chapter_title, label, "Connection, deployment, default")
        bullets(image, HOW, active={int(step_id[-1]) - 1})
        return image
    if step_id.startswith("job-"):
        image = slide_base(chapter_title, label, "Match the model to the job")
        bullets(image, JOBS, active={int(step_id[-1]) - 1})
        return image
    raise SystemExit(f"No slide for {step_id}")
