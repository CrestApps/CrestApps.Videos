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
        ("03-reading", ReadingAsync),
        ("04-board", BoardAsync),
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

    private static ILocator Panel(Scene scene, string text) => scene.Page.Locator(".cc-panel").Filter(new() { HasTextString = text }).First;

    private static async Task ScrollToAsync(Scene scene, ILocator locator, double offset = 160)
    {
        var top = await locator.EvaluateAsync<double>("e => e.getBoundingClientRect().top");
        await scene.ScrollAsync(0, top - offset);
    }

    private static async Task ReadingAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("rd-1", async () =>
        {
            await scene.NavigateAsync("/Admin/contact-center/dashboard");
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.Locator(".cc-connection").First, "Connected");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rd-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".cc-dashboard__summary").First, "Summary");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rd-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".cc-tile").First, "Queue tile");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".cc-tiles").First, "Queue tiles");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rd-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".alert.alert-secondary").First, "Dialer banner");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, Panel(scene, "Why isn't this dialing?"), 140);
            await scene.HighlightAsync(Panel(scene, "Why isn't this dialing?"), "Why isn't this dialing?");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task BoardAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/dashboard");
        await scene.Page.WaitForSelectorAsync(".cc-agent__kebab");
        await ScrollToAsync(scene, scene.Page.Locator(".cc-panel").Filter(new() { Has = scene.Page.Locator("input[placeholder='Search by name']") }).First, 40);
        await scene.StartCaptureAsync();

        await scene.StepAsync("brd-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".cc-agent__kebab").First.Locator("xpath=../../.."), "Agent card");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("brd-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("input[placeholder='Search by name']").First, "adm", delay: 80);
            await scene.PauseAsync(1200);
            await scene.SelectAsync(scene.Page.Locator("select").Filter(new() { HasTextString = "Any queue" }).First, "Support", pauseAfter: 1200);
            await scene.SelectAsync(scene.Page.Locator("select").Filter(new() { HasTextString = "Any status" }).First, "Available", pauseAfter: 1200);
        });

        await scene.StepAsync("brd-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".cc-panel").Filter(new() { Has = scene.Page.Locator("input[placeholder='Search by name']") }).First.Locator(".cc-panel__header").First, "Agents that match");
            await scene.PauseAsync(2500);
        });

        await scene.ClearHighlightsAsync();
    }
}
