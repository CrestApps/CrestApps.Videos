using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the "Omnichannel Reports" video, one method per "clip" of storyboard.json.
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
        ("02b-permission", PermissionAsync),
        ("03-volume", VolumeAsync),
        ("04-backlog", BacklogAsync),
        ("05-groupings", GroupingsAsync),
        ("06-people", PeopleAsync),
        ("07-campaigns", CampaignsAsync),
        ("08-dialer", DialerAsync),
        ("09-leads", LeadsAsync),
        ("10-builder", BuilderAsync),
        ("11-uses", UsesAsync),
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

    // ----- Helpers -----

    // A report opened on the last 90 days, the way Last 90 Days and Show would open it.
    private static string Last90(string report)
    {
        var to = DateTime.Now;
        var from = to.AddDays(-90);

        return $"/Admin/Reports/view/omnichannel-{report}?ReportFilter.Range=last90&ReportFilter.From={from:yyyy-MM-dd}T00:00&ReportFilter.To={to:yyyy-MM-dd}T23:59";
    }

    private static ILocator Nav(Scene scene, string text)
        => scene.Page.Locator("#left-nav button:visible, #left-nav a:visible").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex($"^\\s*{System.Text.RegularExpressions.Regex.Escape(text)}\\s*$") }).First;

    private static ILocator Show(Scene scene) => scene.Page.Locator("form button[type=submit].btn-primary").Filter(new() { HasTextString = "Show" }).First;

    private static ILocator RangeButton(Scene scene) => scene.Page.Locator("[data-drp-toggle]").First;

    private static ILocator RangeChoice(Scene scene, string label) => scene.Page.Locator(".dropdown-menu.show label.form-check-label").Filter(new() { HasTextString = label }).First;

    private static ILocator FilterBar(Scene scene) => scene.Page.Locator(".card-header .row").First;

    private static ILocator Section(Scene scene, int index) => scene.Page.Locator("section.report-section").Nth(index);

    // The header and first rows of a long table, so a highlight does not scroll the page down the table.
    private static ILocator Head(Scene scene) => Section(scene, 0).Locator("thead").First;

    private static ILocator Row(Scene scene, string text)
        => scene.Page.Locator("section.report-section tbody tr").Filter(new() { HasTextString = text }).First;

    // Opens another report during the capture, on the last 90 days, and lets it settle.
    private static async Task GoAsync(Scene scene, string report, int settle = 500)
    {
        await scene.NavigateAsync(Last90(report));
        await scene.Page.EvaluateAsync("() => window.scrollTo(0, 0)");
        await scene.PauseAsync(settle);
    }

    // ----- Clips -----

    private static async Task WhereAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("where-1", async () =>
        {
            await scene.ClickAsync(Nav(scene, "Reports"));
            await scene.PauseAsync(400);
            await scene.ClickAsync(Nav(scene, "CRM & Campaigns"));
            await scene.HighlightAsync(scene.Page.Locator("#left-nav").First, "The Reports menu");
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/Reports");
            var heading = scene.Page.Locator("h2.h4").Filter(new() { HasTextString = "CRM & Campaigns" }).First;
            await heading.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(500);
        });

        await scene.StepAsync("where-2", async () =>
        {
            var card = scene.Page.Locator("a.card[href$='omnichannel-activity-summary']").First;
            await card.ScrollIntoViewIfNeededAsync();
            await scene.ClickAndWaitForNavigationAsync(card);
            await scene.PauseAsync(400);
            await scene.HighlightAsync(FilterBar(scene));
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(RangeButton(scene));
            await scene.ClickAsync(RangeChoice(scene, "Last 90 Days"));
            await scene.ClickAndWaitForNavigationAsync(Show(scene));
            await scene.ClickAsync(scene.Page.Locator("#report-export-menu-button").First, pauseAfter: 300);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PermissionAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Roles/Edit/Editor");
        await scene.StartCaptureAsync();

        await scene.StepAsync("perm-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "omnichannel reports", delay: 70);
            await scene.PauseAsync(600);
            var row = scene.Page.Locator("tr").Filter(new() { Has = scene.Page.Locator("[id='Checkbox.ViewOmnichannelReports']") }).First;
            await scene.HighlightAsync(row);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task VolumeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync(Last90("activity-summary"));
        await scene.StartCaptureAsync();

        await scene.StepAsync("vol-1", async () =>
        {
            await scene.HighlightAsync(Section(scene, 0));
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 520);
            await scene.PauseAsync(300);
        });

        await scene.StepAsync("vol-2", async () =>
        {
            await GoAsync(scene, "campaign-performance");
            await scene.HighlightAsync(Section(scene, 0));
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(scene, 1));
        });

        await scene.StepAsync("vol-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, "disposition-breakdown");
            await scene.HighlightAsync(Section(scene, 0));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task BacklogAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync(Last90("activity-backlog"));
        await scene.StartCaptureAsync();

        await scene.StepAsync("back-1", async () =>
        {
            await scene.HighlightAsync(Section(scene, 0));
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(scene, 1));
        });

        await scene.StepAsync("back-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, "activity-aging");
            await scene.HighlightAsync(Row(scene, "7+ days"));
        });

        await scene.StepAsync("back-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, "overdue-by-user");
            await scene.HighlightAsync(Section(scene, 0));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task GroupingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync(Last90("source-performance"));
        await scene.StartCaptureAsync();

        await scene.StepAsync("grp-1", async () =>
        {
            await scene.HighlightAsync(Row(scene, "PreviewDial"));
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(scene, "Automatic"));
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(scene, 0));
        });

        await scene.StepAsync("grp-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, "channel-performance", 1200);
            await GoAsync(scene, "kind-performance", 1200);
            await GoAsync(scene, "contact-type-workload", 600);
            await scene.HighlightAsync(Section(scene, 0));
        });

        await scene.StepAsync("grp-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, "assignment-performance", 2500);
            await GoAsync(scene, "urgency-performance", 600);
            await scene.HighlightAsync(Section(scene, 0));
        });

        await scene.StepAsync("grp-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, "attempt-analysis", 3500);
            await scene.HighlightAsync(Section(scene, 0));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, "schedule-completion", 600);
            await scene.HighlightAsync(Section(scene, 0));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PeopleAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync(Last90("assigned-user-performance"));
        await scene.StartCaptureAsync();

        await scene.StepAsync("ppl-1", async () =>
        {
            await scene.HighlightAsync(Section(scene, 0));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(scene, "(Not set)"));
        });

        await scene.StepAsync("ppl-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, "user-completion-time", 3500);
            await scene.HighlightAsync(Section(scene, 0));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, "user-daily-productivity", 600);
            await scene.HighlightAsync(Head(scene));
        });

        await scene.StepAsync("ppl-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, "created-by-performance", 2500);
            await scene.HighlightAsync(Row(scene, "(Not set)"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task CampaignsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync(Last90("campaign-source-mix"));
        await scene.StartCaptureAsync();

        await scene.StepAsync("camp-1", async () =>
        {
            await scene.HighlightAsync(Head(scene));
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, "campaign-channel-mix", 300);
            await scene.HighlightAsync(Head(scene));
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, "campaign-disposition-mix", 300);
            await scene.HighlightAsync(Head(scene));
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, "campaign-attempt-performance", 300);
            await scene.HighlightAsync(Head(scene));
        });

        await scene.StepAsync("camp-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, "customer-workload", 600);
            await scene.HighlightAsync(Head(scene));
        });

        await scene.StepAsync("camp-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, "channel-endpoint-usage", 600);
            await scene.HighlightAsync(Section(scene, 0));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task DialerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync(Last90("dialer-call-detail"));
        await scene.StartCaptureAsync();

        await scene.StepAsync("dial-1", async () =>
        {
            await scene.HighlightAsync(Section(scene, 0).Locator("thead").First);
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("select[name='ReportFilter.Source']").First);
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 400);
        });

        await scene.StepAsync("dial-2", async () =>
        {
            await GoAsync(scene, "ai-handoff-containment");
            await scene.HighlightAsync(Section(scene, 0));
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(scene, 1));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task LeadsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync(Last90("lead-funnel"));
        await scene.StartCaptureAsync();

        await scene.StepAsync("lead-1", async () =>
        {
            await scene.HighlightAsync(Section(scene, 0));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(scene, 1));
        });

        await scene.StepAsync("lead-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, "lead-conversion");
            await scene.HighlightAsync(Section(scene, 0));
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(scene, 1));
        });

        await scene.StepAsync("lead-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, "opportunity-pipeline");
            await scene.HighlightAsync(Section(scene, 0));
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(scene, 1));
        });

        await scene.ClearHighlightsAsync();
    }

    // ----- The Report Builder -----

    private static ILocator Tab(Scene scene, string name)
        => scene.Page.Locator("[data-report-designer] .nav-link").Filter(new() { HasTextString = name });

    private static ILocator Field(Scene scene, string key)
        => scene.Page.Locator($".report-designer-field[data-field-key=\"{key}\"]");

    private static ILocator Shelf(Scene scene, string kind)
        => scene.Page.Locator($".report-designer-shelf-zone[data-shelf={kind}]");

    private static ILocator Button(Scene scene, string name)
        => scene.Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).First;

    private static async Task CompactMenuAsync(Scene scene)
    {
        await scene.Page.EvaluateAsync("() => { if (!document.body.classList.contains('left-sidebar-compact')) document.querySelector('.leftbar-compactor')?.click(); }");
        await Task.Delay(400);
    }

    private static ILocator ModalSource(Scene scene, string source)
        => scene.Page.Locator(".modal.show .nav-link").Filter(new() { HasTextString = source }).First;

    private static ILocator ModalCard(Scene scene, string dataSet)
        => scene.Page.Locator(".modal.show .card").Filter(new() { Has = scene.Page.Locator(".card-title", new() { HasTextRegex = new System.Text.RegularExpressions.Regex($"^\\s*{dataSet}\\s*$") }) }).First;

    private static async Task BuilderAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/reports/designs");
        await CompactMenuAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("rb-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a").Filter(new() { HasTextString = "New Report" }).First);
            await scene.PauseAsync(1200);
            await CompactMenuAsync(scene);
            await scene.ClickAsync(Button(scene, "Add data set"), pauseAfter: 600);
        });

        await scene.StepAsync("rb-2", async () =>
        {
            await scene.ClickAsync(ModalSource(scene, "Omnichannel"), pauseAfter: 3000);
            await scene.ClickAsync(ModalSource(scene, "Content items"), pauseAfter: 2200);
        });

        await scene.StepAsync("rb-3", async () =>
        {
            await scene.ClickAsync(ModalSource(scene, "Omnichannel"), pauseAfter: 400);
            var card = ModalCard(scene, "Activities");
            await scene.MoveToAsync(card);
            await scene.ClickAsync(card.Locator(".card-footer button").First, pauseAfter: 1500);
            await scene.ClickAsync(Button(scene, "Add data set"), pauseAfter: 600);
            await scene.HighlightAsync(scene.Page.Locator(".modal.show .modal-body").First);
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator(".modal.show .btn-close").First, pauseAfter: 400);
        });

        await scene.StepAsync("rb-4", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".report-designer-data .text-danger, .report-designer-data button[title*='emove']").First, pauseAfter: 700);
            await scene.ClickAsync(Button(scene, "Add data set"), pauseAfter: 500);
            await scene.ClickAsync(ModalSource(scene, "Messaging"), pauseAfter: 1800);
            var card = ModalCard(scene, "Messages");
            await scene.MoveToAsync(card);
            await scene.ClickAsync(card.Locator(".card-footer button").First, pauseAfter: 1200);
        });

        await scene.StepAsync("rb-5", async () =>
        {
            await scene.DragToAsync(Field(scene, "Messages.DeliveryStatus"), Shelf(scene, "columns"));
            await scene.PauseAsync(1200);
            await scene.DragToAsync(Field(scene, "$count"), Shelf(scene, "columns"), 220);
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.Locator(".report-designer-preview table").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task UsesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync(Last90("activity-backlog"));
        await scene.StartCaptureAsync();

        await scene.StepAsync("use-1", async () =>
        {
            await scene.HighlightAsync(Section(scene, 0));
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, "overdue-by-user");
            await scene.HighlightAsync(Section(scene, 0));
        });

        await scene.StepAsync("use-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, "campaign-performance", 1800);
            await GoAsync(scene, "campaign-disposition-mix", 1800);
            await GoAsync(scene, "lead-conversion", 500);
            await scene.HighlightAsync(Section(scene, 0));
        });

        await scene.StepAsync("use-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, "ai-handoff-containment", 1000);
            await scene.HighlightAsync(Section(scene, 1));
        });

        await scene.ClearHighlightsAsync();
    }
}
