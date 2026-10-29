using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Do Not Call Lists video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string ListPath = "/Admin/dnc-registry/local/Index";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-upload", UploadAsync),
        ("05-manage", ManageAsync),
        ("06-enforce", EnforceAsync),
        ("07-import", ImportAsync),
        ("08-registries", RegistriesAsync),
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
    /// Enables Content Transfer and runs the Omnichannel CRM starter recipe, which adds the Contact type (not recorded).
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

        await context.CloseAsync();
    }

    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static ILocator ListRow(Scene scene) => scene.Page.Locator(".list-group-item").Filter(new() { HasTextString = "June 2026 DNC Update" }).First;

    // ----- Clips -----

    private static async Task UploadAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync(ListPath);
        await scene.StartCaptureAsync();

        await scene.StepAsync("up-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, ListPath), "Interaction Center > Local DNC Registry");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByText("Upload new list").First);
        });

        await scene.StepAsync("up-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#ImportLocalDncList_Name"), "June 2026 DNC Update", delay: 55);
        });

        await scene.StepAsync("up-3", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#ImportLocalDncList_CountryCode"), "United States (+1)", pauseAfter: 900);
            await scene.HighlightAsync(Section(scene.Page.Locator("#ImportLocalDncList_CountryCode")), "Country");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("up-4", async () =>
        {
            await scene.Page.Locator("#ImportLocalDncList_File").SetInputFilesAsync(Path.Combine(Root, "seed-dnc-list.csv"));
            await scene.HighlightAsync(Section(scene.Page.Locator("#ImportLocalDncList_File")), "CSV file");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Upload" }).Last);
        });

        await scene.StepAsync("up-5", async () =>
        {
            await scene.PauseAsync(5000);
            await scene.NavigateAsync(ListPath);
            await scene.HighlightAsync(ListRow(scene), "The list imports in the background");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ManageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync(ListPath);
        await scene.StartCaptureAsync();

        await scene.StepAsync("man-1", async () =>
        {
            await scene.HighlightAsync(ListRow(scene), "File name, country, status, imported, rejected, last processed");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("man-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Completed with errors").First, "Status");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("man-3", async () =>
        {
            await scene.ClickAsync(ListRow(scene).GetByRole(AriaRole.Button, new() { Name = "Actions" }), pauseAfter: 1000);
            await scene.HighlightAsync(scene.Page.GetByText("Download errors").First, "Download errors");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("man-4", async () =>
        {
            await scene.PauseAsync(500);
        });

        await scene.StepAsync("man-5", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show a").Filter(new() { HasTextString = "Delete" }).First, "Delete");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("man-6", async () =>
        {
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.HighlightAsync(ListRow(scene), "Upload the new list first, then delete the old one");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task EnforceAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/importContentSettings");
        await scene.StartCaptureAsync();

        await scene.StepAsync("enf-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/importContentSettings"), "Settings > Content Import");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("enf-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#ISite_DncRegistrySettings_EnforceGlobally"), pauseAfter: 900);
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_DncRegistrySettings_EnforceGlobally")), "Enforce globally");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("enf-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("label").Filter(new() { HasTextString = "Local Do Not Call Registry" }).First, pauseAfter: 900);
            await scene.HighlightAsync(scene.Page.Locator("#EnforcedRegistryKeys_0").Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]"), "Enforced registries");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("enf-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(800);
        });
    }

    private static async Task ImportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/content-transfer-entries");
        await scene.StartCaptureAsync();

        await scene.StepAsync("imp-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/content-transfer-entries"), "Content > Import");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Import" }).First, pauseAfter: 900);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".dropdown-menu.show a").Filter(new() { HasTextString = "Contact" }).First);
        });

        await scene.StepAsync("imp-2", async () =>
        {
            await scene.Page.Locator("#ImportContent_IgnoreDoNotCallNumbers").First.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(scene.Page.GetByText("Enforced").First, "Enforced");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("imp-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("When a number is on a registry").First.Locator("xpath=ancestor::div[2]"), "When a number is on a registry");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("imp-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Select registries to check against").First.Locator("xpath=ancestor::div[2]"), "Select registries to check against");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            var country = scene.Page.Locator("#ImportContent_SelectedCountryCode");
            await country.SelectOptionAsync(await country.EvaluateAsync<string>("s => [...s.options].find(o => /United States/.test(o.text))?.value"));
            await scene.Page.Locator("input[type=file]").SetInputFilesAsync(Path.Combine(Root, "seed-import-contacts.csv"));
            await scene.Page.Locator("#ImportContent_PublishImportedContent").CheckAsync();
            await scene.ClickAsync(scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Upload" }).Last, pauseAfter: 600);
        });

        await scene.StepAsync("imp-5", async () =>
        {
            await scene.PauseAsync(6000);
            await scene.NavigateAsync("/Admin/content-transfer-entries");
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Filter(new() { HasTextString = "seed-import-contacts.csv" }).First, "Completed with errors");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task RegistriesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/usaFtcRegistry");
        await scene.StartCaptureAsync();

        await scene.StepAsync("reg-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/usaFtcRegistry"), "Settings > DNC Registries > USA FTC Registry");
            await scene.PauseAsync(1500);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("reg-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#ISite_UsaFtcDncRegistrySettings_OrganizationId"), "contoso-0001", delay: 55);
        });

        await scene.StepAsync("reg-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#ISite_UsaFtcDncRegistrySettings_ApiKey"), "demo-key-not-a-real-secret", delay: 30);
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_UsaFtcDncRegistrySettings_BaseUrl")), "Base URL");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.StepAsync("reg-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/Settings/canadaDnclRegistry"));
            await scene.TypeAsync(scene.Page.Locator("#ISite_CanadaDnclRegistrySettings_AccountNumber"), "contoso-4821", delay: 55);
            await scene.TypeAsync(scene.Page.Locator("#ISite_CanadaDnclRegistrySettings_ApiKey"), "demo-key-not-a-real-secret", delay: 30);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.StepAsync("reg-5", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_CanadaDnclRegistrySettings_ApiKey")), "A key is stored");
        });

        await scene.ClearHighlightsAsync();
    }
}
