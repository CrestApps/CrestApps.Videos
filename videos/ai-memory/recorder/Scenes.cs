using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the AI Memory video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // Recorded in this order: each clip builds on the one before (the index, the settings, the profile, the memory).
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-index", IndexAsync),
        ("04-settings", SettingsAsync),
        ("05-profile", ProfileAsync),
        ("06-chat", ChatAsync),
        ("07-clear", ClearAsync),
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
            "OrchardCore.Search.Elasticsearch",
            "CrestApps.OrchardCore.AI.Memory.Elasticsearch",
            "CrestApps.OrchardCore.AI.Chat",
            "CrestApps.OrchardCore.AI.Chat.Interactions",
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

    private static Task ScrollToAsync(Scene scene, string selector)
        => scene.Page.EvaluateAsync("(s) => { const h = document.querySelector(s); window.scrollTo(0, h.getBoundingClientRect().top + window.scrollY - 90); }", selector);

    // Waits, off camera, until the assistant has answered the given number of messages.
    private static Task WaitForAnswerAsync(Scene scene, int answers)
        => scene.OffCameraAsync(async () =>
        {
            await scene.Page.WaitForFunctionAsync("n => document.querySelectorAll('.ai-chat-message-heading-assistant').length >= n", answers, new() { Timeout = 120_000 });
            await scene.PauseAsync(4000);
        });

    private static async Task SendAsync(Scene scene, string text)
    {
        await scene.TypeAsync(scene.Page.Locator("#AIChatSession_UserPrompt"), text, delay: 18);
        await scene.ClickAsync(scene.Page.Locator("#AIChatSession_SendButton"), pauseAfter: 400);
    }

    // ----- Clips -----

    private static async Task IndexAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/indexing");
        await scene.StartCaptureAsync();

        await scene.StepAsync("idx-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/indexing"), "Search > Indexes");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button, a").Filter(new() { HasTextString = "Add index" }).First, pauseAfter: 700);
        });

        await scene.StepAsync("idx-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("a[href$='/create/Elasticsearch/AIMemory']").Locator("xpath=ancestor::*[contains(concat(' ',normalize-space(@class),' '),' card ')][1]"), "AI Memory (Elasticsearch)");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/create/Elasticsearch/AIMemory']"));
        });

        await scene.StepAsync("idx-3", async () =>
        {
            await scene.Page.Locator("#IndexProfile_IndexName").FillAsync("memory");
            await scene.PauseAsync(900);
            await scene.Page.Locator("#IndexProfile_Name").FillAsync("Contoso memory");
            await scene.PauseAsync(900);
        });

        await scene.StepAsync("idx-4", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#IndexProfile_EmbeddingDeploymentName"), "contoso-embedding", pauseAfter: 900);
            await scene.HighlightAsync(Section(scene.Page.Locator("#IndexProfile_EmbeddingDeploymentName")), "Embedding deployment");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("idx-5", async () =>
        {
            await scene.ScrollAsync(0, 400);
            await scene.PauseAsync(500);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1500);
        });
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/ai-settings");
        await ScrollToAsync(scene, "#heading-card-memory");
        await scene.StartCaptureAsync();

        await scene.StepAsync("set-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/ai-settings"), "Settings > Artificial Intelligence");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#heading-card-memory"), "Memory");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("set-2", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#ISite_AIMemorySettings_IndexProfileName"), "Contoso memory", pauseAfter: 900);
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_AIMemorySettings_IndexProfileName")), "Index profile");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("set-3", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_AIMemorySettings_TopN")), "Default top N");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("set-4", async () =>
        {
            await scene.Page.EvaluateAsync("() => window.scrollTo(0, 0)");
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.Locator("#ISite_GeneralAISettings_EnablePreemptiveMemoryRetrieval").Locator("xpath=ancestor::*[contains(@class,'form-check')][1]"), "Enable preemptive memory retrieval");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("set-5", async () =>
        {
            await ScrollToAsync(scene, "#heading-card-memory");
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.Locator("#ISite_MemoryMetadata_EnableUserMemory").Locator("xpath=ancestor::*[contains(@class,'form-check')][1]"), "Enable user memory");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1200);
        });
    }

    private static async Task ProfileAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.StartCaptureAsync();

        await scene.StepAsync("pro-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ai/profiles"), "Artificial Intelligence > Profiles");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='/Admin/ai/profile/edit/']").Filter(new() { HasTextString = "Support assistant" }).First);
        });

        await scene.StepAsync("pro-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".nav-tabs a, .nav-tabs button").Filter(new() { HasTextString = "Knowledge" }).First, pauseAfter: 900);
            await scene.ClickAsync(scene.Page.Locator("label[for='AIProfile_EnableUserMemory']"), pauseAfter: 700);
            await scene.HighlightAsync(scene.Page.Locator("#AIProfile_EnableUserMemory").Locator("xpath=ancestor::*[contains(@class,'form-check')][1]"), "Enable user memory");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("pro-3", async () =>
        {
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("pro-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1500);
        });
    }

    private static async Task ChatAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.StartCaptureAsync();

        await scene.StepAsync("cha-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.dropdown-toggle.actions").First, pauseAfter: 900);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='/Admin/ai/chat/session/']").Filter(new() { HasTextString = "New chat" }).First);
        });

        await scene.StepAsync("cha-2", async () =>
        {
            await SendAsync(scene, "Call me Sam, and keep your answers short.");
        });

        await scene.StepAsync("cha-3", async () =>
        {
            await WaitForAnswerAsync(scene, 1);
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("cha-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a, button").Filter(new() { HasTextString = "New Chat" }).First);
        });

        await scene.StepAsync("cha-5", async () =>
        {
            await SendAsync(scene, "What do you remember about me?");
            await WaitForAnswerAsync(scene, 1);
            await scene.PauseAsync(2500);
        });
    }

    private static async Task ClearAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("clr-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#navbarDropdown"), pauseAfter: 900);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href='/Admin/Users/Edit']").First);
        });

        await scene.StepAsync("clr-2", async () =>
        {
            await ScrollToAsync(scene, "fieldset legend");
            await scene.HighlightAsync(scene.Page.Locator("fieldset").Filter(new() { HasTextString = "AI Memory" }).First, "AI Memory");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("clr-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("a.btn-danger").Filter(new() { HasTextString = "Clear saved AI memory" }), pauseAfter: 1500);
            await scene.ClickAsync(scene.Page.Locator(".modal.show button, .modal.show a").Filter(new() { HasTextString = "Clear" }).Last, pauseAfter: 1000);
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("clr-4", async () =>
        {
            await ScrollToAsync(scene, "fieldset legend");
            await scene.HighlightAsync(scene.Page.Locator("fieldset").Filter(new() { HasTextString = "AI Memory" }).First, "AI Memory");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });
    }
}
