using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Contact Center Reports video, one method per "clip" of storyboard.json.
/// The site must hold the history made by seed-reports.py (see README.md).
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
        ("02-where", WhereAsync),
        ("03-run", RunAsync),
        ("04-filters", FiltersAsync),
        ("05-kpis", KpisAsync),
        ("06-export", ExportAsync),
        ("07-limit", LimitAsync),
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

    private static ILocator Button(Scene scene, string name) => scene.Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).First;

    private static ILocator RangeButton(Scene scene) => scene.Page.Locator("button.form-select.text-start").First;

    private static ILocator Picker(Scene scene, string id) => scene.Page.Locator($"button[data-id='{id}']");

    // Off camera: picks a date range preset and shows the report.
    private static async Task PrepareRangeAsync(Scene scene, string preset)
    {
        await RangeButton(scene).ClickAsync();
        await scene.Page.Locator(".dropdown-menu.show label", new() { HasTextString = preset }).First.ClickAsync();
        await scene.WaitForNavigationAsync(() => Button(scene, "Show").ClickAsync());
    }

    private static Task PickAsync(Scene scene, string id, string text)
        => scene.SelectAsync(scene.Page.Locator("#" + id), text, pauseAfter: 900);

    // Highlights an item of an open drop-down menu (the scene's own highlight scrolls, which closes the menu).
    private static Task HighlightMenuAsync(Scene scene, string itemText, string caption)
        => scene.Page.EvaluateAsync("""
            ({ text, caption }) => {
              const el = text ? [...document.querySelectorAll('.dropdown-menu.show .dropdown-item')].find(e => e.textContent.includes(text)) : document.querySelector('.dropdown-menu.show');
              const b = el.getBoundingClientRect(), p = 6;
              const ring = document.createElement('div');
              ring.className = 'demo-highlight';
              Object.assign(ring.style, { left: (b.x - p) + 'px', top: (b.y - p) + 'px', width: (b.width + 2 * p) + 'px', height: (b.height + 2 * p) + 'px' });
              document.documentElement.appendChild(ring);
              const label = document.createElement('div');
              label.className = 'demo-caption';
              label.textContent = caption;
              Object.assign(label.style, { left: Math.max(8, Math.min(b.x - p, window.innerWidth - 380)) + 'px', top: (b.y + b.height + p + 10) + 'px' });
              document.documentElement.appendChild(label);
            }
            """, new { text = itemText, caption });

    private static async Task WhereAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync("/Admin/Reports");
        await scene.StartCaptureAsync();

        await scene.StepAsync("where-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#left-nav li.reports ul.nav-group-items").First, "Reports, grouped by category");
            await scene.PauseAsync(4500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#left-nav li.report-category > figure", new() { HasTextString = "CRM & Campaigns" }).First, "CRM reports");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("where-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#left-nav li.report-category > figure", new() { HasTextString = "Operations" }).First, pauseAfter: 1500);
        });
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync("/Admin/Reports/view/contact-center-call-insights");
        await scene.StartCaptureAsync();

        await scene.StepAsync("run-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("h1").First, "Operations > Call insights");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("form").Filter(new() { Has = RangeButton(scene) }).First, "Filters");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("run-2", async () =>
        {
            await scene.ClickAsync(RangeButton(scene), pauseAfter: 1200);
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show").First, "Date range");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("run-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show label", new() { HasTextString = "Custom Range" }).First, pauseAfter: 1000);
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show label", new() { HasTextString = "On or before" }).First, "One side open");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("run-4", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show label", new() { HasTextString = "Last 7 Days" }).First, pauseAfter: 800);
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(Button(scene, "Show"), pauseAfter: 600));
            await scene.PauseAsync(1500);
            await scene.ScrollAsync(0, 200);
        });
    }

    private static async Task FiltersAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync("/Admin/Reports/view/contact-center-call-insights");
        await PrepareRangeAsync(scene, "Last 7 Days");
        await scene.StartCaptureAsync();

        await scene.StepAsync("filt-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#ReportFilter_QueueGroupId").Locator("xpath=..").First, "Queue group");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#ReportFilter_Channel").Locator("xpath=..").First, "Channel");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("filt-2", async () =>
        {
            await PickAsync(scene, "ReportFilter_QueueGroupId", "Customer care");
            await PickAsync(scene, "ReportFilter_Direction", "Inbound");
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(Button(scene, "Show"), pauseAfter: 600));
        });

        await scene.StepAsync("filt-3", async () =>
        {
            await PickAsync(scene, "ReportFilter_AgentId", "jamie.chen");
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(Button(scene, "Show"), pauseAfter: 600));
        });
    }

    private static async Task KpisAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync("/Admin/Reports/view/contact-center-executive-performance");
        await PrepareRangeAsync(scene, "Last 7 Days");
        await scene.Page.EvaluateAsync("() => window.scrollTo(0, 330)");
        await scene.StartCaptureAsync();

        ILocator Card(string text) => scene.Page.Locator(".card.h-100").Filter(new() { HasText = text }).First;

        await scene.StepAsync("kpi-1", async () =>
        {
            await scene.HighlightAsync(Card("Interactions"), "Interactions");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Card("Inbound offered"), "Inbound offered");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Card("Inbound answered"), "Inbound answered, and the answer rate");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("kpi-2", async () =>
        {
            await scene.HighlightAsync(Card("Abandoned"), "Abandoned");
            await scene.PauseAsync(4500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Card("Voicemail"), "Voicemail");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Card("Callback requested"), "Callback requested");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("kpi-3", async () =>
        {
            await scene.HighlightAsync(Card("Failed"), "Failed");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Card("Avg speed of answer"), "Average speed of answer");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Card("Avg handle time"), "Average handle time");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("kpi-4", async () =>
        {
            await scene.HighlightAsync(Card("Transfer rate"), "Transfer rate");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Card("Recording coverage"), "Recording coverage");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 700);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ExportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync("/Admin/Reports/view/contact-center-executive-performance");
        await PrepareRangeAsync(scene, "Last 7 Days");
        await scene.StartCaptureAsync();

        await scene.StepAsync("exp-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#report-export-menu-button"), pauseAfter: 1200);
            await HighlightMenuAsync(scene, null, "Export");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("exp-2", async () =>
        {
            await HighlightMenuAsync(scene, "Excel", "One worksheet per section");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await HighlightMenuAsync(scene, "PDF", "As it looks on screen");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task LimitAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync("/Admin/Reports/view/contact-center-agent-time-summary");
        await PrepareRangeAsync(scene, "Last 30 Days");
        await scene.StartCaptureAsync();

        await scene.StepAsync("lim-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("h1").First, "Workforce & Payroll > Agent time summary");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 300);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("lim-2", async () =>
        {
            await scene.ScrollAsync(0, -600);
            await scene.ClickAsync(RangeButton(scene), pauseAfter: 900);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show label", new() { HasTextString = "Last 90 Days" }).First, pauseAfter: 700);
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(Button(scene, "Show"), pauseAfter: 600));
            await scene.HighlightAsync(scene.Page.GetByText("This period is too long for this report").First.Locator("xpath=.."), "Longest period: 31 days");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("lim-3", async () =>
        {
            await scene.ClickAsync(RangeButton(scene), pauseAfter: 900);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show label", new() { HasTextString = "Last 30 Days" }).First, pauseAfter: 700);
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(Button(scene, "Show"), pauseAfter: 600));
            await scene.PauseAsync(1500);
        });
    }
}
