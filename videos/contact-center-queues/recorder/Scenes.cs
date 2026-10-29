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
        ("03-create", CreateAsync),
        ("04-routing", RoutingAsync),
        ("05-skills", SkillsAsync),
        ("06-overflow", OverflowAsync),
        ("07-waiting", WaitingAsync),
        ("08-clone", CloneAsync),
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
    /// Seeds two skills, a weekday calendar, an overflow queue and a hold music recording (not recorded). Fake data only.
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await Seed.OpenAsync(browser);
        var wav = Environment.GetEnvironmentVariable("VIDEO_WAV") ?? "C:/cavideo/comms-b/welcome.wav";

        await Seed.SkillAsync(page, "Spanish", "Speaks Spanish with customers");
        await Seed.SkillAsync(page, "Billing", "Can resolve invoice and payment questions");
        await Seed.BusinessHoursAsync(page, "Weekdays 9 to 5", "Open Monday to Friday", "America/New_York");
        await Seed.QueueAsync(page, "General help", "Anything that does not fit another queue");
        await Seed.VoiceMediaAsync(page, "Hold music", "Plays while callers wait", wav);
        await context.CloseAsync();
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    // ----- Helpers -----

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static ILocator Field(Scene scene, string id) => scene.Page.Locator("#" + id);

    private static ILocator Part(Scene scene, string suffix) => scene.Page.Locator($"[id$='{suffix}']").First;

    private static async Task ScrollToAsync(Scene scene, ILocator locator, double offset = 160)
    {
        var top = await locator.EvaluateAsync<double>("e => e.getBoundingClientRect().top");
        await scene.ScrollAsync(0, top - offset);
    }

    private static async Task PickAsync(Scene scene, string selectId, string text, bool close = false)
    {
        await scene.ClickAsync(scene.Page.Locator($"button[data-id='{selectId}']"), pauseAfter: 400);
        await scene.ClickAsync(scene.Page.Locator($"div.bootstrap-select:has(button[data-id='{selectId}']) .dropdown-item").Filter(new() { HasTextString = text }).First, pauseAfter: 400);

        if (close)
        {
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.PauseAsync(300);
        }
    }

    /// <summary>Replaces the value of a text or number box, visibly.</summary>
    private static async Task SetAsync(Scene scene, ILocator field, string text, int delay = 40)
    {
        await scene.ClickAsync(field, pauseAfter: 150);
        await field.FillAsync(string.Empty);
        await field.PressSequentiallyAsync(text, new() { Delay = delay });
        await scene.PauseAsync(250);
    }

    private static async Task OpenQueueAsync(Scene scene, string name)
    {
        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("ul.admin-list a").Filter(new() { HasTextString = name }).First);
    }

    // ----- Clips -----

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/queue-groups");
        await scene.StartCaptureAsync();

        await scene.StepAsync("cre-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a").Filter(new() { HasTextString = "Add queue group" }).First);
            await scene.TypeAsync(Field(scene, "ActivityQueueGroup_Name"), "Service", delay: 45);
            await scene.TypeAsync(Field(scene, "ActivityQueueGroup_Description"), "Support and billing queues", delay: 15);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.StepAsync("cre-2", async () =>
        {
            await scene.NavigateAsync("/Admin/contact-center/queues");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a").Filter(new() { HasTextString = "Add Queue" }).First);
            await scene.TypeAsync(Field(scene, "ActivityQueue_Name"), "Support", delay: 45);
            await scene.TypeAsync(Field(scene, "ActivityQueue_Description"), "Customers who need help with an order", delay: 12);
        });

        await scene.StepAsync("cre-3", async () =>
        {
            await PickAsync(scene, "ActivityQueue_QueueGroupId", "Service");
            await scene.HighlightAsync(Section(Field(scene, "ActivityQueue_Enabled")), "Enabled");
        });

        await scene.ClearHighlightsAsync();
        await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
    }

    private static async Task RoutingAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/queues");
        await scene.StartCaptureAsync();
        await OpenQueueAsync(scene, "Support");

        await scene.StepAsync("rou-1", async () =>
        {
            await ScrollToAsync(scene, Field(scene, "ActivityQueue_DefaultPriority"), 260);
            await scene.HighlightAsync(Section(Field(scene, "ActivityQueue_DefaultPriority")), "Default priority");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rou-2", async () =>
        {
            await scene.SelectAsync(Field(scene, "ActivityQueue_RoutingStrategy"), "Round robin", pauseAfter: 2200);
            await scene.SelectAsync(Field(scene, "ActivityQueue_RoutingStrategy"), "Least busy", pauseAfter: 2200);
            await scene.SelectAsync(Field(scene, "ActivityQueue_RoutingStrategy"), "Longest idle", pauseAfter: 500);
            await scene.HighlightAsync(Section(Field(scene, "ActivityQueue_RoutingStrategy")), "Routing strategy");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rou-3", async () =>
        {
            await scene.ClickAsync(Field(scene, "ActivityQueue_PreferStickyAgent"), pauseAfter: 600);
            await scene.HighlightAsync(Section(Field(scene, "ActivityQueue_PreferStickyAgent")), "Sticky agent");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rou-4", async () =>
        {
            await SetAsync(scene, Field(scene, "ActivityQueue_ReservationTimeoutSeconds"), "20");
            await scene.SelectAsync(Field(scene, "ActivityQueue_UnansweredOfferAction"), "Send to voicemail", pauseAfter: 1500);
            await scene.SelectAsync(Field(scene, "ActivityQueue_UnansweredOfferAction"), "Requeue", pauseAfter: 400);
            await scene.HighlightAsync(Section(Field(scene, "ActivityQueue_UnansweredOfferAction")), "Unanswered offer action");
        });

        await scene.ClearHighlightsAsync();
        await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
    }

    private static async Task SkillsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/queues");
        await scene.StartCaptureAsync();
        await OpenQueueAsync(scene, "Support");

        await scene.StepAsync("ski-1", async () =>
        {
            await ScrollToAsync(scene, Field(scene, "ActivityQueue_SlaThresholdSeconds"), 260);
            await scene.HighlightAsync(Section(Field(scene, "ActivityQueue_SlaThresholdSeconds")), "SLA threshold");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ski-2", async () =>
        {
            await scene.ClickAsync(Field(scene, "ActivityQueue_EnableSlaAging"), pauseAfter: 500);
            await scene.HighlightAsync(Section(Field(scene, "ActivityQueue_EnableSlaAging")), "Age waiting items");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Field(scene, "ActivityQueue_FirstResponseTargetSeconds")), "First-response target");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ski-3", async () =>
        {
            await ScrollToAsync(scene, scene.Page.Locator("#heading-card-skills"), 120);
            await PickAsync(scene, "ActivityQueue_RequiredSkills", "Spanish", close: true);
            await scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Add skill requirement" }).First, pauseAfter: 700);
            await scene.SelectAsync(Part(scene, "-skill"), "Billing", pauseAfter: 300);
            await scene.SelectAsync(Part(scene, "-proficiency"), "3", pauseAfter: 300);
            await SetAsync(scene, Part(scene, "-relax"), "120");
            await scene.HighlightAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Add skill requirement" }).First, "Skill requirements");
        });

        await scene.ClearHighlightsAsync();
        await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
    }

    private static async Task OverflowAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/queues");
        await scene.StartCaptureAsync();
        await OpenQueueAsync(scene, "Support");

        await scene.StepAsync("ovf-1", async () =>
        {
            await ScrollToAsync(scene, scene.Page.Locator("#heading-card-hours-and-overflow"), 120);
            await PickAsync(scene, "ActivityQueue_BusinessHoursCalendarId", "Weekdays 9 to 5");
            await scene.SelectAsync(Field(scene, "ActivityQueue_AfterHoursAction"), "Overflow", pauseAfter: 600);
            await scene.HighlightAsync(Section(Field(scene, "ActivityQueue_AfterHoursAction")), "After-hours action");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ovf-2", async () =>
        {
            await PickAsync(scene, "ActivityQueue_OverflowQueueId", "General help");
            await SetAsync(scene, Field(scene, "ActivityQueue_OverflowAfterSeconds"), "120");
            await scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Add overflow hop" }).First, pauseAfter: 800);
            await scene.HighlightAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Add overflow hop" }).First, "Overflow chain");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ovf-3", async () =>
        {
            await ScrollToAsync(scene, scene.Page.Locator("#heading-card-limits"), 120);
            await SetAsync(scene, Field(scene, "ActivityQueue_MaxQueueSize"), "25");
            await scene.SelectAsync(Field(scene, "ActivityQueue_QueueFullAction"), "Send to voicemail", pauseAfter: 400);
            await SetAsync(scene, Field(scene, "ActivityQueue_MaxWaitSeconds"), "600");
            await scene.SelectAsync(Field(scene, "ActivityQueue_MaxWaitAction"), "Overflow to the next queue", pauseAfter: 400);
            await scene.HighlightAsync(Section(Field(scene, "ActivityQueue_MaxWaitAction")), "Maximum-wait action");
        });

        await scene.ClearHighlightsAsync();
        await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
    }

    private static async Task WaitingAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/queues");
        await scene.StartCaptureAsync();
        await OpenQueueAsync(scene, "Support");

        await scene.StepAsync("wai-1", async () =>
        {
            await ScrollToAsync(scene, scene.Page.Locator("#heading-card-while-callers-wait"), 120);
            await scene.TypeAsync(Field(scene, "ActivityQueue_Treatment_WelcomeMessage"), "Thanks for calling Contoso support.", delay: 20);
            await PickAsync(scene, "ActivityQueue_Treatment_HoldMusicMediaId", "Hold music");
            await scene.HighlightAsync(Section(Field(scene, "ActivityQueue_Treatment_HoldMusicMediaId")), "Hold music");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("wai-2", async () =>
        {
            await SetAsync(scene, Field(scene, "ActivityQueue_Treatment_AnnouncementIntervalSeconds"), "60");
            await scene.ClickAsync(Field(scene, "ActivityQueue_Treatment_AnnouncePosition"), pauseAfter: 400);
            await scene.ClickAsync(Field(scene, "ActivityQueue_Treatment_AnnounceEstimatedWait"), pauseAfter: 400);
            await SetAsync(scene, Field(scene, "ActivityQueue_Treatment_AverageHandleTimeSeconds"), "300");
            await scene.HighlightAsync(Section(Field(scene, "ActivityQueue_Treatment_AverageHandleTimeSeconds")), "Average handle time");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("wai-3", async () =>
        {
            await ScrollToAsync(scene, scene.Page.Locator("#heading-card-callback"), 120);
            await SetAsync(scene, Field(scene, "ActivityQueue_Treatment_CallbackDtmfKey"), "9");
            await SetAsync(scene, Field(scene, "ActivityQueue_Treatment_CallbackOfferAfterSeconds"), "45");
            await scene.HighlightAsync(Section(Field(scene, "ActivityQueue_Treatment_CallbackDtmfKey")), "Callback key");
        });

        await scene.ClearHighlightsAsync();
        await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
    }

    private static async Task CloneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/queues");
        await scene.StartCaptureAsync();

        await scene.StepAsync("cln-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("ul.admin-list li").Filter(new() { HasTextString = "Support" }).First.Locator("button.dropdown-toggle").Filter(new() { HasTextString = "Actions" }), pauseAfter: 1200);
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show a").Filter(new() { HasTextString = "Clone" }).First, "Clone");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("cln-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".dropdown-menu.show a").Filter(new() { HasTextString = "Clone" }).First);
            await scene.HighlightAsync(Section(Field(scene, "ActivityQueue_Name")), "Copy of ...");
        });

        await scene.ClearHighlightsAsync();
    }
}
