using Microsoft.Playwright;

namespace Recorder;

public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-signin", SignInAsync),
        ("04-presence", PresenceAsync),
        ("05-workspace", WorkspaceAsync),
        ("06-bar", BarAsync),
    ];

    public static async Task RecordAsync(IBrowser browser, string outputRoot, IReadOnlyDictionary<string, double> durations, string[] names)
    {
        var selected = names is ["all"] ? s_clips : s_clips.Where(clip => names.Contains(clip.Name)).ToArray();

        foreach (var (name, record) in selected)
        {
            Console.WriteLine($"Recording {name}");
            await record(browser, Path.Combine(outputRoot, name), durations);
        }
    }

    /// <summary>
    /// Seeds two skills, three queues and one agent's entitlements (not recorded). Fake data only.
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await Seed.OpenAsync(browser);

        await Seed.QueueAsync(page, "Support", "Customers who need help with an order");
        await Seed.QueueAsync(page, "Sales", "New customers and quotes");
        await Seed.QueueAsync(page, "Billing", "Invoices and payments");
        await Seed.EntitlementAsync(page, "admin", ["Support", "Sales"]);
        await context.CloseAsync();
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    // ----- Helpers -----

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static ILocator Field(Scene scene, string id) => scene.Page.Locator("#" + id);

    private static ILocator Part(Scene scene, string suffix) => scene.Page.Locator($"[id$='{suffix}']").First;

    private static async Task ScrollToAsync(Scene scene, ILocator locator, double offset = 160)
    {
        var top = await locator.EvaluateAsync<double>("e => e.getBoundingClientRect().top");
        await scene.ScrollAsync(0, top - offset);
    }

    private static async Task PickAsync(Scene scene, string selectId, string text, bool close = false)
    {
        await scene.ClickAsync(scene.Page.Locator($"button[data-id='{selectId}']"), pauseAfter: 400);
        await scene.ClickAsync(scene.Page.Locator($"div.bootstrap-select:has(button[data-id='{selectId}']) .dropdown-item").Filter(new() { HasTextString = text }).First, pauseAfter: 400);

        if (close)
        {
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.PauseAsync(300);
        }
    }

    private static async Task SetAsync(Scene scene, ILocator field, string text, int delay = 40)
    {
        await scene.ClickAsync(field, pauseAfter: 150);
        await field.FillAsync(string.Empty);
        await field.PressSequentiallyAsync(text, new() { Delay = delay });
        await scene.PauseAsync(250);
    }


    private static ILocator Tab(Scene scene, string name) => scene.Page.Locator("button.telephony-soft-phone__tab").Filter(new() { HasTextString = name }).First;

    private static ILocator PresenceButton(Scene scene) => scene.Page.Locator("button.telephony-soft-phone__icon-btn.dropdown-toggle").First;

    // ----- Clips -----

    private static async Task SignInAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/softphone");
        await scene.StartCaptureAsync();

        await scene.StepAsync("sg-1", async () =>
        {
            await scene.ClickAsync(Tab(scene, "Work"), pauseAfter: 900);
            await scene.HighlightAsync(scene.Page.Locator("text=Signed out").First, "Signed out");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("sg-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.dropdown-toggle").Filter(new() { HasTextString = "Select queue(s)" }).First, pauseAfter: 600);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = "Support" }).First, pauseAfter: 400);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = "Sales" }).First, pauseAfter: 400);
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.PauseAsync(500);
        });

        await scene.StepAsync("sg-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.btn-success").Filter(new() { HasTextString = "Sign in" }).First, pauseAfter: 2500);
        });

        await scene.StepAsync("sg-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Sign out" }).First, "Sign out");
            await scene.PauseAsync(2500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PresenceAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/softphone");
        await scene.StartCaptureAsync();

        await scene.StepAsync("prs-1", async () =>
        {
            await scene.HighlightAsync(PresenceButton(scene), "Presence");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(PresenceButton(scene), pauseAfter: 700);
        });

        await scene.StepAsync("prs-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show").First, "Available, reasons and Break");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("prs-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show *").Filter(new() { HasTextString = "Offline" }).Last, "Offline");
            await scene.PauseAsync(2500);
        });

        await scene.ClearHighlightsAsync();
        await scene.Page.Keyboard.PressAsync("Escape");
    }

    private static async Task WorkspaceAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("wsp-1", async () =>
        {
            await scene.NavigateAsync("/Admin/contact-center/workspace");
            await scene.HighlightAsync(scene.Page.Locator(".cc-presence__button").First, "Presence");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("wsp-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".cc-panel").Filter(new() { HasTextString = "Signed in to" }).First, "Signed in to");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("wsp-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".cc-panel").Filter(new() { HasTextString = "Active interaction" }).First, "Active interaction");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("wsp-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".cc-panel").Filter(new() { HasTextString = "Recent activity" }).First, "Recent activity");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task BarAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("bar-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".cc-agent-bar__handle").First, "Agent bar");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator(".cc-agent-bar__handle").First, pauseAfter: 800);
        });

        await scene.StepAsync("bar-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("a.cc-bar__workspace").First, "Opens My workspace");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("bar-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.cc-bar__collapse").First, pauseAfter: 500);
            await scene.NavigateAsync("/Admin/contact-center/agent-states");
            await scene.PauseAsync(1500);
        });
    }
}
