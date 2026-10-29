using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Build an AI Knowledge Assistant video, one method per "clip" of storyboard.json.
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
        ("03-upload", UploadAsync),
        ("04-connect", ConnectAsync),
        ("05-test", TestAsync),
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
            "CrestApps.OrchardCore.AI.DataSources.Elasticsearch",
            "CrestApps.OrchardCore.AI.Documents.Elasticsearch",
            "CrestApps.OrchardCore.AI.Documents.Profiles",
            "CrestApps.OrchardCore.AI.Documents.ChatSessions",
            "CrestApps.OrchardCore.AI.Documents.OpenXml",
            "CrestApps.OrchardCore.AI.Documents.Pdf",
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

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static ILocator FormCheck(ILocator box) => box.Locator("xpath=ancestor::*[contains(@class,'form-check')][1]");

    private static ILocator KnowledgeTab(Scene scene) => scene.Page.Locator(".nav-tabs a, .nav-tabs button").Filter(new() { HasTextString = "Knowledge" }).First;

    private static ILocator Profile(Scene scene) => scene.Page.Locator("a[href*='/Admin/ai/profile/edit/']").Filter(new() { HasTextString = "Support assistant" }).First;

    private static ILocator Setting(Scene scene, string name) => scene.Page.Locator($"[name$='{name}']").First;

    private static Task ScrollToAsync(Scene scene, string selector)
        => scene.Page.EvaluateAsync("(s) => { const h = document.querySelector(s); window.scrollTo(0, h.getBoundingClientRect().top + window.scrollY - 90); }", selector);

    // ----- Clips -----

    private static async Task UploadAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.Page.RunAndWaitForNavigationAsync(() => Profile(scene).ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("up-1", async () =>
        {
            await scene.ClickAsync(KnowledgeTab(scene), pauseAfter: 800);
            var browse = scene.Page.Locator("button").Filter(new() { HasTextString = "Browse Files" }).First;
            var chooser = await scene.Page.RunAndWaitForFileChooserAsync(() => scene.ClickAsync(browse, pauseAfter: 300));
            await chooser.SetFilesAsync(Path.Combine(Root, "seed", "return-policy.md"));
            await scene.PauseAsync(300);
            await scene.ScrollAsync(0, 450, steps: 6);
            await scene.HighlightAsync(scene.Page.Locator("[data-pending-name]").First, "Pending");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("up-2", async () =>
        {
            await scene.PauseAsync(1500);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1200);
        });
    }

    private static async Task ConnectAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.Page.RunAndWaitForNavigationAsync(() => Profile(scene).ClickAsync());
        await KnowledgeTab(scene).ClickAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("con-1", async () =>
        {
            await ScrollToAsync(scene, "#AIProfile_DataSourceId");
            await scene.SelectAsync(scene.Page.Locator("#AIProfile_DataSourceId"), "Help articles", pauseAfter: 900);
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIProfile_DataSourceId")), "Data source");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("con-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("label[for='AIProfile_IsInScope']"), pauseAfter: 600);
            await scene.HighlightAsync(FormCheck(scene.Page.Locator("#AIProfile_IsInScope")), "Restrict answers to retrieved data only");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("con-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1500);
        });
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
            await scene.PauseAsync(5000);
        });
        await scene.Page.EvaluateAsync("() => { const l = document.querySelector('.ai-chat-messages'); if (l) l.scrollTop = l.scrollHeight; }");
    }

    private static async Task TestAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.Page.Locator("button.dropdown-toggle.actions").First.ClickAsync();
        await scene.Page.RunAndWaitForNavigationAsync(() => scene.Page.Locator("a[href*='/Admin/ai/chat/session/']").Filter(new() { HasTextString = "New chat" }).First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("tst-1", async () => await AskAsync(scene, "Can I return opened software?"));

        await scene.StepAsync("tst-2", async () =>
        {
            await WaitAnswerAsync(scene, 1);
            await scene.PauseAsync(2500);
        });

        await scene.StepAsync("tst-3", async () => await AskAsync(scene, "How long does express shipping take?"));

        await scene.StepAsync("tst-4", async () =>
        {
            await WaitAnswerAsync(scene, 2);
            await scene.PauseAsync(3500);
        });

        await scene.StepAsync("tst-5", async () => await AskAsync(scene, "What is the favorite color of the CEO?"));

        await scene.StepAsync("tst-6", async () =>
        {
            await WaitAnswerAsync(scene, 3);
            await scene.PauseAsync(3500);
        });
    }
}
