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
        ("02-dial", DialAsync),
        ("04-extension", ExtensionAsync),
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

    /// <summary>
    /// Gives two fake users an extension each (not recorded).
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await Seed.OpenAsync(browser);

        await Seed.ExtensionAsync(page, "Jamie Chen", "1001", "jamie.chen");
        await Seed.ExtensionAsync(page, "Maria Lopez", "1002", "maria.lopez");
        await context.CloseAsync();
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    private static ILocator Number(Scene scene) => scene.Page.Locator("input.telephony-soft-phone__number").First;

    private static ILocator DialMode(Scene scene) => scene.Page.Locator("button.telephony-soft-phone__dial-mode").First;

    private static async Task DialAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/softphone");
        await scene.StartCaptureAsync();

        await scene.StepAsync("dl-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("button.telephony-soft-phone__tab").Filter(new() { HasTextString = "Keypad" }).First, "Keypad");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("button.iti__selected-country").First, "Country flag");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(Number(scene), "2125550199", delay: 120);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("dl-2", async () =>
        {
            await scene.HighlightAsync(Number(scene), "Formatted as you type");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("button.btn-success, .telephony-soft-phone__call, button[aria-label='Call']").First, "Call");
            await scene.PauseAsync(2500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ExtensionAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/softphone");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ext-1", async () =>
        {
            await scene.ClickAsync(DialMode(scene), pauseAfter: 900);
            await scene.HighlightAsync(Number(scene), "Search a name, or enter an extension");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ext-2", async () =>
        {
            await scene.TypeAsync(Number(scene), "1001", delay: 160);
            await scene.PauseAsync(2500);
            await Number(scene).FillAsync(string.Empty);
            await scene.TypeAsync(Number(scene), "mar", delay: 160);
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("ext-3", async () =>
        {
            await Number(scene).FillAsync(string.Empty);
            await scene.ClickAsync(DialMode(scene), pauseAfter: 1200);
            await scene.HighlightAsync(DialMode(scene), "Dial extension again, or Dial phone number");
            await scene.PauseAsync(2000);
        });

        await scene.ClearHighlightsAsync();
    }
}
