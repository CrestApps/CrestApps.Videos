using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Export and Import Your AI Setup video, one method per "clip" of storyboard.json.
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
        ("02-plan", PlanAsync),
        ("03-export", ExportAsync),
        ("04-import", ImportAsync),
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

    private static ILocator Submit(Scene scene) => scene.Page.Locator("form button[type=submit]").Last;

    private static async Task AddStepAsync(Scene scene, string type, string caption, Func<Task> fill)
    {
        var add = scene.Page.Locator($"a[href*='/Step/Create?type={type}']").First;
        await scene.ClickAsync(scene.Page.Locator("button, a").Filter(new() { HasTextString = "Add Step" }).First, pauseAfter: 700);
        await scene.HighlightAsync(CardOf(add), caption);
        await scene.PauseAsync(600);
        await scene.ClearHighlightsAsync();
        await scene.ClickAndWaitForNavigationAsync(add);
        await fill();
        await scene.ClickAndWaitForNavigationAsync(Submit(scene));
        await scene.PauseAsync(600);
    }

    private static async Task PlanAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/DeploymentPlan/Index");
        await scene.StartCaptureAsync();

        await scene.StepAsync("pl-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/DeploymentPlan/Index"), "Tools > Deployments > Plans");
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create, button.create, a.btn-secondary").First);
            await scene.Page.Locator("#Name").FillAsync("AI setup");
            await scene.PauseAsync(800);
            await scene.ClickAndWaitForNavigationAsync(Submit(scene));
            await scene.PauseAsync(800);
        });

        await scene.StepAsync("pl-2", async () =>
        {
            await AddStepAsync(scene, "AIProviderConnectionDeploymentStep", "AI Provider Connections", async () =>
            {
                await scene.ClickAsync(scene.Page.Locator("#AIProviderConnectionDeploymentStep_IncludeAll"), pauseAfter: 600);
                await ShowAsync(scene, FormCheck(scene.Page.Locator("#AIProviderConnectionDeploymentStep_IncludeAll")), "Include all", 1200);
            });
        });

        await scene.StepAsync("pl-3", async () =>
        {
            await AddStepAsync(scene, "AIDeploymentDeploymentStep", "AI Deployments", async () =>
            {
                await scene.ClickAsync(scene.Page.Locator("#AIDeploymentDeploymentStep_IncludeAll"), pauseAfter: 300);
            });
            await AddStepAsync(scene, "AIProfileDeploymentStep", "AI Profiles", async () =>
            {
                await scene.ClickAsync(scene.Page.Locator("input[name='AIProfileDeploymentStep.ProfileNames']").First, pauseAfter: 600);
                await ShowAsync(scene, scene.Page.Locator("input[name='AIProfileDeploymentStep.ProfileNames']").First.Locator("xpath=ancestor::*[contains(@class,'form-check')][1]"), "Only the profiles you pick", 900);
            });
        });

        await scene.StepAsync("pl-4", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button, a").Filter(new() { HasTextString = "Add Step" }).First, pauseAfter: 700);
            await ScrollToAsync(scene, "a[href*='/Step/Create?type=AIToolInstanceDeploymentStep']");
            await ShowAsync(scene, CardOf(scene.Page.Locator("a[href*='/Step/Create?type=AIToolInstanceDeploymentStep']").First), "AI Tool Instances", 2000);
            await ShowAsync(scene, CardOf(scene.Page.Locator("a[href*='/Step/Create?type=AIDataSourceDeploymentStep']").First), "AI Data Sources", 2000);
            await ShowAsync(scene, CardOf(scene.Page.Locator("a[href*='/Step/Create?type=McpConnectionDeploymentStep']").First), "MCP Connection", 2000);
        });
    }

    private static async Task ExportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/DeploymentPlan/Index");
        await scene.Page.RunAndWaitForNavigationAsync(() => scene.Page.Locator("a[href*='/Admin/DeploymentPlan/Display/']").Filter(new() { HasTextString = "AI setup" }).First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("exp-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button, a").Filter(new() { HasTextString = "Execute" }).First, pauseAfter: 900);
            await scene.PauseAsync(8000);
        });

        await scene.StepAsync("exp-2", async () =>
        {
            await scene.Page.EvaluateAsync("() => window.scrollTo(0, 0)");
            await scene.PauseAsync(6000);
        });
    }

    private static async Task ImportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/DeploymentPlan/Index");
        await scene.StartCaptureAsync();

        await scene.StepAsync("imp-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/DeploymentPlan/Import/Json"), "JSON Import");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/DeploymentPlan/Import/Json"));
            await scene.PauseAsync(1200);
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/DeploymentPlan/Import/Index"));
            await scene.HighlightAsync(Menu(scene, "/Admin/DeploymentPlan/Import/Index"), "Package Import");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("imp-2", async () =>
        {
            await scene.PauseAsync(2000);
        });
    }

}
