using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the "Report Builder administration" video, one method per "clip" of storyboard.json.
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
        ("01-roles", RolesAsync),
        ("02-before", BeforeAsync),
        ("02b-grant", GrantAsync),
        ("02c-after", AfterAsync),
        ("03-deploy", DeployAsync),
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
    /// Turns the Report Builder and Deployments on, imports the Call dashboard, and signs the Report viewer user in (not recorded).
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

        foreach (var feature in new[] { "CrestApps.OrchardCore.Reports.Builder", "OrchardCore.Deployment" })
        {
            await page.GotoAsync(BaseUrl + "/Admin/Features");
            var enable = page.Locator($"a[href$='/{feature}/Enable']");

            if (await enable.CountAsync() > 0)
            {
                await page.RunAndWaitForNavigationAsync(() => enable.First.ClickAsync());
                Console.WriteLine("Enabled " + feature);
            }
        }

        await page.GotoAsync(BaseUrl + "/Admin/reports/designs");

        if (await page.GetByText("Nothing here!").CountAsync() > 0)
        {
            await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
            var json = await File.ReadAllTextAsync(Path.Combine(Root, "seed-reports.json"));
            await page.EvaluateAsync("json => document.querySelector('.CodeMirror').CodeMirror.setValue(json)", json);
            await page.WaitForTimeoutAsync(800);
            await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Import" }).Last.ClickAsync());
            Console.WriteLine("Imported the dashboard report");
        }

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

    private static ILocator Nav(Scene scene, string text)
        => scene.Page.Locator("#left-nav button:visible, #left-nav a:visible").Filter(new() { HasTextRegex = new Regex("^\\s*" + Regex.Escape(text) + "\\s*$") }).First;

    private static ILocator Row(Scene scene, string permission) => scene.Page.Locator("tr").Filter(new() { Has = scene.Page.Locator($"[id='Checkbox.{permission}']") }).First;

    // ----- Clips -----

    private static async Task RolesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/Roles/Edit/Report%20viewer");
        await scene.StartCaptureAsync();

        await scene.StepAsync("perm-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "custom report", delay: 70);
            await scene.PauseAsync(700);
            await scene.HighlightAsync(Row(scene, "ManageOwnReportDesigns"));
        });

        await scene.StepAsync("perm-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(scene, "ViewAllReportDesigns"));
            await scene.PauseAsync(2800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(scene, "ManageAllReportDesigns"));
        });

        await scene.StepAsync("perm-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(scene, "ShareReportsPublicly"));
        });

        await scene.StepAsync("perm-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.Page.Locator("#search-box").FillAsync(string.Empty);
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "report groups", delay: 70);
            await scene.PauseAsync(700);
            await scene.HighlightAsync(Row(scene, "ManageReportGroups"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task BeforeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, ViewerStatePath);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/Reports");
        await scene.StartCaptureAsync();

        await scene.StepAsync("try-1", async () =>
        {
            await scene.PauseAsync(500);
            await scene.HighlightAsync(scene.Page.Locator("#left-nav").First, "No Report Builder");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task GrantAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/Roles/Edit/Report%20viewer");
        await scene.StartCaptureAsync();

        await scene.StepAsync("try-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "custom report", delay: 60);
            await scene.PauseAsync(600);
            var row = Row(scene, "ManageOwnReportDesigns");
            await scene.ClickAsync(row.Locator("[id='Checkbox.ManageOwnReportDesigns']"));
            await scene.HighlightAsync(row);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last);
        });

        await scene.HoldAsync(1);
    }

    private static async Task AfterAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        // A person who is signed in when a role changes sees the change after signing in again, so the viewer signs in again here.
        await using var scene = await NewSceneAsync(browser, directory, durations, Path.Combine(Root, "no-state.json"));
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Login");
        await scene.Page.Locator("#LoginForm_UserName, [name='UserName']").First.FillAsync("jamie.chen");
        await scene.Page.Locator("#LoginForm_Password, [name='Password']").First.FillAsync("Password1!");
        await scene.WaitForNavigationAsync(() => scene.Page.Locator("form button[type=submit]").Last.ClickAsync());
        await scene.OpenAsync("/Admin/Reports");
        await scene.StartCaptureAsync();

        await scene.StepAsync("try-3", async () =>
        {
            await scene.PauseAsync(500);
            var builder = scene.Page.Locator("#left-nav a:visible").Filter(new() { HasTextRegex = new Regex("^\\s*Report Builder\\s*$") }).First;
            await scene.HighlightAsync(builder, "Report Builder");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(builder);
        });

        await scene.HoldAsync(2);
    }

    private static async Task DeployAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        Directory.CreateDirectory(DownloadsPath);
        var package = Path.Combine(DownloadsPath, "report-designs.json");
        File.Copy(Path.Combine(Root, "seed-reports.json"), package, overwrite: true);

        await scene.OpenAsync("/Admin/DeploymentPlan/Import/Json");
        await scene.StartCaptureAsync();

        await scene.StepAsync("dep-1", async () =>
        {
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.Locator("#left-nav a:visible").Filter(new() { HasTextRegex = new Regex("^\\s*JSON Import\\s*$") }).First);
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#left-nav a:visible").Filter(new() { HasTextRegex = new Regex("^\\s*Package Import\\s*$") }).First);
        });

        await scene.StepAsync("dep-2", async () =>
        {
            await scene.Page.Locator("input[name=importedPackage]").SetInputFilesAsync(package);
            await scene.PauseAsync(1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Import" }).Last);
        });

        await scene.StepAsync("dep-3", async () =>
        {
            await scene.NavigateAsync("/Admin/reports/designs");
            await scene.PauseAsync(1500);
        });

        await scene.HoldAsync(3);
    }
}
