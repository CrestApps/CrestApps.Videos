using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the External Data Sources video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // Recorded in this order: 03 creates the data source that 04 asks about.
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-elastic", ElasticAsync),
        ("04-ask", AskAsync),
        ("05-azure", AzureAsync),
        ("06-postgres", PostgresAsync),
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
            "CrestApps.OrchardCore.AI.DataSources.AzureAI",
            "CrestApps.OrchardCore.AI.DataSources.PostgreSQL",
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

    private static ILocator Fieldset(Scene scene, string legend) => scene.Page.Locator("legend").Filter(new() { HasTextString = legend }).First.Locator("xpath=..");

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static ILocator CardOf(ILocator link) => link.Locator("xpath=ancestor::*[contains(concat(' ',normalize-space(@class),' '),' card ')][1]");

    private static Task ScrollToAsync(Scene scene, string selector)
        => scene.Page.EvaluateAsync("(s) => { const h = document.querySelector(s); window.scrollTo(0, h.getBoundingClientRect().top + window.scrollY - 90); }", selector);

    private static async Task OpenTypeAsync(Scene scene, string type, string caption)
    {
        await scene.ClickAsync(scene.Page.Locator("button.create, a.create").First, pauseAfter: 900);
        await scene.HighlightAsync(CardOf(scene.Page.Locator($"a[href$='/data-source/create/{type}']")), caption);
        await scene.PauseAsync(1800);
        await scene.ClearHighlightsAsync();
        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator($"a[href$='/data-source/create/{type}']"));
    }

    // ----- Clips -----

    private static async Task ElasticAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/data-sources");
        await scene.StartCaptureAsync();

        await scene.StepAsync("es-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ai/data-sources"), "Artificial Intelligence > Data Sources");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await OpenTypeAsync(scene, "Elasticsearch", "Elasticsearch");
        });

        await scene.StepAsync("es-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#AIDataSource_DisplayText"), "Support notes", delay: 30);
            await scene.TypeAsync(scene.Page.Locator("#AIDataSource_IndexName"), "support-notes", delay: 30);
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIDataSource_IndexName")), "Index name");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("es-3", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIDataSource_EnvironmentType")), "Environment type");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.Page.Locator("#AIDataSource_Url").FillAsync("http://localhost:9200");
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIDataSource_Url")), "Elasticsearch URL");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIDataSource_AuthenticationType")), "Authentication type");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("es-4", async () =>
        {
            await ScrollToAsync(scene, "#AIDataSource_AIKnowledgeBaseIndexProfileName");
            await scene.SelectAsync(scene.Page.Locator("#AIDataSource_AIKnowledgeBaseIndexProfileName"), "Contoso knowledge", pauseAfter: 900);
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIDataSource_AIKnowledgeBaseIndexProfileName")), "Destination index");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("es-5", async () =>
        {
            await scene.ScrollAsync(0, 350);
            await scene.Page.Locator("#AIDataSource_ContentFieldName").FillAsync("body");
            await scene.PauseAsync(500);
            await scene.Page.Locator("#AIDataSource_TitleFieldName").FillAsync("title");
            await scene.PauseAsync(500);
            await scene.Page.Locator("#AIDataSource_KeyFieldName").FillAsync("id");
            await scene.HighlightAsync(Fieldset(scene, "Field Mapping"), "Field Mapping");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("es-6", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(2000);
        });
    }

    private static async Task AskAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        // The data source reads the remote index in the background: give it time before the clip.
        await Task.Delay(1_000);

        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.Page.RunAndWaitForNavigationAsync(() => scene.Page.Locator("a[href*='/Admin/ai/profile/edit/']").Filter(new() { HasTextString = "Support assistant" }).First.ClickAsync());
        await scene.Page.Locator(".nav-tabs a, .nav-tabs button").Filter(new() { HasTextString = "Knowledge" }).First.ClickAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("ask-1", async () =>
        {
            await ScrollToAsync(scene, "#AIProfile_DataSourceId");
            await scene.SelectAsync(scene.Page.Locator("#AIProfile_DataSourceId"), "Support notes", pauseAfter: 900);
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIProfile_DataSourceId")), "Data source");
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.StepAsync("ask-2", async () =>
        {
            await scene.Page.Locator("button.dropdown-toggle.actions").First.ClickAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='/Admin/ai/chat/session/']").Filter(new() { HasTextString = "New chat" }).First);
            await scene.TypeAsync(scene.Page.Locator("#AIChatSession_UserPrompt"), "How do I reset the router?", delay: 10);
            await scene.ClickAsync(scene.Page.Locator("#AIChatSession_SendButton"), pauseAfter: 400);
        });

        await scene.StepAsync("ask-3", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                await scene.Page.WaitForFunctionAsync("() => document.querySelectorAll('.ai-chat-message-heading-assistant').length >= 1", null, new() { Timeout = 120_000 });
                await scene.PauseAsync(6000);
            });
            await scene.PauseAsync(3500);
        });
    }

    private static async Task AzureAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/data-sources");
        await scene.StartCaptureAsync();

        await scene.StepAsync("az-1", async () =>
        {
            await OpenTypeAsync(scene, "AzureAISearch", "Azure AI Search");
            await scene.TypeAsync(scene.Page.Locator("#AIDataSource_DisplayText"), "Manuals", delay: 35);
            await scene.Page.Locator("#AIDataSource_IndexName").FillAsync("product-manuals");
            await scene.Page.Locator("#AIDataSource_Endpoint").FillAsync("https://contoso-search.search.windows.net");
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIDataSource_Endpoint")), "Endpoint");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("az-2", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIDataSource_AuthenticationType")), "Authentication");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(scene.Page.Locator("#AIDataSource_AuthenticationType"), "ApiKey", pauseAfter: 900);
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIDataSource_ApiKey")), "API key");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("az-3", async () =>
        {
            await ScrollToAsync(scene, "#AIDataSource_AIKnowledgeBaseIndexProfileName");
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIDataSource_AIKnowledgeBaseIndexProfileName")), "Destination index");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 350);
            await scene.HighlightAsync(Fieldset(scene, "Field Mapping"), "Field Mapping");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PostgresAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/data-sources");
        await scene.StartCaptureAsync();

        await scene.StepAsync("pg-1", async () =>
        {
            await OpenTypeAsync(scene, "PostgreSQL", "PostgreSQL");
            await scene.TypeAsync(scene.Page.Locator("#AIDataSource_DisplayText"), "Articles table", delay: 35);
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIDataSource_ConnectionString")), "Connection string");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("pg-2", async () =>
        {
            await scene.Page.Locator("#AIDataSource_TableName").FillAsync("articles");
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIDataSource_TableName")), "Table name");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, "#AIDataSource_ContentFieldName");
            await scene.HighlightAsync(Fieldset(scene, "Field Mapping"), "Field Mapping");
        });

        await scene.ClearHighlightsAsync();
    }
}
