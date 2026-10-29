using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the AI File Sources video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // Recorded in this order: each clip builds on the one before (the index, the data source, the file source, its run).
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-index", IndexAsync),
        ("04-datasource", DataSourceAsync),
        ("05-source", SourceAsync),
        ("06-run", RunAsync),
        ("07-remote", RemoteAsync),
        ("08-use", UseAsync),
        ("09-ask", AskAsync),
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
            "CrestApps.OrchardCore.AI.FileSources",
            "CrestApps.OrchardCore.AI.FileSources.Ftp",
            "CrestApps.OrchardCore.AI.FileSources.Sftp",
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

    private static ILocator CardOf(ILocator link) => link.Locator("xpath=ancestor::*[contains(concat(' ',normalize-space(@class),' '),' card ')][1]");

    private static Task ScrollToAsync(Scene scene, string selector)
        => scene.Page.EvaluateAsync("(s) => { const h = document.querySelector(s); window.scrollTo(0, h.getBoundingClientRect().top + window.scrollY - 90); }", selector);

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
            await scene.HighlightAsync(CardOf(scene.Page.Locator("a[href$='/create/Elasticsearch/DataSourceIndex']")), "AI Knowledge Base Index");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/create/Elasticsearch/DataSourceIndex']"));
        });

        await scene.StepAsync("idx-2", async () =>
        {
            await scene.Page.Locator("#IndexProfile_IndexName").FillAsync("knowledge");
            await scene.PauseAsync(500);
            await scene.Page.Locator("#IndexProfile_Name").FillAsync("Contoso knowledge");
            await scene.PauseAsync(500);
            await scene.SelectAsync(scene.Page.Locator("#IndexProfile_EmbeddingDeploymentName"), "contoso-embedding", pauseAfter: 700);
            await scene.HighlightAsync(Section(scene.Page.Locator("#IndexProfile_EmbeddingDeploymentName")), "Embedding deployment");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("idx-3", async () =>
        {
            await scene.ScrollAsync(0, 400);
            await scene.PauseAsync(500);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(2500);
        });
    }

    private static async Task DataSourceAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/data-sources");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ds-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ai/data-sources"), "Artificial Intelligence > Data Sources");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button.create, a.create").First, pauseAfter: 900);
        });

        await scene.StepAsync("ds-2", async () =>
        {
            await scene.HighlightAsync(CardOf(scene.Page.Locator("a[href$='/data-source/create/File']")), "Files");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/data-source/create/File']"));
        });

        await scene.StepAsync("ds-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#AIDataSource_DisplayText"), "Staff handbook", delay: 30);
            await scene.SelectAsync(scene.Page.Locator("#AIDataSource_AIKnowledgeBaseIndexProfileName"), "Contoso knowledge", pauseAfter: 700);
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIDataSource_AIKnowledgeBaseIndexProfileName")), "Destination index");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ds-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1500);
        });
    }

    private static async Task SourceAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/file-sources");
        await scene.StartCaptureAsync();

        await scene.StepAsync("src-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ai/file-sources"), "Artificial Intelligence > File Sources");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button.create, a.create").First, pauseAfter: 900);
        });

        await scene.StepAsync("src-2", async () =>
        {
            await scene.HighlightAsync(CardOf(scene.Page.Locator("a[href$='/file-source/create/FileSystem']")), "File system");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/file-source/create/FileSystem']"));
        });

        await scene.StepAsync("src-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#FileSource_DisplayText"), "Handbooks folder", delay: 30);
            await scene.SelectAsync(scene.Page.Locator("#FileSource_AIDataSourceId"), "Staff handbook", pauseAfter: 700);
            await scene.HighlightAsync(Section(scene.Page.Locator("#FileSource_AIDataSourceId")), "Target File data source");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("src-4", async () =>
        {
            await scene.HighlightAsync(FormCheck(scene.Page.Locator("#FileSource_Enabled")), "Enabled");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#FileSource_ReindexIntervalMinutes"), "60", delay: 80);
            await scene.HighlightAsync(Section(scene.Page.Locator("#FileSource_ReindexIntervalMinutes")), "Re-read interval (minutes)");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("src-5", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#FileSource_FigureMode")), "Figures");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("src-6", async () =>
        {
            await scene.ScrollAsync(0, 700);
            await scene.TypeAsync(scene.Page.Locator("#FileSource_RootPath"), "handbooks", delay: 40);
            await scene.ClickAsync(scene.Page.Locator("label[for='FileSource_Recursive']"), pauseAfter: 500);
            await scene.HighlightAsync(Section(scene.Page.Locator("#FileSource_RootPath")), "Folder");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("src-7", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(2000);
        });
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/file-sources");
        await scene.StartCaptureAsync();

        await scene.StepAsync("run-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.dropdown-toggle.actions").First, pauseAfter: 1200);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='/file-source/run/']").First);
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                for (var attempt = 0; attempt < 40; attempt++)
                {
                    await scene.Page.ReloadAsync();

                    if (await scene.Page.GetByText("Succeeded").CountAsync() > 0)
                    {
                        break;
                    }

                    await scene.PauseAsync(3000);
                }
            });
            await scene.HighlightAsync(scene.Page.GetByText("found,").First, "Last run");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("run-3", async () =>
        {
            await scene.PauseAsync(2500);
        });
    }

    private static async Task RemoteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/file-sources");
        await scene.StartCaptureAsync();

        await scene.StepAsync("rem-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.create, a.create").First, pauseAfter: 1200);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/file-source/create/Sftp']"));
            await scene.Page.Locator("#FileSource_DisplayText").FillAsync("Partner exports");
            await scene.ScrollAsync(0, 750);
            await scene.Page.Locator("#FileSource_Host").FillAsync("files.partner.example");
            await scene.PauseAsync(500);
            await scene.Page.Locator("#FileSource_Username").FillAsync("contoso");
        });

        await scene.StepAsync("rem-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#FileSource_Password"), "demo-password", delay: 40);
            await scene.ScrollAsync(0, 250);
            await scene.HighlightAsync(Section(scene.Page.Locator("#FileSource_PrivateKey")), "Private key");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rem-3", async () =>
        {
            await scene.ScrollAsync(0, 350);
            await scene.HighlightAsync(Section(scene.Page.Locator("#FileSource_RemoteRootPath")), "Folder");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(scene.Page.Locator("#FileSource_RemoteMaxItems")), "Files per listing");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rem-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a").Filter(new() { HasTextString = "Cancel" }).Last);
            await scene.ClickAsync(scene.Page.Locator("button.create, a.create").First, pauseAfter: 1200);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/file-source/create/Ftp']"));
            await scene.ScrollAsync(0, 900);
            await scene.HighlightAsync(Section(scene.Page.Locator("#FileSource_EncryptionMode")), "Encryption mode");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(scene.Page.Locator("#FileSource_DataConnectionType")), "Data connection");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task UseAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.Page.RunAndWaitForNavigationAsync(() => scene.Page.Locator("a[href*='/Admin/ai/profile/edit/']").Filter(new() { HasTextString = "Support assistant" }).First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("use-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".nav-tabs a, .nav-tabs button").Filter(new() { HasTextString = "Knowledge" }).First, pauseAfter: 800);
            await ScrollToAsync(scene, "#AIProfile_DataSourceId");
            await scene.SelectAsync(scene.Page.Locator("#AIProfile_DataSourceId"), "Staff handbook", pauseAfter: 900);
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIProfile_DataSourceId")), "Data source");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("use-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("label[for='AIProfile_IsInScope']"), pauseAfter: 600);
            await scene.HighlightAsync(FormCheck(scene.Page.Locator("#AIProfile_IsInScope")), "Restrict answers to retrieved data only");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("use-3", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIProfile_Strictness")), "Strictness");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIProfile_TopNDocuments")), "Retrieved documents");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1200);
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
            await scene.TypeAsync(scene.Page.Locator("#AIChatSession_UserPrompt"), "How many days of annual leave does a full-time employee get?", delay: 18);
            await scene.ClickAsync(scene.Page.Locator("#AIChatSession_SendButton"), pauseAfter: 400);
        });

        await scene.StepAsync("ask-2", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await scene.Page.WaitForFunctionAsync("() => document.querySelectorAll('.ai-chat-message-heading-assistant').length >= 1", null, new() { Timeout = 120_000 });
                await scene.PauseAsync(6000);
            });
            await scene.PauseAsync(2500);
        });

        await scene.StepAsync("ask-3", async () =>
        {
            await scene.Page.EvaluateAsync("() => { const l = document.querySelector('.ai-chat-messages'); if (l) l.scrollTop = l.scrollHeight; }");
            await scene.PauseAsync(3500);
        });
    }
}
