using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Realtime Voice video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // Recorded in this order: 03 creates the deployment that 05 uses.
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-speech", SpeechAsync),
        ("03-cascade", CascadeAsync),
        ("04-defaults", DefaultsAsync),
        ("05-profile", ProfileAsync),
        ("06-chat", ChatAsync),
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

    private static ILocator CardOf(ILocator link) => link.Locator("xpath=ancestor::*[contains(concat(' ',normalize-space(@class),' '),' card ')][1]");

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator Field(ILocator input) => input.Locator("xpath=ancestor::div[contains(@class,'mb-3')][1]");

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static ILocator FormCheck(ILocator box) => box.Locator("xpath=ancestor::*[contains(@class,'form-check')][1]");

    private static ILocator Profile(Scene scene) => scene.Page.Locator("a[href*='/Admin/ai/profile/edit/']").Filter(new() { HasTextString = "Support assistant" }).First;

    private static ILocator Setting(Scene scene, string name) => scene.Page.Locator($"[name$='{name}']").First;

    private static ILocator Feature(Scene scene, string value) => scene.Page.Locator($"input[name='AIDeployment.SelectedFeatures'][value='{value}']");

    private static Task ScrollToAsync(Scene scene, string selector)
        => scene.Page.EvaluateAsync("(s) => { const h = document.querySelector(s); window.scrollTo(0, h.getBoundingClientRect().top + window.scrollY - 90); }", selector);

    private static async Task OpenAddDeploymentAsync(Scene scene, string type, string caption)
    {
        await scene.ClickAsync(scene.Page.Locator("button.create, a.create").First, pauseAfter: 600);
        await scene.HighlightAsync(CardOf(scene.Page.Locator($"a[href$='/deployment/create/{type}']")), caption);
        await scene.PauseAsync(800);
        await scene.ClearHighlightsAsync();
        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator($"a[href$='/deployment/create/{type}']"));
    }

    private static async Task TickFeatureAsync(Scene scene, string value)
    {
        var id = await Feature(scene, value).GetAttributeAsync("id");
        await scene.ClickAsync(scene.Page.Locator($"label[for='{id}']"), pauseAfter: 500);
    }

    // ----- Clips -----

    private static async Task SpeechAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/deployments");
        await scene.StartCaptureAsync();

        await scene.StepAsync("sp-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ai/deployments"), "Artificial Intelligence > Deployments");
            await scene.PauseAsync(600);
            await scene.ClearHighlightsAsync();
            await OpenAddDeploymentAsync(scene, "AzureSpeech", "Azure AI Services");
        });

        await scene.StepAsync("sp-2", async () =>
        {
            await scene.Page.Locator("#AIDeployment_Name").FillAsync("contoso-speech");
            await scene.Page.Locator("#AIDeployment_ModelName").FillAsync("whisper");
            await scene.Page.Locator("#AIDeployment_Endpoint").FillAsync("https://contoso-speech.cognitiveservices.azure.com");
            await scene.SelectAsync(scene.Page.Locator("#AIDeployment_AuthenticationType"), "ApiKey", pauseAfter: 600);
            await scene.HighlightAsync(Section(scene.Page.Locator("#AIDeployment_ApiKey")), "API key");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("sp-3", async () =>
        {
            await ScrollToAsync(scene, "input[name='AIDeployment.SelectedFeatures'][value='speechToText']");
            await TickFeatureAsync(scene, "speechToText");
            await TickFeatureAsync(scene, "realtime");
            await scene.HighlightAsync(FormCheck(Feature(scene, "speechToText")), "Speech to text");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(FormCheck(Feature(scene, "realtime")), "Realtime");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(FormCheck(Feature(scene, "textToSpeech")), "Text to speech");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task CascadeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/deployments");
        await scene.StartCaptureAsync();

        await scene.StepAsync("cas-1", async () =>
        {
            await OpenAddDeploymentAsync(scene, "CascadedRealtime", "Cascaded Realtime");
            await scene.Page.Locator("#AIDeployment_ModelName").FillAsync("conversation");
            await scene.Page.Locator("#AIDeployment_Name").FillAsync("contoso-conversation");
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("cas-2", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#AIDeployment_SpeechToTextDeploymentName"), "contoso-transcribe", pauseAfter: 200);
            await scene.SelectAsync(scene.Page.Locator("#AIDeployment_ChatDeploymentName"), "contoso-chat", pauseAfter: 200);
            await scene.SelectAsync(scene.Page.Locator("#AIDeployment_TextToSpeechDeploymentName"), "contoso-voice", pauseAfter: 200);
            await scene.HighlightAsync(Field(scene.Page.Locator("#AIDeployment_SpeechToTextDeploymentName")), "Speech to text");
            await scene.PauseAsync(900);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene.Page.Locator("#AIDeployment_ChatDeploymentName")), "Chat");
            await scene.PauseAsync(900);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene.Page.Locator("#AIDeployment_TextToSpeechDeploymentName")), "Text to speech");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("cas-3", async () =>
        {
            await ScrollToAsync(scene, "input[name='AIDeployment.SelectedFeatures'][value='realtime']");
            await scene.HighlightAsync(FormCheck(Feature(scene, "realtime")), "Realtime");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(2000);
        });
    }

    private static async Task DefaultsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/ai-settings");
        await scene.StartCaptureAsync();

        await scene.StepAsync("def-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/ai-settings"), "Settings > Artificial Intelligence");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await ScrollToAsync(scene, "[name$='DefaultSpeechToTextDeploymentName']");
            await scene.HighlightAsync(Section(Setting(scene, "DefaultSpeechToTextDeploymentName")), "Default speech-to-text deployment");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Setting(scene, "DefaultTextToSpeechDeploymentName")), "Default text-to-speech deployment");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("def-2", async () =>
        {
            await scene.HighlightAsync(Section(Setting(scene, "DefaultRealtimeDeploymentName")), "Default realtime deployment");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ProfileAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.Page.RunAndWaitForNavigationAsync(() => Profile(scene).ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("pro-1", async () =>
        {
            await ScrollToAsync(scene, "#chatModeSelector");
            await scene.SelectAsync(scene.Page.Locator("#chatModeSelector"), "Conversation", pauseAfter: 900);
            await scene.HighlightAsync(Section(scene.Page.Locator("#chatModeSelector")), "Chat mode");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("pro-2", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#chatModeConversationDeploymentSelect"), "contoso-conversation", pauseAfter: 900);
            await scene.HighlightAsync(Section(scene.Page.Locator("#chatModeConversationDeploymentSelect")), "Conversation deployment");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("pro-3", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#chatModeVoiceSelect")), "Voice");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(FormCheck(scene.Page.Locator("#AIProfile_EnableTextToSpeechPlayback")), "Enable text-to-speech playback");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1000);
        });
    }

    private static async Task ChatAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.Page.Locator("button.dropdown-toggle.actions").First.ClickAsync();
        await scene.Page.RunAndWaitForNavigationAsync(() => scene.Page.Locator("a[href*='/Admin/ai/chat/session/']").Filter(new() { HasTextString = "New chat" }).First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("cht-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#AIChatSession_ConversationBtn"), "Start speaking");
            await scene.PauseAsync(8000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("cht-2", async () =>
        {
            await scene.PauseAsync(12500);
        });

        await scene.StepAsync("cht-3", async () =>
        {
            await scene.PauseAsync(11500);
        });
    }
}
