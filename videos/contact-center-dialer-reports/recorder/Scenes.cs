using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Dialer reports video, one method per "clip" of storyboard.json.
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
        ("02-health", HealthAsync),
        ("03-log", LogAsync),
        ("04-inventory", InventoryAsync),
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
    private static ILocator Heading(Scene scene, string text)
        => scene.Page.Locator("h1, h2, h3, h4, h5, h6, .card-header").Filter(new() { HasTextString = text }).First;

    private static async Task HealthAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        scene.Page.SetDefaultTimeout(120_000);
        await scene.OpenAsync("/Admin/Reports/view/contact-center-dialer-activity");
        await scene.StartCaptureAsync();

        await scene.StepAsync("hl-1", async () =>
        {
            await scene.HighlightAsync(Heading(scene, "Dialer right now"), "Dialer right now");
            await scene.PauseAsync(5000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".form-select, select").First, "Date range");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("hl-2", async () =>
        {
            await ScrollToAsync(scene, Heading(scene, "Campaign queues"), margin: 100);
            await scene.HighlightAsync(Heading(scene, "Campaign queues"), "Campaign queues");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("th").Filter(new() { HasTextString = "Why" }).First, "Why");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("hl-3", async () =>
        {
            await ScrollToAsync(scene, Heading(scene, "Dialer profiles"), margin: 100);
            await scene.HighlightAsync(Heading(scene, "Dialer profiles"), "Dialer profiles");
            await scene.PauseAsync(4500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 700);
            await scene.HighlightAsync(Heading(scene, "Suppressed attempts by reason"), "Suppressed attempts by reason");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task LogAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        scene.Page.SetDefaultTimeout(120_000);
        await scene.OpenAsync("/Admin/Reports/view/contact-center-dialer-decision-log");
        await scene.StartCaptureAsync();

        await scene.StepAsync("log-1", async () =>
        {
            await scene.HighlightAsync(Heading(scene, "Summary"), "Summary");
            await scene.PauseAsync(4500);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, Heading(scene, "Decisions by reason"), margin: 100);
            await scene.HighlightAsync(Heading(scene, "Decisions by reason"), "Decisions by reason");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("log-2", async () =>
        {
            await ScrollToAsync(scene, Heading(scene, "Queues with work due now"), margin: 100);
            await scene.HighlightAsync(Heading(scene, "Queues with work due now"), "Queues with work due now");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("log-3", async () =>
        {
            await ScrollToAsync(scene, scene.Page.Locator("h2, h3, h4, h5").Filter(new() { HasTextString = "Decisions" }).Last, margin: 100);
            await scene.HighlightAsync(scene.Page.Locator("th").Filter(new() { HasTextString = "Candidates" }).First, "Candidates");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("th").Filter(new() { HasTextString = "Reason" }).Last, "Reason");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("log-4", async () =>
        {
            await scene.ScrollAsync(0, -4000);
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.Locator("label, .form-label").Filter(new() { HasTextString = "Activity ID" }).First, "Activity ID");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("label, .form-label").Filter(new() { HasTextString = "Agent" }).First, "Agent");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task InventoryAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        scene.Page.SetDefaultTimeout(120_000);
        await scene.OpenAsync("/Admin/Reports/view/contact-center-dialer-queue-inventory");
        await scene.StartCaptureAsync();

        await scene.StepAsync("inv-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("h1").First, "Dialer queue inventory");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 350);
        });

        await scene.ClearHighlightsAsync();
    }

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = StatePath,
            ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height },
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        async Task SetSelectAsync(string id, string text)
        {
            await page.Locator("#" + id).EvaluateAsync("(el, text) => { const o = [...el.options].find(x => x.text.includes(text)); el.value = o.value; el.dispatchEvent(new Event('change', { bubbles: true })); }", text);
            await page.WaitForTimeoutAsync(400);
        }

        // A dialer load with nobody signed in: the pacing task then records real cycles and routing decisions.
        await page.GotoAsync(BaseUrl + "/Admin/omnichannel/activity/batches/create/Dialer");
        await page.Locator("#OmnichannelActivityBatch_DisplayText").FillAsync("Spring leads - power dial");
        await SetSelectAsync("OmnichannelActivityBatch_SubjectContentType", "Spring lead drive");
        await SetSelectAsync("OmnichannelActivityBatch_CampaignId", "Spring lead drive");
        await SetSelectAsync("OmnichannelActivityBatch_DialerProfileId", "Lead drive - power");
        await page.WaitForTimeoutAsync(800);
        await SetSelectAsync("OmnichannelActivityBatch_ChannelEndpointId", "Campaign line");
        await SetSelectAsync("OmnichannelActivityBatch_ContactContentType", "Contact");

        if (await page.Locator("#OmnichannelActivityBatch_PreventDuplicates").IsCheckedAsync())
        {
            await page.Locator("#OmnichannelActivityBatch_PreventDuplicates").UncheckAsync();
        }

        await page.Locator("button.SaveAndLoad, button[name='submit.SaveAndLoad']").First.ClickAsync();
        await page.WaitForTimeoutAsync(1000);
        await page.RunAndWaitForNavigationAsync(() => page.Locator(".modal.show .btn-danger").First.ClickAsync());
        Console.WriteLine("Loaded the dialer batch: " + page.Url);
        await context.CloseAsync();
    }
}
