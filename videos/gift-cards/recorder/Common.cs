using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// What every clip of the video shares: the paths, the list of clips, and small helpers.
/// </summary>
public static partial class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

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

    // A shopper who is not signed in: a clip of the storefront starts from an empty browser.
    private static Task<Scene> NewGuestSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string statePath = null)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, statePath ?? Path.Combine(Root, "no-state.json"), Path.GetDirectoryName(directory), durations);

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    // The block of an editor that holds a label, such as a field with its hint.
    private static ILocator Field(Scene scene, string label)
        => scene.Page.Locator("label, span.ocat-label, h5").Filter(new() { HasTextString = label }).First
            .Locator("xpath=ancestor::div[contains(@class,'ocat-wrapper') or contains(@class,'ocat-limited-wrapper') or contains(@class,'mb-3') or contains(@class,'form-group')][1]");

    private static ILocator Row(Scene scene, string text) => scene.Page.Locator("li, tr").Filter(new() { HasTextString = text }).Last;

    private static ILocator Link(Scene scene, string text) => scene.Page.Locator("a, button").Filter(new() { HasTextString = text }).First;

    private static ILocator Publish(Scene scene) => scene.Page.Locator("button.publish").First;

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    // A wheel turn over a focused number box changes its value: leave the box first.
    private static async Task ScrollAsync(Scene scene, double deltaX, double deltaY)
    {
        await scene.Page.EvaluateAsync("() => document.activeElement && document.activeElement.blur()");
        await scene.ScrollAsync(deltaX, deltaY);
    }

    private static async Task ShowAsync(Scene scene, ILocator target, string caption, int hold = 1800)
    {
        await scene.HighlightAsync(target, caption);
        await scene.PauseAsync(hold);
        await scene.ClearHighlightsAsync();
    }
}
