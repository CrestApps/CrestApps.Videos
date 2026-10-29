using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the AI Documents video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // Recorded in this order: 04 allows what 05 and 06 use.
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-settings", SettingsAsync),
        ("04-profile", ProfileAsync),
        ("05-attach", AttachAsync),
        ("06-figures", FiguresAsync),
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

    private static async Task AttachFileAsync(Scene scene, string file)
    {
        var button = scene.Page.Locator(".ai-chat-doc-add-btn");
        var chooser = await scene.Page.RunAndWaitForFileChooserAsync(() => scene.ClickAsync(button, pauseAfter: 300));
        await chooser.SetFilesAsync(Path.Combine(Root, "seed", file));
    }

    // ----- Clips -----

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/ai-settings");
        await scene.StartCaptureAsync();

        await scene.StepAsync("set-1", async () =>
        {
            await ScrollToAsync(scene, "#heading-card-documents");
            await scene.HighlightAsync(scene.Page.Locator("#heading-card-documents"), "Documents");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Setting(scene, "InteractionDocumentSettings.IndexProfileName")), "Index profile");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("set-2", async () =>
        {
            await scene.HighlightAsync(Section(Setting(scene, "InteractionDocumentSettings.RetrievalMode")), "Document retrieval mode");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("set-3", async () =>
        {
            await scene.HighlightAsync(FormCheck(Setting(scene, "InteractionDocumentSettings.AllowDocumentUploads")), "Allow document uploads in chat interactions");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(FormCheck(Setting(scene, "InteractionDocumentSettings.AllowImageUploads")), "Allow image uploads in chat interactions");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("set-4", async () =>
        {
            await scene.HighlightAsync(Section(Setting(scene, "InteractionDocumentSettings.MaxIndexableCharacters")), "Largest document that can be indexed");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("set-5", async () =>
        {
            await scene.ScrollAsync(0, 200);
            await scene.HighlightAsync(FormCheck(Setting(scene, "InteractionDocumentSettings.DescribeFiguresInUploads")), "Describe figures in uploaded documents");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("set-6", async () =>
        {
            await ScrollToAsync(scene, "[name$='DefaultVisionDeploymentName']");
            await scene.HighlightAsync(Section(Setting(scene, "DefaultVisionDeploymentName")), "Default vision deployment");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ProfileAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.Page.RunAndWaitForNavigationAsync(() => Profile(scene).ClickAsync());
        await KnowledgeTab(scene).ClickAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("pro-1", async () =>
        {
            await ScrollToAsync(scene, "#AIProfile_AllowSessionDocuments");
            await scene.ClickAsync(scene.Page.Locator("label[for='AIProfile_AllowSessionDocuments']"), pauseAfter: 600);
            await scene.HighlightAsync(FormCheck(scene.Page.Locator("#AIProfile_AllowSessionDocuments")), "Allow session document uploads");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("pro-2", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#AIProfile_DescribeFiguresInUploadsSelection"), "Describe figures", pauseAfter: 700);
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIProfile_DescribeFiguresInUploadsSelection")), "Describe figures in uploaded documents");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(800);
        });
    }

    private static async Task AttachAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.Page.Locator("button.dropdown-toggle.actions").First.ClickAsync();
        await scene.Page.RunAndWaitForNavigationAsync(() => scene.Page.Locator("a[href*='/Admin/ai/chat/session/']").Filter(new() { HasTextString = "New chat" }).First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("att-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".ai-chat-doc-add-btn"), "Attach files");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("att-2", async () =>
        {
            await AttachFileAsync(scene, "return-policy.md");
            await scene.PauseAsync(3000);
        });

        await scene.StepAsync("att-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#AIChatSession_UserPrompt"), "Can I return opened software?", delay: 25);
            await scene.ClickAsync(scene.Page.Locator("#AIChatSession_SendButton"), pauseAfter: 400);
        });

        await scene.StepAsync("att-4", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await scene.Page.WaitForFunctionAsync("() => document.querySelectorAll('.ai-chat-message-heading-assistant').length >= 1", null, new() { Timeout = 120_000 });
                await scene.PauseAsync(5000);
            });
            await scene.PauseAsync(3000);
        });
    }

    private static async Task FiguresAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.Page.Locator("button.dropdown-toggle.actions").First.ClickAsync();
        await scene.Page.RunAndWaitForNavigationAsync(() => scene.Page.Locator("a[href*='/Admin/ai/chat/session/']").Filter(new() { HasTextString = "New chat" }).First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("fig-1", async () =>
        {
            await AttachFileAsync(scene, "annual-sales-review.pdf");
            await scene.OffCameraAsync(async () => await scene.PauseAsync(10_000));
            await scene.PauseAsync(4500);
        });

        await scene.StepAsync("fig-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#AIChatSession_UserPrompt"), "Which quarter sold the most units?", delay: 10);
            await scene.ClickAsync(scene.Page.Locator("#AIChatSession_SendButton"), pauseAfter: 400);
        });

        await scene.StepAsync("fig-3", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await scene.Page.WaitForFunctionAsync("() => document.querySelectorAll('.ai-chat-message-heading-assistant').length >= 1", null, new() { Timeout = 120_000 });
                await scene.PauseAsync(5000);
            });
            await scene.PauseAsync(3500);
        });
    }
}
