using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the AI Connections and Deployments video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-connection", ConnectionAsync),
        ("04-deployment", DeploymentAsync),
        ("05-capabilities", CapabilitiesAsync),
        ("06-defaults", DefaultsAsync),
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
    /// Enables the provider features (not recorded).
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
            "CrestApps.OrchardCore.OpenAI",
            "CrestApps.OrchardCore.Ollama",
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

    // The block that holds every capability tick box.
    private static ILocator Features(Scene scene)
        => scene.Page.Locator("xpath=//*[.//input[@id='SelectedFeatures_audioInput'] and .//input[@id='SelectedFeatures_videoOutput']][not(.//*[.//input[@id='SelectedFeatures_audioInput'] and .//input[@id='SelectedFeatures_videoOutput']])]");

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static async Task TickAsync(Scene scene, string id, bool on)
    {
        var box = scene.Page.Locator("#" + id);

        if (await box.IsCheckedAsync() != on)
        {
            await scene.ClickAsync(box, pauseAfter: 400);
        }
    }

    // ----- Clips -----

    private static async Task ConnectionAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/provider/connections");
        await scene.StartCaptureAsync();

        await scene.StepAsync("conn-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ai/provider/connections"), "Artificial Intelligence > Provider Connections");
            await scene.PauseAsync(2800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("h5").Filter(new() { HasTextString = "Configuration-based connections" }), "Read-only");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("conn-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.create"), pauseAfter: 1500);
            await scene.HighlightAsync(scene.Page.Locator("#modalAddInstance .modal-body"), "Available Providers");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalAddInstance a[href$='/create/Azure']"));
        });

        await scene.StepAsync("conn-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#AIProviderConnection_DisplayText"), "Contoso Azure OpenAI", delay: 45);
            await scene.HighlightAsync(scene.Page.Locator("#AIProviderConnection_Name"), "Technical name");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("conn-4", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#AIProviderConnection_Endpoint"), "https://contoso-ai.openai.azure.com/", delay: 30);
            await scene.SelectAsync(scene.Page.Locator("#AIProviderConnection_AuthenticationType"), "API key", pauseAfter: 900);
            await scene.HighlightAsync(scene.Page.Locator("#AIProviderConnection_AuthenticationType"), "Authentication type");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("conn-5", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#AIProviderConnection_ApiKey"), "demo-key-not-a-real-secret", delay: 30);
            await scene.HighlightAsync(scene.Page.Locator("#AIProviderConnection_ApiKey"), "API key");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("conn-6", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1200);
        });
    }

    private static async Task DeploymentAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/deployments");
        await scene.StartCaptureAsync();

        await scene.StepAsync("dep-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ai/deployments"), "Artificial Intelligence > Deployments");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button.create"), pauseAfter: 900);
        });

        await scene.StepAsync("dep-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalAddDeployment a[href$='/deployment/create/Azure']"));
        });

        await scene.StepAsync("dep-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#AIDeployment_ModelName"), "gpt-4.1-mini", delay: 45);
            await scene.HighlightAsync(scene.Page.Locator("#AIDeployment_Name"), "Technical name");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("dep-4", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("[name$='ConnectionName']").First), "Connection name");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("dep-5", async () =>
        {
            await scene.ScrollAsync(0, 500);
            await scene.HighlightAsync(Features(scene), "Trained features");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.StepAsync("dep-6", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.create"), pauseAfter: 700);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalAddDeployment a[href$='/deployment/create/Azure']"));
            await scene.TypeAsync(scene.Page.Locator("#AIDeployment_ModelName"), "text-embedding-3-small", delay: 30);
        });

        await scene.StepAsync("dep-7", async () =>
        {
            await scene.ScrollAsync(0, 500);
            await TickAsync(scene, "SelectedFeatures_textGeneration", false);
            await TickAsync(scene, "SelectedFeatures_toolCalling", false);
            await TickAsync(scene, "SelectedFeatures_streaming", false);
            await TickAsync(scene, "SelectedFeatures_textEmbedding", true);
            await scene.PauseAsync(1200);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.StepAsync("dep-8", async () =>
        {
            await scene.PauseAsync(1500);
        });
    }

    private static async Task CapabilitiesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        // Reached from the list, so the menu shows Deployments as the current item.
        await scene.OpenAsync("/Admin/ai/deployments");
        await scene.Page.Locator("button.create").ClickAsync();
        await scene.Page.RunAndWaitForNavigationAsync(() => scene.Page.Locator("#modalAddDeployment a[href$='/deployment/create/Azure']").ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("cap-1", async () =>
        {
            await scene.ScrollAsync(0, 480);
            await scene.HighlightAsync(scene.Page.Locator("#SelectedFeatures_textEmbedding").Locator("xpath=.."), "Text embedding");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("cap-2", async () =>
        {
            await scene.HighlightAsync(Features(scene), "Trained features");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("cap-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#SelectedFeatures_imageInput").Locator("xpath=.."), "Images");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#SelectedFeatures_speechToText").Locator("xpath=.."), "Speech");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#SelectedFeatures_realtime").Locator("xpath=.."), "Realtime");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("cap-4", async () =>
        {
            await TickAsync(scene, "SelectedFeatures_reasoning", true);
            await TickAsync(scene, "ModelParameters_reasoningEffort_IsSupported", true);
            await scene.HighlightAsync(scene.Page.Locator("#ModelParameters_reasoningEffort_DefaultValue").Locator("xpath=ancestor::*[contains(@class,'border') or contains(@class,'card')][1]"), "Reasoning effort");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task DefaultsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/ai-settings");
        await scene.StartCaptureAsync();

        await scene.StepAsync("def-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/ai-settings"), "Settings > Artificial Intelligence");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(scene.Page.Locator("select[name$='DefaultChatDeploymentName']")), "Default Deployments");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("def-2", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("select[name$='DefaultChatDeploymentName']"), "Gpt41Mini (gpt-4.1-mini)", pauseAfter: 900);
            await scene.HighlightAsync(Section(scene.Page.Locator("select[name$='DefaultChatDeploymentName']")), "Default chat deployment");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("def-3", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("select[name$='DefaultUtilityDeploymentName']"), "Gpt41Mini (gpt-4.1-mini)", pauseAfter: 900);
            await scene.HighlightAsync(Section(scene.Page.Locator("select[name$='DefaultUtilityDeploymentName']")), "Default utility deployment");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("def-4", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("select[name$='DefaultEmbeddingDeploymentName']"), "TextEmbedding3Small (text-embedding-3-small)", pauseAfter: 900);
            await scene.HighlightAsync(Section(scene.Page.Locator("select[name$='DefaultEmbeddingDeploymentName']")), "Default embedding deployment");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("def-5", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }
}
