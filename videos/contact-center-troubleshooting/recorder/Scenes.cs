using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Contact Center Troubleshooting video, one method per "clip" of storyboard.json.
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
        ("02-menu", MenuAsync),
        ("03-empty", EmptyAsync),
        ("04-ring", RingAsync),
        ("05b-diag", DiagAsync),
        ("07-dialer", DialerAsync),
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
    private static async Task MenuAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Roles/Index");
        await scene.StartCaptureAsync();

        await scene.StepAsync("menu-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/Roles/Edit/Editor']").Last);
            await scene.TypeAsync(scene.Page.GetByPlaceholder("Search permissions"), "soft phone", delay: 70);
            await scene.PauseAsync(800);
            await scene.HighlightAsync(scene.Page.Locator("tr").Filter(new() { HasTextString = "Use the telephony soft phone" }).First, "Use the telephony soft phone");
            await scene.PauseAsync(3000);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("menu-2", async () =>
        {
            await scene.NavigateAsync("/Admin/Settings/contactcenter");
            await scene.PauseAsync(600);
            await scene.ClickAsync(scene.Page.GetByText("Recording governance", new() { Exact = true }).First, pauseAfter: 700);
            await scene.HighlightAsync(scene.Page.GetByText("Allow agents to pause recording").First, "Allow agents to pause recording");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.GetByText("Secure Data Capture", new() { Exact = true }).First, pauseAfter: 700);
            await scene.HighlightAsync(scene.Page.GetByText("Enable agent-assisted secure data capture").First, "Enable agent-assisted secure data capture");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("menu-3", async () =>
        {
            await scene.NavigateAsync("/Admin/Settings/telephony");
            await scene.PauseAsync(600);
            await ScrollToAsync(scene, Section(scene.Page.Locator("#ISite_SoftPhoneWidgetSettings_EnableDiagnostics")), margin: 300);
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_SoftPhoneWidgetSettings_EnableDiagnostics")), "Enable the diagnostics tab");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();

            if (!await scene.Page.Locator("#ISite_SoftPhoneWidgetSettings_EnableDiagnostics").IsCheckedAsync())
            {
                await scene.ClickAsync(scene.Page.Locator("#ISite_SoftPhoneWidgetSettings_EnableDiagnostics"), pauseAfter: 500);
            }

            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task EmptyAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/dashboard");
        await scene.StartCaptureAsync();

        await scene.StepAsync("empty-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("No queues are configured.").First, "No queues on this administrator's record");
            await scene.PauseAsync(8000);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("empty-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".telephony-soft-phone__toggle"), pauseAfter: 1200);
            await scene.ClickAsync(scene.Page.Locator("[data-telephony-tab='contact-center']"), pauseAfter: 900);
            await scene.HighlightAsync(scene.Page.GetByText("Select queue(s)").First, "Only what you are entitled to");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task RingAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/workspace");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ring-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Connected", new() { Exact = true }).First, "Connected");
            await scene.PauseAsync(8000);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task DiagAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/omnichannel/campaigns");
        await scene.StartCaptureAsync();

        await scene.StepAsync("audio-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".telephony-soft-phone__toggle"), pauseAfter: 1200);
            await scene.ClickAsync(scene.Page.Locator("[data-telephony-tab='diagnostics']"), pauseAfter: 900);
            await scene.HighlightAsync(scene.Page.GetByText("Run audio test").First, "Run audio test");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task DialerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/dashboard");
        await scene.StartCaptureAsync();

        await scene.StepAsync("dialer-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Why isn't this dialing?").First, "Why isn't this dialing?");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.GetByText("No work is waiting to be dispatched.").First, "What holds a campaign back");
            await scene.PauseAsync(3000);
        });

        await scene.ClearHighlightsAsync();
    }
}
