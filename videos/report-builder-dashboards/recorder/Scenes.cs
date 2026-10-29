using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the "Report Builder dashboards" video, one method per "clip" of storyboard.json.
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
        ("02-run", RunAsync),
        ("03-queries", QueriesAsync),
        ("04-containers", ContainersAsync),
        ("05-clicks", ClicksAsync),
        ("06-export", ExportAsync),
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

    private const string Dashboard = "/Admin/reports/designs/call-dashboard";

    private static ILocator Visual(Scene scene, string id) => scene.Page.Locator($"section[data-report-visual='{id}']");

    private static ILocator Row(Scene scene, string visual, string key) => Visual(scene, visual).Locator($"tr[data-report-key='{key}']");

    private static ILocator EditDesign(Scene scene) => scene.Page.GetByRole(AriaRole.Link, new() { Name = "Edit design" }).First;

    private static async Task WaitForDesignerAsync(Scene scene)
    {
        await scene.Page.Locator(".rd-query-bar").WaitForAsync();
        await scene.Page.Locator(".rd-preview-panel .report-container-header").First.WaitForAsync();
        await scene.PauseAsync(800);
    }

    private static ILocator VisualItem(Scene scene, string title)
        => scene.Page.Locator(".list-group-item button.text-reset").Filter(new() { HasTextRegex = new Regex("^\\s*" + Regex.Escape(title)) }).First;

    private static ILocator PropertyLabel(Scene scene, string text)
        => scene.Page.Locator("label, .form-label, h6, .small").Filter(new() { HasTextRegex = new Regex("^\\s*" + Regex.Escape(text) + "\\s*$", RegexOptions.IgnoreCase) }).First;

    private static async Task ClickPointAsync(Scene scene, int index)
    {
        var canvas = Visual(scene, "v-trend").Locator("canvas");
        await canvas.ScrollIntoViewIfNeededAsync();
        await scene.PauseAsync(500);
        var point = await canvas.EvaluateAsync<double[]>("(c, i) => { const m = Chart.getChart(c).getDatasetMeta(0).data[i]; const r = c.getBoundingClientRect(); return [r.x + m.x, r.y + m.y]; }", index);
        await scene.MoveToAsync(point[0], point[1]);
        await scene.Page.Mouse.ClickAsync((float)point[0], (float)point[1]);
        await scene.PauseAsync(900);
    }

    // ----- Clips -----

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync(Dashboard);
        await scene.StartCaptureAsync();

        await scene.StepAsync("run-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".report-container-header").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(1000, 500);
            await scene.ScrollAsync(0, 520);
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.ScrollAsync(0, -700);
            await scene.ClickAsync(Row(scene, "v-agents", "sam.patel"));
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("run-3", async () =>
        {
            var maria = Row(scene, "v-agents", "maria.lopez");
            await scene.MoveToAsync(maria);
            await maria.ClickAsync(new() { Modifiers = [KeyboardModifier.Control] });
            await scene.PauseAsync(2200);
            await scene.ClickAsync(scene.Page.GetByText("Clear selection").First);
            await scene.PauseAsync(800);
        });

        await scene.StepAsync("run-4", async () =>
        {
            await ClickPointAsync(scene, 15);
            await scene.ScrollAsync(0, 260);
        });

        await scene.HoldAsync(1.5);
    }

    private static async Task QueriesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync(Dashboard);
        await scene.StartCaptureAsync();

        await scene.StepAsync("q-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(EditDesign(scene));
            await WaitForDesignerAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator(".rd-query-bar"));
        });

        await scene.StepAsync("q-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(scene.Page.Locator("#rd-query-pick"), "byday");
            await scene.PauseAsync(900);
            await scene.HighlightAsync(scene.Page.Locator(".report-designer-shelf").Filter(new() { HasTextRegex = new Regex("^\\s*Columns") }).First);
        });

        await scene.StepAsync("q-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(scene.Page.Locator("#rd-query-pick"), "");
            await scene.ClickAsync(scene.Page.Locator(".rd-query-bar button").Filter(new() { HasTextString = "Add query" }).First);
            await scene.PauseAsync(600);
            await scene.TypeAsync(scene.Page.Locator(".rd-query-bar input[type=text]"), "By status", delay: 60);
            await scene.HighlightAsync(scene.Page.Locator(".rd-query-bar"));
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator(".rd-query-bar button").Filter(new() { HasTextString = "Remove query" }).First);
        });

        await scene.HoldAsync(1.5);
    }

    private static async Task ContainersAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/reports/builder/edit/call-dashboard");
        await WaitForDesignerAsync(scene);
        await scene.StartCaptureAsync();

        var card = scene.Page.Locator(".card").Filter(new() { Has = scene.Page.Locator(".card-header", new() { HasTextString = "Containers" }) }).Last;

        await scene.StepAsync("con-1", async () =>
        {
            await scene.MoveToAsync(1700, 500);
            await card.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(card);
        });

        await scene.StepAsync("con-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(card.GetByRole(AriaRole.Button, new() { Name = "Add container" }));
            await scene.PauseAsync(900);
            await scene.ScrollAsync(0, 400);
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("con-3", async () =>
        {
            await card.GetByRole(AriaRole.Button, new() { Name = "Remove" }).Last.ClickAsync();
            await scene.PauseAsync(500);
            await scene.ScrollAsync(0, -3000);
            await scene.ClickAsync(VisualItem(scene, "Agents"));
            var label = PropertyLabel(scene, "Container");
            await label.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(label.Locator("xpath=following::select[1]"));
        });

        await scene.HoldAsync(1);
    }

    private static async Task ClicksAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/reports/builder/edit/call-dashboard");
        await WaitForDesignerAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("clk-1", async () =>
        {
            await scene.ClickAsync(VisualItem(scene, "Agents"));
            var label = PropertyLabel(scene, "Pick the value of");
            await label.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(label.Locator("xpath=following::select[1]"));
        });

        await scene.StepAsync("clk-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var label = PropertyLabel(scene, "Filter these visuals");
            await label.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(label.Locator("xpath=following::div[contains(@class,'form-check')][1]/ancestor::div[1]"));
        });

        await scene.StepAsync("clk-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, -3000);
            await scene.ClickAsync(VisualItem(scene, "Calls per day"));
            var label = PropertyLabel(scene, "Pick the value of");
            await label.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(label.Locator("xpath=following::select[1]"));
        });

        await scene.HoldAsync(1);
    }

    private static async Task ExportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync(Dashboard);
        await scene.StartCaptureAsync();

        await scene.StepAsync("exp-1", async () =>
        {
            Directory.CreateDirectory(DownloadsPath);

            foreach (var item in new[] { "Excel", "PDF" })
            {
                await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Export" }).First, pauseAfter: 400);
                var menuItem = scene.Page.Locator(".dropdown-menu.show .dropdown-item, .dropdown-menu.show a").Filter(new() { HasTextString = item }).First;
                await scene.MoveToAsync(menuItem);
                var download = await scene.Page.RunAndWaitForDownloadAsync(() => scene.ClickAsync(menuItem, pauseAfter: 600));
                await download.SaveAsAsync(Path.Combine(DownloadsPath, download.SuggestedFilename));
                Console.WriteLine($"  downloaded {download.SuggestedFilename}");
            }
        });

        await scene.HoldAsync(1);
    }
}
