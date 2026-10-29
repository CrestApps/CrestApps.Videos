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
        ("06-recent", RecentAsync),
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

    private static ILocator Tab(Scene scene, string name) => scene.Page.Locator("button.telephony-soft-phone__tab").Filter(new() { HasTextString = name }).First;

    private static async Task RecentAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/workspace");
        await scene.StartCaptureAsync();

        await scene.StepAsync("rec-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".cc-panel").Filter(new() { HasTextString = "Recent activity" }).First, "Recent activity");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rec-2", async () =>
        {
            await scene.NavigateAsync("/softphone");
            await scene.ClickAsync(Tab(scene, "Recent"), pauseAfter: 1200);
            await scene.HighlightAsync(Tab(scene, "Recent"), "Recent");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rec-3", async () =>
        {
            await scene.ClickAsync(Tab(scene, "Voicemail"), pauseAfter: 1200);
            await scene.HighlightAsync(Tab(scene, "Voicemail"), "Voicemail");
        });

        await scene.ClearHighlightsAsync();
    }
}
