using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the "Report widgets on the dashboard" video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static string ViewerStatePath => Path.Combine(Root, "state-viewer.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-features", FeaturesAsync),
        ("03-report", ReportAsync),
        ("04-dashboard", DashboardAsync),
        ("05-custom", CustomAsync),
        ("06-viewer", ViewerAsync),
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

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string statePath = null)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, statePath ?? StatePath, Path.GetDirectoryName(directory), durations);

    /// <summary>
    /// Run it twice: before recording (Report Builder on, the ready-made reports imported, the Report viewer role may open the
    /// dashboard), and again after the 02-features clip has turned the Admin Dashboard on (a Lead funnel widget, for the
    /// viewer clip). Nothing here is recorded.
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
        var enable = page.Locator("a[href$='/CrestApps.OrchardCore.Reports.Builder/Enable']");

        if (await enable.CountAsync() > 0)
        {
            await page.RunAndWaitForNavigationAsync(() => enable.First.ClickAsync());
            Console.WriteLine("Enabled the Report Builder");
        }

        await page.GotoAsync(BaseUrl + "/Admin/reports/designs");

        if (await page.GetByText("Nothing here!").CountAsync() > 0)
        {
            await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
            var json = await File.ReadAllTextAsync(Path.Combine(Root, "seed-reports.json"));
            await page.EvaluateAsync("json => document.querySelector('.CodeMirror').CodeMirror.setValue(json)", json);
            await page.WaitForTimeoutAsync(800);
            await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Import" }).Last.ClickAsync());
            Console.WriteLine("Imported the ready-made reports: " + page.Url);
            await page.ScreenshotAsync(new() { Path = Path.Combine(Root, "import-debug.png") });
        }

        await page.GotoAsync(BaseUrl + "/Admin/dashboard/manage");

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Console.WriteLine("dashboard editor: " + page.Url + ", add widget buttons: " + await page.Locator("a:has-text('Add Widget'), button:has-text('Add Widget')").CountAsync() + ", lead funnel: " + await page.GetByText("Lead funnel").CountAsync());

        if (await page.Locator("a:has-text('Add Widget'), button:has-text('Add Widget')").CountAsync() > 0 && !File.Exists(Path.Combine(Root, "lead-funnel-widget.done")))
        {
            await page.GotoAsync(BaseUrl + "/Admin/Roles/Edit/Report%20viewer");
            await page.Locator("[id='Checkbox.AccessAdminDashboard']").CheckAsync();
            await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
            Console.WriteLine("The Report viewer role may open the dashboard");

            await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentTypes/ReportWidget/Create?returnUrl=%2FAdmin%2Fdashboard%2Fmanage");
            await page.Locator("#TitlePart_Title").FillAsync("Lead funnel");
            await page.Locator("#ReportWidgetPart_ReportName").SelectOptionAsync("omnichannel-lead-funnel");
            await page.Locator("#ReportWidgetPart_Period").SelectOptionAsync("Last90Days");
            await page.Locator("#ReportWidgetPart_Content").SelectOptionAsync("FiguresAndTables");
            await page.Locator("#DashboardPart_Position").FillAsync("5");
            await page.Locator("#DashboardPart_Width").FillAsync("2");
            await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Publish" }).First.ClickAsync());
            File.WriteAllText(Path.Combine(Root, "lead-funnel-widget.done"), "done");
            Console.WriteLine("Added the Lead funnel widget");
        }

        await context.CloseAsync();

        var viewer = await browser.NewContextAsync(new BrowserNewContextOptions { ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var viewerPage = await viewer.NewPageAsync();
        viewerPage.SetDefaultTimeout(60_000);
        await viewerPage.GotoAsync(BaseUrl + "/Login");
        await viewerPage.Locator("#LoginForm_UserName, [name='UserName']").First.FillAsync("jamie.chen");
        await viewerPage.Locator("#LoginForm_Password, [name='Password']").First.FillAsync("Password1!");
        await viewerPage.RunAndWaitForNavigationAsync(() => viewerPage.Locator("form button[type=submit]").Last.ClickAsync());
        await viewerPage.GotoAsync(BaseUrl + "/Admin");
        Console.WriteLine("viewer at " + viewerPage.Url);
        await viewer.StorageStateAsync(new() { Path = ViewerStatePath });
        await viewer.CloseAsync();
    }

    // ----- Helpers -----

    private static ILocator Nav(Scene scene, string text)
        => scene.Page.Locator("#left-nav button:visible, #left-nav a:visible").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex($"^\\s*{System.Text.RegularExpressions.Regex.Escape(text)}\\s*$") }).First;

    private static ILocator FeatureRow(Scene scene, string id)
        => scene.Page.Locator("li.list-group-item").Filter(new() { Has = scene.Page.Locator($"#btn-enable-{id.Replace('.', '_')}, #btn-disable-{id.Replace('.', '_')}") }).First;

    private static ILocator Field(Scene scene, string id) => scene.Page.Locator("#" + id);

    private static ILocator Group(ILocator field) => field.Locator("xpath=ancestor::div[contains(@class,'mb-3') or contains(@class,'ocat-wrapper') or contains(@class,'form-group')][1]");

    private static ILocator Publish(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Publish" }).First;

    private static async Task OpenAddWidgetAsync(Scene scene, string widget)
    {
        await scene.ClickAsync(scene.Page.Locator("a.dropdown-toggle, button.dropdown-toggle").Filter(new() { HasTextString = "Add Widget" }).First);
        await scene.PauseAsync(300);
        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".dropdown-menu.show a").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex($"^\\s*{widget}\\s*$") }).First);
    }

    // ----- Clips -----

    private static async Task FeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();

        await scene.StepAsync("feat-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "dashboard", delay: 80);
            await scene.PauseAsync(600);
            await scene.HighlightAsync(FeatureRow(scene, "OrchardCore.AdminDashboard"));
            await scene.PauseAsync(900);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#btn-enable-OrchardCore_AdminDashboard"));
        });

        await scene.StepAsync("feat-2", async () =>
        {
            await scene.NavigateAsync("/Admin");
            await scene.PauseAsync(500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Manage Dashboard" }).First);
            await scene.ClickAsync(scene.Page.Locator("a.dropdown-toggle, button.dropdown-toggle").Filter(new() { HasTextString = "Add Widget" }).First);
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ReportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/dashboard/manage");
        await scene.StartCaptureAsync();

        await scene.StepAsync("rep-1", async () =>
        {
            await OpenAddWidgetAsync(scene, "Report");
            await scene.Page.Locator("#DashboardPart_Width").FillAsync("2");
            await scene.HighlightAsync(Field(scene, "DashboardPart_Position"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(Field(scene, "TitlePart_Title"), "Service level, last 30 days", delay: 45);
        });

        await scene.StepAsync("rep-2", async () =>
        {
            await scene.SelectAsync(Field(scene, "ReportWidgetPart_ReportName"), "contact-center-queue-service-level");
            await scene.HighlightAsync(Field(scene, "ReportWidgetPart_ReportName"));
        });

        await scene.StepAsync("rep-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(Field(scene, "ReportWidgetPart_Period"), "Custom");
            await scene.HighlightAsync(Field(scene, "ReportWidgetPart_From"));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(Field(scene, "ReportWidgetPart_Period"), "Last30Days");
        });

        await scene.StepAsync("rep-4", async () =>
        {
            await scene.SelectAsync(Field(scene, "ReportWidgetPart_Content"), "FiguresAndTables");
            await scene.HighlightAsync(Field(scene, "ReportWidgetPart_Content"));
        });

        await scene.StepAsync("rep-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "ReportWidgetPart_MaxRows"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "ReportWidgetPart_CacheMinutes"));
        });

        await scene.StepAsync("rep-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Publish(scene));
            await scene.PauseAsync(900);
            if (await scene.Page.Locator(".toast .btn-close, .toast-container .btn-close, .alert .btn-close").CountAsync() > 0)
            {
                await scene.ClickAsync(scene.Page.Locator(".toast .btn-close, .toast-container .btn-close, .alert .btn-close").First);
            }

            await OpenAddWidgetAsync(scene, "Report");
            await scene.Page.Locator("#DashboardPart_Position").FillAsync("1");
            await scene.Page.Locator("#DashboardPart_Width").FillAsync("2");
            await scene.TypeAsync(Field(scene, "TitlePart_Title"), "Calls, last 7 days", delay: 35);
            await scene.SelectAsync(Field(scene, "ReportWidgetPart_ReportName"), "contact-center-call-insights", pauseAfter: 300);
            await scene.SelectAsync(Field(scene, "ReportWidgetPart_Period"), "Last7Days", pauseAfter: 300);
            await scene.SelectAsync(Field(scene, "ReportWidgetPart_Content"), "FiguresOnly", pauseAfter: 300);
            await scene.ClickAndWaitForNavigationAsync(Publish(scene));
        });

        await scene.HoldAsync(2);
    }

    private static async Task DashboardAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync("/Admin/dashboard/manage");
        await scene.StartCaptureAsync();

        await scene.StepAsync("dash-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Close Editor" }).First);
            await scene.PauseAsync(2500);
        });

        await scene.StepAsync("dash-2", async () =>
        {
            var link = scene.Page.GetByRole(AriaRole.Link, new() { Name = "Open the full report" }).First;
            await link.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(link.Locator("xpath=ancestor::div[1]"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task CustomAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/dashboard/manage");
        await scene.StartCaptureAsync();

        await scene.StepAsync("cus-1", async () =>
        {
            await OpenAddWidgetAsync(scene, "Custom Report");
            await scene.TypeAsync(Field(scene, "TitlePart_Title"), "Activities by agent", delay: 45);
        });

        await scene.StepAsync("cus-2", async () =>
        {
            await scene.SelectAsync(Field(scene, "ReportDesignWidgetPart_Source"), "report:activity-outcomes-by-agent");
            await scene.HighlightAsync(Field(scene, "ReportDesignWidgetPart_Source"));
        });

        await scene.StepAsync("cus-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(Field(scene, "ReportDesignWidgetPart_Period"), "Last30Days");
            await scene.HighlightAsync(Field(scene, "ReportDesignWidgetPart_Period"));
        });

        await scene.StepAsync("cus-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Publish(scene));
        });

        await scene.HoldAsync(2);
    }

    private static async Task ViewerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, ViewerStatePath);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("perm-1", async () =>
        {
            var tile = scene.Page.Locator("text=You can't view the").First;
            await tile.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(tile.Locator("xpath=ancestor::div[contains(@class,'card') or contains(@class,'widget')][1]"));
        });

        await scene.ClearHighlightsAsync();
    }
}
