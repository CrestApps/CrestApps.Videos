"""The slides of the Soft phone apps video."""
from brand import bullets, slide_base

WHY = [
    ("1", "A page forgets the call", "Reload or leave the page and the call is lost; a closed page cannot ring"),
    ("2", "An app keeps it", "Its own window, and a background connection that rings even when the window is closed"),
]

APPS = [
    ("1", "Browser extension", "Chrome 116 or later, Firefox 140 or later; Chrome Web Store or Firefox Add-ons"),
    ("2", "Windows app", "Windows 10 (1809) or later, x64 and ARM64; installer, portable zip, MSIX or Microsoft Store"),
    ("3", "Your site's own soft phone", "Same call controls; only your site; your normal sign-in, no separate password"),
]

EXT = [
    ("1", "Install", "CrestApps Soft Phone, from the Chrome Web Store or Firefox Add-ons"),
    ("2", "Options", "Your site's domain without https, then Save & grant access"),
    ("3", "Open Soft Phone", "Sign in to the site first; the window remembers its size and position"),
    ("4", "Incoming calls", "A popup with Answer, Decline and Voicemail, a ringtone and a notification"),
]

WIN = [
    ("1", "Install", "For your user only, with no administrator rights"),
    ("2", "Site domain and options", "Start with Windows, ringtone, always on top; then sign in to the site"),
    ("3", "The system tray", "Open phone, Settings and Quit; closing the window keeps the app running"),
    ("4", "Incoming calls", "A Windows notification and a call window with Answer, Decline and Voicemail"),
]

NOTES = [
    ("1", "The kill switch", "Enable the soft phone hides only the in-page phone; turn the Soft Phone Extension feature off to stop the apps"),
    ("2", "Managed installs", "Group Policy or Intune can set and lock the domain and other settings"),
]

USES = [
    ("1", "Working in the CRM all day", "The call keeps going while you move between records"),
    ("2", "Never missing a call", "It rings even with the phone window closed"),
    ("3", "A managed rollout", "The Windows app through Intune, with the domain already set"),
    ("4", "A remote worker", "The extension, in the browser they already use"),
]


def slide(step_id, chapter_title, label):
    n = int(step_id[-1]) - 1
    table = {
        "why-": ("A phone that does not live in a page", WHY),
        "app-": ("Two apps", APPS),
        "ext-": ("The browser extension", EXT),
        "win-": ("The Windows app", WIN),
        "nts-": ("The switch, and managed installs", NOTES),
        "use-": ("Where the apps help", USES),
    }
    for prefix, (title, items) in table.items():
        if step_id.startswith(prefix):
            image = slide_base(chapter_title, label, title)
            bullets(image, items, active={n})
            return image
    raise SystemExit(f"No slide for {step_id}")
