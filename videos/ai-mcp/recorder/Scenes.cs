using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Model Context Protocol video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // Recorded in this order: 03 and 04 create the connections that 06 ticks.
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-connection", ConnectionAsync),
        ("04-local", LocalAsync),
        ("06-use", UseAsync),
        ("07-server", ServerAsync),
        ("08-prompts", PromptsAsync),
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
            "CrestApps.OrchardCore.AI.ConnectionManagement",
            "CrestApps.OrchardCore.OpenAI.Azure",
            "CrestApps.OrchardCore.AI.Mcp",
            "CrestApps.OrchardCore.AI.Mcp.Stdio",
            "CrestApps.OrchardCore.AI.Mcp.Server",
            "CrestApps.OrchardCore.AI.Mcp.Resources.Ftp",
            "CrestApps.OrchardCore.AI.Mcp.Resources.Sftp",
            "CrestApps.OrchardCore.AI.Agent",
            "CrestApps.OrchardCore.AI.ToolInstances",
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

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static async Task OpenAddAsync(Scene scene, string modal, string hrefEnd, int pause = 1500)
    {
        await scene.ClickAsync(scene.Page.Locator("button.create"), pauseAfter: pause);
        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator($"#{modal} a[href$='{hrefEnd}']"));
    }

    private static Task ScrollToServerAsync(Scene scene)
        => scene.Page.EvaluateAsync("() => { const h = document.querySelector('#heading-card-m-c-p-server'); window.scrollTo(0, h.getBoundingClientRect().top + window.scrollY - 90); }");

    // ----- Clips -----

    private static async Task ConnectionAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/mcp/connections");
        await scene.StartCaptureAsync();

        await scene.StepAsync("conn-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ai/mcp/connections"), "Artificial Intelligence > Model Context Protocol > MCP Hosts");
            await scene.PauseAsync(3000);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("conn-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.create"), pauseAfter: 1500);
            await scene.HighlightAsync(scene.Page.Locator("#modalAddConnection .modal-body"), "Available Sources");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalAddConnection a[href$='/create/sse']"));
        });

        await scene.StepAsync("conn-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#McpConnection_DisplayText"), "Contoso tools server", delay: 45);
            await scene.TypeAsync(scene.Page.Locator("#McpConnection_Endpoint"), "https://tools.contoso.example/mcp", delay: 30);
        });

        await scene.StepAsync("conn-4", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#McpConnection_AuthenticationType"), "API Key", pauseAfter: 900);
            await scene.HighlightAsync(Section(scene.Page.Locator("#McpConnection_AuthenticationType")), "Authentication");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("conn-5", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#McpConnection_ApiKeyHeaderName"), "Authorization", delay: 30);
            await scene.TypeAsync(scene.Page.Locator("#McpConnection_ApiKeyPrefix"), "Bearer", delay: 30);
            await scene.TypeAsync(scene.Page.Locator("#McpConnection_ApiKey"), "demo-key-not-a-real-secret", delay: 25);
            await scene.HighlightAsync(Section(scene.Page.Locator("#McpConnection_ApiKey")), "API Key");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("conn-6", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1500);
        });
    }

    private static async Task LocalAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/mcp/connections");
        await scene.StartCaptureAsync();

        await scene.StepAsync("loc-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.create"), pauseAfter: 1200);
            await scene.HighlightAsync(scene.Page.Locator("#modalAddConnection a[href$='/create/stdIo']").Locator("xpath=ancestor::*[contains(@class,'list-group-item') or self::li or self::div][1]"), "Standard Input/Output");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalAddConnection a[href$='/create/stdIo']"));
        });

        await scene.StepAsync("loc-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#McpConnection_DisplayText"), "Local reports tool", delay: 45);
            await scene.TypeAsync(scene.Page.Locator("#McpConnection_Command"), "contoso-reports-server", delay: 35);
            await scene.HighlightAsync(Section(scene.Page.Locator("#McpConnection_Command")), "Command");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("loc-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("h1").First, "Model Context Protocol Local Client");
            await scene.PauseAsync(2500);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("loc-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1500);
        });
    }

    private static async Task UseAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.StartCaptureAsync();

        await scene.StepAsync("use-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ai/profiles"), "Artificial Intelligence > Profiles");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='/Admin/ai/profile/edit/']").Filter(new() { HasTextString = "Support assistant" }).First);
        });

        await scene.StepAsync("use-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".nav-tabs a, .nav-tabs button").Filter(new() { HasTextString = "Capabilities" }).First, pauseAfter: 900);
            await scene.HighlightAsync(scene.Page.Locator("text=MCP Connections").First.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]"), "MCP Connections");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("input[id^='AIProfile_Connections_'][type=checkbox]").First, pauseAfter: 600);
        });

        await scene.StepAsync("use-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("use-4", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ai/templates"), "Profile templates");
            await scene.PauseAsync(2500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ServerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/ai-settings");
        await ScrollToServerAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("srv-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/ai-settings"), "Settings > Artificial Intelligence");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await ScrollToServerAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator("#heading-card-m-c-p-server"), "MCP Server");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("srv-2", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_McpServerSettings_AuthenticationType")), "Authentication type");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(scene.Page.Locator("#ISite_McpServerSettings_AuthenticationType"), "API key", pauseAfter: 800);
        });

        await scene.StepAsync("srv-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#ISite_McpServerSettings_ApiKey"), "demo-key-not-a-real-secret", delay: 25);
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_McpServerSettings_ApiKey")), "API key");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("srv-4", async () =>
        {
            await scene.ScrollAsync(0, 380);
            await scene.ClickAsync(scene.Page.Locator("label").Filter(new() { HasTextString = "Select All Tools" }).First, pauseAfter: 700);
        });

        await scene.StepAsync("srv-5", async () =>
        {
            await scene.ScrollAsync(0, -330);
            await scene.HighlightAsync(scene.Page.Locator("#mcpServerExposeAllTools").Locator("xpath=ancestor::*[contains(@class,'form-check')][1]"), "Expose all tools");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("srv-6", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1500);
        });
    }

    private static async Task PromptsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/mcp/prompts");
        await scene.StartCaptureAsync();

        await scene.StepAsync("prm-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ai/mcp/prompts"), "Artificial Intelligence > Model Context Protocol > Prompts");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create"));
        });

        await scene.StepAsync("prm-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#McpPrompt_Title"), "Summarize a support ticket", delay: 25);
            await scene.Page.Locator("#McpPrompt_Name").FillAsync(string.Empty);
            await scene.TypeAsync(scene.Page.Locator("#McpPrompt_Name"), "summarize-ticket", delay: 25);
            await scene.Page.Locator("#McpPrompt_Description").FillAsync("Summarizes a support ticket in three bullet points.");
            await scene.PauseAsync(800);
        });

        await scene.StepAsync("prm-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#add-argument"), pauseAfter: 700);
            var argument = scene.Page.Locator("input[name$='.Name'][name^='McpPrompt.Arguments']").First;
            await scene.TypeAsync(argument, "ticket", delay: 45);
            await scene.ClickAsync(scene.Page.Locator("input[type=checkbox][name^='McpPrompt.Arguments']").First, pauseAfter: 500);
            await scene.HighlightAsync(scene.Page.Locator("input[name$='.Name'][name^='McpPrompt.Arguments']").First.Locator("xpath=ancestor::*[contains(@class,'card') or contains(@class,'border')][1]"), "Arguments");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("prm-4", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#add-message"), pauseAfter: 700);
            await scene.TypeAsync(scene.Page.Locator("textarea[name^='McpPrompt.Messages']").First, "Summarize this support ticket in three bullet points: {{ticket}}", delay: 22);
            await scene.HighlightAsync(scene.Page.Locator("textarea[name^='McpPrompt.Messages']").First.Locator("xpath=ancestor::*[contains(@class,'card') or contains(@class,'border')][1]"), "Messages");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("prm-5", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1500);
        });
    }
}
