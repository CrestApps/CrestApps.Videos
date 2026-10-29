using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Asterisk Setup video, one method per "clip" of storyboard.json.
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
        ("02-connect", ConnectAsync),
        ("03-voicemail", VoicemailAsync),
        ("04-browser", BrowserAsync),
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

    private const string Tab = "tab-asterisk";

    private static async Task ConnectAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);
        await scene.OpenAsync("/Admin/Settings/telephony");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ast-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("a[href='#tab-asterisk']").First, pauseAfter: 1000);
            await scene.HighlightAsync(scene.Page.Locator("label", new() { HasTextString = "Enable Asterisk provider" }).First, "Enable the provider");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ast-2", async () =>
        {
            await ShowAsync(scene, Tab, "ARI base URL", "Address of Asterisk's web interface", 3000);
            await ShowAsync(scene, Tab, "ARI user name", "Signs the platform in", 2500);
            await ShowAsync(scene, Tab, "ARI password", "Stored encrypted", 2500);
        });

        await scene.StepAsync("ast-3", async () =>
        {
            await ShowAsync(scene, Tab, "Stasis application name", "Required, and unique per site", 7000);
        });

        await scene.StepAsync("ast-4", async () =>
        {
            await ShowAsync(scene, Tab, "Endpoint template", "Dialed number to endpoint", 3500);
            await ShowAsync(scene, Tab, "Outbound caller id", "Shown on outbound calls", 2500);
            await ShowAsync(scene, Tab, "Dial timeout", "How long it keeps ringing", 2500, keep: true);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task VoicemailAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenTabAsync(browser, directory, durations, Tab);

        await scene.StepAsync("vm-1", async () =>
        {
            await ShowAsync(scene, Tab, "Voicemail context", "Where the call continues", 3500);
            await ShowAsync(scene, Tab, "Voicemail extension template", "Which extension", 4000);
        });

        await scene.StepAsync("vm-2", async () =>
        {
            await ShowAsync(scene, Tab, "Voicemail priority", "Where in the extension it starts", 3500, keep: true);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task BrowserAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenTabAsync(browser, directory, durations, Tab);

        await scene.StepAsync("br-1", async () =>
        {
            await ShowAsync(scene, Tab, "SIP WebSocket URL", "The browser connects here", 3500);
            await ShowAsync(scene, Tab, "SIP domain", "Builds each browser login", 3000);
        });

        await scene.StepAsync("br-2", async () =>
        {
            await ShowAsync(scene, Tab, "ICE server URLs", "Help audio cross firewalls", 3500);
            await ShowAsync(scene, Tab, "TURN shared secret", "Short-lived relay credentials", 3500);
            await ShowAsync(scene, Tab, "ICE transport policy", "Direct paths, or the relay", 2500);
        });

        await scene.StepAsync("br-3", async () =>
        {
            await ShowAsync(scene, Tab, "Browser audio codecs", "In order of preference", 3500);
            await ShowAsync(scene, Tab, "PJSIP credential lifetime", "How long a login lasts", 3000);
            await ShowAsync(scene, Tab, "PJSIP contact expiration", "How long a registration lasts", 2500);
        });

        await scene.StepAsync("br-4", async () =>
        {
            await ShowAsync(scene, Tab, "PJSIP Realtime provider", "Database provider", 3000);
            await ShowAsync(scene, Tab, "PJSIP Realtime connection string", "Stored encrypted", 3000);
            await ShowAsync(scene, Tab, "PJSIP Realtime table prefix", "Optional", 2500, keep: true);
        });

        await scene.ClearHighlightsAsync();
    }
}
