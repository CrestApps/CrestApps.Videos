using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Inbound Call Center video, one method per "clip" of storyboard.json.
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
        ("04-numbers", NumbersAsync),
        ("05-queue", QueueAsync),
        ("06-route", RouteAsync),
        ("07-agents", AgentsAsync),
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
    private static async Task PickOptionAsync(Scene scene, ILocator select, string containing)
    {
        var value = await select.EvaluateAsync<string>("(el, t) => [...el.options].find(o => o.textContent.includes(t))?.value", containing);
        await scene.SelectAsync(select, value, pauseAfter: 800);
    }

    private static ILocator Q(Scene scene, string name) => scene.Page.Locator($"#ActivityQueue_{name}");

    private static ILocator E(Scene scene, string name) => scene.Page.Locator($"#ContactCenterEntryPoint_{name}");

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
            await scene.HighlightAsync(Feature(scene, "CrestApps.OrchardCore.ContactCenter.Queues"), "Contact Center Work Distribution");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Feature(scene, "CrestApps.OrchardCore.ContactCenter.EntryPoints"), "Contact Center Inbound Entry Points");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Feature(scene, "CrestApps.OrchardCore.ContactCenter.InboundVoice"), "Contact Center Inbound Voice");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Feature(scene, "CrestApps.OrchardCore.ContactCenter.Supervision"), "Supervision & Live Dashboard");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task NumbersAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/omnichannel/channel-endpoints");
        await scene.StartCaptureAsync();

        await scene.StepAsync("num-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/omnichannel/channel-endpoints"), "Interaction Center > Management > Omnichannel Addresses");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.GetByText("Main line").First, "Voice calls");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("num-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/contact-center/business-hours"));
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.GetByText("Weekday calling hours").First, "Business hours calendar");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task QueueAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/queues");
        await scene.StartCaptureAsync();

        await scene.StepAsync("queue-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create"));
            await scene.TypeAsync(Q(scene, "Name"), "Sales", delay: 60);
            await scene.Page.Locator("#ActivityQueue_Description").FillAsync("Calls from customers who want to buy");
            await ScrollToAsync(scene, Section(Q(scene, "RoutingStrategy")), margin: 250);
            await scene.HighlightAsync(Section(Q(scene, "RoutingStrategy")), "Routing strategy");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("queue-2", async () =>
        {
            await ScrollToAsync(scene, Section(Q(scene, "SlaThresholdSeconds")), margin: 250);
            await scene.HighlightAsync(Section(Q(scene, "SlaThresholdSeconds")), "Service level threshold");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, Section(Q(scene, "BusinessHoursCalendarId")), margin: 250);
            await PickAsync(scene, "ActivityQueue_BusinessHoursCalendarId", "Weekday calling hours");
            await scene.HighlightAsync(Section(Q(scene, "BusinessHoursCalendarId")), "Business hours calendar");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Q(scene, "AfterHoursAction")), "After-hours action");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("queue-3", async () =>
        {
            await ScrollToAsync(scene, Section(Q(scene, "OverflowQueueId")), margin: 250);
            await scene.HighlightAsync(Section(Q(scene, "OverflowQueueId")), "Overflow queue");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, Section(Q(scene, "Treatment_HoldMusicMediaId")), margin: 250);
            await scene.HighlightAsync(Section(Q(scene, "Treatment_HoldMusicMediaId")), "Hold music");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, Section(Q(scene, "Treatment_CallbackDtmfKey")), margin: 250);
            await scene.HighlightAsync(Section(Q(scene, "Treatment_CallbackDtmfKey")), "Callback key");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1000);
        });
    }

    private static async Task RouteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/entry-points");
        await scene.StartCaptureAsync();

        await scene.StepAsync("route-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("a.create, button.create").First, pauseAfter: 1500);
            await scene.PauseAsync(1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".modal.show a[href*='channel=Phone']"));
            await scene.TypeAsync(E(scene, "Name"), "Main line to Sales", delay: 45);
            await scene.ClickAsync(scene.Page.Locator("button[data-id='ContactCenterEntryPoint_AddressIds']"), pauseAfter: 900);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = "Main line" }).First, pauseAfter: 600);
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.HighlightAsync(Section(E(scene, "AddressIds")), "Numbers");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("route-2", async () =>
        {
            await ScrollToAsync(scene, Section(E(scene, "TargetType")), margin: 200);
            await PickAsync(scene, "ContactCenterEntryPoint_TargetQueueId", "Sales");
            await scene.HighlightAsync(Section(E(scene, "TargetQueueId")), "Target queue");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await PickAsync(scene, "ContactCenterEntryPoint_BusinessHoursCalendarId", "Weekday calling hours");
            await scene.SelectAsync(E(scene, "ClosedAction"), "Voicemail", pauseAfter: 600);
            await scene.HighlightAsync(Section(E(scene, "ClosedAction")), "Closed action");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("route-3", async () =>
        {
            await ScrollToAsync(scene, Section(E(scene, "ClosedMessage")), margin: 200);
            await scene.Page.Locator("#ContactCenterEntryPoint_WelcomeMessage").FillAsync("Thank you for calling Contoso. Please hold, and the next available person will answer.");
            await scene.Page.Locator("#ContactCenterEntryPoint_ClosedMessage").FillAsync("Contoso is closed. Our hours are Monday to Friday, 9 AM to 5 PM.");
            await scene.HighlightAsync(Section(E(scene, "WelcomeMessage")), "Welcome message");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(E(scene, "ClosedMessage")), "Closed message");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, Section(E(scene, "VoicemailDestination")), margin: 200);
            await scene.SelectAsync(E(scene, "VoicemailDestination"), "The queue's shared voicemail box", pauseAfter: 600);
            await scene.HighlightAsync(Section(E(scene, "VoicemailDestination")), "Deliver voicemail to");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(800);
        });
    }

    private static async Task AgentsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/agent-entitlements");
        await scene.StartCaptureAsync();

        await scene.StepAsync("agent-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn", new() { HasTextString = "Edit" }).First);
            await scene.ClickAsync(scene.Page.Locator("button[data-id='AllowedQueueIds']"), pauseAfter: 900);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = "Sales" }).First, pauseAfter: 700);
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.PauseAsync(800);
            await scene.HighlightAsync(scene.Page.GetByText("Queue preferences").First, "Priority and delay");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("agent-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("jamie.chen").First, "One record for each agent and supervisor");
            await scene.PauseAsync(3000);
        });

        await scene.ClearHighlightsAsync();
    }
}
