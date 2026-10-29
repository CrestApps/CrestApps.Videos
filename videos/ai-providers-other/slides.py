"""The slides of the Other AI Providers video."""
from brand import bullets, slide_base

PRV = [
    ('1', 'OpenAI and look-alikes', 'DeepSeek, Gemini, Together AI, vLLM, LM Studio'),
    ('2', 'Ollama', 'Open models on your own machine'),
    ('3', 'Azure AI Inference', 'Azure AI Foundry and GitHub Models'),
]

TIP = [
    ('1', 'Read-only items', 'Come from the site configuration'),
    ('2', 'Choose your defaults', 'Settings or a profile'),
    ('3', 'One provider for look-alikes', 'OpenAI; keys stay with your technical team'),
]


def slide(step_id, chapter_title, label):
    kinds = {
        'prv': ('What you can connect', PRV, lambda n: set(range(n))),
        'tip': ('Read-only items and defaults', TIP, lambda n: {n - 1}),
    }
    prefix, _, number = step_id.partition("-")

    if prefix in kinds:
        heading, items, active = kinds[prefix]
        image = slide_base(chapter_title, label, heading)
        bullets(image, items, active=active(int(number)))
        return image

    raise SystemExit(f"No slide for {step_id}")
