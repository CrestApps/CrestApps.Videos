using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Telnyx Settings video, one method per "clip" of storyboard.json.
/// The site is the Contact Center demo site with Asterisk set up with a fake address (see README.md); nothing reaches a phone provider.
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
        ("02-enable", EnableAsync),
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

    // The group (label, input and hint) of a field of a settings tab.
    private static ILocator Field(Scene scene, string tab, string label)
        => scene.Page.Locator($"#{tab} label", new() { HasTextString = label }).First.Locator("xpath=..");

    private static async Task ShowAsync(Scene scene, string tab, string label, string caption, int hold = 2500, bool keep = false)
    {
        await scene.HighlightAsync(Field(scene, tab, label), caption);
        await scene.PauseAsync(hold);

        if (!keep)
        {
            await scene.ClearHighlightsAsync();
        }
    }

    private static async Task<Scene> OpenTabAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string tab)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);
        await scene.OpenAsync("/Admin/Settings/telephony");
        await scene.Page.Locator($"a[href='#{tab}']").First.ClickAsync();
        await Task.Delay(500);
        await scene.StartCaptureAsync();
        return scene;
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenTabAsync(browser, directory, durations, "tab-telnyx");

        await scene.StepAsync("en-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Connect to Telnyx in a few clicks").First.Locator("xpath=.."), "Four steps");
            await scene.PauseAsync(6000);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("en-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("label", new() { HasTextString = "Enable Telnyx provider" }).First, "Make it selectable");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await ShowAsync(scene, "tab-telnyx", "API key", "Stored encrypted", 3000, keep: true);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("en-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Enter your API key above and click Save").First, "Connect appears once a key is stored");
            await scene.PauseAsync(2000);
        });

        await scene.ClearHighlightsAsync();
    }
}
