using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the AI Agents video, one method per "clip" of storyboard.json.
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
        ("02-create", CreateAsync),
        ("03-use", UseAsync),
        ("04-tools", ToolsAsync),
        ("05-invoke", InvokeAsync),
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

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ag-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.create, a.create").First, pauseAfter: 600);
            await scene.PauseAsync(900);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='/Admin/ai/profile/create']").Filter(new() { HasTextString = "Start" }).First);
            await scene.Page.Locator("#AIProfile_DisplayText").FillAsync("Research agent");
            await scene.PauseAsync(600);
        });

        await scene.StepAsync("ag-2", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#AIProfile_ProfileType"), "Agent", pauseAfter: 900);
            await scene.Page.Locator("#AIProfile_Description").FillAsync("Researches a topic and returns three short, sourced bullet points.");
            await ShowAsync(scene, Field(scene.Page.Locator("#AIProfile_Description")), "Description", 4000);
        });

        await scene.StepAsync("ag-3", async () =>
        {
            await ShowAsync(scene, Field(scene.Page.Locator("#AIProfile_AgentAvailability")), "Availability", 6000);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1200);
        });
    }

    private static async Task UseAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.Page.RunAndWaitForNavigationAsync(() => Profile(scene).ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("use-1", async () =>
        {
            await scene.ClickAsync(TabNamed(scene, "Capabilities"), pauseAfter: 900);
            await scene.PauseAsync(4500);
        });

        await scene.StepAsync("use-2", async () =>
        {
            var label = scene.Page.Locator("label").Filter(new() { HasTextString = "Research agent" }).First;
            var box = scene.Page.Locator("input[name='AIProfile.Agents[0].IsSelected'][type=checkbox]");
            await scene.ClickAsync(box, pauseAfter: 600);
            await ShowAsync(scene, label, "Research agent", 3000);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1000);
        });
    }

    private static async Task ToolsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.Page.RunAndWaitForNavigationAsync(() => Profile(scene).ClickAsync());
        await TabNamed(scene, "Capabilities").ClickAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("tol-1", async () =>
        {
            var tools = scene.Page.Locator("label").Filter(new() { HasTextString = "Select All Tools" }).First;
            await ShowAsync(scene, tools, "Tools", 3000);
            await scene.Page.EvaluateAsync("() => { const l = [...document.querySelectorAll('h5')].find(h => h.textContent.trim() === 'Content Management'); if (l) l.scrollIntoView({ block: 'start' }); }");
            await scene.PauseAsync(6000);
            await scene.Page.EvaluateAsync("() => { const l = [...document.querySelectorAll('h5')].find(h => h.textContent.trim() === 'Users Management'); if (l) l.scrollIntoView({ block: 'start' }); }");
            await scene.PauseAsync(4000);
        });

        await scene.StepAsync("tol-2", async () =>
        {
            var all = scene.Page.Locator("label").Filter(new() { HasTextString = "Select All in Utilities" }).First;
            await ShowAsync(scene, all, "Select All in Utilities", 5000);
        });
    }

    private static async Task InvokeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.StartCaptureAsync();

        await scene.StepAsync("inv-1", async () =>
        {
            var row = scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = "Research agent" }).First;
            await scene.ClickAsync(row.Locator("button.dropdown-toggle.actions").First, pauseAfter: 900);
            await ShowAsync(scene, row.Locator("a[href*='/Admin/ai/chat/test/']").First, "Invoke Profile", 1500);
            await scene.ClickAndWaitForNavigationAsync(row.Locator("a[href*='/Admin/ai/chat/test/']").First);
        });

        await scene.StepAsync("inv-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#_UserPrompt"), "Give me three facts about shipping boxes.", delay: 14);
            await scene.ClickAsync(scene.Page.Locator("#_SendButton"), pauseAfter: 400);
        });

        await scene.StepAsync("inv-3", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await scene.Page.WaitForFunctionAsync("() => document.querySelectorAll('.ai-chat-message-heading-assistant').length >= 1", null, new() { Timeout = 120_000 });
                await scene.PauseAsync(6000);
            });
            await scene.PauseAsync(5000);
        });
    }

}
