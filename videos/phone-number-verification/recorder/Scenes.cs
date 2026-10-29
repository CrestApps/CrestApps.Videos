using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Phone Number Verification video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string QueuePath = "/Admin/CrestApps.OrchardCore.PhoneNumbers.Verifications/Records/Index";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-provider", ProviderAsync),
        ("04-icons", IconsAsync),
        ("05-queue", QueueAsync),
        ("06-report", ReportAsync),
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
    /// Enables Content Transfer, runs the Omnichannel CRM starter recipe and imports the fake contacts of seed-contacts.csv
    /// (not recorded). seed-verification.py then gives them their results, with the site stopped.
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = StatePath,
            ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height },
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(120_000);

        await page.GotoAsync(BaseUrl + "/Admin/Features");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("a[href$='/CrestApps.OrchardCore.ContentTransfer/Enable']").First.ClickAsync());

        await page.GotoAsync(BaseUrl + "/Admin/Recipes");
        await page.Locator("#btn-run-OmnichannelCrmStarter").ClickAsync();
        await page.WaitForTimeoutAsync(800);
        await page.RunAndWaitForNavigationAsync(() => page.Locator(".modal.show .btn-primary, .modal.show .btn-danger").First.ClickAsync());

        await page.GotoAsync(BaseUrl + "/Admin/import/contents/Contact");
        await page.Locator("input[type=file]").SetInputFilesAsync(Path.Combine(Root, "seed-contacts.csv"));
        await page.Locator("#ImportContent_PublishImportedContent").CheckAsync();
        var country = page.Locator("#ImportContent_SelectedCountryCode");
        await country.SelectOptionAsync(await country.EvaluateAsync<string>("s => [...s.options].find(o => /United States/.test(o.text))?.value"));
        await page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Upload" }).First.ClickAsync();
        await page.WaitForTimeoutAsync(15_000);

        await context.CloseAsync();
    }

    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static ILocator Tab(Scene scene, string name) => scene.Page.Locator($"a[href='#tab-{name}']");

    private static ILocator Row(Scene scene, string phone) => scene.Page.Locator(".list-group-item").Filter(new() { HasTextString = phone }).First;

    private static ILocator Tile(Scene scene, string name) => scene.Page.Locator("a, button, div").Filter(new() { HasTextString = name }).Filter(new() { Has = scene.Page.Locator("xpath=.//*[normalize-space(text())!='']") }).Last;

    // ----- Clips -----

    private static async Task ProviderAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/phoneNumberVerifications");
        await scene.StartCaptureAsync();

        await scene.StepAsync("prov-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/phoneNumberVerifications"), "Settings > Phone Number Verifications");
            await scene.PauseAsync(2800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#tab-general").GetByText("No verification providers are enabled").First, "No provider yet");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("prov-2", async () =>
        {
            await scene.ClickAsync(Tab(scene, "abstract-a-p-i"), pauseAfter: 600);
            await scene.ClickAsync(scene.Page.Locator("#ISite_AbstractApiPhoneNumberVerificationSettings_IsEnabled"), pauseAfter: 900);
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_AbstractApiPhoneNumberVerificationSettings_ApiKey")), "API key");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#ISite_AbstractApiPhoneNumberVerificationSettings_IsEnabled"), pauseAfter: 400);
        });

        await scene.StepAsync("prov-3", async () =>
        {
            await scene.ClickAsync(Tab(scene, "twilio"), pauseAfter: 600);
            await scene.ClickAsync(scene.Page.Locator("#ISite_TwilioPhoneNumberVerificationSettings_IsEnabled"), pauseAfter: 900);
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_TwilioPhoneNumberVerificationSettings_AuthenticationType")), "Authentication type");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(scene.Page.Locator("#ISite_TwilioPhoneNumberVerificationSettings_AuthenticationType"), "Account SID and Auth Token", pauseAfter: 1500);
            await scene.ScrollAsync(0, 400);
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_TwilioPhoneNumberVerificationSettings_CountryCode")), "Country code");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_TwilioPhoneNumberVerificationSettings_Fields")), "Data packages");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#ISite_TwilioPhoneNumberVerificationSettings_IsEnabled"), pauseAfter: 400);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("prov-4", async () =>
        {
            await scene.ScrollAsync(0, -1500);
            await scene.ClickAsync(Tab(scene, "veriphone"), pauseAfter: 600);
            await scene.ClickAsync(scene.Page.Locator("#ISite_VeriphonePhoneNumberVerificationSettings_IsEnabled"), pauseAfter: 900);
            await scene.TypeAsync(scene.Page.Locator("#ISite_VeriphonePhoneNumberVerificationSettings_ApiKey"), "demo-key-not-a-real-secret", delay: 30);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.StepAsync("prov-5", async () =>
        {
            await scene.ClickAsync(Tab(scene, "general"), pauseAfter: 600);
            await scene.SelectAsync(scene.Page.Locator("#ISite_PhoneNumberVerificationsSettings_SelectedProvider"), "Veriphone", pauseAfter: 700);
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_PhoneNumberVerificationsSettings_SelectedProvider")), "Default provider");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("prov-6", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_PhoneNumberVerificationsSettings_RevalidationIntervalDays")), "Revalidation interval (days)");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_PhoneNumberVerificationsSettings_MaxVerificationAttempts")), "Maximum verification attempts");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("prov-7", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_PhoneNumberVerificationsSettings_RequestDelayMilliseconds")), "Request delay (milliseconds)");
            await scene.PauseAsync(4500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(800);
        });
    }

    private static async Task OpenContactAsync(Scene scene, string name)
    {
        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".list-group-item a").Filter(new() { HasTextString = name }).First);
        await scene.Page.Locator(".phone-field-editor-row").First.ScrollIntoViewIfNeededAsync();
        await scene.PauseAsync(600);
    }

    private static async Task IconsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/Contact");
        await scene.StartCaptureAsync();

        var icon = scene.Page.Locator(".phone-field-editor-row > i").First;

        await scene.StepAsync("icon-1", async () =>
        {
            await OpenContactAsync(scene, "Taylor Morgan");
            await scene.HighlightAsync(icon, "Verified");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("icon-2", async () =>
        {
            await scene.NavigateAsync("/Admin/Contents/ContentItems/Contact");
            await OpenContactAsync(scene, "Casey Nguyen");
            await scene.HighlightAsync(icon, "Invalid");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("icon-3", async () =>
        {
            await scene.NavigateAsync("/Admin/Contents/ContentItems/Contact");
            await OpenContactAsync(scene, "Morgan Davis");
            await scene.HighlightAsync(icon, "Failed");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("icon-4", async () =>
        {
            await scene.NavigateAsync("/Admin/Contents/ContentItems/Contact");
            await OpenContactAsync(scene, "Avery Collins");
            await scene.HighlightAsync(icon, "Not checked yet");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task QueueAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        scene.Page.SetDefaultTimeout(120_000);
        await scene.OpenAsync(QueuePath);
        await scene.StartCaptureAsync();

        await scene.StepAsync("que-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, QueuePath), "Tools > Phone Verifications Queue");
            await scene.PauseAsync(2800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("a[title='Show All records']").Locator("xpath=ancestor::div[contains(@class,'row')][1]"), "One tile for each status");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("que-2", async () =>
        {
            await scene.HighlightAsync(Row(scene, "+12125550101"), "Number, status, provider, attempts, last attempt");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("que-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[title='Show Failed records']"));
        });

        await scene.StepAsync("que-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[title='Show Needs attention records']"));
            await scene.HighlightAsync(Row(scene, "+12125550106"), "The latest error");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[title='Show All records']"));
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("que-5", async () =>
        {
            await scene.TypeAsync(scene.Page.GetByPlaceholder("Search by phone number"), "5550104", delay: 90);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button[type=submit][title='Search'], button[type=submit][aria-label='Search']").First);
            await scene.PauseAsync(1000);
            await scene.HighlightAsync(scene.Page.Locator(".filter button.dropdown-toggle").First, "Sort");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("que-6", async () =>
        {
            await scene.NavigateAsync(QueuePath);
            await scene.ClickAsync(Row(scene, "+12125550104").GetByRole(AriaRole.Button, new() { Name = "Actions" }), pauseAfter: 1200);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => Row(scene, "+12125550104").GetByText("Verify again").ClickAsync()));
            await scene.HighlightAsync(Row(scene, "+12125550104"), "Checked again");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("que-7", async () =>
        {
            await scene.ClickAsync(Row(scene, "+12125550102").Locator("input[type=checkbox]"), pauseAfter: 500);
            await scene.ClickAsync(Row(scene, "+12125550105").Locator("input[type=checkbox]"), pauseAfter: 700);
            await scene.HighlightAsync(scene.Page.Locator("#actions"), "Retry selected");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Retry all failed" }), "Retry all failed");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ReportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Reports/view/phone-number-verifications");
        await scene.StartCaptureAsync();

        await scene.StepAsync("rep-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Verification status", new() { Exact = true }).First, "Verification status");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rep-2", async () =>
        {
            await scene.ScrollAsync(0, 400);
            await scene.HighlightAsync(scene.Page.GetByText("Line and queue status", new() { Exact = true }).First, "Line and queue status");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rep-3", async () =>
        {
            await scene.ScrollAsync(0, 500);
            await scene.HighlightAsync(scene.Page.GetByText("Provider usage", new() { Exact = true }).First, "Provider usage");
        });

        await scene.ClearHighlightsAsync();
    }
}
