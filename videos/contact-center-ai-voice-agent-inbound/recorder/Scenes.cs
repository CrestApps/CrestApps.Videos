using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the AI inbound video, one method per "clip" of storyboard.json.
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
        ("02-entry", EntryAsync),
        ("03-hours", HoursAsync),
        ("04-subject", SubjectAsync),
        ("05-voice", VoiceAsync),
        ("07-review", ReviewAsync),
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
        => scene.Page.Locator("label").Filter(new() { HasTextString = text }).First;

    private static ILocator ById(Scene scene, string suffix)
        => scene.Page.Locator($"[id$='{suffix}']").First;

    private static async Task PickOptionAsync(Scene scene, ILocator select, string text)
    {
        var id = await select.GetAttributeAsync("id");

        if (await scene.Page.Locator($"button[data-id='{id}']").CountAsync() > 0)
        {
            await PickAsync(scene, id, text);
            return;
        }

        var value = await select.EvaluateAsync<string>("(el, text) => [...el.options].find(o => o.text.includes(text)).value", text);
        await scene.SelectAsync(select, value, pauseAfter: 700);
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

        await page.GotoAsync(BaseUrl + "/Admin/ai/profile/new/voice-front-desk");
        await page.Locator("input[type=text]").First.FillAsync("Front desk answerer");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("button, input[type=submit]").Filter(new() { HasTextString = "Create profile" }).First.ClickAsync());
        Console.WriteLine("Created the front desk profile: " + page.Url);
        await context.CloseAsync();
    }

    private static async Task EntryAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/entry-points");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ent-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("a, button").Filter(new() { HasTextString = "Add inbound entry point" }).First, pauseAfter: 800);
            await scene.HighlightAsync(scene.Page.Locator(".modal.show .card, .modal.show .list-group-item").Filter(new() { HasTextString = "Voice calls" }).First, "Voice calls");
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='create?channel=Phone']").First);
            await scene.TypeAsync(scene.Page.Locator("#ContactCenterEntryPoint_Name"), "Front desk line", delay: 30);
            await PickOptionAsync(scene, scene.Page.Locator("#ContactCenterEntryPoint_AddressIds"), "Main line");
            await scene.HighlightAsync(Section(scene.Page.Locator("#ContactCenterEntryPoint_AddressIds")), "Numbers");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ent-2", async () =>
        {
            await ScrollToAsync(scene, Card(scene, "Routing"), margin: 120);
            await PickOptionAsync(scene, scene.Page.Locator("#ContactCenterEntryPoint_TargetType"), "AI voice agent");
            await scene.HighlightAsync(Section(scene.Page.Locator("#ContactCenterEntryPoint_TargetType")), "Route to: AI voice agent");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ent-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1200);
            await scene.HighlightAsync(scene.Page.Locator(".text-danger, .validation-summary-errors, .alert-danger").Filter(new() { HasTextString = "AI" }).First, "The AI agent is required");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await PickOptionAsync(scene, scene.Page.Locator("#ContactCenterEntryPoint_TargetAIProfileId"), "Front desk answerer");
            await scene.HighlightAsync(Section(scene.Page.Locator("#ContactCenterEntryPoint_TargetAIProfileId")), "AI agent");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task HoursAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/entry-points/create?channel=Phone");
        await scene.StartCaptureAsync();

        await scene.StepAsync("hrs-1", async () =>
        {
            await ScrollToAsync(scene, Card(scene, "Hours"), margin: 120);
            await PickOptionAsync(scene, scene.Page.Locator("#ContactCenterEntryPoint_BusinessHoursCalendarId"), "Weekday calling hours");
            await scene.HighlightAsync(Section(scene.Page.Locator("#ContactCenterEntryPoint_BusinessHoursCalendarId")), "Business hours calendar");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("hrs-2", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#ContactCenterEntryPoint_ClosedAction")), "Closed action");
            await scene.PauseAsync(3000);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("hrs-3", async () =>
        {
            await ScrollToAsync(scene, Card(scene, "Voicemail"), margin: 120);
            await scene.HighlightAsync(Section(scene.Page.Locator("#ContactCenterEntryPoint_VoicemailDestination")), "Deliver voicemail to");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(scene.Page.Locator("#ContactCenterEntryPoint_VoicemailRecipientAgentId")), "Voicemail inbox");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SubjectAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ContentTypes/Springleaddrive/ContentParts/OmnichannelSubjectPart/Edit");
        await scene.StartCaptureAsync();

        await scene.StepAsync("subj-1", async () =>
        {
            await PickOptionAsync(scene, ById(scene, "_Direction"), "Inbound");
            await scene.PauseAsync(600);
            await PickOptionAsync(scene, ById(scene, "_InteractionType"), "Automated");
            await PickOptionAsync(scene, ById(scene, "_Channel"), "Phone");
            await scene.PauseAsync(600);
            await PickOptionAsync(scene, ById(scene, "_ChannelEndpointId"), "Main line");
            await scene.HighlightAsync(Section(ById(scene, "_ChannelEndpointId")), "Address: the number it answers");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("subj-2", async () =>
        {
            await ScrollToAsync(scene, ById(scene, "_ProfileId"), margin: 250);
            await PickOptionAsync(scene, ById(scene, "_ProfileId"), "Front desk answerer");
            await scene.HighlightAsync(Section(ById(scene, "_ProfileId")), "AI profile");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(ById(scene, "_SubjectGoal")), "Subject goal");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("subj-3", async () =>
        {
            await ScrollToAsync(scene, Label(scene, "Allow the AI to hand off to a live agent"), margin: 250);
            await scene.HighlightAsync(Section(Label(scene, "Allow the AI to hand off to a live agent")), "Live agent handoff");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task VoiceAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();

        await scene.StepAsync("voice-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "Voice Media", delay: 60);
            await scene.PauseAsync(1200);
            await scene.HighlightAsync(Feature(scene, "CrestApps.OrchardCore.ContactCenter.Voice.Media"), "Contact Center Voice Media");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("voice-2", async () =>
        {
            await scene.NavigateAsync("/Admin/contact-center/voice-media");
            await scene.PauseAsync(500);
            await OpenMenuAsync(scene, "/Admin/contact-center/voice-media");
            await scene.HighlightAsync(Menu(scene, "/Admin/contact-center/voice-media"), "Interaction Center > Management > Voice Media");
        });

        await scene.ClearHighlightsAsync();
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

    private static async Task ReviewAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/omnichannel/activities/4qe1sdy3n9w7141mcntanad5wf");
        await scene.StartCaptureAsync();

        await scene.StepAsync("rev-1", async () =>
        {
            var row = scene.Page.Locator(".list-group-item").Filter(new() { HasTextString = "AI voice agent" }).First;
            await scene.HighlightAsync(row, "Dispositioned by: AI voice agent");
            await scene.PauseAsync(5000);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rev-2", async () =>
        {
            var review = scene.Page.Locator("a").Filter(new() { HasTextString = "Review AI conversation" }).First;
            await scene.ClickAndWaitForNavigationAsync(review);
            await scene.PauseAsync(1000);
            await scene.ScrollAsync(0, 200);
        });

        await scene.ClearHighlightsAsync();
    }
}
