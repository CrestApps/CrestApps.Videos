using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the AI Data Extraction and Post-Session Processing video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // Recorded in this order: each clip builds on the one before.
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-extraction", ExtractionAsync),
        ("03-postsession", PostSessionAsync),
        ("04-metrics", MetricsAsync),
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
    /// Enables the features of the video (not recorded).
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

        foreach (var feature in new[]
        {
            "CrestApps.OrchardCore.AI.Chat",
            "CrestApps.OrchardCore.AI.Chat.Analytics",
        })
        {
            await page.GotoAsync(BaseUrl + "/Admin/Features");
            var enable = page.Locator($"a[href$=\"/{feature}/Enable\"]");

            if (await enable.CountAsync() > 0)
            {
                await page.RunAndWaitForNavigationAsync(() => enable.First.ClickAsync());
                Console.WriteLine($"Enabled {feature}");
            }
        }

        await context.CloseAsync();
    }

    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator CardOf(ILocator link) => link.Locator("xpath=ancestor::*[contains(concat(' ',normalize-space(@class),' '),' card ')][1]");

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator Field(ILocator input) => input.Locator("xpath=ancestor::*[count(.//select | .//textarea | .//input[not(@type='hidden')])=1][last()]");

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static ILocator FormCheck(ILocator box) => box.Locator("xpath=ancestor::*[contains(@class,'form-check')][1]");

    private static ILocator Profile(Scene scene) => scene.Page.Locator("a[href*='/Admin/ai/profile/edit/']").Filter(new() { HasTextString = "Support assistant" }).First;

    private static ILocator KnowledgeTab(Scene scene) => scene.Page.Locator(".nav-tabs a, .nav-tabs button").Filter(new() { HasTextString = "Knowledge" }).First;

    private static ILocator TabNamed(Scene scene, string text) => scene.Page.Locator(".nav-tabs a, .nav-tabs button").Filter(new() { HasTextString = text }).First;

    private static ILocator Setting(Scene scene, string name) => scene.Page.Locator($"[name$='{name}']").First;

    private static Task ScrollToAsync(Scene scene, string selector)
        => scene.Page.EvaluateAsync("(s) => { const h = document.querySelector(s); window.scrollTo(0, h.getBoundingClientRect().top + window.scrollY - 90); }", selector);

    private static async Task ShowAsync(Scene scene, ILocator target, string caption, int hold = 2500)
    {
        await scene.HighlightAsync(target, caption);
        await scene.PauseAsync(hold);
        await scene.ClearHighlightsAsync();
    }

    private static async Task OpenChatAsync(Scene scene)
    {
        await scene.Page.Locator("button.dropdown-toggle.actions").First.ClickAsync();
        await scene.Page.RunAndWaitForNavigationAsync(() => scene.Page.Locator("a[href*='/Admin/ai/chat/session/']").Filter(new() { HasTextString = "New chat" }).First.ClickAsync());
    }

    private static async Task AskAsync(Scene scene, string question)
    {
        await scene.TypeAsync(scene.Page.Locator("#AIChatSession_UserPrompt"), question, delay: 14);
        await scene.ClickAsync(scene.Page.Locator("#AIChatSession_SendButton"), pauseAfter: 400);
    }

    private static async Task WaitAnswerAsync(Scene scene, int answers)
    {
        await scene.OffCameraAsync(async () =>
        {
            await scene.Page.WaitForFunctionAsync("(n) => document.querySelectorAll('.ai-chat-message-heading-assistant').length >= n", answers, new() { Timeout = 120_000 });
            await scene.PauseAsync(4000);
        });
    }

    // ----- Clips -----

    private static async Task OpenDataTabAsync(Scene scene)
    {
        await scene.ClickAsync(TabNamed(scene, "Data Processing"), pauseAfter: 700);
    }

    private static ILocator Pane(Scene scene, string id) => scene.Page.Locator("#" + id);

    private static async Task ExtractionAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.Page.RunAndWaitForNavigationAsync(() => Profile(scene).ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("ext-1", async () =>
        {
            await OpenDataTabAsync(scene);
            await ShowAsync(scene, Section(scene.Page.Locator("#AIProfile_SessionInactivityTimeoutInMinutes")), "Session inactivity timeout (minutes)", 3500);
        });

        await scene.StepAsync("ext-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("label[for='AIProfile_EnableDataExtraction']"), pauseAfter: 500);
            await ShowAsync(scene, FormCheck(scene.Page.Locator("#AIProfile_EnableDataExtraction")), "Enable data extraction", 2500);
            await ShowAsync(scene, Section(scene.Page.Locator("#AIProfile_ExtractionCheckInterval")), "Extraction check interval", 3000);
        });

        await scene.StepAsync("ext-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Add Entry" }).First, pauseAfter: 600);
            await scene.Page.Locator("[name='AIProfile.Entries[0].Name']").FillAsync("customer_email");
            await scene.Page.Locator("[name='AIProfile.Entries[0].Description']").FillAsync("The email address of the customer");
            await scene.PauseAsync(1200);
            await ShowAsync(scene, scene.Page.Locator("[name='AIProfile.Entries[0].Name']"), "Name", 2000);
            await ShowAsync(scene, scene.Page.Locator("[name='AIProfile.Entries[0].Description']"), "Description", 2000);
        });

        await scene.StepAsync("ext-4", async () =>
        {
            await ShowAsync(scene, FormCheck(scene.Page.Locator("[name='AIProfile.Entries[0].AllowMultipleValues']")), "Allow Multiple Values", 3500);
            await ShowAsync(scene, FormCheck(scene.Page.Locator("[name='AIProfile.Entries[0].IsUpdatable']")), "Updatable", 3500);
        });
    }

    private static async Task PostSessionAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.Page.RunAndWaitForNavigationAsync(() => Profile(scene).ClickAsync());
        await TabNamed(scene, "Data Processing").ClickAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("pst-1", async () =>
        {
            await ScrollToAsync(scene, "#AIProfile_EnablePostSessionProcessing");
            await scene.ClickAsync(scene.Page.Locator("label[for='AIProfile_EnablePostSessionProcessing']"), pauseAfter: 500);
            await ShowAsync(scene, FormCheck(scene.Page.Locator("#AIProfile_EnablePostSessionProcessing")), "Enable post-session processing", 4000);
        });

        await scene.StepAsync("pst-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Tasks" }).First, pauseAfter: 600);
            await scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Add Task" }).First, pauseAfter: 600);
            await scene.Page.Locator("[name='AIProfile.Tasks[0].Name']").FillAsync("Summary");
            await scene.PauseAsync(800);
            await ShowAsync(scene, scene.Page.Locator("[name='AIProfile.Tasks[0].Type']"), "Type: Semantic", 3000);
        });

        await scene.StepAsync("pst-3", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("[name='AIProfile.Tasks[0].Type']"), "Predefined Options", pauseAfter: 800);
            await scene.HighlightAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Add Option" }).First, "Add Option");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("pst-4", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Capabilities" }).Last, pauseAfter: 900);
            await scene.PauseAsync(3500);
        });
    }

    private static async Task MetricsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.Page.RunAndWaitForNavigationAsync(() => Profile(scene).ClickAsync());
        await TabNamed(scene, "Data Processing").ClickAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("met-1", async () =>
        {
            await ScrollToAsync(scene, "#AIProfile_EnableSessionMetrics");
            await scene.ClickAsync(scene.Page.Locator("label[for='AIProfile_EnableSessionMetrics']"), pauseAfter: 500);
            await ShowAsync(scene, FormCheck(scene.Page.Locator("#AIProfile_EnableSessionMetrics")), "Enable session metrics", 5000);
        });

        await scene.StepAsync("met-2", async () =>
        {
            await ShowAsync(scene, FormCheck(scene.Page.Locator("#AIProfile_EnableAIResolutionDetection")), "Enable AI resolution detection", 4500);
        });

        await scene.StepAsync("met-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("label[for='AIProfile_EnableConversionMetrics']"), pauseAfter: 500);
            await scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Add Goal" }).First, pauseAfter: 600);
            await scene.Page.Locator("[name='AIProfile.ConversionGoals[0].Name']").FillAsync("Booked a demo");
            await scene.Page.Locator("[name='AIProfile.ConversionGoals[0].Description']").FillAsync("The visitor agreed to a demo");
            await scene.PauseAsync(800);
            await ShowAsync(scene, scene.Page.Locator("[name='AIProfile.ConversionGoals[0].MinScore']"), "Min score", 2000);
            await ShowAsync(scene, scene.Page.Locator("[name='AIProfile.ConversionGoals[0].MaxScore']"), "Max score", 2000);
        });
    }

}
