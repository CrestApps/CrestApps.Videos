using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Content Access Control video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    // The customers file made by make-data.py.
    public static string DataFolder { get; } = Environment.GetEnvironmentVariable("VIDEO_DATA") ?? Path.Combine(Root, "data");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-setup", SetupAsync),
        ("04-item", ItemAsync),
        ("05-check", CheckAsync),
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
            "CrestApps.OrchardCore.Roles",
            "CrestApps.OrchardCore.ContentAccessControl",
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

        await ImportRecipeAsync(page, Path.Combine(Root, "seed", "handbook.recipe.json"));

        await CreateUserAsync(browser, page, "jordan.lee", "jordan.lee@example.com", "Supervisor");
        await CreateUserAsync(browser, page, "casey.morgan", "casey.morgan@example.com", "Support Agent");

        await context.CloseAsync();
    }

    // ----- Helpers -----

    // Runs a recipe through Configuration > Import/Export > JSON Import (not recorded).
    private static async Task ImportRecipeAsync(IPage page, string path)
    {
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        var status = await page.EvaluateAsync<int>(
            @"async (json) => {
                const form = document.querySelector('form[action*=""Import/Json""], form:has(textarea#Json)');
                const data = new URLSearchParams(new FormData(form));
                data.set('Json', json);
                const response = await fetch(form.action, { method: 'POST', body: data, redirect: 'manual' });
                return response.status;
            }",
            await File.ReadAllTextAsync(path));
        Console.WriteLine($"Imported {Path.GetFileName(path)} ({status})");
    }

    // Creates a user with one role through Access Control > Users, and saves the person's sign-in as state-<name>.json (not recorded).
    private static async Task CreateUserAsync(IBrowser browser, IPage page, string userName, string email, string role)
    {
        await page.GotoAsync(BaseUrl + "/Admin/Users/Create");
        await page.Locator("#User_UserName").FillAsync(userName);
        await page.Locator("#User_Email").FillAsync(email);
        await page.Locator("#User_Password").FillAsync(Password);
        await page.Locator("#User_PasswordConfirmation").FillAsync(Password);
        await page.EvaluateAsync(
            @"(role) => {
                const hidden = [...document.querySelectorAll(""input[type=hidden][name$='.Role']"")].find(input => input.value === role);
                const index = hidden.name.match(/Roles\[(\d+)\]/)[1];
                document.getElementById('User_Roles_' + index + '__IsSelected').click();
            }",
            role);
        await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
        Console.WriteLine($"Created {userName} ({role})");

        var context = await browser.NewContextAsync();
        var login = await context.NewPageAsync();
        await login.GotoAsync(BaseUrl + "/Login");
        await login.Locator("#LoginForm_UserName").FillAsync(userName);
        await login.Locator("#LoginForm_Password").FillAsync(Password);
        await login.Locator("#LoginForm_Password").PressAsync("Enter");
        await login.WaitForURLAsync(url => !url.Contains("/Login", StringComparison.OrdinalIgnoreCase));
        await context.StorageStateAsync(new BrowserContextStorageStateOptions { Path = Path.Combine(Root, $"state-{userName}.json") });
        await context.CloseAsync();
    }

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator Limited(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-limited-wrapper')][1]");

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static async Task TickAsync(Scene scene, string id, bool on)
    {
        var box = scene.Page.Locator("#" + id);

        if (await box.IsCheckedAsync() != on)
        {
            await scene.ClickAsync(box, pauseAfter: 400);
        }
    }

    // Replaces the signed-in person of a scene with the one saved in state-<userName>.json (not shown on screen).
    private static async Task SignInAsAsync(Scene scene, string userName)
    {
        using var document = System.Text.Json.JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(Root, $"state-{userName}.json")));
        var cookies = document.RootElement.GetProperty("cookies").EnumerateArray().Select(cookie => new Cookie
        {
            Name = cookie.GetProperty("name").GetString(),
            Value = cookie.GetProperty("value").GetString(),
            Domain = cookie.GetProperty("domain").GetString(),
            Path = cookie.GetProperty("path").GetString(),
            Expires = (float)cookie.GetProperty("expires").GetDouble(),
            HttpOnly = cookie.GetProperty("httpOnly").GetBoolean(),
            Secure = cookie.GetProperty("secure").GetBoolean(),
            SameSite = Enum.Parse<SameSiteAttribute>(cookie.GetProperty("sameSite").GetString()),
        }).ToList();

        await scene.Context.ClearCookiesAsync();
        await scene.Context.AddCookiesAsync(cookies);
    }

    private static ILocator RolePickerButton(Scene scene, string suffix) => scene.Page.Locator($"button.dropdown-toggle[data-id$='{suffix}']");

    // ----- Clips -----

    private static async Task SetupAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.StartCaptureAsync();

        await scene.StepAsync("setup-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ContentTypes/List"), "Design > Content Definition > Content Types");
            await scene.PauseAsync(2800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/ContentTypes/Edit/HandbookPage']").Last);
        });

        await scene.StepAsync("setup-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/ContentTypes/AddPartsTo/HandbookPage']"));
            var row = scene.Page.Locator("li, .list-group-item, div.row").Filter(new() { HasTextString = "Role Picker" }).Last;
            await scene.ScrollAsync(0, 400);
            await scene.HighlightAsync(row, "Role Picker");
            await scene.PauseAsync(2200);
        });

        await scene.StepAsync("setup-3", async () =>
        {
            var row = scene.Page.Locator("li, .list-group-item, div.row").Filter(new() { HasTextString = "Role Picker" }).Last;
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(row.Locator("a, button").Filter(new() { HasTextString = "Add & Configure" }).First);
            await scene.Page.Locator("#DisplayName").FillAsync(string.Empty);
            await scene.TypeAsync(scene.Page.Locator("#DisplayName"), "Limit access to selected roles", delay: 45);
        });

        await scene.StepAsync("setup-4", async () =>
        {
            await scene.ScrollAsync(0, 380);
            await scene.ClickAsync(RolePickerButton(scene, "_ExcludedRoles"), pauseAfter: 500);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show li a").Filter(new() { HasTextString = "Anonymous" }).First, pauseAfter: 300);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show li a").Filter(new() { HasTextString = "Authenticated" }).First, pauseAfter: 400);
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.ClickAsync(scene.Page.Locator("[id$='_RolePickerPartSettingsDisplayDriver_Required']"), pauseAfter: 300);
            await scene.ClickAsync(scene.Page.Locator("[id$='_AllowSelectMultiple']"), pauseAfter: 300);
            await scene.TypeAsync(scene.Page.Locator("[id$='_RolePickerPartSettingsDisplayDriver_Hint']"), "Only people with one of these roles can view this page.", delay: 12);
        });

        await scene.StepAsync("setup-5", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("[id$='_IsContentRestricted']"), pauseAfter: 600);
            await scene.HighlightAsync(scene.Page.Locator("[id$='_IsContentRestricted']").Locator("xpath=.."), "Restrict content?");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button[name='submit.Save']"));
        });
    }

    private static async Task ItemAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();

        await scene.StepAsync("item-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Contents/ContentItems/"), "Content > Content Items");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button.dropdown-toggle, a.dropdown-toggle").Filter(new() { HasTextString = "New" }).First, pauseAfter: 900);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".dropdown-menu.show a").Filter(new() { HasTextString = "Handbook Page" }).First);
        });

        await scene.StepAsync("item-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#TitlePart_Title"), "Supervisor handbook", delay: 30);
            await scene.TypeAsync(scene.Page.Locator("textarea[name$='Markdown']").First, "How to handle escalations, approve refunds and lead the weekly team review.", delay: 12);
        });

        await scene.StepAsync("item-3", async () =>
        {
            await scene.ScrollAsync(0, 420);
            var picker = scene.Page.Locator("button.dropdown-toggle[data-id$='_Roles']").First;
            await scene.HighlightAsync(Section(picker), "Limit access to selected roles");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(picker, pauseAfter: 900);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show li a").Filter(new() { HasTextString = "Supervisor" }).First, pauseAfter: 900);
            await scene.Page.Keyboard.PressAsync("Escape");
        });

        await scene.StepAsync("item-4", async () =>
        {
            await scene.ScrollAsync(0, 600);
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator("button[name='submit.Publish']").First, pauseAfter: 100));
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("item-5", async () =>
        {
            await scene.PauseAsync(1500);
        });
    }

    private static async Task CheckAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await SignInAsAsync(scene, "casey.morgan");
        await scene.OpenAsync("/support-hours");
        await scene.StartCaptureAsync();

        await scene.StepAsync("check-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("h1, h2").Filter(new() { HasTextString = "Support hours" }).First, "Opens for Casey");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("check-2", async () =>
        {
            await scene.NavigateAsync("/supervisor-handbook");
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("check-3", async () =>
        {
            await SignInAsAsync(scene, "jordan.lee");
            await scene.NavigateAsync("/supervisor-handbook");
            await scene.HighlightAsync(scene.Page.Locator("h1, h2").Filter(new() { HasTextString = "Supervisor handbook" }).First, "Opens for Jordan");
        });

        await scene.ClearHighlightsAsync();
    }
}
