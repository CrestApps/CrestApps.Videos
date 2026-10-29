using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Voicemail video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // The fake greeting: a short tone made with ffmpeg, kept outside the repository (VIDEO_AUDIO).
    public static string AudioPath { get; } = Environment.GetEnvironmentVariable("VIDEO_AUDIO") ?? throw new InvalidOperationException("Set VIDEO_AUDIO to an MP3 file.");

    private const string GreetingPath = "/Admin/contact-center/my-voicemail-greeting";

    private const string SharedPath = "/Admin/contact-center/shared-voicemail";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-greeting", GreetingAsync),
        ("04-listen", ListenAsync),
        ("06-shared", SharedAsync),
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

    private static ILocator Picker(Scene scene, string selectName) => scene.Page.Locator($"select[name='{selectName}']").Locator("xpath=following-sibling::button[1]");

    private static async Task GreetingAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync(GreetingPath);
        await scene.StartCaptureAsync();

        await scene.StepAsync("greet-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, GreetingPath), "Interaction Center > My voicemail greeting");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.GetByText("Record or upload the greeting").First, "What callers hear");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("greet-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("label[for='vm-source-mic']"), "Record with microphone");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Record" }).First, "Record");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("greet-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("label[for='vm-source-file']"), pauseAfter: 900);
            await scene.Page.Locator("input[type=file]").SetInputFilesAsync(AudioPath);
            await scene.HighlightAsync(scene.Page.Locator("input[type=file]"), "Upload a file");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("greet-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("audio").First, "Preview");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.GetByRole(AriaRole.Button, new() { Name = "Save greeting" }).ClickAsync()));
        });

        await scene.StepAsync("greet-5", async () =>
        {
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Remove greeting" }), "Remove greeting");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ListenAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync(SharedPath);
        await scene.StartCaptureAsync();

        await scene.StepAsync("listen-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".telephony-soft-phone__toggle"), pauseAfter: 1200);
            await scene.ClickAsync(scene.Page.Locator("[data-telephony-tab='voicemail']"), pauseAfter: 900);
            await scene.HighlightAsync(scene.Page.Locator("[data-telephony-tab='voicemail']"), "Voicemail");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("listen-2", async () =>
        {
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("listen-3", async () =>
        {
            await scene.PauseAsync(1000);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SharedAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync(SharedPath);
        await scene.StartCaptureAsync();

        await scene.StepAsync("shared-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, SharedPath), "Interaction Center > Shared voicemail");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.GetByText("There is no shared voicemail here").First, "Messages left on a queue's line appear here");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("shared-2", async () =>
        {
            await scene.HighlightAsync(Picker(scene, "queueId"), "Queue");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Picker(scene, "status"), "Status");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Filter" }).First, "Filter");
        });

        await scene.ClearHighlightsAsync();
    }
}
