using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Diagnostics video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("01-open", OpenAsync),
        ("02-banner", BannerAsync),
        ("03-panel", PanelAsync),
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

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator Card(Scene scene, string title)
        => scene.Page.Locator(".card").Filter(new() { Has = scene.Page.Locator(".card-header", new() { HasTextString = title }) }).First;

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static ILocator Feature(Scene scene, string id)
        => scene.Page.Locator($"#btn-enable-{id.Replace('.', '_')}, #btn-disable-{id.Replace('.', '_')}").First.Locator("xpath=ancestor::li[contains(@class,'list-group-item')][1]");

    private static async Task ScrollToAsync(Scene scene, ILocator target, double margin = 150)
    {
        var box = await target.BoundingBoxAsync();

        if (box is not null)
        {
            await scene.ScrollAsync(0, box.Y - margin);
        }
    }

    private static async Task PickAsync(Scene scene, string selectId, string text)
    {
        await scene.ClickAsync(scene.Page.Locator($"button[data-id='{selectId}']"), pauseAfter: 900);
        await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = text }).First, pauseAfter: 600);
    }

    // ----- Clips -----
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = StatePath,
            ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height },
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        // The supervisor needs their own entitlements before the live dashboard shows anything.
        await page.GotoAsync(BaseUrl + "/Admin/contact-center/agent-entitlements/create");
        await page.Locator("button[data-id='agent-entitlement-user-selector']").ClickAsync();
        await page.Locator(".dropdown-menu.show input[type=search]").FillAsync("admin");
        await page.WaitForTimeoutAsync(2500);
        await page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = "admin" }).First.ClickAsync();
        await page.WaitForTimeoutAsync(600);

        foreach (var (id, text) in new[] { ("AllowedQueueIds", "Sales handoff"), ("AllowedCampaignIds", "Spring lead drive") })
        {
            await page.Locator($"button[data-id='{id}']").ClickAsync();
            await page.WaitForTimeoutAsync(400);
            await page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = text }).First.ClickAsync();
            await page.Keyboard.PressAsync("Escape");
            await page.WaitForTimeoutAsync(400);
        }

        await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
        Console.WriteLine("Created the supervisor entitlements: " + page.Url);
        await context.CloseAsync();
    }

    private static async Task OpenAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/dashboard");
        await scene.Page.WaitForTimeoutAsync(5000);
        await scene.StartCaptureAsync();

        await scene.StepAsync("open-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("h1").First, "Interaction Center > Live dashboard");
            await scene.PauseAsync(5000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("text=Connected").First, "Connected: live updates work");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("open-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".alert").First, "The dialer banner");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 400);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task BannerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/dashboard");
        await scene.Page.WaitForTimeoutAsync(5000);
        await scene.StartCaptureAsync();

        await scene.StepAsync("ban-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".alert").First, "Nothing due yet: records wait, but none may be dialed now");
            await scene.PauseAsync(9000);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PanelAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/dashboard");
        await scene.Page.WaitForTimeoutAsync(5000);
        await scene.StartCaptureAsync();

        await scene.StepAsync("pan-1", async () =>
        {
            var title = scene.Page.GetByText("Why isn't this dialing?").First;
            await ScrollToAsync(scene, title, margin: 120);
            await scene.HighlightAsync(title, "Why isn't this dialing?");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.GetByText("Pending 2").First, "Pending, dialable now, held back");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("pan-2", async () =>
        {
            await scene.ScrollAsync(0, 200);
            await scene.HighlightAsync(scene.Page.GetByText("No agents are signed in").First, "Agents signed in");
            await scene.PauseAsync(4500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.GetByText("Routing found no agent").First, "What routing found");
        });

        await scene.ClearHighlightsAsync();
    }
}
