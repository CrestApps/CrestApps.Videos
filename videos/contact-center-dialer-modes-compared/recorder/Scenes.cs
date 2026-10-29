using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Dialer modes compared video, one method per "clip" of storyboard.json.
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
        ("02-preview", PreviewAsync),
        ("03-power", PowerAsync),
        ("04-progressive", ProgressiveAsync),
        ("05-predictive", PredictiveAsync),
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
    private static ILocator Field(Scene scene, string name) => scene.Page.Locator($"#DialerProfile_{name}");

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

    private static async Task OpenFormAsync(Scene scene)
    {
        await scene.OpenAsync("/Admin/contact-center/dialers");
        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create"));
    }

    private static async Task PreviewAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/dialers");
        await scene.StartCaptureAsync();

        await scene.StepAsync("prev-3", async () =>
        {
            await OpenMenuAsync(scene, "/Admin/contact-center/dialers");
            await scene.HighlightAsync(Menu(scene, "/Admin/contact-center/dialers"), "Interaction Center > Management > Dialer Profiles");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create"));
            await scene.PauseAsync(500);
            await ScrollToAsync(scene, Card(scene, "Dialing"), margin: 120);
            await scene.HighlightAsync(Section(Field(scene, "Mode")), "Mode: Preview");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Field(scene, "ProviderName")), "Only Mode and the provider are left");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 500);
            await scene.HighlightAsync(Card(scene, "Compliance").Locator(".card-header"), "Last card: Compliance");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PowerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await OpenFormAsync(scene);
        await ScrollToAsync(scene, Card(scene, "Dialing"), margin: 120);
        await scene.StartCaptureAsync();

        await scene.StepAsync("pow-3", async () =>
        {
            await PickAsync(scene, "DialerProfile_Mode", "Power");
            await scene.PauseAsync(500);
            await scene.HighlightAsync(Section(Field(scene, "CallsPerAgent")), "Calls per agent: calls started in one look");
            await scene.PauseAsync(2000);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("pow-4", async () =>
        {
            await scene.ScrollAsync(0, 250);
            await scene.HighlightAsync(Section(Field(scene, "RingTimeoutSeconds")), "Ring time");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Field(scene, "AnsweringMachineDetection")), "Screen out answering machines");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Field(scene, "ShowDialProgressToAgent")), "Show each dial to the agent");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 900);
            await scene.HighlightAsync(Card(scene, "Abandoned calls").Locator(".card-header"), "Abandoned calls");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ProgressiveAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await OpenFormAsync(scene);
        await ScrollToAsync(scene, Card(scene, "Dialing"), margin: 120);
        await scene.StartCaptureAsync();

        await scene.StepAsync("prog-1", async () =>
        {
            await PickAsync(scene, "DialerProfile_Mode", "Progressive");
            await scene.PauseAsync(500);
            await scene.HighlightAsync(Section(Field(scene, "Mode")), "Progressive: no Calls per agent");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Field(scene, "MaxAttempts")), "The next field is Max attempts");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("prog-2", async () =>
        {
            await scene.ScrollAsync(0, 1100);
            await scene.HighlightAsync(Card(scene, "Abandoned calls").Locator(".card-header"), "Abandoned calls: the same card as Power");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PredictiveAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await OpenFormAsync(scene);
        await ScrollToAsync(scene, Card(scene, "Dialing"), margin: 120);
        await scene.StartCaptureAsync();

        await scene.StepAsync("pred-1", async () =>
        {
            await PickAsync(scene, "DialerProfile_Mode", "Predictive");
            await scene.PauseAsync(500);
            await ScrollToAsync(scene, Card(scene, "Predictive pacing"), margin: 120);
            await scene.HighlightAsync(Section(Field(scene, "PredictivePacingModel")), "Pacing: One call per reserved agent");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("pred-2", async () =>
        {
            await scene.SelectAsync(Field(scene, "PredictivePacingModel"), "OverDial", pauseAfter: 900);
            await scene.HighlightAsync(Section(Field(scene, "PredictivePacingModel")), "Pacing: Over-dial");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("pred-3", async () =>
        {
            await scene.HighlightAsync(Section(Field(scene, "TargetAbandonmentRatePercent")), "Target abandonment rate");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 200);
            await scene.HighlightAsync(Section(Field(scene, "MaxLinesPerAgent")), "Lines per agent");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Field(scene, "MaxCallsInFlight")), "Calls in flight");
        });

        await scene.ClearHighlightsAsync();
    }
}
