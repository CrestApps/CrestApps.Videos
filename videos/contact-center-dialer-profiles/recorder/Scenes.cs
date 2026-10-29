using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Dialer Profiles video, one method per "clip" of storyboard.json.
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
        ("03-find", FindAsync),
        ("04-create", CreateAsync),
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
    /// Creates the business hours calendar and the two voice numbers the profile picks from (not recorded).
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = StatePath,
            ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height },
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        async Task SaveAsync() => await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());

        await page.GotoAsync(BaseUrl + "/Admin/contact-center/business-hours/Create");
        await page.Locator("#BusinessHoursCalendar_Name").FillAsync("Weekday calling hours");
        await page.Locator("#BusinessHoursCalendar_Description").FillAsync("Monday to Friday, 9 AM to 5 PM");
        await SaveAsync();
        Console.WriteLine("Created the calendar");

        foreach (var (name, number) in new[] { ("Main line", "+17025550142"), ("Campaign line", "+17025550177") })
        {
            await page.GotoAsync(BaseUrl + "/Admin/omnichannel/channel-endpoints/create/PhoneNumber");
            await page.Locator("#OmnichannelChannelEndpoint_DisplayText").FillAsync(name);
            await page.Locator("#OmnichannelChannelEndpoint_Value").FillAsync(number);
            await page.Locator("#OmnichannelChannelEndpoint_Capabilities_0").CheckAsync();
            await SaveAsync();
            Console.WriteLine($"Created {name}");
        }

        await context.CloseAsync();
    }

    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator Field(Scene scene, string name) => scene.Page.Locator($"#DialerProfile_{name}");

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

    private static async Task PickAsync(Scene scene, string selectId, string text)
    {
        await scene.ClickAsync(scene.Page.Locator($"button[data-id='{selectId}']"), pauseAfter: 900);
        await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = text }).First, pauseAfter: 600);
    }

    // ----- Clips -----

    private static async Task FindAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();

        await scene.StepAsync("find-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "Dialer", delay: 70);
            await scene.PauseAsync(1000);
            await scene.ScrollAsync(0, 560);
            await scene.HighlightAsync(Feature(scene, "CrestApps.OrchardCore.ContactCenter.Dialer"), "Contact Center Outbound Dialer");
            await scene.PauseAsync(4500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Feature(scene, "CrestApps.OrchardCore.ContactCenter.Dialer.Paced"), "Contact Center Paced Dialing");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("find-2", async () =>
        {
            await scene.NavigateAsync("/Admin/contact-center/dialers");
            await scene.PauseAsync(500);
            await OpenMenuAsync(scene, "/Admin/contact-center/dialers");
            await scene.HighlightAsync(Menu(scene, "/Admin/contact-center/dialers"), "Interaction Center > Management > Dialer Profiles");
            await scene.PauseAsync(3500);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("find-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create"));
            await scene.PauseAsync(600);
            await scene.HighlightAsync(Card(scene, "General").Locator(".card-header"), "General");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Card(scene, "Dialing").Locator(".card-header"), "Dialing");
            await scene.PauseAsync(1500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/dialers");
        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create"));
        await scene.StartCaptureAsync();

        await scene.StepAsync("create-1", async () =>
        {
            await scene.TypeAsync(Field(scene, "Name"), "Lead drive - power", delay: 45);
            await scene.TypeAsync(Field(scene, "Description"), "Calls placed by the system for available agents", delay: 22);
            await scene.HighlightAsync(Section(Field(scene, "Enabled")), "Enabled");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("create-2", async () =>
        {
            await ScrollToAsync(scene, Card(scene, "Dialing"));
            await scene.HighlightAsync(Section(Field(scene, "Mode")), "Mode");
            await scene.PauseAsync(4500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Field(scene, "ProviderName")), "Voice call provider");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("create-3", async () =>
        {
            await PickAsync(scene, "DialerProfile_Mode", "Power");
            await ScrollToAsync(scene, Section(Field(scene, "CallsPerAgent")), margin: 250);
            await scene.HighlightAsync(Section(Field(scene, "CallsPerAgent")), "Calls per agent");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("create-4", async () =>
        {
            await scene.HighlightAsync(Section(Field(scene, "MaxAttempts")), "Max attempts");
            await scene.PauseAsync(5500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Field(scene, "RetryDelayMinutes")), "Retry delay (minutes)");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("create-5", async () =>
        {
            await scene.ScrollAsync(0, 250);
            await scene.HighlightAsync(Section(Field(scene, "RingTimeoutSeconds")), "Ring time (seconds)");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Field(scene, "AnsweringMachineDetection")), "Screen out answering machines");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("create-6", async () =>
        {
            await scene.ScrollAsync(0, 200);
            await scene.HighlightAsync(Section(Field(scene, "ShowDialProgressToAgent")), "Show each dial to the agent");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("cid-1", async () =>
        {
            await ScrollToAsync(scene, Card(scene, "Caller ID"));
            await PickAsync(scene, "DialerProfile_CallerId", "Main line");
            await scene.HighlightAsync(Section(Field(scene, "CallerId")), "Caller ID");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("cid-2", async () =>
        {
            await scene.HighlightAsync(Section(Field(scene, "AlwaysUseCallerId")), "Always show this caller ID");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("cid-3", async () =>
        {
            await scene.HighlightAsync(Section(Field(scene, "DefaultRegionCode")), "Default calling region");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("comp-1", async () =>
        {
            await ScrollToAsync(scene, Card(scene, "Compliance"));
            await scene.HighlightAsync(Section(Field(scene, "RespectDoNotCall")), "Respect do-not-call");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("comp-2", async () =>
        {
            await scene.ClickAsync(Field(scene, "EnforceCallingWindow"), pauseAfter: 700);
            await PickAsync(scene, "DialerProfile_CallingCalendarId", "Weekday calling hours");
            await scene.HighlightAsync(Section(Field(scene, "CallingCalendarId")), "Outbound calling calendar");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("comp-3", async () =>
        {
            await ScrollToAsync(scene, Card(scene, "Abandoned calls"), margin: 120);
            await scene.HighlightAsync(Card(scene, "Abandoned calls").Locator(".card-header"), "Abandoned calls");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("comp-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1200);
        });
    }
}
