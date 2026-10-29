using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Contact Center Report Catalog video, one method per "clip" of storyboard.json.
/// The site must hold the history made by seed-reports.py (see README.md).
/// Every report is opened with a custom range of the last seven days, so it shows numbers.
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
        ("02-executive", ExecutiveAsync),
        ("03-operations", OperationsAsync),
        ("04-queues", QueuesAsync),
        ("05-agents", AgentsAsync),
        ("06-workforce", WorkforceAsync),
        ("07-billing", BillingAsync),
        ("08-compliance", ComplianceAsync),
        ("09-technical", TechnicalAsync),
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

    // The last seven days up to yesterday, which the seeded history covers.
    private static string ReportPath(string report)
        => $"/Admin/Reports/view/{report}?ReportFilter.Range=custom&ReportFilter.From={DateTime.Now.AddDays(-7):yyyy-MM-dd}T00:00&ReportFilter.To={DateTime.Now.AddDays(-1):yyyy-MM-dd}T23:59";

    // Cuts to another report: the navigation is not recorded.
    private static Task JumpAsync(Scene scene, string report)
        => scene.OffCameraAsync(() => scene.NavigateAsync(ReportPath(report)));

    // Cuts to the Reports page, scrolled to a category.
    private static Task JumpToIndexAsync(Scene scene, string category)
        => scene.OffCameraAsync(async () =>
        {
            await scene.NavigateAsync("/Admin/Reports");
            await ScrollToCategoryAsync(scene, category);
        });

    private static async Task ScrollToCategoryAsync(Scene scene, string category)
    {
        await scene.Page.Locator("h2", new() { HasTextString = category }).First.EvaluateAsync("el => { el.scrollIntoView(); window.scrollBy(0, -80); }");
        await Task.Delay(500);
    }

    private static ILocator Card(Scene scene, string name)
        => scene.Page.Locator("a.card").Filter(new() { Has = scene.Page.Locator("h3", new() { HasTextRegex = new Regex($"^{Regex.Escape(name)}$") }) }).First;

    private static async Task ShowCardAsync(Scene scene, string name, int hold = 1800)
    {
        await Card(scene, name).ScrollIntoViewIfNeededAsync();
        await scene.HighlightAsync(Card(scene, name), name);
        await scene.PauseAsync(hold);
        await scene.ClearHighlightsAsync();
    }

    private static async Task ShowTableAsync(Scene scene, string caption, int hold = 2500)
    {
        await scene.HighlightAsync(scene.Page.Locator("table").First, caption);
        await scene.PauseAsync(hold);
        await scene.ClearHighlightsAsync();
    }

    private static async Task<Scene> OpenIndexAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string category)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);
        await scene.OpenAsync("/Admin/Reports");
        await ScrollToCategoryAsync(scene, category);
        await scene.StartCaptureAsync();
        return scene;
    }

    private static async Task ExecutiveAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenIndexAsync(browser, directory, durations, "Executive");

        await scene.StepAsync("ex-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("h2", new() { HasTextString = "Executive" }).First, "Executive");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await ShowCardAsync(scene, "Executive performance dashboard", 2500);
            await JumpAsync(scene, "contact-center-executive-performance");
            await scene.Page.EvaluateAsync("() => window.scrollTo(0, 330)");
            await scene.PauseAsync(1800);
            await scene.ScrollAsync(0, 650);
        });

        await scene.StepAsync("ex-2", async () =>
        {
            await scene.ScrollAsync(0, -1000);
            await scene.HighlightAsync(scene.Page.Locator("#ReportFilter_QueueGroupId").Locator("xpath=..").First, "Filters");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task OperationsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenIndexAsync(browser, directory, durations, "Operations");

        await scene.StepAsync("ops-1", async () =>
        {
            await ShowCardAsync(scene, "Call insights", 2200);
            await ShowCardAsync(scene, "Interaction volume trend", 2200);
            await ShowCardAsync(scene, "Interval performance", 1800);
        });

        await scene.StepAsync("ops-2", async () =>
        {
            await JumpAsync(scene, "contact-center-hour-of-day-performance");
            await ShowTableAsync(scene, "Calls by hour (UTC)", 3000);
        });

        await scene.StepAsync("ops-3", async () =>
        {
            await JumpToIndexAsync(scene, "Operations");
            await ShowCardAsync(scene, "Channel performance", 1500);
            await ShowCardAsync(scene, "Direction performance", 1500);
            await ShowCardAsync(scene, "Interaction outcome summary", 1500);
            await ShowCardAsync(scene, "Interaction lifecycle duration", 2000);
            await ShowCardAsync(scene, "Call quality", 1500);
        });
    }

    private static async Task QueuesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenIndexAsync(browser, directory, durations, "Queue & Routing");

        await scene.StepAsync("qr-1", async () =>
        {
            await JumpAsync(scene, "contact-center-queue-service-level");
            await ShowTableAsync(scene, "Service level per queue", 5000);
            await JumpAsync(scene, "contact-center-queue-usage");
            await ShowTableAsync(scene, "Handled, answered, abandoned and waiting now", 3000);
        });

        await scene.StepAsync("qr-2", async () =>
        {
            await JumpToIndexAsync(scene, "Queue & Routing");
            await ShowCardAsync(scene, "Queue abandonment analysis", 1600);
            await ShowCardAsync(scene, "Queue wait time analysis", 1600);
            await ShowCardAsync(scene, "Queue handle time analysis", 1600);
            await ShowCardAsync(scene, "Queue performance summary", 1600);
            await ShowCardAsync(scene, "Queue transfer performance", 1600);
            await ShowCardAsync(scene, "Transfer analysis", 1600);
            await ShowCardAsync(scene, "High-wait interaction detail", 1600);
        });

        await scene.StepAsync("qr-3", async () =>
        {
            await ShowCardAsync(scene, "Agent availability and wait time", 3000);
            await ShowCardAsync(scene, "Dialer decision log", 2500);
        });
    }

    private static async Task AgentsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenIndexAsync(browser, directory, durations, "Agent Performance");

        await scene.StepAsync("ag-1", async () =>
        {
            await ShowCardAsync(scene, "Agent productivity", 2000);
            await JumpAsync(scene, "contact-center-agent-productivity");
            await ShowTableAsync(scene, "One row per agent", 4000);
        });

        await scene.StepAsync("ag-2", async () =>
        {
            await JumpAsync(scene, "contact-center-agent-utilization");
            await ShowTableAsync(scene, "Utilization", 4000);
            await JumpAsync(scene, "contact-center-agent-occupancy");
            await ShowTableAsync(scene, "Occupancy", 3000);
        });

        await scene.StepAsync("ag-3", async () =>
        {
            await JumpToIndexAsync(scene, "Agent Performance");
            await ShowCardAsync(scene, "Agent handle time analysis", 1600);
            await ShowCardAsync(scene, "Agent wrap-up performance", 1600);
            await ShowCardAsync(scene, "Agent inbound performance", 1400);
            await ShowCardAsync(scene, "Agent outbound performance", 1400);
            await ShowCardAsync(scene, "Agent transfer performance", 1400);
            await ShowCardAsync(scene, "Agent outcome performance", 1400);
            await ShowCardAsync(scene, "Talk, hold, ring and queue wait", 2000);
        });
    }

    private static async Task WorkforceAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenIndexAsync(browser, directory, durations, "Workforce & Payroll");

        await scene.StepAsync("wf-1", async () =>
        {
            await ShowCardAsync(scene, "Agent time summary", 1800);
            await JumpAsync(scene, "contact-center-agent-time-summary");
            await ShowTableAsync(scene, "Time in each state", 3500);
            await JumpAsync(scene, "contact-center-agent-daily-timecard");
            await ShowTableAsync(scene, "Day by day", 2500);
        });

        await scene.StepAsync("wf-2", async () =>
        {
            await JumpAsync(scene, "contact-center-payroll-reconciled-timecard");
            await ShowTableAsync(scene, "Each day, checked", 6000);
        });

        await scene.StepAsync("wf-3", async () =>
        {
            await JumpToIndexAsync(scene, "Workforce & Payroll");
            await ShowCardAsync(scene, "Presence status duration", 1500);
            await ShowCardAsync(scene, "Presence reason breakdown", 1500);
            await ShowCardAsync(scene, "Ready versus not-ready time", 1500);
            await ShowCardAsync(scene, "Agent break and away analysis", 1500);
            await ShowCardAsync(scene, "Queue signed-in hours", 1500);
            await ShowCardAsync(scene, "Campaign signed-in hours", 1500);
        });
    }

    private static async Task BillingAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenIndexAsync(browser, directory, durations, "Billing & Usage");

        await scene.StepAsync("bu-1", async () =>
        {
            await ShowCardAsync(scene, "Queue usage for billing", 1800);
            await JumpAsync(scene, "contact-center-queue-usage-billing");
            await ShowTableAsync(scene, "Measured usage per queue", 5000);
        });

        await scene.StepAsync("bu-2", async () =>
        {
            await JumpToIndexAsync(scene, "Billing & Usage");
            await ShowCardAsync(scene, "Agent usage for billing", 1800);
            await ShowCardAsync(scene, "Channel usage for billing", 1800);
            await ShowCardAsync(scene, "Provider usage for billing", 1800);
            await ShowCardAsync(scene, "Daily usage for billing", 1800);
        });
    }

    private static async Task ComplianceAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenIndexAsync(browser, directory, durations, "Compliance & Audit");

        await scene.StepAsync("co-1", async () =>
        {
            await ShowCardAsync(scene, "Interaction detail", 1500);
            await JumpAsync(scene, "contact-center-interaction-detail");
            await ShowTableAsync(scene, "One row per interaction", 5000);
        });

        await scene.StepAsync("co-2", async () =>
        {
            await JumpAsync(scene, "contact-center-recording-coverage");
            await ShowTableAsync(scene, "Recording coverage by provider", 3000);
        });

        await scene.StepAsync("co-3", async () =>
        {
            await JumpToIndexAsync(scene, "Compliance & Audit");
            await ShowCardAsync(scene, "Abandoned interaction detail", 1800);
            await ShowCardAsync(scene, "Long interaction detail", 1800);
            await ShowCardAsync(scene, "Agent presence audit", 1800);
            await ShowCardAsync(scene, "Agent activity timeline", 2000);
        });
    }

    private static async Task TechnicalAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await OpenIndexAsync(browser, directory, durations, "Technical & IT");

        await scene.StepAsync("te-1", async () =>
        {
            await ShowCardAsync(scene, "Provider performance", 1500);
            await JumpAsync(scene, "contact-center-provider-performance");
            await ShowTableAsync(scene, "Outcomes by provider", 4000);
        });

        await scene.StepAsync("te-2", async () =>
        {
            await JumpAsync(scene, "contact-center-call-leg-performance");
            await ShowTableAsync(scene, "Call legs", 2500);
            await JumpAsync(scene, "contact-center-failed-interactions");
            await ShowTableAsync(scene, "Failed calls", 2500);
        });
    }
}
