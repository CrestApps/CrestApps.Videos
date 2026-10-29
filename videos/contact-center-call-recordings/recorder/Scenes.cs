using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Call Recordings video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string ListPath = "/Admin/contact-center/call-recordings";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("04-search", SearchAsync),
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

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    // A bootstrap-select box shows the select as a button next to it.
    private static ILocator Picker(Scene scene, string selectName) => scene.Page.Locator($"select[name='{selectName}']").Locator("xpath=following-sibling::button[1]");

    private static async Task SearchAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync(ListPath);
        await scene.StartCaptureAsync();

        await scene.StepAsync("find-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, ListPath), "Interaction Center > Call recordings");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.GetByText("No recorded call matches").First, "Listed once the recording is saved");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("find-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Any date").First, "Started");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("find-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#Number"), "5550142", delay: 90);
            await scene.HighlightAsync(scene.Page.Locator("#Number"), "Phone number");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("find-4", async () =>
        {
            await scene.HighlightAsync(Picker(scene, "Direction"), "Direction");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Picker(scene, "Source"), "Call type");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("find-5", async () =>
        {
            await scene.HighlightAsync(Picker(scene, "AgentUserId"), "Agent");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("find-6", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Filter" }).First);
            await scene.PauseAsync(1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByText("Clear", new() { Exact = true }).First);
        });
    }
}
