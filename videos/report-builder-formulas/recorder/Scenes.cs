using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the "Report Builder formulas" video, one method per "clip" of storyboard.json.
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
        ("02-add", AddAsync),
        ("03-labels", LabelsAsync),
        ("04-groups", GroupsAsync),
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
    /// Turns the Report Builder on and imports the two starting reports (not recorded).
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
            Console.WriteLine("Imported the starting reports");
        }

        await context.CloseAsync();
    }

    // ----- Helpers -----

    private static async Task OpenDesignerAsync(Scene scene, string id)
    {
        await scene.OpenAsync("/Admin/reports/builder/edit/" + id);
        await scene.Page.Locator(".rd-query-bar").WaitForAsync();
        await scene.Page.Locator(".rd-preview-panel .report-container-header, .rd-preview-panel table").First.WaitForAsync();
        await scene.PauseAsync(1200);
    }

    private static ILocator CalcCard(Scene scene)
        => scene.Page.Locator(".card").Filter(new() { Has = scene.Page.Locator(".card-header", new() { HasTextString = "Calculated fields" }) }).First;

    private static ILocator Modal(Scene scene) => scene.Page.Locator(".modal.show");

    private static async Task OpenNewCalculatedFieldAsync(Scene scene, string label, string name)
    {
        await scene.ClickAsync(CalcCard(scene).Locator(".card-header button").Filter(new() { HasTextString = "New" }).First);
        await scene.PauseAsync(700);
        await scene.TypeAsync(Modal(scene).Locator("input[placeholder^='Label']"), label, delay: 22);
        await Modal(scene).Locator("input[placeholder^='Name used']").FillAsync(string.Empty);
        await scene.TypeAsync(Modal(scene).Locator("input[placeholder^='Name used']"), name, delay: 22);
    }

    private static async Task ApplyAsync(Scene scene)
    {
        await scene.ClickAsync(Modal(scene).GetByRole(AriaRole.Button, new() { Name = "Check" }));
        await scene.PauseAsync(800);
        await scene.ClickAsync(Modal(scene).GetByRole(AriaRole.Button, new() { Name = "Apply" }));
        await scene.PauseAsync(400);
    }

    private static async Task DragToColumnsAsync(Scene scene, string label)
    {
        var item = CalcCard(scene).Locator(".list-group-item, li, .rd-field").Filter(new() { HasTextString = label }).First;
        var columns = scene.Page.Locator(".report-designer-shelf").Filter(new() { HasTextRegex = new Regex("^\\s*Columns") }).First.Locator(".report-designer-shelf-zone");
        await scene.DragToAsync(item, columns, 30, 10);
        await scene.PauseAsync(700);
    }

    private static async Task NoAggregateAsync(Scene scene)
    {
        var label = scene.Page.Locator("label, .form-label, .small").Filter(new() { HasTextRegex = new Regex("^\\s*Aggregate\\s*$") }).First;
        var select = label.Locator("xpath=following::select[1]");
        await select.ScrollIntoViewIfNeededAsync();
        await scene.MoveToAsync(select);
        await select.EvaluateAsync("s => { const o = [...s.options].find(x => /none/i.test(x.text)); s.value = o.value; s.dispatchEvent(new Event('change', { bubbles: true })); }");
        await scene.PauseAsync(1200);
    }

    private static ILocator FormulaBox(Scene scene) => Modal(scene).Locator("textarea");

    private static async Task InsertFieldAsync(Scene scene, string title)
        => await scene.ClickAsync(Modal(scene).Locator($".report-designer-formula-list button[title='{title}']").First, pauseAfter: 300);

    // ----- Clips -----

    private static async Task AddAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await OpenDesignerAsync(scene, "calls-with-formulas");
        await scene.StartCaptureAsync();

        await scene.StepAsync("add-1", async () =>
        {
            await scene.HighlightAsync(CalcCard(scene));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(CalcCard(scene).Locator(".card-header button").Filter(new() { HasTextString = "New" }).First);
        });

        await scene.StepAsync("add-2", async () =>
        {
            await scene.TypeAsync(Modal(scene).Locator("input[placeholder^='Label']"), "Wait in minutes", delay: 22);
            await Modal(scene).Locator("input[placeholder^='Name used']").FillAsync(string.Empty);
            await scene.TypeAsync(Modal(scene).Locator("input[placeholder^='Name used']"), "WaitMinutes", delay: 22);
        });

        await scene.StepAsync("add-3", async () =>
        {
            await scene.ClickAsync(FormulaBox(scene));
            await InsertFieldAsync(scene, "I.WaitSeconds");
            await scene.TypeAsync(FormulaBox(scene), " / 60", delay: 50);
        });

        await scene.StepAsync("add-4", async () =>
        {
            await scene.ClickAsync(Modal(scene).GetByRole(AriaRole.Button, new() { Name = "Check" }));
            await scene.HighlightAsync(Modal(scene).Locator(".modal-body").First);
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Modal(scene).GetByRole(AriaRole.Button, new() { Name = "Apply" }));
        });

        await scene.StepAsync("add-5", async () =>
        {
            await scene.PauseAsync(600);
            await scene.HighlightAsync(CalcCard(scene));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await DragToColumnsAsync(scene, "Wait in minutes");
            await NoAggregateAsync(scene);
        });

        await scene.HoldAsync(2);
    }

    private static async Task LabelsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await OpenDesignerAsync(scene, "calls-with-formulas");
        await scene.StartCaptureAsync();

        await scene.StepAsync("lab-1", async () =>
        {
            await OpenNewCalculatedFieldAsync(scene, "Call length", "CallLength");
            await scene.TypeAsync(FormulaBox(scene), "IF([I.TalkSeconds] >= 600, 'Long', 'Short')", delay: 10);
            await ApplyAsync(scene);
            await DragToColumnsAsync(scene, "Call length");
        });

        await scene.StepAsync("lab-2", async () =>
        {
            await OpenNewCalculatedFieldAsync(scene, "Queue or campaigns", "QueueName");
            await scene.TypeAsync(FormulaBox(scene), "COALESCE([Q.Name], 'Outbound campaigns')", delay: 10);
            await ApplyAsync(scene);
            await DragToColumnsAsync(scene, "Queue or campaigns");
        });

        await scene.StepAsync("lab-3", async () =>
        {
            await OpenNewCalculatedFieldAsync(scene, "Age in days", "AgeDays");
            await scene.TypeAsync(FormulaBox(scene), "DATEDIFF('day', [I.CreatedUtc], TODAY())", delay: 10);
            await scene.ScrollAsync(0, 0);
            await scene.HighlightAsync(Modal(scene).GetByText("Functions", new() { Exact = true }).First.Locator("xpath=following::div[1]"));
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await ApplyAsync(scene);
            await DragToColumnsAsync(scene, "Age in days");
            await NoAggregateAsync(scene);
        });

        await scene.HoldAsync(2);
    }

    private static async Task GroupsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await OpenDesignerAsync(scene, "queues-with-formulas");
        await scene.StartCaptureAsync();

        await scene.StepAsync("grp-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".report-designer-shelf").Filter(new() { HasTextRegex = new Regex("^\\s*Columns") }).First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await OpenNewCalculatedFieldAsync(scene, "Answer rate", "AnswerRate");
        });

        await scene.StepAsync("grp-2", async () =>
        {
            await scene.TypeAsync(FormulaBox(scene), "[I.TalkSeconds] / COUNT()", delay: 10);
            await scene.ClickAsync(Modal(scene).GetByRole(AriaRole.Button, new() { Name = "Check" }));
            await scene.HighlightAsync(Modal(scene).Locator(".modal-body").First);
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await FormulaBox(scene).FillAsync(string.Empty);
            await scene.TypeAsync(FormulaBox(scene), "SUM(IF(ISNULL([I.AnsweredUtc]), 0, 1)) / COUNT()", delay: 10);
        });

        await scene.StepAsync("grp-3", async () =>
        {
            await ApplyAsync(scene);
            await DragToColumnsAsync(scene, "Answer rate");
            await scene.TypeAsync(scene.Page.Locator("input[placeholder^='N2, C2']").First, "P0", delay: 80);
            await scene.Page.Keyboard.PressAsync("Tab");
            await scene.PauseAsync(800);
        });

        await scene.HoldAsync(2);
    }
}
