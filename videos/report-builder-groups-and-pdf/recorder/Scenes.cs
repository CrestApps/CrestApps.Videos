using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the "Report groups and PDF export" video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static string DownloadsPath => Path.Combine(Root, "shots", "downloads");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-groups", GroupsAsync),
        ("03-settings", SettingsAsync),
        ("04-export", ExportAsync),
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
    /// Turns the Report Builder on and imports the ready-made dashboard report (not recorded).
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

        if (await page.GetByText("Nothing here!").CountAsync() > 0 || Environment.GetEnvironmentVariable("VIDEO_REIMPORT") == "1")
        {
            await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
            var json = await File.ReadAllTextAsync(Path.Combine(Root, "seed-reports.json"));
            await page.EvaluateAsync("json => document.querySelector('.CodeMirror').CodeMirror.setValue(json)", json);
            await page.WaitForTimeoutAsync(800);
            await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Import" }).Last.ClickAsync());
            Console.WriteLine("Imported the ready-made reports: " + page.Url);
        }

        await context.CloseAsync();
    }

    // ----- Helpers -----

    private static ILocator Nav(Scene scene, string text)
        => scene.Page.Locator("#left-nav button:visible, #left-nav a:visible").Filter(new() { HasTextRegex = new Regex("^\\s*" + Regex.Escape(text) + "\\s*$") }).First;

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static async Task WaitForDesignerAsync(Scene scene)
    {
        await scene.Page.Locator(".rd-query-bar").WaitForAsync();
        await scene.PauseAsync(1200);
    }

    // ----- Clips -----

    private static async Task GroupsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("grp-1", async () =>
        {
            await scene.ClickAsync(Nav(scene, "Reports"));
            await scene.PauseAsync(400);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#left-nav a:visible").Filter(new() { HasTextRegex = new Regex("^\\s*Report Groups\\s*$") }).First);
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").First);
        });

        await scene.StepAsync("grp-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Add Group" }).First);
            await scene.TypeAsync(scene.Page.Locator("input[maxlength='200']").First, "Service", delay: 70);
            await scene.HighlightAsync(scene.Page.Locator("select").Filter(new() { Has = scene.Page.Locator("option", new() { HasTextString = "None (top level)" }) }).First);
        });

        await scene.StepAsync("grp-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("input[type=number]").First);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.StepAsync("grp-4", async () =>
        {
            var row = scene.Page.Locator(".list-group-item").Filter(new() { HasTextRegex = new Regex("^\\s*Service") }).First;
            await scene.ClickAsync(row.GetByRole(AriaRole.Button, new() { Name = "Actions" }));
            await scene.ClickAndWaitForNavigationAsync(row.GetByRole(AriaRole.Link, new() { Name = "Add a group inside" }));
            await scene.TypeAsync(scene.Page.Locator("input[maxlength='200']").First, "Regional", delay: 70);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Filter(new() { HasTextString = "Regional" }).First);
        });

        await scene.HoldAsync(2);
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/reports/builder/edit/call-dashboard");
        await WaitForDesignerAsync(scene);
        await scene.StartCaptureAsync();

        var group = scene.Page.Locator("#report-designer-settings select").First;
        var menu = scene.Page.Locator("#report-designer-settings .form-check-input").First;
        var menuLabel = scene.Page.Locator("#report-designer-settings label.form-check-label").Filter(new() { HasTextString = "Show in the admin menu" }).First;
        var export = scene.Page.Locator("#report-designer-settings .form-check-input").Last;

        await scene.StepAsync("set-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("a.nav-link, .nav-tabs a, .nav-tabs button").Filter(new() { HasTextString = "Settings" }).First);
            await scene.PauseAsync(500);
            await scene.HighlightAsync(group);
        });

        await scene.StepAsync("set-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(group, "Service / Regional");
            await scene.ClickAsync(menuLabel);
            await scene.PauseAsync(300);
            if (!await menu.IsCheckedAsync())
            {
                await menu.CheckAsync();
            }

            await scene.HighlightAsync(menu.Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"));
        });

        await scene.StepAsync("set-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(export.Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Publish" }).First);
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("set-4", async () =>
        {
            await scene.NavigateAsync("/Admin");
            await scene.ClickAsync(Nav(scene, "Reports"));
            await scene.PauseAsync(400);
            await scene.ClickAsync(Nav(scene, "Service"));
            await scene.PauseAsync(400);
            await scene.ClickAsync(Nav(scene, "Regional"));
            await scene.HighlightAsync(scene.Page.Locator("#left-nav a:visible").Filter(new() { HasTextString = "Call dashboard" }).First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ExportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync("/Admin/reports/designs/call-dashboard");
        await scene.StartCaptureAsync();

        await scene.StepAsync("pdf-1", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Export" }).First);
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show").First);
        });

        await scene.StepAsync("pdf-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            Directory.CreateDirectory(DownloadsPath);
            var menuItem = scene.Page.Locator(".dropdown-menu.show .dropdown-item, .dropdown-menu.show a").Filter(new() { HasTextString = "PDF" }).First;
            await scene.MoveToAsync(menuItem);
            var download = await scene.Page.RunAndWaitForDownloadAsync(() => scene.ClickAsync(menuItem, pauseAfter: 600));
            await download.SaveAsAsync(Path.Combine(DownloadsPath, download.SuggestedFilename));
            Console.WriteLine($"  downloaded {download.SuggestedFilename}");
        });

        await scene.HoldAsync(1);
    }
}
