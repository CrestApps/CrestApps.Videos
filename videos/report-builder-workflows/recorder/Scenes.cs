using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the "Report workflows" video, one method per "clip" of storyboard.json.
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
        ("02-events", EventsAsync),
        ("03-tasks", TasksAsync),
        ("04-run", RunAsync),
        ("05-export", ExportAsync),
        ("06-email", EmailAsync),
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
    /// Turns on the Report Builder, Workflows (with timers) and Email (with no provider), and imports the dashboard report and
    /// the three ready-made workflows (not recorded).
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

        foreach (var feature in new[] { "CrestApps.OrchardCore.Reports.Builder", "OrchardCore.Workflows", "OrchardCore.Workflows.Timers", "OrchardCore.Email" })
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
            foreach (var file in new[] { "seed-reports.json", "seed-workflows.json" })
            {
                await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
                var json = await File.ReadAllTextAsync(Path.Combine(Root, file));
                await page.EvaluateAsync("json => document.querySelector('.CodeMirror').CodeMirror.setValue(json)", json);
                await page.WaitForTimeoutAsync(800);
                await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Import" }).Last.ClickAsync());
                Console.WriteLine("Imported " + file);
            }
        }

        await context.CloseAsync();
    }

    // ----- Helpers -----

    private static ILocator WorkflowLink(Scene scene, string name)
        => scene.Page.Locator("a").Filter(new() { HasTextRegex = new Regex("^\\s*" + Regex.Escape(name) + "\\s*$") }).First;

    private static ILocator Activity(Scene scene, string title)
        => scene.Page.Locator(".activity.jtk-managed").Filter(new() { HasText = title }).First;

    private static async Task OpenActivityEditorAsync(Scene scene, string title)
    {
        var activity = Activity(scene, title);
        await scene.MoveToAsync(activity);
        await scene.PauseAsync(400);
        var href = await activity.Locator("a.activity-edit-action").GetAttributeAsync("href");
        await scene.NavigateAsync(href);
    }

    private static ILocator Field(Scene scene, string id) => scene.Page.Locator("#" + id);

    private static ILocator FieldRow(ILocator field) => field.Locator("xpath=ancestor::div[contains(@class,'ocat-wrapper') or contains(@class,'mb-3')][1]");

    private static async Task OpenModalCategoryAsync(Scene scene, string buttonText, string category)
    {
        await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = buttonText }).First);
        await scene.PauseAsync(700);
        var tab = scene.Page.Locator(".modal.show").GetByText(category, new() { Exact = true }).First;
        await scene.ClickAsync(tab);
        await scene.PauseAsync(500);
    }

    // ----- Clips -----

    private static async Task EventsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/Workflows/Types");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ev-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").First);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(WorkflowLink(scene, "Share a link when a report is published"));
            await scene.Page.Locator(".activity.jtk-managed:visible").First.WaitForAsync();
            await scene.PauseAsync(800);
            await scene.HighlightAsync(scene.Page.Locator("#workflow-container, .workflow-container, .jtk-surface, .activity").First.Locator("xpath=ancestor::div[1]"));
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("a, button").Filter(new() { HasTextString = "Add Event" }).First);
        });

        await scene.StepAsync("ev-2", async () =>
        {
            await scene.PauseAsync(500);
            var tab = scene.Page.Locator(".modal.show").GetByText("Reports", new() { Exact = true }).First;
            await scene.ClickAsync(tab);
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.Locator(".modal.show .modal-body").First);
        });

        await scene.StepAsync("ev-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator(".modal.show").GetByRole(AriaRole.Button, new() { Name = "Cancel" }).First);
            await scene.PauseAsync(500);
            await OpenActivityEditorAsync(scene, "Report Published");
            await scene.HighlightAsync(FieldRow(Field(scene, "ReportPublishedEvent_ReportIds")));
        });

        await scene.HoldAsync(1.5);
    }

    private static async Task TasksAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/Workflows/Types");
        await scene.ClickAndWaitForNavigationAsync(WorkflowLink(scene, "Share a link when a report is published"));
        await scene.Page.Locator(".activity.jtk-managed:visible").First.WaitForAsync();
        await scene.PauseAsync(800);
        await scene.StartCaptureAsync();

        await scene.StepAsync("task-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("a, button").Filter(new() { HasTextString = "Add Task" }).First);
            await scene.PauseAsync(600);
            await scene.ClickAsync(scene.Page.Locator(".modal.show").GetByText("Reports", new() { Exact = true }).First);
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.Locator(".modal.show .modal-body").First);
        });

        await scene.StepAsync("task-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator(".modal.show").GetByRole(AriaRole.Button, new() { Name = "Cancel" }).First);
            await scene.PauseAsync(500);
            await OpenActivityEditorAsync(scene, "Create Report Share Link");
            await scene.HighlightAsync(FieldRow(Field(scene, "CreateReportShareLinkTask_ReportId")));
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(FieldRow(Field(scene, "CreateReportShareLinkTask_ReportIdExpression")));
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(FieldRow(Field(scene, "CreateReportShareLinkTask_Filters")));
        });

        await scene.StepAsync("task-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var recipients = Field(scene, "CreateReportShareLinkTask_Recipients");
            await recipients.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(400);
            await scene.HighlightAsync(FieldRow(recipients));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            var email = Field(scene, "CreateReportShareLinkTask_NotifyByEmail");
            await email.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(FieldRow(email));
        });

        await scene.HoldAsync(1.5);
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync("/Admin/reports/builder/edit/call-dashboard");
        await scene.Page.Locator(".rd-query-bar").WaitForAsync();
        await scene.PauseAsync(1500);
        await scene.StartCaptureAsync();

        await scene.StepAsync("run-1", async () =>
        {
            var publish = scene.Page.GetByRole(AriaRole.Button, new() { Name = "Publish" }).First;
            await scene.MoveToAsync(publish);
            await scene.HighlightAsync(publish);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(publish);
            await scene.PauseAsync(1800);
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.NavigateAsync("/Admin/Workflows/Types");
            await scene.ClickAndWaitForNavigationAsync(WorkflowLink(scene, "Share a link when a report is published"));
            await scene.Page.Locator(".activity.jtk-managed:visible").First.WaitForAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='Instances']:visible").First);
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item, table tbody tr").First);
        });

        await scene.StepAsync("run-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/SharedLinks");
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Filter(new() { HasTextString = "Call dashboard" }).First);
        });

        await scene.HoldAsync(1.5);
    }

    private static async Task ExportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync("/Admin/Workflows/Types");
        await scene.ClickAndWaitForNavigationAsync(WorkflowLink(scene, "Save an Excel copy when the dashboard runs"));
        await scene.Page.Locator(".activity.jtk-managed:visible").First.WaitForAsync();
        await scene.PauseAsync(800);
        await scene.StartCaptureAsync();

        await scene.StepAsync("exp-1", async () =>
        {
            await OpenActivityEditorAsync(scene, "Export Report");
            await scene.HighlightAsync(FieldRow(Field(scene, "ExportReportTask_Format")));
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(FieldRow(Field(scene, "ExportReportTask_MediaFolder")));
        });

        await scene.StepAsync("exp-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/reports/designs/call-dashboard");
            await scene.PauseAsync(700);
            await scene.NavigateAsync("/Admin/Media");
            await scene.PauseAsync(700);
            await scene.ClickAsync(scene.Page.Locator("#media-library, body").GetByText("reports", new() { Exact = true }).First, pauseAfter: 200);
            await scene.PauseAsync(400);
            await scene.ClickAsync(scene.Page.GetByText("runs", new() { Exact = true }).First, pauseAfter: 200);
        });

        await scene.HoldAsync(2);
    }

    private static async Task EmailAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/Workflows/Types");
        await scene.StartCaptureAsync();

        await scene.StepAsync("mail-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(WorkflowLink(scene, "Email the dashboard every morning"));
            await scene.Page.Locator(".activity.jtk-managed:visible").First.WaitForAsync();
            await scene.PauseAsync(800);
            await OpenActivityEditorAsync(scene, "Email Report");
            await scene.HighlightAsync(FieldRow(Field(scene, "EmailReportTask_To")));
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            var attachment = Field(scene, "EmailReportTask_AttachmentFormat");
            await attachment.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(FieldRow(attachment));
        });

        await scene.StepAsync("mail-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("a:visible").Filter(new() { HasTextString = "Cancel" }).First);
            await scene.PauseAsync(1500);
            if (await scene.Page.Locator(".activity.jtk-managed:visible").CountAsync() == 0)
            {
                await scene.NavigateAsync("/Admin/Workflows/Types");
                await scene.ClickAndWaitForNavigationAsync(WorkflowLink(scene, "Email the dashboard every morning"));
            }
            await scene.Page.Locator(".activity.jtk-managed:visible").First.WaitForAsync();
            await scene.PauseAsync(800);
            await scene.HighlightAsync(Activity(scene, "Email Report"));
        });

        await scene.HoldAsync(1.5);
    }
}
