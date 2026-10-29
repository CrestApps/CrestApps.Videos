using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the AI Chat video, one method per "clip" of storyboard.json.
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
        ("03-chat", ChatAsync),
        ("04-history", HistoryAsync),
        ("05-widget", WidgetAsync),
        ("06-website", WebsiteAsync),
        ("07-engines", EnginesAsync),
        ("08-protect", ProtectAsync),
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
    /// Enables the features, and chooses the default deployments of the site's configuration (not recorded).
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
            "CrestApps.OrchardCore.AI.Chat.AdminWidget",
            "CrestApps.OrchardCore.AI.Chat.Analytics",
            "CrestApps.OrchardCore.AI.Chat.Claude",
            "CrestApps.OrchardCore.AI.Chat.Copilot",
            "CrestApps.OrchardCore.AI.Personas",
            "OrchardCore.Widgets",
            "OrchardCore.Layers",
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

        await page.GotoAsync(BaseUrl + "/Admin/Settings/ai-settings");
        await page.Locator("select[name$='DefaultChatDeploymentName']").SelectOptionAsync(new SelectOptionValue { Index = 1 });
        await page.Locator("select[name$='DefaultUtilityDeploymentName']").SelectOptionAsync(new SelectOptionValue { Index = 1 });
        await page.Locator("select[name$='DefaultEmbeddingDeploymentName']").SelectOptionAsync(new SelectOptionValue { Index = 1 });
        await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
        Console.WriteLine("Chose the default deployments");

        // The Theme has a Footer zone for the website chat widget.
        await page.GotoAsync(BaseUrl + "/Admin/Themes");
        var enableTheme = page.Locator("form[action='/Admin/Themes/Enable/TheTheme'] button");

        if (await enableTheme.CountAsync() > 0)
        {
            await enableTheme.ClickAsync();
            await Task.Delay(8000);
            await page.GotoAsync(BaseUrl + "/Admin/Themes");
        }

        var currentTheme = page.Locator("form[action='/Admin/Themes/SetCurrentTheme/TheTheme'] button");

        if (await currentTheme.CountAsync() > 0)
        {
            await currentTheme.ClickAsync();
            await Task.Delay(8000);
            Console.WriteLine("Made The Theme current");
        }

        await page.GotoAsync(BaseUrl + "/Admin/Settings/zones");
        await page.Locator("#ISite_LayerSettings_Zones").FillAsync("Footer");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());

        // A layer that is always active, so a widget shows on every page.
        await page.GotoAsync(BaseUrl + "/Admin/Layers");

        if (await page.Locator("text=Always").CountAsync() == 0)
        {
            await page.GotoAsync(BaseUrl + "/Admin/Layers/Create");
            await page.Locator("#Name").FillAsync("Always");
            await page.Locator(".btn-primary:has-text('Create')").First.ClickAsync();
            await Task.Delay(4000);
            Console.WriteLine("Created the layer");
        }

        await page.GotoAsync(BaseUrl + "/Admin/Layers/Edit?name=Always");

        if (await page.Locator("text=No conditions found").CountAsync() > 0)
        {
            await page.Locator("a:has-text('Add'), button:has-text('Add')").First.ClickAsync();
            var boolean = page.Locator(".modal.show .card:has-text('Boolean condition') a, .modal.show .card:has-text('Boolean condition') button").First;
            await boolean.WaitForAsync();
            await boolean.ClickAsync();
            await page.WaitForSelectorAsync("#BooleanCondition_ValueTrue", new() { Timeout = 20000 });
            await page.Locator(".btn-primary:has-text('Create')").First.ClickAsync();
            await Task.Delay(4000);
            await page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync();
            await Task.Delay(4000);
            Console.WriteLine("Created the rule");
        }

        Console.WriteLine("Added the Always layer");

        // The profile every clip chats with.
        await page.GotoAsync(BaseUrl + "/Admin/ai/profiles");

        if (await page.Locator("text=Contoso website assistant").CountAsync() == 0)
        {
            await page.Locator("button.create").ClickAsync();
            await page.RunAndWaitForNavigationAsync(() => page.Locator("#newProfileModal [data-profile-picker-card]").Filter(new() { HasTextString = "Website assistant" }).Locator("a.btn").First.ClickAsync());
            await page.Locator("#DisplayText").FillAsync("Contoso website assistant");
            await page.RunAndWaitForNavigationAsync(() => page.Locator("button:has-text('Create profile')").ClickAsync());
            await page.Locator("#AIProfile_WelcomeMessage").FillAsync("Hi! Ask me about Contoso Bikes orders, repairs and opening hours.");
            await page.Locator("#AIProfile_IsOnAdminMenu").SetCheckedAsync(true);
            var instructions = "You are the assistant on the website of Contoso Bikes, a bicycle shop. Answer questions about orders, repairs and the shop in short, friendly answers.\n\n"
                + "Facts about the shop:\n"
                + "- Opening hours: Monday to Saturday 9:00 to 18:00. Closed on Sundays.\n"
                + "- Repairs: a basic tune-up takes two working days and costs 60 dollars.\n"
                + "- Returns: unused bikes can be returned within 30 days.\n\n"
                + "If you do not know something, say so and offer to have the team call back. Never ask for payment details.";
            await page.EvaluateAsync("text => (() => { const cm = document.getElementById('AIProfile_SystemMessage').parentElement.querySelector('.CodeMirror').CodeMirror; cm.setValue(text); cm.save(); document.getElementById('AIProfile_SystemMessage').value = text; })()", instructions);
            await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
            Console.WriteLine("Created the profile");
        }

        await context.CloseAsync();

        await FixProfileAsync(browser);
        await SeedChatsAsync(browser);
    }

    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;
}
