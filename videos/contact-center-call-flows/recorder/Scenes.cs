using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Call flows video, one method per "clip" of storyboard.json.
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
        ("02-entry", EntryAsync),
        ("03-queue", QueueAsync),
        ("04-limits", LimitsAsync),
        ("05-after", AfterAsync),
        ("06-profile", ProfileAsync),
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
    private static ILocator Label(Scene scene, string text)
        => scene.Page.Locator("label, .ocat-label").Filter(new() { HasTextString = text }).First;

    private static async Task OpenMenuAsync(Scene scene, string href)
    {
        var item = Menu(scene, href);

        if (!await item.IsVisibleAsync())
        {
            await scene.ClickAsync(scene.Page.Locator("#left-nav button.interaction-center").First, pauseAfter: 800);
        }

        if (!await item.IsVisibleAsync())
        {
            await scene.ClickAsync(scene.Page.Locator("#left-nav button.interaction-center-management").First, pauseAfter: 800);
        }
    }

    private static async Task EntryAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/entry-points/create?channel=Phone");
        await ScrollToAsync(scene, Card(scene, "Welcome and IVR menu"), margin: 120);
        await scene.StartCaptureAsync();

        await scene.StepAsync("ent-1", async () =>
        {
            await scene.HighlightAsync(Section(Label(scene, "Welcome message")), "Welcome message");
            await scene.PauseAsync(4000);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ent-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button, a").Filter(new() { HasTextString = "Build an IVR menu" }).First, pauseAfter: 1500);
            await scene.ScrollAsync(0, 250);
            await scene.HighlightAsync(Label(scene, "Tries"), "Tries");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Label(scene, "When the tries run out"), "When the tries run out");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task QueueAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/queues/create");
        await ScrollToAsync(scene, Card(scene, "While callers wait"), margin: 120);
        await scene.StartCaptureAsync();

        await scene.StepAsync("que-1", async () =>
        {
            await scene.HighlightAsync(Section(Label(scene, "Welcome message")), "Welcome message");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Label(scene, "Hold music")), "Hold music");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("que-2", async () =>
        {
            await scene.ScrollAsync(0, 300);
            await scene.HighlightAsync(Section(Label(scene, "Announce the caller's place in line")), "Announce the caller's place in line");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Label(scene, "Average handle time")), "Average handle time");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("que-3", async () =>
        {
            await ScrollToAsync(scene, Card(scene, "Callback"), margin: 120);
            await scene.HighlightAsync(Card(scene, "Callback"), "Callback");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task LimitsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/queues/create");
        await ScrollToAsync(scene, Card(scene, "Limits"), margin: 120);
        await scene.StartCaptureAsync();

        await scene.StepAsync("lim-1", async () =>
        {
            await scene.HighlightAsync(Section(Label(scene, "Queue-full action")), "Queue-full action");
            await scene.PauseAsync(4500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Label(scene, "Maximum-wait action")), "Maximum-wait action");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("lim-2", async () =>
        {
            await scene.NavigateAsync("/Admin/contact-center/shared-voicemail");
            await scene.PauseAsync(800);
            await OpenMenuAsync(scene, "/Admin/contact-center/shared-voicemail");
            await scene.HighlightAsync(scene.Page.Locator("#left-nav a[href$='/Admin/contact-center/shared-voicemail']").First, "Interaction Center > Shared voicemail");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task AfterAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/omnichannel/subject-flows");
        await scene.StartCaptureAsync();

        await scene.StepAsync("aft-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("a").Filter(new() { HasTextString = "Manage Flow" }).First, "Manage Flow");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a").Filter(new() { HasTextString = "Manage Flow" }).First);
            await scene.ClickAsync(scene.Page.Locator("a, button").Filter(new() { HasTextString = "Add Action" }).First, pauseAfter: 1200);
        });

        await scene.StepAsync("aft-2", async () =>
        {
            await scene.NavigateAsync("/Admin/contact-center/agent-states");
            await scene.PauseAsync(1500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ProfileAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/dialers");
        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn").Filter(new() { HasTextString = "Edit" }).First);
        await scene.StartCaptureAsync();

        await scene.StepAsync("out-4", async () =>
        {
            await ScrollToAsync(scene, Section(scene.Page.Locator("#DialerProfile_MaxAttempts")), margin: 250);
            await scene.HighlightAsync(Section(scene.Page.Locator("#DialerProfile_MaxAttempts")), "Max attempts");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(scene.Page.Locator("#DialerProfile_RetryDelayMinutes")), "Retry delay (minutes)");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, Card(scene, "Compliance"), margin: 120);
            await scene.HighlightAsync(Card(scene, "Compliance").Locator(".card-header"), "Compliance");
        });

        await scene.ClearHighlightsAsync();
    }
}
