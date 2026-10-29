using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Other AI Providers video, one method per "clip" of storyboard.json.
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
        ("02-openai", OpenAiAsync),
        ("03-ollama", OllamaAsync),
        ("04-inference", InferenceAsync),
        ("05-deployments", DeploymentsAsync),
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
            "CrestApps.OrchardCore.OpenAI",
            "CrestApps.OrchardCore.Ollama",
            "CrestApps.OrchardCore.AzureAIInference",
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

    private static async Task OpenAddConnectionAsync(Scene scene, string type, string caption)
    {
        await scene.ClickAsync(scene.Page.Locator("button.create, a.create").First, pauseAfter: 600);
        await scene.HighlightAsync(CardOf(scene.Page.Locator($"a[href$='/connection/create/{type}']")), caption);
        await scene.PauseAsync(1200);
        await scene.ClearHighlightsAsync();
        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator($"a[href$='/connection/create/{type}']"));
    }

    private static ILocator Conn(Scene scene, string name) => scene.Page.Locator("#AIProviderConnection_" + name);

    private static async Task OpenAiAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/provider/connections");
        await scene.StartCaptureAsync();

        await scene.StepAsync("oai-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ai/provider/connections"), "Artificial Intelligence > Provider Connections");
            await scene.PauseAsync(1000);
            await scene.ClearHighlightsAsync();
            await OpenAddConnectionAsync(scene, "OpenAI", "OpenAI");
        });

        await scene.StepAsync("oai-2", async () =>
        {
            await Conn(scene, "DisplayText").FillAsync("Contoso models");
            await Conn(scene, "Name").FillAsync("contoso-models");
            await scene.PauseAsync(500);
            await Conn(scene, "Endpoint").FillAsync("https://api.contoso-models.example/v1");
            await ShowAsync(scene, Field(Conn(scene, "Endpoint")), "Endpoint", 3500);
        });

        await scene.StepAsync("oai-3", async () =>
        {
            await Conn(scene, "ApiKey").FillAsync("demo-key-not-real");
            await ShowAsync(scene, Field(Conn(scene, "ApiKey")), "API key", 2500);
            await scene.OffCameraAsync(async () =>
            {
                await scene.ClickAsync(SaveButton(scene), pauseAfter: 200);
                await scene.Page.WaitForURLAsync("**/provider/connections*", new() { Timeout = 90_000 });
                await scene.PauseAsync(3000);
            });
        });
    }

    private static async Task OllamaAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/provider/connections");
        await scene.StartCaptureAsync();

        await scene.StepAsync("oll-1", async () =>
        {
            await OpenAddConnectionAsync(scene, "Ollama", "Ollama");
            await Conn(scene, "DisplayText").FillAsync("Local models");
            await Conn(scene, "Name").FillAsync("local-models");
            await ShowAsync(scene, Field(Conn(scene, "DisplayText")), "Title", 2500);
            await ShowAsync(scene, Field(Conn(scene, "Name")), "Technical name", 2500);
        });
    }

    private static async Task InferenceAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/provider/connections");
        await scene.StartCaptureAsync();

        await scene.StepAsync("inf-1", async () =>
        {
            await OpenAddConnectionAsync(scene, "AzureAIInference", "Azure AI Inference");
            await Conn(scene, "DisplayText").FillAsync("Contoso inference");
            await Conn(scene, "Name").FillAsync("contoso-inference");
            await Conn(scene, "Endpoint").FillAsync("https://contoso-inference.services.ai.azure.example/models");
            await ShowAsync(scene, Field(Conn(scene, "Endpoint")), "Endpoint", 800);
        });

        await scene.StepAsync("inf-2", async () =>
        {
            await ShowAsync(scene, Field(Conn(scene, "AuthenticationType")), "Authentication type", 4500);
            await scene.SelectAsync(Conn(scene, "AuthenticationType"), "ApiKey", pauseAfter: 600);
            await Conn(scene, "ApiKey").FillAsync("demo-key-not-real");
            await ShowAsync(scene, Field(Conn(scene, "ApiKey")), "API key", 2500);
        });
    }

    private static async Task DeploymentsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/deployments");
        await scene.StartCaptureAsync();

        await scene.StepAsync("dpl-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.create, a.create").First, pauseAfter: 900);
            await scene.PauseAsync(2500);
            await scene.HighlightAsync(CardOf(scene.Page.Locator("a[href$='/deployment/create/OpenAI']")), "OpenAI");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(CardOf(scene.Page.Locator("a[href$='/deployment/create/Ollama']")), "Ollama");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(CardOf(scene.Page.Locator("a[href$='/deployment/create/AzureAIInference']")), "Azure AI Inference");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("dpl-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/deployment/create/OpenAI']"));
            await scene.Page.Locator("#AIDeployment_ModelName").FillAsync("contoso-chat-model");
            await scene.Page.Locator("#AIDeployment_Name").FillAsync("contoso-compatible");
            await scene.PauseAsync(600);
            await ShowAsync(scene, Field(scene.Page.Locator("select[name$='ConnectionName']")), "Connection", 2500);
            await ShowAsync(scene, Field(scene.Page.Locator("#AIDeployment_ModelName")), "Model name", 2500);
            await ScrollToAsync(scene, "input[name='AIDeployment.SelectedFeatures']");
            await scene.PauseAsync(2500);
        });
    }

}
