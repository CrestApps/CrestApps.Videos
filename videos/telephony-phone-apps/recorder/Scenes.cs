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
        ("03-admin", AdminAsync),
        ("04-page", PageAsync),
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

    private static ILocator Feature(Scene scene, string id)
        => scene.Page.Locator($"#btn-enable-{id.Replace('.', '_')}, #btn-disable-{id.Replace('.', '_')}").First.Locator("xpath=ancestor::li[contains(@class,'list-group-item')][1]");

    private static async Task AdminAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();

        await scene.StepAsync("adm-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "Soft Phone", delay: 45);
            await scene.PauseAsync(700);
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = "Telephony Soft Phone Extension" }).First, "Telephony Soft Phone Extension");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("adm-2", async () =>
        {
            await scene.NavigateAsync("/Admin/Roles/Edit/Editor");
            await scene.TypeAsync(scene.Page.GetByPlaceholder("Search permissions"), "soft phone", delay: 40);
            await scene.PauseAsync(500);
            await scene.HighlightAsync(scene.Page.Locator("text=Use the telephony soft phone").First, "Use the telephony soft phone");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("adm-3", async () =>
        {
            await scene.PauseAsync(2500);
        });
    }

    private static async Task PageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/softphone");
        await scene.StartCaptureAsync();

        await scene.StepAsync("pg-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("button.telephony-soft-phone__tab").First.Locator("xpath=.."), "Keypad, Recent, Voicemail, Work");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button.telephony-soft-phone__tab").Filter(new() { HasTextString = "Recent" }).First, pauseAfter: 1500);
            await scene.ClickAsync(scene.Page.Locator("button.telephony-soft-phone__tab").Filter(new() { HasTextString = "Voicemail" }).First, pauseAfter: 1500);
            await scene.ClickAsync(scene.Page.Locator("button.telephony-soft-phone__tab").Filter(new() { HasTextString = "Work" }).First, pauseAfter: 1500);
            await scene.ClickAsync(scene.Page.Locator("button.telephony-soft-phone__tab").Filter(new() { HasTextString = "Keypad" }).First, pauseAfter: 1000);
        });

        await scene.ClearHighlightsAsync();
    }
}
