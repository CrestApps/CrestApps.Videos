using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the AI Profiles, Personas and Templates video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-start", StartAsync),
        ("04-editor", EditorAsync),
        ("05-manage", ManageAsync),
        ("06-personas", PersonasAsync),
        ("07-templates", TemplatesAsync),
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
    /// Enables the features, and adds one made-up connection and chat deployment (not recorded).
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
            "CrestApps.OrchardCore.AI.Chat",
            "CrestApps.OrchardCore.AI.Personas",
            "CrestApps.OrchardCore.AI.Prompting",
            "CrestApps.OrchardCore.AI.Agent",
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

        // A made-up connection and deployment, so the profile pages have a model to point at. Nothing contacts it.
        await page.GotoAsync(BaseUrl + "/Admin/ai/provider/connections");

        if (await page.Locator("text=Contoso Azure OpenAI").CountAsync() == 0)
        {
            await page.GotoAsync(BaseUrl + "/Admin/ai/provider/connection/create/Azure");
            await page.Locator("#AIProviderConnection_DisplayText").FillAsync("Contoso Azure OpenAI");
            await page.Locator("#AIProviderConnection_Endpoint").FillAsync("https://contoso-ai.openai.azure.com/");
            await page.Locator("#AIProviderConnection_AuthenticationType").SelectOptionAsync(new SelectOptionValue { Label = "API key" });
            await page.Locator("#AIProviderConnection_ApiKey").FillAsync("demo-key-not-a-real-secret");
            await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
            Console.WriteLine("Added the connection");
        }

        await page.GotoAsync(BaseUrl + "/Admin/ai/deployments");

        if (await page.Locator("text=gpt-4.1-mini").CountAsync() == 0)
        {
            await page.GotoAsync(BaseUrl + "/Admin/ai/deployment/create/Azure");
            await page.Locator("#AIDeployment_ModelName").FillAsync("gpt-4.1-mini");
            await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
            Console.WriteLine("Added the deployment");
        }

        await page.GotoAsync(BaseUrl + "/Admin/Settings/ai-settings");
        await page.Locator("select[name$='DefaultChatDeploymentName']").SelectOptionAsync(new SelectOptionValue { Index = 1 });
        await page.Locator("select[name$='DefaultUtilityDeploymentName']").SelectOptionAsync(new SelectOptionValue { Index = 1 });
        await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
        Console.WriteLine("Chose the default deployments");

        await context.CloseAsync();
    }

    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;
}
