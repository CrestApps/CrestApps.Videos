using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Contact Center Secure Capture video, one method per "clip" of storyboard.json.
/// Record 02-settings, 03-agent and 04-customer in that order on the Development site made by seed-live-call.py (see README.md):
/// 03 writes the one-time link to secure-link.txt, which 04 and 06 read. Record 06-production on a site started in Production.
/// Needs VIDEO_INIT_SCRIPT=secure-link-overlay.js, which shows the native prompt of the link as a dialog that can be filmed.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static string LinkPath => Path.Combine(Root, "secure-link.txt");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-settings", SettingsAsync),
        ("03-agent", AgentAsync),
        ("04-customer", CustomerAsync),
        ("06-production", ProductionAsync),
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

    private static ILocator Button(Scene scene, string name) => scene.Page.GetByRole(AriaRole.Button, new() { Name = name }).First;

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync("/Admin");
        await scene.Page.Locator("#left-nav").GetByText("Settings", new() { Exact = true }).First.ClickAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("set-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#left-nav a[href$='/Admin/Settings/contactcenter']").First);
            await scene.ClickAsync(scene.Page.Locator("a[href^='#tab-']", new() { HasTextString = "Secure Data Capture" }).First, pauseAfter: 1000);
        });

        await scene.StepAsync("set-2", async () =>
        {
            var enable = scene.Page.Locator("label", new() { HasTextString = "Enable agent-assisted secure data capture" }).First;
            await scene.ClickAsync(enable, pauseAfter: 1200);
            await scene.HighlightAsync(enable, "Master switch");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("set-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("label", new() { HasTextString = "Capture link lifetime" }).First, "How long the link works");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("label", new() { HasTextString = "Pause recording during capture" }).First, "Pause the recording while the customer types");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).Last.ClickAsync()));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task AgentAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync("/Admin/contact-center/workspace");
        await scene.Page.GetByText("Collect data securely").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("agent-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Active interaction").First, "Active interaction");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Button(scene, "Collect data securely"), "Collect data securely");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("agent-2", async () =>
        {
            await scene.ClickAsync(Button(scene, "Collect data securely"), pauseAfter: 1500);
            await scene.HighlightAsync(scene.Page.Locator("#demo-secure-link-dialog"), "A one-time link for the customer");
            var link = await scene.Page.EvaluateAsync<string>("() => window.__secureCaptureLink");
            await File.WriteAllTextAsync(LinkPath, link ?? string.Empty);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("agent-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("[data-demo-secure-link-ok]"), pauseAfter: 800);
            await scene.HighlightAsync(scene.Page.GetByText("Recording", new() { Exact = true }).First, "Recording stays on until the capture ends");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task CustomerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var link = (await File.ReadAllTextAsync(LinkPath)).Trim();
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync(new Uri(link).PathAndQuery);
        await scene.StartCaptureAsync();

        await scene.StepAsync("cust-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Secure page").First, "Secure page");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.GetByText("The agent on your call cannot see what you type here.").First, "Not visible to the agent");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("cust-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#field_CreditCardNumber"), "4242 4242 4242 4242", delay: 45);
            await scene.TypeAsync(scene.Page.Locator("#field_CardExpiry"), "12/30", delay: 45);
            await scene.TypeAsync(scene.Page.Locator("#field_CardSecurityCode"), "123", delay: 45);
        });

        await scene.StepAsync("cust-3", async () =>
        {
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(Button(scene, "Submit securely"), pauseAfter: 400));
            await scene.HighlightAsync(scene.Page.GetByText("Thank you").First, "Submitted securely");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ProductionAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        // Off camera: the agent starts a capture on the Production site, which is allowed; the failure comes on submit.
        string link;
        await using (var agent = await NewSceneAsync(browser, directory + "-agent", durations))
        {
            agent.Page.SetDefaultTimeout(90_000);
            await agent.OpenAsync("/Admin/contact-center/workspace");
            await agent.Page.GetByText("Collect data securely").First.WaitForAsync();
            await agent.Page.GetByRole(AriaRole.Button, new() { Name = "Collect data securely" }).First.ClickAsync();
            await agent.Page.Locator("#demo-secure-link-dialog").WaitForAsync();
            link = await agent.Page.EvaluateAsync<string>("() => window.__secureCaptureLink");
        }

        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync(new Uri(link).PathAndQuery);
        await scene.Page.Locator("#field_CreditCardNumber").FillAsync("4242 4242 4242 4242");
        await scene.Page.Locator("#field_CardExpiry").FillAsync("12/30");
        await scene.Page.Locator("#field_CardSecurityCode").FillAsync("123");
        await scene.StartCaptureAsync();

        await scene.StepAsync("prod-1", async () =>
        {
            await scene.HighlightAsync(Button(scene, "Submit securely"), "Valid test values");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("prod-2", async () =>
        {
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(Button(scene, "Submit securely"), pauseAfter: 400));
            await scene.HighlightAsync(scene.Page.GetByText("Secure data capture is not configured for this environment").First, "Refused: no tokenization provider");
        });

        await scene.ClearHighlightsAsync();
    }
}
