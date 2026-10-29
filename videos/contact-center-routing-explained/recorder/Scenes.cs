using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Routing explained video, one method per "clip" of storyboard.json.
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
        ("02-queue", QueueAsync),
        ("03-skills", SkillsAsync),
        ("04-priority", PriorityAsync),
        ("04-aging", AgingAsync),
        ("06-overflow", OverflowAsync),
        ("07-report", ReportAsync),
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

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator Card(Scene scene, string title)
        => scene.Page.Locator(".card").Filter(new() { Has = scene.Page.Locator(".card-header", new() { HasTextString = title }) }).First;

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static ILocator Feature(Scene scene, string id)
        => scene.Page.Locator($"#btn-enable-{id.Replace('.', '_')}, #btn-disable-{id.Replace('.', '_')}").First.Locator("xpath=ancestor::li[contains(@class,'list-group-item')][1]");

    private static async Task ScrollToAsync(Scene scene, ILocator target, double margin = 150)
    {
        var box = await target.BoundingBoxAsync();

        if (box is not null)
        {
            await scene.ScrollAsync(0, box.Y - margin);
        }
    }

    private static async Task PickAsync(Scene scene, string selectId, string text)
    {
        await scene.ClickAsync(scene.Page.Locator($"button[data-id='{selectId}']"), pauseAfter: 900);
        await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = text }).First, pauseAfter: 600);
    }

    // ----- Clips -----
    private static ILocator Label(Scene scene, string text)
        => scene.Page.Locator("label, .ocat-label").Filter(new() { HasTextString = text }).First;

    private static ILocator Q(Scene scene, string name) => scene.Page.Locator($"#ActivityQueue_{name}");

    private static async Task OpenMenuAsync(Scene scene, string href)
    {
        var item = Menu(scene, href);

        if (!await item.IsVisibleAsync())
        {
            await scene.ClickAsync(scene.Page.Locator("#left-nav button.interaction-center").First, pauseAfter: 800);
        }

        if (!await item.IsVisibleAsync())
        {
            await scene.ClickAsync(scene.Page.Locator("#left-nav button.interaction-center-management").First, pauseAfter: 800);
        }
    }

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = StatePath,
            ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height },
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(30_000);

        foreach (var skill in new[] { "Spanish", "Billing" })
        {
            await page.GotoAsync(BaseUrl + "/Admin/contact-center/skills/create");
            await page.Locator("#ContactCenterSkill_Name").FillAsync(skill);
            await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
            Console.WriteLine("Created the skill " + skill);
        }

        await context.CloseAsync();
    }

    private static async Task QueueAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/queues");
        await scene.StartCaptureAsync();

        await scene.StepAsync("que-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create"));
            await ScrollToAsync(scene, Card(scene, "Routing"), margin: 120);
            await scene.HighlightAsync(Section(Q(scene, "RoutingStrategy")), "Routing strategy");
            await scene.PauseAsync(5000);
            await scene.SelectAsync(Q(scene, "RoutingStrategy"), "Round robin", pauseAfter: 1500);
            await scene.SelectAsync(Q(scene, "RoutingStrategy"), "Least busy", pauseAfter: 1500);
            await scene.SelectAsync(Q(scene, "RoutingStrategy"), "Longest idle", pauseAfter: 600);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("que-2", async () =>
        {
            await scene.HighlightAsync(Section(Q(scene, "PreferStickyAgent")), "Prefer the last assigned agent (sticky)");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("que-3", async () =>
        {
            await scene.HighlightAsync(Section(Q(scene, "ReservationTimeoutSeconds")), "Reservation timeout");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Q(scene, "UnansweredOfferAction")), "Unanswered offer action");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SkillsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/queues/create");
        await ScrollToAsync(scene, Card(scene, "Skills"), margin: 120);
        await scene.StartCaptureAsync();

        await scene.StepAsync("sk-1", async () =>
        {
            await scene.HighlightAsync(Card(scene, "Skills").Locator(".card-header"), "Skills");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            var add = scene.Page.Locator("button, a").Filter(new() { HasTextString = "Add skill requirement" }).First;
            await scene.ClickAsync(add, pauseAfter: 1200);
            await scene.HighlightAsync(Card(scene, "Skills"), "Skill, Minimum proficiency, Required, Relax after");
            await scene.PauseAsync(3000);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("sk-2", async () =>
        {
            await scene.NavigateAsync("/Admin/contact-center/agent-entitlements/edit/4m8vxr1hyxc25tq4jrcm1d5pms");
            await scene.PauseAsync(600);
            await scene.HighlightAsync(Label(scene, "Skills"), "Skills: proficiency 1 to 5");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("sk-3", async () =>
        {
            await scene.HighlightAsync(Label(scene, "Queue preferences"), "Queue preferences: Priority and Delay");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PriorityAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/entry-points/create?channel=Phone");
        await ScrollToAsync(scene, Card(scene, "Routing"), margin: 120);
        await scene.StartCaptureAsync();

        await scene.StepAsync("pri-1", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#ContactCenterEntryPoint_Priority")), "Entry point: Priority");
            await scene.PauseAsync(5000);
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/contact-center/queues/create");
            await ScrollToAsync(scene, Card(scene, "Routing"), margin: 120);
            await scene.HighlightAsync(Section(Q(scene, "DefaultPriority")), "Queue: Default priority");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task AgingAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/queues/create");
        await ScrollToAsync(scene, Card(scene, "Service levels"), margin: 120);
        await scene.StartCaptureAsync();

        await scene.StepAsync("pri-3", async () =>
        {
            await scene.HighlightAsync(Section(Q(scene, "SlaThresholdSeconds")), "SLA threshold (seconds)");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Q(scene, "EnableSlaAging"), pauseAfter: 600);
            await scene.HighlightAsync(Section(Q(scene, "EnableSlaAging")), "Age waiting items past the SLA threshold");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task OverflowAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/queues/create");
        await ScrollToAsync(scene, Card(scene, "Hours and overflow"), margin: 120);
        await scene.StartCaptureAsync();

        await scene.StepAsync("ov-1", async () =>
        {
            await scene.HighlightAsync(Section(Label(scene, "Overflow queue")), "Overflow queue and Overflow after");
            await scene.PauseAsync(4500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 350);
            await scene.HighlightAsync(Label(scene, "Overflow chain"), "Overflow chain");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ov-2", async () =>
        {
            await ScrollToAsync(scene, Card(scene, "Limits"), margin: 120);
            await scene.HighlightAsync(Section(Label(scene, "Maximum wait (seconds)")), "Maximum wait");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Label(scene, "Maximum-wait action")), "Maximum-wait action");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ReportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        scene.Page.SetDefaultTimeout(120000);
        await scene.OpenAsync("/Admin/Reports/view/contact-center-agent-availability");
        await scene.StartCaptureAsync();

        await scene.StepAsync("rep-1", async () =>
        {
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.Locator("main h1, h1").First, "Reports > Queue & Routing > Agent availability and wait time");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 400);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rep-2", async () =>
        {
            await scene.NavigateAsync("/Admin/Reports/view/contact-center-dialer-decision-log");
            await scene.PauseAsync(1500);
            await scene.ScrollAsync(0, 500);
        });

        await scene.ClearHighlightsAsync();
    }
}
