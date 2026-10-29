using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the AI Knowledge video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // Recorded in this order: 03 creates the index that 04 fills, and 05 asks about what 04 uploaded.
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-index", IndexAsync),
        ("04-upload", UploadAsync),
        ("05-ask", AskAsync),
        ("06-attach", AttachAsync),
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

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static ILocator FormCheck(ILocator box) => box.Locator("xpath=ancestor::*[contains(@class,'form-check')][1]");

    private static Task ScrollToAsync(Scene scene, string selector)
        => scene.Page.EvaluateAsync("(s) => { const h = document.querySelector(s); window.scrollTo(0, h.getBoundingClientRect().top + window.scrollY - 90); }", selector);

    private static ILocator KnowledgeTab(Scene scene) => scene.Page.Locator(".nav-tabs a, .nav-tabs button").Filter(new() { HasTextString = "Knowledge" }).First;

    private static ILocator Profile(Scene scene) => scene.Page.Locator("a[href*='/Admin/ai/profile/edit/']").Filter(new() { HasTextString = "Support assistant" }).First;

    // ----- Clips -----

    private static async Task IndexAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/indexing");
        await scene.StartCaptureAsync();

        await scene.StepAsync("idx-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/indexing"), "Search > Indexes");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button, a").Filter(new() { HasTextString = "Add index" }).First, pauseAfter: 900);
            await scene.HighlightAsync(scene.Page.Locator("a[href$='/create/Elasticsearch/AIDocuments']").Locator("xpath=ancestor::*[contains(concat(' ',normalize-space(@class),' '),' card ')][1]"), "AI Documents (Elasticsearch)");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/create/Elasticsearch/AIDocuments']"));
        });

        await scene.StepAsync("idx-2", async () =>
        {
            await scene.Page.Locator("#IndexProfile_IndexName").FillAsync("documents");
            await scene.PauseAsync(500);
            await scene.Page.Locator("#IndexProfile_Name").FillAsync("Contoso documents");
            await scene.PauseAsync(500);
            await scene.PauseAsync(1500);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.StepAsync("idx-3", async () =>
        {
            await scene.NavigateAsync("/Admin/Settings/ai-settings");
            await ScrollToAsync(scene, "#heading-card-documents");
            await scene.HighlightAsync(scene.Page.Locator("#heading-card-documents"), "Documents");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(scene.Page.Locator("[name$='InteractionDocumentSettings.IndexProfileName']"), "Contoso documents", pauseAfter: 700);
            await scene.HighlightAsync(Section(scene.Page.Locator("[name$='InteractionDocumentSettings.IndexProfileName']")), "Index profile");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("idx-4", async () =>
        {
            await scene.PauseAsync(1500);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1200);
        });
    }

    private static async Task UploadAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.StartCaptureAsync();

        await scene.StepAsync("upl-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ai/profiles"), "Artificial Intelligence > Profiles");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Profile(scene));
        });

        await scene.StepAsync("upl-2", async () =>
        {
            await scene.ClickAsync(KnowledgeTab(scene), pauseAfter: 800);
            var browse = scene.Page.Locator("button").Filter(new() { HasTextString = "Browse Files" }).First;
            var chooser = await scene.Page.RunAndWaitForFileChooserAsync(() => scene.ClickAsync(browse, pauseAfter: 300));
            await chooser.SetFilesAsync(Path.Combine(Root, "seed", "contoso-return-policy.md"));
            await scene.PauseAsync(900);
            await scene.ScrollAsync(0, 450);
            await scene.HighlightAsync(scene.Page.Locator("[data-pending-name]").First, "Pending");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("upl-3", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("input[name='AIProfile.TopN']")), "Document Top N");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("upl-4", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIProfile_DocumentRetrievalMode")), "Document retrieval mode");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("upl-5", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1500);
        });
    }

    private static async Task AskAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ask-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.dropdown-toggle.actions").First, pauseAfter: 900);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='/Admin/ai/chat/session/']").Filter(new() { HasTextString = "New chat" }).First);
        });

        await scene.StepAsync("ask-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#AIChatSession_UserPrompt"), "Within how many business days does Contoso refund a returned item?", delay: 18);
            await scene.ClickAsync(scene.Page.Locator("#AIChatSession_SendButton"), pauseAfter: 400);
        });

        await scene.StepAsync("ask-3", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await scene.Page.WaitForFunctionAsync("() => document.querySelectorAll('.ai-chat-message-heading-assistant').length >= 1", null, new() { Timeout = 120_000 });
                await scene.PauseAsync(5000);
            });
            await scene.PauseAsync(2000);
        });

        await scene.StepAsync("ask-4", async () =>
        {
            await scene.Page.EvaluateAsync("() => { const l = document.querySelector('.ai-chat-messages'); if (l) l.scrollTop = l.scrollHeight; }");
            await scene.PauseAsync(2500);
        });
    }

    private static async Task AttachAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.Page.RunAndWaitForNavigationAsync(() => Profile(scene).ClickAsync());
        await KnowledgeTab(scene).ClickAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("att-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("label[for='AIProfile_AllowSessionDocuments']"), pauseAfter: 600);
            await scene.HighlightAsync(FormCheck(scene.Page.Locator("#AIProfile_AllowSessionDocuments")), "Allow session document uploads");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(FormCheck(scene.Page.Locator("#AIProfile_AllowSessionImageUploads")), "Allow session image uploads");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("att-2", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIProfile_MaxIndexableCharacters")), "Largest document that can be indexed");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("att-3", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIProfile_DescribeFiguresInUploadsSelection")), "Describe figures in uploaded documents");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("att-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1500);
        });
    }
}
