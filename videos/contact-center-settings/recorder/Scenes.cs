using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Contact Center Settings video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string SettingsPath = "/Admin/Settings/contactcenter";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-numbers", NumbersAsync),
        ("03-recording", RecordingAsync),
        ("05-transfers", TransfersAsync),
        ("06-secure", SecureAsync),
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

    /// <summary>
    /// Creates two fake Omnichannel Addresses (a voice number and a text number) for the Default numbers tab (not recorded).
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = StatePath,
            ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height },
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        foreach (var (name, number, capability) in new[] { ("Main line", "+12125550100", 0), ("Support texts", "+12125550110", 1) })
        {
            await page.GotoAsync(BaseUrl + "/Admin/omnichannel/channel-endpoints/create/PhoneNumber");
            await page.Locator("#OmnichannelChannelEndpoint_DisplayText").FillAsync(name);
            await page.Locator("#OmnichannelChannelEndpoint_Value").FillAsync(number);
            await page.Locator($"#OmnichannelChannelEndpoint_Capabilities_{capability}").CheckAsync();
            await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
            Console.WriteLine($"Created {name}");
        }

        await context.CloseAsync();
    }

    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static ILocator Tab(Scene scene, string name) => scene.Page.Locator($"a[href='#tab-{name}']");

    private static ILocator Rec(Scene scene, string property) => scene.Page.Locator($"#ISite_ContactCenterRecordingSettings_{property}");

    private static async Task PickAsync(Scene scene, string selectId, string text)
    {
        await scene.ClickAsync(scene.Page.Locator($"button[data-id='{selectId}']"), pauseAfter: 800);
        await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = text }).First, pauseAfter: 700);
    }

    // ----- Clips -----

    private static async Task NumbersAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync(SettingsPath);
        await scene.StartCaptureAsync();

        await scene.StepAsync("num-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, SettingsPath), "Settings > Contact Center");
            await scene.PauseAsync(2800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Tab(scene, "default-numbers"), "Default numbers");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("num-2", async () =>
        {
            await PickAsync(scene, "ISite_ContactCenterDefaultAddressSettings_DefaultPhoneAddressId", "Main line");
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_ContactCenterDefaultAddressSettings_DefaultPhoneAddressId")), "Default phone number");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("num-3", async () =>
        {
            await PickAsync(scene, "ISite_ContactCenterDefaultAddressSettings_DefaultSmsAddressId", "Support texts");
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_ContactCenterDefaultAddressSettings_DefaultSmsAddressId")), "Default SMS number");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }

    private static async Task RecordingAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync(SettingsPath);
        await scene.Page.Locator("a[href='#tab-recording-governance']").ClickAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("rec-1", async () =>
        {
            await scene.HighlightAsync(Section(Rec(scene, "RecordAllCalls")), "Record every call automatically");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rec-2", async () =>
        {
            await scene.HighlightAsync(Section(Rec(scene, "ConsentModel")), "Consent model");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rec-3", async () =>
        {
            await scene.ClickAsync(Rec(scene, "EnableRecordingDisclosure"), pauseAfter: 800);
            await scene.HighlightAsync(Section(Rec(scene, "EnableRecordingDisclosure")), "Tell callers the call is recorded");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Rec(scene, "RecordingDisclosureText")), "Disclosure");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rec-4", async () =>
        {
            await scene.ScrollAsync(0, 350);
            await scene.ClickAsync(Rec(scene, "DiscloseOnInboundCalls"), pauseAfter: 700);
            await scene.ClickAsync(Rec(scene, "DiscloseOnAIVoiceCalls"), pauseAfter: 700);
            await scene.HighlightAsync(Section(Rec(scene, "DiscloseOnInboundCalls")), "Say it to inbound callers");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rec-5", async () =>
        {
            await scene.ClickAsync(Rec(scene, "PromptAgentsToDisclose"), pauseAfter: 700);
            await scene.HighlightAsync(Section(Rec(scene, "PromptAgentsToDisclose")), "Ask agents to say it");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rec-6", async () =>
        {
            await scene.ClickAsync(Rec(scene, "RequireExplicitConsent"), pauseAfter: 700);
            await scene.HighlightAsync(Section(Rec(scene, "RequireExplicitConsent")), "Do not record until the caller has heard it");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rec-7", async () =>
        {
            await scene.ScrollAsync(0, 450);
            await scene.TypeAsync(Rec(scene, "RetentionDays"), "90", delay: 90);
            await scene.HighlightAsync(Section(Rec(scene, "RetentionDays")), "Retention (days)");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Rec(scene, "LegalHoldByDefault")), "Apply legal hold by default");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rec-8", async () =>
        {
            await scene.ScrollAsync(0, 400);
            await scene.HighlightAsync(Section(Rec(scene, "AllowAgentSecurePause")), "Allow agents to pause recording");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(Rec(scene, "MaxSecurePauseSeconds"), "120", delay: 90);
            await scene.ClickAsync(Rec(scene, "RequirePauseReason"), pauseAfter: 700);
            await scene.HighlightAsync(Section(Rec(scene, "RequirePauseReason")), "Require a reason to pause");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rec-9", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(800);
        });
    }

    private static async Task TransfersAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync(SettingsPath);
        await scene.Page.Locator("a[href='#tab-external-transfer-destinations']").ClickAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("xfer-1", async () =>
        {
            await scene.HighlightAsync(Tab(scene, "external-transfer-destinations"), "External transfer destinations");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#cc-add-dest"), pauseAfter: 900);
        });

        await scene.StepAsync("xfer-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("input[name$='Destinations[0].DisplayName']"), "After-hours answering service", delay: 40);
            await scene.TypeAsync(scene.Page.Locator("input[name$='Destinations[0].E164Address']"), "+12125550199", delay: 70);
        });

        await scene.StepAsync("xfer-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#cc-add-dest"), pauseAfter: 800);
            await scene.Page.Locator("input[name$='Destinations[1].DisplayName']").FillAsync("Dispatch desk");
            await scene.Page.Locator("input[name$='Destinations[1].E164Address']").FillAsync("+12125550123");
            await scene.HighlightAsync(scene.Page.Locator("input[name$='Destinations[1].Enabled']"), "Enabled");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.Page.Locator("a[href='#tab-external-transfer-destinations']").ClickAsync();
        });

        await scene.StepAsync("xfer-4", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_ContactCenterExternalTransferSettings_AllowUnlistedNumbers")), "Let agents transfer to numbers that are not on this list");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SecureAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync(SettingsPath);
        await scene.Page.Locator("a[href='#tab-secure-data-capture']").ClickAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("sec-1", async () =>
        {
            await scene.HighlightAsync(Tab(scene, "secure-data-capture"), "Secure Data Capture");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#ISite_SecureCaptureSettings_Enabled"), pauseAfter: 800);
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_SecureCaptureSettings_Enabled")), "Enable agent-assisted secure data capture");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("sec-2", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_SecureCaptureSettings_LinkTimeToLiveSeconds")), "Capture link lifetime (seconds)");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("sec-3", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_SecureCaptureSettings_PauseRecordingDuringCapture")), "Pause recording during capture");
            await scene.PauseAsync(4500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }
}
