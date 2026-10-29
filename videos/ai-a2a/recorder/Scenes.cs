using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Agent-to-Agent (A2A) video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // Recorded in this order: 03 creates the agent that 07 asks for, and 05 the connection that 06 ticks.
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-agent", AgentAsync),
        ("05-connection", ConnectionAsync),
        ("06-use", UseAsync),
        ("07-ask", AskAsync),
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
            "CrestApps.OrchardCore.AI.A2A",
            "CrestApps.OrchardCore.AI.A2A.Host",
            "CrestApps.OrchardCore.AI.Agent",
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

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static ILocator FormCheck(ILocator box) => box.Locator("xpath=ancestor::*[contains(@class,'form-check')][1]");

    // ----- Clips -----

    private static async Task AgentAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ag-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ai/profiles"), "Artificial Intelligence > Profiles");
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button, a").Filter(new() { HasTextString = "Add Profile" }).First, pauseAfter: 1000);
            await scene.PauseAsync(1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/ai/profile/create']").First);
        });

        await scene.StepAsync("ag-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#AIProfile_DisplayText"), "Warranty Agent", delay: 30);
            await scene.SelectAsync(scene.Page.Locator("#AIProfile_ProfileType"), "Agent", pauseAfter: 900);
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIProfile_ProfileType")), "Profile type");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ag-3", async () =>
        {
            await scene.Page.Locator("#AIProfile_Description").FillAsync("Answers questions about the Contoso product warranty: what it covers, how long it lasts, and how to make a claim.");
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIProfile_Description")), "Description");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ag-4", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIProfile_AgentAvailability")), "Availability");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ag-5", async () =>
        {
            await scene.Page.EvaluateAsync("() => { const h = document.querySelector('#AIProfile_SystemMessage').parentElement; window.scrollTo(0, h.getBoundingClientRect().top + window.scrollY - 160); }");
            await scene.Page.EvaluateAsync("(text) => { const cm = document.querySelector('#AIProfile_SystemMessage').parentElement.querySelector('.CodeMirror').CodeMirror; cm.setValue(text); }", "You are the Contoso warranty specialist. Every Contoso product has a two-year limited warranty that covers defects in materials and workmanship. A claim is started from the order in the customer account, and the original receipt is needed. Accidental damage, water damage and normal wear are not covered. Answer briefly.");
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIProfile_SystemMessage")), "System instructions");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1500);
        });
    }

    private static async Task ConnectionAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/a2a/connections");
        await scene.StartCaptureAsync();

        await scene.StepAsync("con-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ai/a2a/connections"), "Artificial Intelligence > Agent to Agent Hosts");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create, button.create").First);
        });

        await scene.StepAsync("con-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#A2AConnection_DisplayText"), "Contoso agent host", delay: 30);
            await scene.TypeAsync(scene.Page.Locator("#A2AConnection_Endpoint"), "http://localhost:5372/a2a", delay: 30);
            await scene.HighlightAsync(Section(scene.Page.Locator("#A2AConnection_Endpoint")), "Endpoint");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("con-3", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#A2AConnection_AuthenticationType")), "Authentication");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("con-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1500);
        });
    }

    private static async Task UseAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.Page.RunAndWaitForNavigationAsync(() => scene.Page.Locator("a[href*='/Admin/ai/profile/edit/']").Filter(new() { HasTextString = "Support assistant" }).First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("use-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".nav-tabs a, .nav-tabs button").Filter(new() { HasTextString = "Capabilities" }).First, pauseAfter: 900);
            var connections = scene.Page.Locator("text=A2A Connections").First.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");
            await scene.ClickAsync(connections.Locator("label").Filter(new() { HasTextString = "Contoso agent host" }).First, pauseAfter: 700);
            await scene.HighlightAsync(connections, "A2A Connections");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("use-2", async () =>
        {
            var agents = scene.Page.Locator("text=Agents").Filter(new() { HasTextString = "always-available" }).First;
            await scene.HighlightAsync(scene.Page.Locator("text=Select All Agents").First.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]"), "Agents");
            await scene.PauseAsync(3000);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("use-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1500);
        });
    }

    private static async Task AskAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.Page.Locator("button.dropdown-toggle.actions").First.ClickAsync();
        await scene.Page.RunAndWaitForNavigationAsync(() => scene.Page.Locator("a[href*='/Admin/ai/chat/session/']").Filter(new() { HasTextString = "New chat" }).First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("ask-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#AIChatSession_UserPrompt"), "What does the Contoso warranty cover?", delay: 18);
            await scene.ClickAsync(scene.Page.Locator("#AIChatSession_SendButton"), pauseAfter: 400);
        });

        await scene.StepAsync("ask-2", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await scene.Page.WaitForFunctionAsync("() => document.querySelectorAll('.ai-chat-message-heading-assistant').length >= 1", null, new() { Timeout = 120_000 });
                await scene.PauseAsync(8000);
            });
            await scene.PauseAsync(3500);
        });
    }
}
