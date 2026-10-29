using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the "Run and export reports" video, one method per "clip" of storyboard.json.
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

    private static string DownloadsPath => Path.Combine(Root, "shots", "downloads");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-find", FindAsync),
        ("03-roles", RolesAsync),
        ("03b-viewer", ViewerAsync),
        ("04-run", RunAsync),
        ("05-filters", FiltersAsync),
        ("06-export", ExportAsync),
        ("07-limits", LimitsAsync),
        ("07b-notice", NoticeAsync),
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
    /// Creates the "Report viewer" role and a user in it (not recorded).
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

        await page.GotoAsync(BaseUrl + "/Admin/Roles/Index");

        if (await page.GetByRole(AriaRole.Link, new() { Name = "Report viewer" }).CountAsync() == 0)
        {
            await page.GotoAsync(BaseUrl + "/Admin/Roles/Create");
            await page.Locator("#RoleName").FillAsync("Report viewer");
            await page.Locator("#RoleDescription").FillAsync("Reads the contact center reports");
            await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save & Configure" }).First.ClickAsync());
        }

        await page.GotoAsync(BaseUrl + "/Admin/Roles/Edit/Report%20viewer");
        await page.Locator("[id='Checkbox.AccessAdminPanel']").CheckAsync();
        await page.Locator("[id='Checkbox.ViewContactCenterReports']").CheckAsync();
        await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
        Console.WriteLine("Role Report viewer ready");

        await page.GotoAsync(BaseUrl + "/Admin/Users/Index");
        var edit = await page.Locator("a[href*='/Admin/Users/Edit/']").EvaluateAllAsync<string[]>("els => els.filter(e => e.closest('li, tr') && /jamie\\.chen/.test(e.closest('li, tr').innerText)).map(e => e.getAttribute('href'))");
        await page.GotoAsync(BaseUrl + edit[0]);
        var role = page.Locator("label").Filter(new() { HasTextString = "Report viewer" }).First;
        await page.Locator("#" + await role.GetAttributeAsync("for")).CheckAsync();
        await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).First.ClickAsync());
        Console.WriteLine("jamie.chen is a Report viewer");

        await context.CloseAsync();

        var viewer = await browser.NewContextAsync(new BrowserNewContextOptions { ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var viewerPage = await viewer.NewPageAsync();
        viewerPage.SetDefaultTimeout(60_000);
        await viewerPage.GotoAsync(BaseUrl + "/Login");
        await viewerPage.Locator("#LoginForm_UserName, [name='UserName']").First.FillAsync("jamie.chen");
        await viewerPage.Locator("#LoginForm_Password, [name='Password']").First.FillAsync("Password1!");
        await viewerPage.RunAndWaitForNavigationAsync(() => viewerPage.Locator("form button[type=submit]").Last.ClickAsync());
        await viewerPage.GotoAsync(BaseUrl + "/Admin/Reports");
        Console.WriteLine("viewer at " + viewerPage.Url);
        await viewer.StorageStateAsync(new() { Path = ViewerStatePath });
        await viewer.CloseAsync();
    }

    // ----- Helpers -----

    private static string Last30(string path)
    {
        var to = DateTime.Now;
        var from = to.AddDays(-30);

        return $"{path}?ReportFilter.Range=last30&ReportFilter.From={from:yyyy-MM-dd}T00:00&ReportFilter.To={to:yyyy-MM-dd}T23:59";
    }

    private static ILocator Nav(Scene scene, string text)
        => scene.Page.Locator("#left-nav button:visible, #left-nav a:visible").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex($"^\\s*{System.Text.RegularExpressions.Regex.Escape(text)}\\s*$") }).First;

    private static ILocator Show(Scene scene) => scene.Page.Locator("form button[type=submit].btn-primary").Filter(new() { HasTextString = "Show" }).First;

    private static ILocator RangeButton(Scene scene) => scene.Page.Locator("[data-drp-toggle]").First;

    private static ILocator RangeChoice(Scene scene, string label) => scene.Page.Locator(".dropdown-menu.show label.form-check-label").Filter(new() { HasTextString = label }).First;

    private static ILocator FilterBar(Scene scene) => scene.Page.Locator(".card-header .row").First;

    private static ILocator Select(Scene scene, string name) => scene.Page.Locator($"select[name='ReportFilter.{name}']");

    private static async Task ChooseRangeAsync(Scene scene, string label)
    {
        await scene.ClickAsync(RangeButton(scene));
        await scene.ClickAsync(RangeChoice(scene, label));
        await scene.PauseAsync(400);
    }

    private static async Task DownloadAsync(Scene scene, string item)
    {
        await scene.ClickAsync(scene.Page.Locator("#report-export-menu-button").First, pauseAfter: 500);
        var menuItem = scene.Page.Locator(".dropdown-menu.show button.dropdown-item").Filter(new() { HasTextString = item }).First;
        await scene.MoveToAsync(menuItem);
        Directory.CreateDirectory(DownloadsPath);
        var download = await scene.Page.RunAndWaitForDownloadAsync(() => scene.ClickAsync(menuItem, pauseAfter: 900));
        await download.SaveAsAsync(Path.Combine(DownloadsPath, download.SuggestedFilename));
        Console.WriteLine($"  downloaded {download.SuggestedFilename}");
    }

    // ----- Clips -----

    private static async Task FindAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("find-1", async () =>
        {
            await scene.ClickAsync(Nav(scene, "Reports"));
            await scene.PauseAsync(500);
            await scene.ClickAsync(Nav(scene, "Operations"));
            await scene.HighlightAsync(scene.Page.Locator("#left-nav").First, "The Reports menu");
        });

        await scene.StepAsync("find-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/Reports");
            await scene.PauseAsync(700);
            await scene.MoveToAsync(1000, 500);
            await scene.ScrollAsync(0, 500);
        });

        await scene.StepAsync("find-3", async () =>
        {
            var card = scene.Page.Locator("a.card[href$='contact-center-call-insights']").First;
            await card.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(card);
            await scene.PauseAsync(900);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(card);
        });
    }

    private static async Task RolesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Roles/Edit/Report%20viewer");
        await scene.StartCaptureAsync();

        ILocator Row(string permission) => scene.Page.Locator("tr").Filter(new() { Has = scene.Page.Locator($"[id='Checkbox.{permission}']") }).First;

        await scene.StepAsync("perm-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "report", delay: 90);
            await scene.PauseAsync(700);
            await scene.HighlightAsync(Row("ViewContactCenterReports"));
        });

        await scene.StepAsync("perm-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row("ViewOmnichannelReports"));
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row("RunPhoneNumberVerificationsReport"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ViewerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, ViewerStatePath);

        await scene.OpenAsync("/Admin/Reports");
        await scene.StartCaptureAsync();

        await scene.StepAsync("perm-3", async () =>
        {
            var heading = scene.Page.Locator("h2.h4").Filter(new() { HasTextString = "CRM & Campaigns" }).First;
            await heading.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(600);
            await scene.HighlightAsync(heading.Locator("xpath=following-sibling::div[1]"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/Reports/view/contact-center-call-insights");
        await scene.StartCaptureAsync();

        await scene.StepAsync("run-1", async () =>
        {
            await scene.MoveToAsync(RangeButton(scene));
            await scene.HighlightAsync(RangeButton(scene), "Date range");
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(RangeButton(scene));
            await scene.MoveToAsync(RangeChoice(scene, "Last 90 Days"));
            await scene.PauseAsync(900);
            await scene.MoveToAsync(RangeChoice(scene, "This Quarter"));
        });

        await scene.StepAsync("run-3", async () =>
        {
            await scene.MoveToAsync(RangeChoice(scene, "Last 6 Months"));
            await scene.PauseAsync(1500);
            await scene.MoveToAsync(RangeChoice(scene, "Custom Range"));
            await scene.PauseAsync(1500);
            await scene.MoveToAsync(RangeChoice(scene, "On or before"));
        });

        await scene.StepAsync("run-4", async () =>
        {
            await scene.ClickAsync(RangeChoice(scene, "Last 30 Days"));
            await scene.PauseAsync(500);
            await scene.ClickAndWaitForNavigationAsync(Show(scene));
        });

        await scene.StepAsync("run-5", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("section.report-section").First);
        });

        await scene.StepAsync("run-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 650);
        });
    }

    private static async Task FiltersAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync(Last30("/Admin/Reports/view/contact-center-call-insights"));
        await scene.StartCaptureAsync();

        await scene.StepAsync("filt-1", async () =>
        {
            await scene.HighlightAsync(FilterBar(scene));
        });

        await scene.StepAsync("filt-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(Select(scene, "QueueId"), "Support");
            await scene.SelectAsync(Select(scene, "Direction"), "Inbound");
            await scene.ClickAndWaitForNavigationAsync(Show(scene));
        });

        await scene.StepAsync("filt-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("section.report-section").First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 600);
        });

        await scene.StepAsync("filt-4", async () =>
        {
            await scene.NavigateAsync(Last30("/Admin/Reports/view/omnichannel-dialer-call-detail"));
            await scene.PauseAsync(800);
            await scene.HighlightAsync(FilterBar(scene));
        });

        await scene.StepAsync("filt-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button[data-id='ReportFilter_DispositionIds']"));
            var menu = scene.Page.Locator(".dropdown-menu.show");
            await scene.ClickAsync(menu.Locator("li a").Filter(new() { HasTextString = "Left voicemail" }).First);
            await scene.ClickAsync(menu.Locator("li a").Filter(new() { HasTextString = "No answer" }).First);
            await scene.ClickAsync(scene.Page.Locator("h1").First, 700, 10, pauseAfter: 300);
            await scene.ClickAndWaitForNavigationAsync(Show(scene));
        });

        await scene.HoldAsync(3.5);
    }

    private static async Task ExportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync(Last30("/Admin/Reports/view/contact-center-call-insights"));
        await scene.StartCaptureAsync();

        await scene.StepAsync("exp-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#report-export-menu-button").First);
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show").First);
        });

        await scene.StepAsync("exp-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.Page.Keyboard.PressAsync("Escape");
            await DownloadAsync(scene, "Export CSV");
        });

        await scene.StepAsync("exp-3", async () => await DownloadAsync(scene, "Export Excel"));

        await scene.StepAsync("exp-4", async () => await DownloadAsync(scene, "Export PDF"));
    }

    private static async Task LimitsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/Reports/view/contact-center-agent-time-summary");
        await scene.StartCaptureAsync();

        await scene.StepAsync("lim-1", async () =>
        {
            await ChooseRangeAsync(scene, "Last 90 Days");
            await scene.ClickAsync(scene.Page.Locator("h1").First, 700, 10, pauseAfter: 200);
            await scene.ClickAndWaitForNavigationAsync(Show(scene));
        });

        await scene.StepAsync("lim-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".card-body").Last);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await ChooseRangeAsync(scene, "Last 30 Days");
            await scene.ClickAsync(scene.Page.Locator("h1").First, 700, 10, pauseAfter: 200);
            await scene.ClickAndWaitForNavigationAsync(Show(scene));
        });

        await scene.HoldAsync(3.5);
    }

    private static async Task NoticeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/Reports/view/contact-center-recording-coverage");
        await scene.StartCaptureAsync();

        await scene.StepAsync("lim-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".card-body").Last);
        });

        await scene.ClearHighlightsAsync();
    }
}
