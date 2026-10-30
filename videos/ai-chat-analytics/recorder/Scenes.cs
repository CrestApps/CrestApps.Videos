using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the AI Chat Analytics video, one method per "clip" of storyboard.json.
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
        ("02-collect", CollectAsync),
        ("03-session", SessionAsync),
        ("04-extracted", ExtractedAsync),
        ("05-goals", GoalsAsync),
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

    private static ILocator Txt(Scene scene, string text) => scene.Page.GetByText(text, new() { Exact = true }).First;

    private static async Task ShowTextAsync(Scene scene, string text, string caption, int hold = 2500)
    {
        var target = Txt(scene, text);
        await scene.Page.EvaluateAsync("(t) => { const e = [...document.querySelectorAll('*')].find(x => x.children.length === 0 && x.textContent.trim() === t); if (e) { window.scrollTo(0, e.getBoundingClientRect().top + window.scrollY - 140); } }", text);
        await scene.PauseAsync(300);
        await ShowAsync(scene, target, caption, hold);
    }

    private static async Task OpenReportAsync(Scene scene, string href)
    {
        await scene.ClickAsync(scene.Page.Locator("#left-nav button").Filter(new() { HasTextString = "Reports" }).First, pauseAfter: 600);
        var group = scene.Page.Locator("#left-nav button").Filter(new() { HasTextString = "Artificial Intelligence Reports" });
        if (await group.CountAsync() > 0 && !await scene.Page.Locator($"#left-nav a[href$='{href}']").First.IsVisibleAsync())
        {
            await scene.ClickAsync(group.First, pauseAfter: 600);
        }

        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator($"#left-nav a[href$='{href}']").First);
    }

    private static async Task RunReportAsync(Scene scene, bool profile)
    {
        await scene.ClickAsync(scene.Page.GetByText("Select range").First, pauseAfter: 600);
        await scene.ClickAsync(scene.Page.Locator("label[for$='-last-30']").First, pauseAfter: 500);
        if (profile)
        {
            var select = scene.Page.Locator("#ProfileId");
            await scene.SelectAsync(select, "Support assistant", pauseAfter: 500);
        }

        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button, input[type=submit]").Filter(new() { HasTextString = "Show" }).First);
        await scene.PauseAsync(1200);
    }

    private static async Task CollectAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.Page.RunAndWaitForNavigationAsync(() => Profile(scene).ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("col-1", async () =>
        {
            await scene.ClickAsync(TabNamed(scene, "Data Processing"), pauseAfter: 900);
            await ShowAsync(scene, Section(scene.Page.Locator("#AIProfile_SessionInactivityTimeoutInMinutes")), "Session inactivity timeout (minutes)", 4500);
        });

        await scene.StepAsync("col-2", async () =>
        {
            await ShowAsync(scene, FormCheck(scene.Page.Locator("#AIProfile_EnableSessionMetrics")), "Enable session metrics", 4500);
            await ShowAsync(scene, FormCheck(scene.Page.Locator("#AIProfile_EnableAIResolutionDetection")), "Enable AI resolution detection", 4500);
        });

        await scene.StepAsync("col-3", async () =>
        {
            await ShowAsync(scene, FormCheck(scene.Page.Locator("#AIProfile_EnableConversionMetrics")), "Enable conversion metrics", 4000);
            await ShowAsync(scene, FormCheck(scene.Page.Locator("#AIProfile_EnableDataExtraction")), "Enable data extraction", 4000);
        });
    }

    private static async Task SessionAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ses-1", async () =>
        {
            await OpenReportAsync(scene, "/Admin/AI/ChatAnalytics/Index");
            await RunReportAsync(scene, false);
        });

        await scene.StepAsync("ses-2", async () =>
        {
            await ShowTextAsync(scene, "Conversation & Usage", "Conversation & Usage", 2000);
            await ShowTextAsync(scene, "Containment Rate", "Containment Rate", 2000);
            await ShowTextAsync(scene, "Abandonment Rate", "Abandonment Rate", 2000);
        });

        await scene.StepAsync("ses-3", async () =>
        {
            await ShowTextAsync(scene, "Avg Session Duration", "Avg Session Duration", 2500);
            await ShowTextAsync(scene, "Resolved Sessions", "Resolved Sessions", 2500);
        });

        await scene.StepAsync("ses-4", async () =>
        {
            await ShowTextAsync(scene, "Usage by Time of Day", "Usage by Time of Day", 3000);
            await ShowTextAsync(scene, "Usage by Day of Week", "Usage by Day of Week", 3000);
        });

        await scene.StepAsync("ses-5", async () =>
        {
            await ShowTextAsync(scene, "User Segmentation", "User Segmentation", 3500);
            await ShowTextAsync(scene, "Total Tokens Used", "Total Tokens Used", 4500);
        });

        await scene.StepAsync("ses-6", async () =>
        {
            await ShowTextAsync(scene, "Resolution Rate", "Resolution Rate", 3500);
            await ShowTextAsync(scene, "Satisfaction Rate", "Satisfaction Rate", 4500);
        });

        await scene.StepAsync("ses-7", async () =>
        {
            await scene.Page.EvaluateAsync("() => window.scrollTo(0, 0)");
            await scene.PauseAsync(500);
            await ShowAsync(scene, scene.Page.GetByText("Export as CSV").First, "Export as CSV", 3000);
        });
    }

    private static async Task ExtractedAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ext-1", async () =>
        {
            await OpenReportAsync(scene, "/Admin/AI/ChatExtractedData/Index");
            await RunReportAsync(scene, true);
        });

        await scene.StepAsync("ext-2", async () =>
        {
            await scene.PauseAsync(9000);
        });
    }

    private static async Task GoalsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.StartCaptureAsync();

        await scene.StepAsync("goa-1", async () =>
        {
            await OpenReportAsync(scene, "/Admin/AI/ChatConversionGoals/Index");
            await RunReportAsync(scene, true);
        });

        await scene.StepAsync("goa-2", async () =>
        {
            await scene.PauseAsync(9000);
        });
    }

}
