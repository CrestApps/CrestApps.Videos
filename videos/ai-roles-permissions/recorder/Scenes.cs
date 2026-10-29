using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the AI Roles and Permissions video, one method per "clip" of storyboard.json.
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
        ("02-builder", BuilderAsync),
        ("03-staff", StaffAsync),
        ("04-tools", ToolsAsync),
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

    private static ILocator Perm(Scene scene, string name) => scene.Page.Locator("[id='Checkbox." + name + "']");

    private static ILocator PermRow(Scene scene, string name) => Perm(scene, name).Locator("xpath=ancestor::tr[1]");

    private static async Task GrantAsync(Scene scene, string name, string caption, int hold = 1500, bool tick = true)
    {
        await scene.Page.EvaluateAsync("(id) => { const h = document.getElementById(id); h.scrollIntoView({ block: 'center' }); }", "Checkbox." + name);
        await scene.PauseAsync(250);

        if (tick)
        {
            await scene.ClickAsync(Perm(scene, name), pauseAfter: 300);
        }

        await ShowAsync(scene, PermRow(scene, name), caption, hold);
    }

    private static async Task BuilderAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Roles/Index");
        await scene.StartCaptureAsync();

        await scene.StepAsync("bld-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Roles/Index"), "Access Control > Roles");
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create, button.create").First);
            await scene.Page.Locator("#RoleName").FillAsync("AI builder");
            await scene.Page.Locator("#RoleDescription").FillAsync("Creates and tunes the AI assistants");
            await scene.PauseAsync(800);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(600);
        });

        await scene.StepAsync("bld-2", async () =>
        {
            await GrantAsync(scene, "ManageAIProfiles", "Manage AI profiles", 1500);
            await GrantAsync(scene, "ManageAIDeployments", "Manage AI deployments", 1500);
            await GrantAsync(scene, "ManageAIProfileTemplates", "Manage AI profile templates", 1500);
        });

        await scene.StepAsync("bld-3", async () =>
        {
            await GrantAsync(scene, "ManageAIDataSources", "Manage AI data sources", 500);
            await GrantAsync(scene, "ManageFileSources", "Manage file sources", 500);
            await GrantAsync(scene, "ManageWebCrawlers", "Manage web crawlers", 500);
            await GrantAsync(scene, "ManageAIToolInstances", "Manage AI tool instances", 500);
            await GrantAsync(scene, "ManageMcpConnections", "Manage MCP Connections", 500);
            await GrantAsync(scene, "ManageA2AConnections", "Manage Agent-to-Agent Connections", 500);
        });

        await scene.StepAsync("bld-4", async () =>
        {
            await GrantAsync(scene, "ManageProviderConnections", "Manage AI Provider Connections", 3500);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(800);
        });
    }

    private static async Task StaffAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Roles/Edit/Authenticated");
        await scene.StartCaptureAsync();

        await scene.StepAsync("stf-1", async () =>
        {
            await GrantAsync(scene, "QueryAnyAIProfile", "Query any AI profile", 3500, tick: false);
            await GrantAsync(scene, "QueryAIProfile_support-assistant", "Query AI profile - support-assistant", 4500, tick: false);
        });

        await scene.StepAsync("stf-2", async () =>
        {
            await GrantAsync(scene, "DeleteChatSession", "Delete chat session", 2500, tick: false);
            await GrantAsync(scene, "DeleteAllChatSessions", "Delete all chat sessions", 2500, tick: false);
            await GrantAsync(scene, "ClearAIMemory", "Clear AI memory", 2500, tick: false);
        });

        await scene.StepAsync("stf-3", async () =>
        {
            await GrantAsync(scene, "ViewChatAnalytics", "View AI Chat Analytics", 3000, tick: false);
            await GrantAsync(scene, "ExportChatAnalytics", "Export AI Chat Analytics", 3000, tick: false);
            await GrantAsync(scene, "ListChatInteractions", "List chat interactions", 3000, tick: false);
        });
    }

    private static async Task ToolsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Roles/Edit/Anonymous");
        await scene.StartCaptureAsync();

        await scene.StepAsync("tol-1", async () =>
        {
            await GrantAsync(scene, "AccessAnyAITool", "Access any AI tool", 5000, tick: false);
            await GrantAsync(scene, "AccessAITool_search_documents", "Access one tool: search_documents", 4500, tick: false);
        });

        await scene.StepAsync("tol-2", async () =>
        {
            await GrantAsync(scene, "QueryAIProfile_support-assistant", "Query AI profile - support-assistant", 5500, tick: false);
        });
    }

}
