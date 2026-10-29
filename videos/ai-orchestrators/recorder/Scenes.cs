using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the AI Orchestrators video, one method per "clip" of storyboard.json.
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
        ("02-copilot", CopilotAsync),
        ("03-claude", ClaudeAsync),
        ("04-profile", ProfileAsync),
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
            "CrestApps.OrchardCore.AI.Chat.Copilot",
            "CrestApps.OrchardCore.AI.Chat.Claude",
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

    private static async Task CopilotAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/ai-settings");
        await scene.StartCaptureAsync();

        await scene.StepAsync("cop-1", async () =>
        {
            await ScrollToAsync(scene, "#heading-card-copilot");
            await ShowAsync(scene, scene.Page.Locator("#heading-card-copilot"), "Copilot", 2000);
            await ShowAsync(scene, Field(scene.Page.Locator("#copilot-auth-type")), "Authentication type", 4000);
        });

        await scene.StepAsync("cop-2", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#copilot-auth-type"), "GitHubOAuth", pauseAfter: 900);
            await ShowAsync(scene, Field(scene.Page.Locator("#ISite_CopilotSettings_ClientId")), "Client ID", 3000);
            await ShowAsync(scene, Field(scene.Page.Locator("#ISite_CopilotSettings_ClientSecret")), "Client secret", 3000);
        });

        await scene.StepAsync("cop-3", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#copilot-auth-type"), "ApiKey", pauseAfter: 900);
            await ShowAsync(scene, Field(scene.Page.Locator("#copilot-provider-type")), "Provider type", 2500);
            await ShowAsync(scene, Field(scene.Page.Locator("#copilot-base-url")), "Base URL", 2000);
            await ShowAsync(scene, Field(scene.Page.Locator("#ISite_CopilotSettings_ApiKey")), "API key", 2000);
            await ShowAsync(scene, Field(scene.Page.Locator("#ISite_CopilotSettings_DefaultModel")), "Default model", 2000);
        });

        await scene.StepAsync("cop-4", async () =>
        {
            await ShowAsync(scene, Field(scene.Page.Locator("#ISite_CopilotSettings_WireApi")), "Wire API format", 4500);
            await scene.SelectAsync(scene.Page.Locator("#copilot-provider-type"), "azure", pauseAfter: 900);
            await ShowAsync(scene, Field(scene.Page.Locator("#ISite_CopilotSettings_AzureApiVersion")), "Azure API version", 3500);
        });
    }

    private static async Task ClaudeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/ai-settings");
        await scene.StartCaptureAsync();

        await scene.StepAsync("cla-1", async () =>
        {
            await ScrollToAsync(scene, "#heading-card-claude");
            await ShowAsync(scene, scene.Page.Locator("#heading-card-claude"), "Claude", 2000);
            await ShowAsync(scene, Field(scene.Page.Locator("#claude-auth-type")), "Authentication type", 1500);
            await scene.SelectAsync(scene.Page.Locator("#claude-auth-type"), "ApiKey", pauseAfter: 900);
        });

        await scene.StepAsync("cla-2", async () =>
        {
            await ShowAsync(scene, Field(scene.Page.Locator("#ISite_ClaudeSettings_ApiKey")), "API key", 3000);
            await ShowAsync(scene, Field(scene.Page.Locator("#ISite_ClaudeSettings_BaseUrl")), "Base URL", 3000);
            await ShowAsync(scene, Field(scene.Page.Locator("#ISite_ClaudeSettings_DefaultModel")), "Default model", 3500);
        });
    }

    private static async Task ProfileAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.Page.RunAndWaitForNavigationAsync(() => Profile(scene).ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("pro-1", async () =>
        {
            await ScrollToAsync(scene, "#orchestrator-selector");
            await ShowAsync(scene, Field(scene.Page.Locator("#orchestrator-selector")), "Orchestrator", 5000);
        });

        await scene.StepAsync("pro-2", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#orchestrator-selector"), "Copilot", pauseAfter: 1200);
            await scene.Page.EvaluateAsync("() => window.scrollBy(0, 220)");
            await scene.PauseAsync(4500);
        });

        await scene.StepAsync("pro-3", async () =>
        {
            await ShowAsync(scene, FormCheck(scene.Page.Locator("#AIProfile_IsAllowAll")), "Allow all tool executions", 7500);
        });

        await scene.StepAsync("pro-4", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#orchestrator-selector"), "Claude", pauseAfter: 1200);
            await scene.PauseAsync(4000);
        });
    }

}
