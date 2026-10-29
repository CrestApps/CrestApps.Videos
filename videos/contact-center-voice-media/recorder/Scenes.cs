using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Voice Media video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // The fake hold music: a short tone made with ffmpeg, kept outside the repository (VIDEO_AUDIO).
    public static string AudioPath { get; } = Environment.GetEnvironmentVariable("VIDEO_AUDIO") ?? throw new InvalidOperationException("Set VIDEO_AUDIO to an MP3 file.");

    private const string ListPath = "/Admin/contact-center/voice-media";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-add", AddAsync),
        ("04-manage", ManageAsync),
        ("06-delete", DeleteAsync),
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

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static ILocator Row(Scene scene) => scene.Page.Locator(".list-group-item").Filter(new() { HasTextString = "Main hold music" }).First;

    private static async Task AddAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync(ListPath);
        await scene.StartCaptureAsync();

        await scene.StepAsync("add-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, ListPath), "Interaction Center > Management > Voice Media");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create"));
        });

        await scene.StepAsync("add-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#VoiceMediaItem_Name"), "Main hold music", delay: 55);
            await scene.TypeAsync(scene.Page.Locator("#VoiceMediaItem_Description"), "Plays while callers wait in any queue", delay: 30);
        });

        await scene.StepAsync("add-3", async () =>
        {
            await scene.Page.Locator("#VoiceMediaItem_Audio").SetInputFilesAsync(AudioPath);
            await scene.HighlightAsync(Section(scene.Page.Locator("#VoiceMediaItem_Audio")), "Audio");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("add-4", async () =>
        {
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => SaveButton(scene).ClickAsync()));
        });

        await scene.StepAsync("add-5", async () =>
        {
            await scene.HighlightAsync(Row(scene), "Format, Master copy badge, and when it was saved");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ManageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync(ListPath);
        await scene.StartCaptureAsync();

        await scene.StepAsync("man-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Row(scene).GetByText("Edit", new() { Exact = true }));
            await scene.HighlightAsync(Section(scene.Page.Locator("#VoiceMediaItem_Audio")), "Audio");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("man-2", async () =>
        {
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("man-3", async () =>
        {
            await scene.NavigateAsync(ListPath);
            await scene.HighlightAsync(Row(scene).GetByText("Re-sync media"), "Re-sync media");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => Row(scene).GetByText("Re-sync media").ClickAsync()));
        });

        await scene.StepAsync("man-4", async () =>
        {
            await scene.PauseAsync(2500);
            await scene.HighlightAsync(Row(scene), "A message for each provider");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task DeleteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync(ListPath);
        await scene.StartCaptureAsync();

        await scene.StepAsync("del-1", async () =>
        {
            await scene.ClickAsync(Row(scene).GetByText("Delete", new() { Exact = true }), pauseAfter: 1200);
            await scene.HighlightAsync(scene.Page.Locator(".modal.show .modal-content").First, "Confirm");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".modal.show .btn-danger, .modal.show .btn-primary").First);
            await scene.PauseAsync(2500);
        });
    }
}
