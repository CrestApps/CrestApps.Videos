using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the AI outbound calls video, one method per "clip" of storyboard.json.
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
        ("02-profile", ProfileAsync),
        ("03-subject", SubjectAsync),
        ("04-load", LoadAsync),
        ("07-disclosure", DisclosureAsync),
        ("08-review", ReviewAsync),
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

    private static async Task PickOptionAsync(Scene scene, string id, string text)
    {
        if (await scene.Page.Locator($"button[data-id='{id}']").CountAsync() > 0)
        {
            await PickAsync(scene, id, text);
        }
        else
        {
            var select = scene.Page.Locator("#" + id);
            await scene.Page.WaitForFunctionAsync("([id, text]) => [...document.querySelectorAll('#' + id + ' option')].some(o => o.text.includes(text))", new object[] { id, text });
            var value = await select.EvaluateAsync<string>("(el, text) => [...el.options].find(o => o.text.includes(text)).value", text);
            await scene.SelectAsync(select, value, pauseAfter: 700);
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

        await page.GotoAsync(BaseUrl + "/Admin/contact-center/queues/create");
        await page.Locator("#ActivityQueue_Name").FillAsync("Sales handoff");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
        Console.WriteLine("Created the queue: " + page.Url);
        await context.CloseAsync();
    }

    private static async Task ProfileAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.StartCaptureAsync();

        await scene.StepAsync("prof-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("a.btn-primary, button.btn-primary").Filter(new() { HasTextString = "starting point" }).First, pauseAfter: 1500);
            await scene.ClickAsync(scene.Page.Locator(".modal.show button, .modal.show a").Filter(new() { HasTextString = "Phone calls" }).First, pauseAfter: 1200);
            await scene.HighlightAsync(scene.Page.Locator(".modal.show .card, .modal.show .list-group-item").Filter(new() { HasTextString = "Qualify leads by phone" }).First, "Qualify leads by phone");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='voice-qualify-leads']").First);
            await scene.Page.Locator("input[type=text]").First.FillAsync("");
            await scene.TypeAsync(scene.Page.Locator("input[type=text]").First, "Spring lead caller", delay: 45);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button, input[type=submit]").Filter(new() { HasTextString = "Create profile" }).First);
            await scene.PauseAsync(800);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("prof-2", async () =>
        {
            await scene.ScrollAsync(0, 300);
            await scene.HighlightAsync(Section(Label(scene, "Start the conversation automatically")), "Start the conversation automatically");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 250);
            var opening = Section(Label(scene, "Opening message"));
            await Label(scene, "Opening message").Locator("xpath=following::textarea[1]").FillAsync("Hi, this is Contoso calling about your recent inquiry. Do you have a quick minute?");
            await scene.HighlightAsync(opening, "Opening message");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
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
            await scene.HighlightAsync(Section(Label(scene, "Direction")), "Direction: Outbound");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, Label(scene, "Allow the AI to hand off to a live agent"), margin: 250);
            await scene.ClickAsync(Label(scene, "Allow the AI to hand off to a live agent"), pauseAfter: 600);
            await scene.HighlightAsync(Section(Label(scene, "Allow the AI to hand off to a live agent")), "Live agent handoff");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(Section(Label(scene, "Handoff queue")).Locator("select").First, "Sales handoff", pauseAfter: 700);
            await scene.HighlightAsync(Section(Label(scene, "Handoff queue")), "Handoff queue");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("subj-2", async () =>
        {
            await scene.ScrollAsync(0, 250);
            await scene.HighlightAsync(Section(Label(scene, "Escalate when")), "Escalate when");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("subj-3", async () =>
        {
            await scene.NavigateAsync("/Admin/omnichannel/subject-flows/Springleaddrive/actions");
            await scene.ClickAsync(scene.Page.Locator("a, button").Filter(new() { HasTextString = "Add Action" }).First, pauseAfter: 1200);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='actions/create/Finish']").First);
            await scene.PauseAsync(500);
            await scene.HighlightAsync(Section(scene.Page.Locator("#SubjectAction_DispositionGuidance")), "When to choose this disposition");
            await scene.TypeAsync(scene.Page.Locator("#SubjectAction_DispositionGuidance"), "The customer is not interested and does not want to be called again.", delay: 25);
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
            await scene.ClickAsync(scene.Page.Locator("a, button").Filter(new() { HasTextString = "Add Activity Load" }).First, pauseAfter: 1200);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='batches/create/Automatic']").First);
            await scene.PauseAsync(500);
            await scene.TypeAsync(scene.Page.Locator("#OmnichannelActivityBatch_DisplayText"), "Spring leads - AI calls", delay: 35);
            await PickOptionAsync(scene, "OmnichannelActivityBatch_SubjectContentType", "Spring lead drive");
            await PickOptionAsync(scene, "OmnichannelActivityBatch_CampaignId", "Spring lead drive");
            await PickOptionAsync(scene, "OmnichannelActivityBatch_AIProfileId", "Spring lead caller");
            await scene.HighlightAsync(Section(scene.Page.Locator("#OmnichannelActivityBatch_AIProfileId")), "AI profile");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("load-2", async () =>
        {
            await ScrollToAsync(scene, Section(scene.Page.Locator("#OmnichannelActivityBatch_Channel")), margin: 200);
            await PickOptionAsync(scene, "OmnichannelActivityBatch_Channel", "Phone");
            await scene.HighlightAsync(Section(scene.Page.Locator("#OmnichannelActivityBatch_Channel")), "Channel: Phone");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await PickOptionAsync(scene, "OmnichannelActivityBatch_ChannelEndpointId", "Campaign line");
            await scene.HighlightAsync(Section(scene.Page.Locator("#OmnichannelActivityBatch_ChannelEndpointId")), "Address");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("load-3", async () =>
        {
            var save = scene.Page.Locator("form button[type=submit], form a.btn").Filter(new() { HasTextString = "Save" }).Last;
            await ScrollToAsync(scene, save, margin: 500);
            await scene.HighlightAsync(save, "Save & Load activities");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task DisclosureAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/contactcenter");
        await scene.StartCaptureAsync();

        await scene.StepAsync("disc-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("a[href='#tab-recording-governance']"), pauseAfter: 900);
            await scene.HighlightAsync(Section(Label(scene, "Tell callers the call is recorded")), "Tell callers the call is recorded");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Label(scene, "Have automated voice agents say it first")), "Have automated voice agents say it first");
        });

        await scene.ClearHighlightsAsync();
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
            await scene.HighlightAsync(review, "Review AI conversation");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(review);
            await scene.PauseAsync(1000);
            await scene.ScrollAsync(0, 200);
        });

        await scene.ClearHighlightsAsync();
    }
}
