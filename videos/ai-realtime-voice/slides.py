"""The slides of the Realtime Voice video."""
from brand import bullets, slide_base

HOW = [
    ("1", "A realtime deployment", "A model that listens and speaks by itself"),
    ("2", "Or a cascade", "Transcribe, chat and speak: three deployments chained"),
    ("check", "Same chat either way", "Tools, data sources and typing keep working"),
]

TIPS = [
    ("1", "Use headphones", "So the assistant does not hear its own voice"),
    ("2", "A cascade adds delay", "A turn passes through three services"),
    ("3", "Per-person voice settings", "Microphone, speaker, push-to-talk, interruptions"),
]


def slide(step_id, chapter_title, label):
    kinds = {
        "how": ("Two ways to speak", HOW, lambda n: set(range(n))),
        "tip": ("Tips for a spoken conversation", TIPS, lambda n: {n - 1}),
    }
    prefix, _, number = step_id.partition("-")

    if prefix in kinds:
        heading, items, active = kinds[prefix]
        image = slide_base(chapter_title, label, heading)
        bullets(image, items, active=active(int(number)))
        return image

    raise SystemExit(f"No slide for {step_id}")
