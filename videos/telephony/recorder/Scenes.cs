using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Telephony video, one method per "clip" of storyboard.json.
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
        ("03-features", FeaturesAsync),
        ("04-asterisk", AsteriskAsync),
        ("05-telnyx", TelnyxAsync),
        ("06-defaults", DefaultsAsync),
        ("07-extensions", ExtensionsAsync),
        ("08-softphone", SoftPhoneAsync),
        ("09-permissions", PermissionsAsync),
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
    /// Enables the features of this video and creates the fake users (not recorded).
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
            "CrestApps.OrchardCore.Telephony",
            "CrestApps.OrchardCore.Telephony.SoftPhone",
            "CrestApps.OrchardCore.Telephony.SoftPhone.Extension",
            "CrestApps.OrchardCore.Telnyx",
            "CrestApps.OrchardCore.Asterisk",
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

        foreach (var (user, email) in new[]
        {
            ("jamie.chen", "jamie.chen@example.com"),
            ("maria.lopez", "maria.lopez@example.com"),
            ("sam.patel", "sam.patel@example.com"),
        })
        {
            await page.GotoAsync(BaseUrl + "/Admin/Users/Create");
            await page.Locator("[name='User.UserName']").FillAsync(user);
            await page.Locator("[name='User.Email']").FillAsync(email);
            await page.Locator("[name='User.Password']").FillAsync("Password1!");
            await page.Locator("[name='User.PasswordConfirmation']").FillAsync("Password1!");
            await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
            Console.WriteLine($"Created {user}");
        }

        await context.CloseAsync();
    }

    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static ILocator Feature(Scene scene, string id)
        => scene.Page.Locator($"#btn-enable-{id.Replace('.', '_')}, #btn-disable-{id.Replace('.', '_')}").First.Locator("xpath=ancestor::li[contains(@class,'list-group-item')][1]");

    private static ILocator Tab(Scene scene, string name) => scene.Page.Locator($"a[href='#tab-{name}']");

    // ----- Clips -----

    private static async Task FeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();

        await scene.StepAsync("feat-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Features"), "Tools > Features");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "Telephony", delay: 70);
            await scene.PauseAsync(1200);
            await scene.ScrollAsync(0, 560);
            await scene.HighlightAsync(Feature(scene, "CrestApps.OrchardCore.Telephony"), "Telephony");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("feat-2", async () =>
        {
            await scene.HighlightAsync(Feature(scene, "CrestApps.OrchardCore.Telephony.SoftPhone"), "Telephony Soft Phone");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Feature(scene, "CrestApps.OrchardCore.Telephony.SoftPhone.Extension"), "Telephony Soft Phone Extension");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("feat-3", async () =>
        {
            await scene.ScrollAsync(0, 300);
            await scene.HighlightAsync(Feature(scene, "CrestApps.OrchardCore.Telnyx"), "Telnyx");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Feature(scene, "CrestApps.OrchardCore.Asterisk"), "Asterisk");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("feat-4", async () =>
        {
            await scene.HighlightAsync(Feature(scene, "CrestApps.OrchardCore.Telephony.Azure"), "Telephony - Azure Blob Storage");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task AsteriskAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/telephony");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ast-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/telephony"), "Settings > Communication > Telephony");
            await scene.PauseAsync(2800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Tab(scene, "asterisk"), pauseAfter: 600);
        });

        await scene.StepAsync("ast-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#asterisk-is-enabled"), pauseAfter: 900);
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_AsteriskSettings_BaseUrl")), "Required");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ast-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#ISite_AsteriskSettings_BaseUrl"), "http://pbx.contoso.example:8088/ari/", delay: 25);
            await scene.TypeAsync(scene.Page.Locator("#ISite_AsteriskSettings_UserName"), "contoso-calls", delay: 30);
            await scene.TypeAsync(scene.Page.Locator("#ISite_AsteriskSettings_Password"), "demo-not-a-secret", delay: 30);
        });

        await scene.StepAsync("ast-4", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#ISite_AsteriskSettings_ApplicationName"), "contoso-calls", delay: 35);
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_AsteriskSettings_ApplicationName")), "Stasis application name");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ast-5", async () =>
        {
            await scene.ScrollAsync(0, 560);
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_AsteriskSettings_EndpointTemplate")), "Optional");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 700);
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_AsteriskSettings_WebSocketUrl")), "Browser audio");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ast-6", async () =>
        {
            await scene.ScrollAsync(0, -3000);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(800);
        });
    }

    private static async Task TelnyxAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/telephony");
        await scene.StartCaptureAsync();

        await scene.StepAsync("tel-1", async () =>
        {
            await scene.ClickAsync(Tab(scene, "telnyx"), pauseAfter: 700);
            await scene.HighlightAsync(scene.Page.Locator("#tab-telnyx .alert").First, "Four steps");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("tel-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#ISite_TelnyxSettings_IsEnabled"), pauseAfter: 700);
            await scene.TypeAsync(scene.Page.Locator("#ISite_TelnyxSettings_ApiKey"), "demo-key-not-a-real-secret", delay: 30);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.ClickAsync(Tab(scene, "telnyx"), pauseAfter: 500);
        });

        await scene.StepAsync("tel-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#tab-telnyx").GetByText("Connect Telnyx").Last, "Connect Telnyx");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task DefaultsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/telephony");
        await scene.StartCaptureAsync();

        await scene.StepAsync("def-1", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#ISite_TelephonySettings_DefaultProvider"), "Asterisk", pauseAfter: 900);
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_TelephonySettings_DefaultProvider")), "Default telephony provider");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("def-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#ISite_TelephonySettings_AllowedShortCodes"), "611", delay: 90);
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_TelephonySettings_AllowedShortCodes")), "Allowed short codes");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("def-3", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_SoftPhoneWidgetSettings_Enabled")), "Enable the soft phone");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_SoftPhoneWidgetSettings_DisplayOnAdmin")), "Show the soft phone on the admin dashboard");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("def-4", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_SoftPhoneWidgetSettings_EnableDiagnostics")), "Enable the diagnostics tab");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("def-5", async () =>
        {
            await scene.ScrollAsync(0, 500);
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_SoftPhoneWidgetSettings_RecentCallsCount")), "Recent calls to display");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_SoftPhoneWidgetSettings_DefaultCountryCode")), "Default country");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(scene.Page.Locator("#ISite_SoftPhoneWidgetSettings_AccentColor")), "Accent color");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("def-6", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1200);
        });
    }

    private static async Task ExtensionsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/telephony/extensions");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ext-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/telephony/extensions"), "Interaction Center > Management > Extensions");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create"));
        });

        await scene.StepAsync("ext-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#TelephonyExtension_DisplayName"), "Jamie Chen", delay: 50);
            await scene.HighlightAsync(Section(scene.Page.Locator("#TelephonyExtension_DisplayName")), "Display name");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ext-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#TelephonyExtension_Number"), "1001", delay: 90);
            await scene.HighlightAsync(Section(scene.Page.Locator("#TelephonyExtension_Number")), "Extension number");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ext-4", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button[data-id='TelephonyExtension_UserId']"), pauseAfter: 900);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = "jamie.chen" }).First, pauseAfter: 800);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.StepAsync("ext-5", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create"));
            await scene.Page.Locator("#TelephonyExtension_DisplayName").FillAsync("Maria Lopez");
            await scene.Page.Locator("#TelephonyExtension_Number").FillAsync("1002");
            await scene.ClickAsync(scene.Page.Locator("button[data-id='TelephonyExtension_UserId']"), pauseAfter: 500);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = "maria.lopez" }).First, pauseAfter: 400);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.StepAsync("ext-6", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("ul.list-group .list-group-item a.btn-danger").First, "Delete");
            await scene.PauseAsync(1500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SoftPhoneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/telephony/extensions");
        await scene.StartCaptureAsync();

        await scene.StepAsync("soft-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".telephony-soft-phone__toggle"), pauseAfter: 1200);
        });

        await scene.StepAsync("soft-2", async () =>
        {
            foreach (var key in "5550142")
            {
                await scene.ClickAsync(scene.Page.Locator($"[data-telephony-key='{key}']"), pauseAfter: 220);
            }

            await scene.HighlightAsync(scene.Page.Locator("[data-telephony-dial]").First, "Call");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("soft-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("[data-telephony-dial-mode-toggle]"), pauseAfter: 900);
            await scene.TypeAsync(scene.Page.Locator("[data-telephony-number]"), "Mar", delay: 120);
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("soft-5", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("[data-telephony-tab='history']"), pauseAfter: 800);
        });

        await scene.StepAsync("soft-6", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("[data-telephony-tab='voicemail']"), pauseAfter: 800);
        });

        await scene.StepAsync("soft-7", async () =>
        {
            await scene.NavigateAsync("/softphone");
            await scene.PauseAsync(800);
            await scene.ClickAsync(scene.Page.Locator("[data-telephony-tab='keypad']"), pauseAfter: 800);
        });
    }

    private static async Task PermissionsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Roles/Index");
        await scene.StartCaptureAsync();

        await scene.StepAsync("perm-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/Roles/Edit/Editor']").Last);
            await scene.TypeAsync(scene.Page.GetByPlaceholder("Search permissions"), "Telephony", delay: 70);
            await scene.PauseAsync(800);
        });

        await scene.StepAsync("perm-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("tr").Filter(new() { HasTextString = "Manage telephony settings" }).First, "Manage telephony settings");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("tr").Filter(new() { HasTextString = "Manage telephony extensions" }).First, "Manage telephony extensions");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("perm-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("tr").Filter(new() { HasTextString = "Use the telephony soft phone" }).First, "Use the telephony soft phone");
        });

        await scene.ClearHighlightsAsync();
    }
}
