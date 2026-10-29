using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Outbound Calling Campaign video, one method per "clip" of storyboard.json.
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
        ("02-before", BeforeAsync),
        ("04-subject", SubjectAsync),
        ("05-rules", RulesAsync),
        ("06-load", LoadAsync),
        ("08-watch", WatchAsync),
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
    /// <summary>
    /// Visits the pages the subject clip uses with a throwaway type, then deletes it, so the recorded take is not slowed down by a cold site.
    /// </summary>
    private static async Task WarmUpAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = StatePath,
            ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height },
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        await page.GotoAsync(BaseUrl + "/Admin/omnichannel/subject-flows");
        await page.GotoAsync(BaseUrl + "/Admin/ContentTypes/Create");
        await page.Locator("#DisplayName").FillAsync("Zzwarm");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Create" }).First.ClickAsync());
        var row = page.Locator("li, .list-group-item, tr").Filter(new() { HasTextString = "Omnichannel Subject" }).Last;
        await page.RunAndWaitForNavigationAsync(() => row.Locator("button, a").Filter(new() { HasTextString = "Add" }).First.ClickAsync());
        await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
        await page.GotoAsync(BaseUrl + "/Admin/omnichannel/subject-flows");
        await page.GotoAsync(BaseUrl + "/Admin/omnichannel/subject-flows/Zzwarm/actions");
        await page.GotoAsync(BaseUrl + "/Admin/omnichannel/activity/batches");
        await page.GotoAsync(BaseUrl + "/Admin/omnichannel/activity/batches/create/Dialer");
        await page.GotoAsync(BaseUrl + "/Admin/ContentTypes/List");
        var rowZ = page.Locator("li, .list-group-item, tr").Filter(new() { HasTextString = "Zzwarm" }).Last;
        await rowZ.Locator("a, button").Filter(new() { HasTextString = "Delete" }).First.ClickAsync();
        await page.WaitForTimeoutAsync(800);
        await page.RunAndWaitForNavigationAsync(() => page.Locator(".modal.show .btn-danger, .modal.show .btn-primary").Last.ClickAsync());
        Console.WriteLine("Warmed up, and removed the throwaway type");
        await context.CloseAsync();
    }

    /// <summary>
    /// Creates the Contact type and three fake contacts (not recorded).
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        if (Environment.GetEnvironmentVariable("VIDEO_WARM") == "1")
        {
            await WarmUpAsync(browser);
            return;
        }

        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = StatePath,
            ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height },
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(30_000);

        async Task AddPartAsync(string part)
        {
            await page.GotoAsync(BaseUrl + "/Admin/ContentTypes/AddPartsTo/Contact");
            var row = page.Locator("li, .list-group-item, tr").Filter(new() { HasTextString = part }).Last;
            await page.RunAndWaitForNavigationAsync(() => row.Locator("button, a").Filter(new() { HasTextString = "Add" }).First.ClickAsync());

            if (page.Url.Contains("/Edit"))
            {
                await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
            }
        }

        await page.GotoAsync(BaseUrl + "/Admin/ContentTypes/Create");
        await page.Locator("#DisplayName").FillAsync("Contact");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Create" }).First.ClickAsync());
        await AddPartAsync("Omnichannel Contact");
        await AddPartAsync("Title");
        Console.WriteLine("Created the Contact type");

        foreach (var (name, number) in new[] { ("Morgan Lee", "+17025550123"), ("Casey Brooks", "+17025550156"), ("Taylor Reed", "+17025550167") })
        {
            await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentTypes/Contact/Create");
            await page.Locator("#TitlePart_Title").FillAsync(name);
            await page.Locator("button", new() { HasTextString = "Add Item" }).First.ClickAsync();
            await page.WaitForTimeoutAsync(500);
            await page.Locator(".dropdown-menu.show .dropdown-item, .dropdown-menu.show a").Filter(new() { HasTextString = "Phone" }).First.ClickAsync();
            await page.WaitForTimeoutAsync(900);
            await page.Locator("input[type=tel]").First.FillAsync(number);
            await page.Locator("select[id$='PhoneNumberInfoPart_Type_Text']").First.SelectOptionAsync(new SelectOptionValue { Index = 0 });
            await page.RunAndWaitForNavigationAsync(() => page.Locator("button[type=submit]").Filter(new() { HasTextString = "Publish" }).First.ClickAsync());
            Console.WriteLine($"Created {name}: {page.Url}");
        }

        await context.CloseAsync();
    }

    private static async Task OpenMenuAsync(Scene scene, string href)
    {
        var item = Menu(scene, href);

        var management = scene.Page.Locator("#left-nav button.interaction-center-management").First;

        if (!await item.IsVisibleAsync() && !await management.IsVisibleAsync())
        {
            await scene.ClickAsync(scene.Page.Locator("#left-nav button.interaction-center").First, pauseAfter: 800);
        }

        if (!await item.IsVisibleAsync())
        {
            await scene.ClickAsync(management, pauseAfter: 800);
        }
    }

    private static async Task PickOptionAsync(Scene scene, ILocator select, string containing)
    {
        var value = await select.EvaluateAsync<string>("(el, t) => [...el.options].find(o => o.textContent.includes(t))?.value", containing);
        await scene.SelectAsync(select, value, pauseAfter: 800);
    }

    private static ILocator ExactSave(Scene scene) => scene.Page.Locator("button[type=submit]", new() { HasTextRegex = new System.Text.RegularExpressions.Regex("^\\s*Save\\s*$") }).First;

    private static async Task BeforeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();

        await scene.StepAsync("before-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "Contact Center", delay: 60);
            await scene.PauseAsync(1000);
            await scene.ScrollAsync(0, 380);
            await scene.HighlightAsync(Feature(scene, "CrestApps.OrchardCore.ContactCenter.Dialer"), "Contact Center Outbound Dialer");
            await scene.PauseAsync(5000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Feature(scene, "CrestApps.OrchardCore.ContactCenter.Dialer.Paced"), "Contact Center Paced Dialing");
            await scene.PauseAsync(5000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Feature(scene, "CrestApps.OrchardCore.ContactCenter.AgentEntitlements"), "Contact Center Agent Entitlements");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SubjectAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/omnichannel/subject-flows");
        await scene.StartCaptureAsync();

        await scene.StepAsync("subj-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByText("Create a new content type"));
            await scene.TypeAsync(scene.Page.Locator("#DisplayName"), "Spring lead drive", delay: 30);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Create" }).First);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("li, .list-group-item, tr").Filter(new() { HasTextString = "Omnichannel Subject" }).Last.Locator("button, a").Filter(new() { HasTextString = "Add & Configure" }).First);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("subj-2", async () =>
        {
            await PickOptionAsync(scene, scene.Page.Locator("select[id$='DefaultCampaignId']"), "Spring lead drive");
            await scene.HighlightAsync(Section(scene.Page.Locator("select[id$='DefaultCampaignId']")), "Default campaign");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(scene.Page.Locator("select[id$='_Direction']")), "Direction");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(scene.Page.Locator("input[id$='RequireDisposition']")), "Require a disposition");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("subj-3", async () =>
        {
            await scene.NavigateAsync("/Admin/omnichannel/subject-flows");
            await scene.PauseAsync(1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a", new() { HasTextString = "Manage Flow" }).First);
            await scene.PauseAsync(1000);
            await scene.ClickAsync(scene.Page.Locator("a, button", new() { HasTextString = "Add Action" }).First, pauseAfter: 1500);
            await scene.PauseAsync(4000);
        });
    }

    private static async Task RulesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/omnichannel/dispositions");
        await scene.StartCaptureAsync();

        await scene.StepAsync("rules-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Call Back Later", new() { Exact = true }).First, "Call Back Later");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.GetByText("Number not in service", new() { Exact = true }).First, "Outcome: Number not in service");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rules-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/omnichannel/campaigns"));
            await scene.PauseAsync(400);
            await scene.HighlightAsync(scene.Page.GetByText("Spring lead drive", new() { Exact = true }).First, "Spring lead drive");
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/contact-center/business-hours"));
            await scene.PauseAsync(400);
            await scene.HighlightAsync(scene.Page.GetByText("Weekday calling hours").First, "Weekday calling hours");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rules-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/contact-center/dialers"));
            await scene.PauseAsync(500);
            await scene.HighlightAsync(scene.Page.GetByText("Lead drive - power").First, "Dialer profile");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rules-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/contact-center/agent-entitlements"));
            await scene.PauseAsync(500);
            await scene.HighlightAsync(scene.Page.GetByText("jamie.chen").First, "Allowed campaigns");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task LoadAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/omnichannel/activity/batches");
        await scene.StartCaptureAsync();

        await scene.StepAsync("load-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("a.create, button.create").First, pauseAfter: 1500);
            await scene.PauseAsync(3500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".modal.show a[href$='create/Dialer']"));
        });

        await scene.StepAsync("load-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#OmnichannelActivityBatch_DisplayText"), "Spring lead drive - first list", delay: 35);
            await PickOptionAsync(scene, scene.Page.Locator("#OmnichannelActivityBatch_SubjectContentType"), "Spring lead drive");
            await PickOptionAsync(scene, scene.Page.Locator("#OmnichannelActivityBatch_CampaignId"), "Spring lead drive");
            await PickOptionAsync(scene, scene.Page.Locator("#OmnichannelActivityBatch_DialerProfileId"), "Lead drive - power");
            await scene.ScrollAsync(0, 300);
            await PickOptionAsync(scene, scene.Page.Locator("#OmnichannelActivityBatch_ChannelEndpointId"), "Campaign line");
            await scene.HighlightAsync(Section(scene.Page.Locator("#OmnichannelActivityBatch_ChannelEndpointId")), "Dial from");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("load-3", async () =>
        {
            await ScrollToAsync(scene, Section(scene.Page.Locator("#OmnichannelActivityBatch_ContactContentType")), margin: 300);
            await PickOptionAsync(scene, scene.Page.Locator("#OmnichannelActivityBatch_ContactContentType"), "Contact");
            await scene.HighlightAsync(Section(scene.Page.Locator("#OmnichannelActivityBatch_ContactContentType")), "Record type");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(scene.Page.Locator("#OmnichannelActivityBatch_PreventDuplicates")), "Prevent duplicate activity with the same subject");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(ExactSave(scene));
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("load-4", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.dropdown-toggle", new() { HasTextString = "Actions" }).Last, pauseAfter: 700);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = "Load activities" }).First, pauseAfter: 1200);
            await scene.ClickIfVisibleAsync(scene.Page.Locator(".modal.show .btn-primary, .modal.show .btn-danger").Last, pauseAfter: 1000);
            await scene.PauseAsync(2000);
            await scene.NavigateAsync("/Admin/omnichannel/activity/batches");
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.GetByText("Loaded 2 of 3").First, "What it loaded and skipped");
            await scene.PauseAsync(2500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task WatchAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/dashboard");
        await scene.StartCaptureAsync();

        await scene.StepAsync("watch-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Nothing to call").First, "Live dashboard");
            await scene.PauseAsync(6000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.GetByText("Why isn't this dialing?").First, "Why isn't this dialing?");
            await scene.PauseAsync(4000);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("watch-2", async () =>
        {
            await OpenMenuAsync(scene, "/Admin/omnichannel/manage-activities");
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/omnichannel/manage-activities"));
            await scene.PauseAsync(3500);
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/omnichannel/numbers-not-in-service"));
            await scene.PauseAsync(1500);
        });
    }
}
