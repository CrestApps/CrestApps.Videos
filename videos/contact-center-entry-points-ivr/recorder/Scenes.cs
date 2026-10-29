using Microsoft.Playwright;

namespace Recorder;

public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-numbers", NumbersAsync),
        ("04-create", CreateAsync),
        ("05-hours", HoursAsync),
        ("06-voicemail", VoicemailAsync),
        ("07-agent", AgentAsync),
        ("08-text", TextAsync),
        ("09-clone", CloneAsync),
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

    /// <summary>
    /// Seeds the numbers, hours, queues, voice media and agents the clips need (not recorded). Fake data only.
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await Seed.OpenAsync(browser);
        var wav = Environment.GetEnvironmentVariable("VIDEO_WAV") ?? "C:/cavideo/comms-b/welcome.wav";

        await Seed.AddressAsync(page, "Main line", "+12125550100", voice: true, sms: false, description: "The main number on the website");
        await Seed.AddressAsync(page, "Support line", "+12125550101", voice: true, sms: false, description: "Customers who need help");
        await Seed.AddressAsync(page, "Sales line", "+12125550102", voice: true, sms: false, description: "New customers");
        await Seed.AddressAsync(page, "Text line", "+12125550103", voice: false, sms: true, description: "Texts from customers");
        await Seed.BusinessHoursAsync(page, "Weekdays 9 to 5", "Open Monday to Friday", "America/New_York");
        await Seed.QueueAsync(page, "Support", "Customers who need help with an order");
        await Seed.QueueAsync(page, "Sales", "New customers and quotes");
        await Seed.QueueAsync(page, "Billing", "Invoices and payments");
        await Seed.VoiceMediaAsync(page, "Welcome greeting", "Plays before the first menu", wav);
        await Seed.EntitlementAsync(page, "jamie.chen", ["Support", "Sales", "Billing"]);
        await Seed.EntitlementAsync(page, "maria.lopez", ["Support", "Billing"]);
        await Seed.EntitlementAsync(page, "sam.patel", ["Sales"]);
        await context.CloseAsync();
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static ILocator Field(Scene scene, string id) => scene.Page.Locator("#" + id);

    /// <summary>Opens a bootstrap-select list by its select id, and clicks one option.</summary>
    private static async Task PickAsync(Scene scene, string selectId, string text, bool close = false)
    {
        await scene.ClickAsync(scene.Page.Locator($"button[data-id='{selectId}']"), pauseAfter: 500);
        await scene.ClickAsync(scene.Page.Locator($"div.bootstrap-select:has(button[data-id='{selectId}']) .dropdown-item").Filter(new() { HasTextString = text }).First, pauseAfter: 500);

        if (close)
        {
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.PauseAsync(300);
        }
    }

    private static async Task OpenEntryPointAsync(Scene scene, string name)
    {
        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("ul.admin-list a").Filter(new() { HasTextString = name }).First);
    }

    // ----- Clips -----

    private static async Task NumbersAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/omnichannel/channel-endpoints");
        await scene.StartCaptureAsync();

        await scene.StepAsync("num-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/omnichannel/channel-endpoints"), "Management > Omnichannel Addresses");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("ul.admin-list").First, "One address per number");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("num-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button, a").Filter(new() { HasTextString = "Add Address" }).First, pauseAfter: 900);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".modal.show a[href$='create/PhoneNumber']").First);
            await scene.TypeAsync(Field(scene, "OmnichannelChannelEndpoint_DisplayText"), "Billing line", delay: 45);
            await scene.TypeAsync(Field(scene, "OmnichannelChannelEndpoint_Value"), "+12125550104", delay: 45);
            await scene.ClickAsync(Field(scene, "OmnichannelChannelEndpoint_Capabilities_0"), pauseAfter: 500);
            await scene.HighlightAsync(Field(scene, "OmnichannelChannelEndpoint_Capabilities_0").Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]"), "Voice calls or Text messages");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/entry-points");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ep-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Add inbound entry point" }).First, pauseAfter: 2500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".modal.show a[href*='channel=Phone']").First);
        });

        await scene.StepAsync("ep-2", async () =>
        {
            await scene.TypeAsync(Field(scene, "ContactCenterEntryPoint_Name"), "Support line", delay: 55);
            await PickAsync(scene, "ContactCenterEntryPoint_AddressIds", "Support line", close: true);
            await scene.HighlightAsync(Section(Field(scene, "ContactCenterEntryPoint_AddressIds")), "Numbers ticked for calls");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ep-3", async () =>
        {
            await scene.ScrollAsync(0, 620);
            await PickAsync(scene, "ContactCenterEntryPoint_TargetQueueId", "Support");
            await scene.SelectAsync(Field(scene, "ContactCenterEntryPoint_Priority"), "High", pauseAfter: 900);
            await scene.HighlightAsync(Section(Field(scene, "ContactCenterEntryPoint_Priority")), "Priority");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ep-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1200);
        });
    }

    private static async Task HoursAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/entry-points");
        await scene.StartCaptureAsync();
        await OpenEntryPointAsync(scene, "Support line");

        await scene.StepAsync("hrs-1", async () =>
        {
            await scene.ScrollAsync(0, 1020);
            await PickAsync(scene, "ContactCenterEntryPoint_BusinessHoursCalendarId", "Weekdays 9 to 5");
            await scene.HighlightAsync(Section(Field(scene, "ContactCenterEntryPoint_BusinessHoursCalendarId")), "Business hours calendar");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("hrs-2", async () =>
        {
            await scene.HighlightAsync(Section(Field(scene, "ContactCenterEntryPoint_ClosedAction")), "Closed action");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(Field(scene, "ContactCenterEntryPoint_ClosedAction"), "Overflow", pauseAfter: 2200);
            await scene.HighlightAsync(Section(Field(scene, "ContactCenterEntryPoint_OverflowQueueId")), "Overflow queue");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("hrs-3", async () =>
        {
            await scene.SelectAsync(Field(scene, "ContactCenterEntryPoint_ClosedAction"), "Voicemail", pauseAfter: 700);
            await scene.TypeAsync(Field(scene, "ContactCenterEntryPoint_ClosedMessage"), "Thanks for calling Contoso. Our office is closed right now.", delay: 30);
            await scene.HighlightAsync(Section(Field(scene, "ContactCenterEntryPoint_ClosedMessage")), "Closed message");
        });

        await scene.ClearHighlightsAsync();
        await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
    }

    private static async Task VoicemailAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/entry-points");
        await scene.StartCaptureAsync();
        await OpenEntryPointAsync(scene, "Support line");

        await scene.StepAsync("vm-1", async () =>
        {
            await scene.ScrollAsync(0, 2200);
            await scene.TypeAsync(Field(scene, "ContactCenterEntryPoint_VoicemailGreetingText"), "Please leave your name and number after the tone.", delay: 30);
            await scene.HighlightAsync(Section(Field(scene, "ContactCenterEntryPoint_VoicemailGreetingText")), "Default voicemail greeting");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("vm-2", async () =>
        {
            await scene.SelectAsync(Field(scene, "ContactCenterEntryPoint_VoicemailDestination"), "The queue's shared voicemail box", pauseAfter: 1800);
            await scene.SelectAsync(Field(scene, "ContactCenterEntryPoint_VoicemailDestination"), "An agent's inbox", pauseAfter: 500);
            await PickAsync(scene, "ContactCenterEntryPoint_VoicemailRecipientAgentId", "maria.lopez");
            await scene.HighlightAsync(Section(Field(scene, "ContactCenterEntryPoint_VoicemailRecipientAgentId")), "Voicemail inbox");
        });

        await scene.ClearHighlightsAsync();
        await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
    }

    private static async Task AgentAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/entry-points");
        await scene.StartCaptureAsync();

        await scene.StepAsync("agt-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Add inbound entry point" }).First, pauseAfter: 900);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".modal.show a[href*='channel=Phone']").First);
            await scene.TypeAsync(Field(scene, "ContactCenterEntryPoint_Name"), "Sales direct line", delay: 40);
        });

        await scene.StepAsync("agt-2", async () =>
        {
            await PickAsync(scene, "ContactCenterEntryPoint_AddressIds", "Sales line", close: true);
            await scene.ScrollAsync(0, 620);
            await scene.SelectAsync(Field(scene, "ContactCenterEntryPoint_TargetType"), "Specific agent", pauseAfter: 500);
            await PickAsync(scene, "ContactCenterEntryPoint_TargetAgentId", "jamie.chen");
            await scene.HighlightAsync(Section(Field(scene, "ContactCenterEntryPoint_TargetAgentId")), "Target agent");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("agt-3", async () =>
        {
            await scene.HighlightAsync(Section(Field(scene, "ContactCenterEntryPoint_RingTimeoutSeconds")), "Ring window");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Field(scene, "ContactCenterEntryPoint_VoicemailEnabled")), "Send unanswered calls to voicemail");
        });

        await scene.ClearHighlightsAsync();
        await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
    }

    private static async Task TextAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/entry-points");
        await scene.StartCaptureAsync();

        await scene.StepAsync("txt-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Add inbound entry point" }).First, pauseAfter: 1200);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".modal.show a[href*='channel=SMS']").First);
        });

        await scene.StepAsync("txt-1b", async () =>
        {
            await scene.TypeAsync(Field(scene, "ContactCenterEntryPoint_Name"), "Text line", delay: 40);
            await PickAsync(scene, "ContactCenterEntryPoint_AddressIds", "Text line", close: true);
            await scene.HighlightAsync(Section(Field(scene, "ContactCenterEntryPoint_AddressIds")), "Numbers ticked for texts");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("txt-2", async () =>
        {
            await scene.ScrollAsync(0, 620);
            await PickAsync(scene, "ContactCenterEntryPoint_TargetQueueId", "Support");
            await scene.SelectAsync(Field(scene, "ContactCenterEntryPoint_DistributionMode"), "Routed", pauseAfter: 1200);
            await scene.HighlightAsync(Section(Field(scene, "ContactCenterEntryPoint_DistributionMode")), "Queue distribution");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("txt-3", async () =>
        {
            await scene.TypeAsync(Field(scene, "ContactCenterEntryPoint_AutoReplyMessage"), "Thanks for texting Contoso. An agent will reply shortly.", delay: 10);
            await scene.HighlightAsync(Section(Field(scene, "ContactCenterEntryPoint_AutoReplyMessage")), "Auto-reply");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 500);
            await PickAsync(scene, "ContactCenterEntryPoint_BusinessHoursCalendarId", "Weekdays 9 to 5");
            await scene.TypeAsync(Field(scene, "ContactCenterEntryPoint_ClosedAutoReplyMessage"), "We are closed. We will reply when we open.", delay: 10);
            await scene.HighlightAsync(Section(Field(scene, "ContactCenterEntryPoint_ClosedAutoReplyMessage")), "Closed auto-reply");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("txt-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }

    private static async Task CloneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/entry-points");
        await scene.StartCaptureAsync();

        await scene.StepAsync("cln-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("ul.admin-list").First, "Every entry point");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("cln-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("ul.admin-list button.dropdown-toggle").Filter(new() { HasTextString = "Actions" }).First, pauseAfter: 1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".dropdown-menu.show a").Filter(new() { HasTextString = "Clone" }).First);
            await scene.HighlightAsync(Section(Field(scene, "ContactCenterEntryPoint_Name")), "Copy of ...");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Field(scene, "ContactCenterEntryPoint_AddressIds")), "Numbers starts empty");
        });

        await scene.ClearHighlightsAsync();
    }
}
