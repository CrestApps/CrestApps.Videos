using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Soft Phone Administration video, one method per "clip" of storyboard.json.
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
        ("02-tab", TabAsync),
        ("03-widget", WidgetAsync),
        ("04-dial", DialAsync),
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

    private const string Tab = "tab-soft-phone";

    private static async Task TabAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenTabAsync(browser, directory, durations, Tab);

        await scene.StepAsync("tab-1", async () =>
        {
            await ShowAsync(scene, Tab, "Default telephony provider", "Every call goes through it", 4000);
            await ShowAsync(scene, Tab, "Allowed short codes", "One per line", 4000);
        });

        await scene.StepAsync("tab-2", async () =>
        {
            await ShowAsync(scene, Tab, "Enable the soft phone", "Master switch", 3000);
            await ShowAsync(scene, Tab, "Show the soft phone on the admin dashboard", "On admin pages", 3500);
        });

        await scene.StepAsync("tab-3", async () =>
        {
            var diagnostics = Field(scene, Tab, "Enable the diagnostics tab");
            var box = diagnostics.Locator("input[type=checkbox]").First;

            if (!await box.IsCheckedAsync())
            {
                await scene.ClickAsync(box, pauseAfter: 600);
            }

            await scene.HighlightAsync(diagnostics, "A tab for troubleshooting");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await ShowAsync(scene, Tab, "Recent calls to display", "The Recent tab", 2500);
        });

        await scene.StepAsync("tab-4", async () =>
        {
            await ShowAsync(scene, Tab, "Default country", "For national numbers", 3000);
            await ShowAsync(scene, Tab, "Accent color", "The phone's color", 2500);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).Last.ClickAsync()));
        });
    }

    private static async Task WidgetAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("wid-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".telephony-soft-phone__toggle").First, "The soft phone");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator(".telephony-soft-phone__toggle").First, pauseAfter: 1000);
        });

        await scene.StepAsync("wid-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("[data-telephony-tab='history']").First, pauseAfter: 1500);
            await scene.ClickAsync(scene.Page.Locator("[data-telephony-tab='voicemail']").First, pauseAfter: 1500);
            await scene.ClickAsync(scene.Page.Locator("[data-telephony-tab='keypad']").First, pauseAfter: 1000);
        });

        await scene.StepAsync("wid-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("[data-telephony-tab='diagnostics']").First, pauseAfter: 1500);
            await scene.HighlightAsync(scene.Page.Locator("[data-telephony-tab='diagnostics']").First, "Diagnostics");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task DialAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);
        await scene.OpenAsync("/Admin/Contents/ContentTypes/Contact/Create");
        await scene.StartCaptureAsync();

        await scene.StepAsync("dial-1", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Add Item" }).First, pauseAfter: 900);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show a", new() { HasTextString = "Phone Number" }).First, pauseAfter: 1200);
            await scene.TypeAsync(scene.Page.Locator("input.iti__tel-input:visible").First, "212 555 0134", delay: 60);
            await scene.ScrollAsync(0, 250);
        });

        await scene.StepAsync("dial-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("[data-phone-dial]").First, "Call with the soft phone");
            await scene.PauseAsync(3000);
        });

        await scene.ClearHighlightsAsync();
    }
}
