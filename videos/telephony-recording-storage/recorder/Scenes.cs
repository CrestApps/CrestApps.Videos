using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Where Call Recordings Are Stored video, one method per "clip" of storyboard.json.
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
        ("02-screens", ScreensAsync),
        ("03-azure", AzureAsync),
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

    private static async Task ScreensAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);
        await scene.OpenAsync("/Admin/contact-center/call-recordings");
        await scene.StartCaptureAsync();

        await scene.StepAsync("scr-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("h1").First, "Interaction Center > Call recordings");
            await scene.PauseAsync(5000);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("scr-2", async () =>
        {
            await scene.NavigateAsync("/Admin/contact-center/voice-media");
            await scene.HighlightAsync(scene.Page.Locator("h1").First, "Interaction Center > Management > Voice Media");
            await scene.PauseAsync(2500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task AzureAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);
        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();

        await scene.StepAsync("az-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "Azure Blob", delay: 70);
            await scene.PauseAsync(1200);
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item").Filter(new() { HasText = "Telephony - Azure Blob Storage" }).First, "Telephony - Azure Blob Storage");
        });

        await scene.StepAsync("az-2", async () =>
        {
            await scene.PauseAsync(1000);
        });

        await scene.StepAsync("az-3", async () =>
        {
            await scene.PauseAsync(1000);
        });

        await scene.ClearHighlightsAsync();
    }
}
