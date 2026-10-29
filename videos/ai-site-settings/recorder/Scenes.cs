using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the AI Site Settings video, one method per "clip" of storyboard.json.
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
        ("02-general", GeneralAsync),
        ("03-deployments", DeploymentsAsync),
        ("04-retrieval", RetrievalAsync),
        ("05-others", OthersAsync),
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

    private static ILocator Field(ILocator input) => input.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper') or contains(@class,'mb-3')][1]");

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

    private static ILocator Heading(Scene scene, string id) => scene.Page.Locator("#heading-card-" + id);

    private static async Task CardAsync(Scene scene, string id, string caption, int hold = 1800)
    {
        await ScrollToAsync(scene, "#heading-card-" + id);
        await ShowAsync(scene, Heading(scene, id), caption, hold);
    }

    private static async Task GeneralAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/ai-settings");
        await scene.StartCaptureAsync();

        await scene.StepAsync("gen-1", async () =>
        {
            await CardAsync(scene, "general", "General", 1500);
            await ShowAsync(scene, FormCheck(Setting(scene, "EnablePreemptiveMemoryRetrieval")), "Enable preemptive memory retrieval", 3500);
        });

        await scene.StepAsync("gen-2", async () =>
        {
            await ShowAsync(scene, Field(Setting(scene, "MaximumIterationsPerRequest")), "Maximum iterations per request", 4500);
        });

        await scene.StepAsync("gen-3", async () =>
        {
            await ShowAsync(scene, FormCheck(Setting(scene, "EnableDistributedCaching")), "Enable distributed caching", 3500);
            await ShowAsync(scene, FormCheck(Setting(scene, "EnableOpenTelemetry")), "Enable OpenTelemetry", 3500);
        });
    }

    private static async Task DeploymentsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/ai-settings");
        await scene.StartCaptureAsync();

        await scene.StepAsync("dep-1", async () =>
        {
            await CardAsync(scene, "default-deployments", "Default Deployments", 1500);
            await ShowAsync(scene, Field(Setting(scene, "DefaultChatDeploymentName")), "Default chat deployment", 3500);
            await ShowAsync(scene, Field(Setting(scene, "DefaultUtilityDeploymentName")), "Default utility deployment", 3500);
        });

        await scene.StepAsync("dep-2", async () =>
        {
            await ShowAsync(scene, Field(Setting(scene, "DefaultEmbeddingDeploymentName")), "Default embedding deployment", 3500);
            await ShowAsync(scene, Field(Setting(scene, "DefaultImageDeploymentName")), "Default image deployment", 2500);
            await ShowAsync(scene, Field(Setting(scene, "DefaultVisionDeploymentName")), "Default vision deployment", 2500);
        });

        await scene.StepAsync("dep-3", async () =>
        {
            await ShowAsync(scene, Field(Setting(scene, "DefaultSpeechToTextDeploymentName")), "Default speech-to-text deployment", 2500);
            await ShowAsync(scene, Field(Setting(scene, "DefaultTextToSpeechDeploymentName")), "Default text-to-speech deployment", 2000);
            await ShowAsync(scene, Field(Setting(scene, "DefaultRealtimeDeploymentName")), "Default realtime deployment", 2500);
        });
    }

    private static async Task RetrievalAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/ai-settings");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ret-1", async () =>
        {
            await CardAsync(scene, "default-orchestrator", "Default Orchestrator", 1500);
            await ShowAsync(scene, FormCheck(Setting(scene, "EnablePreemptiveRag")), "Enable preemptive retrieval-augmented generation (RAG)", 8000);
        });

        await scene.StepAsync("ret-2", async () =>
        {
            await CardAsync(scene, "data-sources", "Data Sources", 1500);
            await ShowAsync(scene, Field(Setting(scene, "DefaultStrictness")), "Default strictness", 2500);
            await ShowAsync(scene, Field(Setting(scene, "DefaultTopNDocuments")), "Default top documents", 2500);
        });
    }

    private static async Task OthersAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/ai-settings");
        await scene.StartCaptureAsync();

        await scene.StepAsync("oth-1", async () =>
        {
            await CardAsync(scene, "chat-interactions", "Chat Interactions", 3000);
        });

        await scene.StepAsync("oth-2", async () =>
        {
            await CardAsync(scene, "documents", "Documents", 2500);
            await CardAsync(scene, "m-c-p-server", "MCP Server", 2500);
            await CardAsync(scene, "memory", "Memory", 3500);
        });

        await scene.StepAsync("oth-3", async () =>
        {
            await CardAsync(scene, "prompt-security", "Prompt Security", 2500);
            await CardAsync(scene, "visitor-identity", "Visitor Identity", 2500);
        });
    }

}
