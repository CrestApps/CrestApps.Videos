using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Users and Roles video, one method per "clip" of storyboard.json.
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
        ("03-list", ListAsync),
        ("04-add", AddAsync),
        ("05-roles", RolesAsync),
        ("06-assign", AssignAsync),
        ("07-names", NamesAsync),
        ("08-avatars", AvatarsAsync),
        ("09-code", CodeAsync),
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
            "CrestApps.OrchardCore.Users.DisplayName",
            "CrestApps.OrchardCore.Users.Avatars",
            "CrestApps.OrchardCore.Users.EmailCodeSignIn",
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

        await CreateUserAsync(browser, page, "dana.whitfield", "dana.whitfield@example.com", "Editor");
        await CreateUserAsync(browser, page, "omar.haddad", "omar.haddad@example.com", "Author");
        await CreateUserAsync(browser, page, "lena.fischer", "lena.fischer@example.com", "Moderator");

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

    private static ILocator Block(ILocator field) => field.Locator("xpath=ancestor::div[contains(@class,'mb-3')][1]");

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

    private static ILocator UserRow(Scene scene, string text) => scene.Page.Locator(".list-group-item").Filter(new() { HasText = text }).First;

    // ----- Clips -----

    private static async Task ListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Roles/Index");
        await scene.StartCaptureAsync();

        await scene.StepAsync("list-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Users/Index"), "Access Control > Users");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/Users/Index"));
            await scene.HighlightAsync(UserRow(scene, "dana.whitfield"), "User name, email and roles");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("list-2", async () =>
        {
            await scene.HighlightAsync(UserRow(scene, "dana.whitfield").Locator(".badge").Filter(new() { HasTextString = "Unconfirmed" }).First, "Email not confirmed yet");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button.dropdown-toggle, a.dropdown-toggle").Filter(new() { HasTextString = "Role" }).First, pauseAfter: 1800);
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.ClickAsync(scene.Page.Locator("button.dropdown-toggle, a.dropdown-toggle").Filter(new() { HasTextString = "Sort" }).First, pauseAfter: 1500);
            await scene.Page.Keyboard.PressAsync("Escape");
        });

        await scene.StepAsync("list-3", async () =>
        {
            await scene.ClickAsync(UserRow(scene, "dana.whitfield").Locator("button.dropdown-toggle").Filter(new() { HasTextString = "Actions" }).First, pauseAfter: 1000);
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu:visible").First, "More things to do with an account");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task AddAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Users/Index");
        await scene.StartCaptureAsync();

        await scene.StepAsync("add-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn, button.btn").Filter(new() { HasTextString = "Add User" }).First);
            await scene.TypeAsync(scene.Page.Locator("#User_UserName"), "nadia.brooks", delay: 45);
            await scene.TypeAsync(scene.Page.Locator("#User_Email"), "nadia.brooks@example.com", delay: 25);
        });

        await scene.StepAsync("add-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#User_PhoneNumber").Locator("xpath=.."), "Optional, with the country code");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("text=Generate password").First, pauseAfter: 900);
            await scene.ClickAsync(scene.Page.Locator("text=Copy password").First, pauseAfter: 600);
        });

        await scene.StepAsync("add-3", async () =>
        {
            await scene.ScrollAsync(0, 600);
            await scene.ClickAsync(scene.Page.Locator("label").Filter(new() { HasTextString = "Author" }).First, pauseAfter: 600);
            await scene.HighlightAsync(scene.Page.Locator("label").Filter(new() { HasTextString = "Author" }).First, "Author");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("add-4", async () =>
        {
            await scene.ScrollAsync(0, 600);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.HighlightAsync(UserRow(scene, "nadia.brooks"), "The new person");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task RolesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Users/Index");
        await scene.StartCaptureAsync();

        await scene.StepAsync("roles-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/Roles/Index"));
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Filter(new() { HasText = "Anonymous" }).First, "Not signed in");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Filter(new() { HasText = "A system role representing all authenticated users." }).First, "Signed in");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("roles-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn, button.btn").Filter(new() { HasTextString = "Add Role" }).First);
            await scene.TypeAsync(scene.Page.Locator("#RoleName"), "Shift Lead", delay: 50);
            await scene.TypeAsync(scene.Page.Locator("#RoleDescription"), "Leads the support team on a shift.", delay: 20);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button[type=submit]").Filter(new() { HasTextString = "Configure" }).First);
        });

        await scene.StepAsync("roles-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("th").Filter(new() { HasTextString = "Allow" }).First, "Allow");
            await scene.PauseAsync(3200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("input[placeholder*='Search permissions'], input[type=search]").First, "Search permissions");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("roles-4", async () =>
        {
            var row = scene.Page.Locator("tr").Filter(new() { HasText = "Access admin panel" }).First;
            await scene.ClickAsync(row.Locator("input[type=checkbox]").First, pauseAfter: 600);
            await scene.HighlightAsync(row, "Access admin panel");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 6000);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button[type=submit]").Filter(new() { HasTextString = "Save" }).Last);
        });
    }

    private static async Task AssignAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Users/Index");
        await scene.StartCaptureAsync();

        await scene.StepAsync("assign-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(UserRow(scene, "dana.whitfield").Locator("a.btn").Filter(new() { HasTextString = "Edit" }).First);
        });

        await scene.StepAsync("assign-2", async () =>
        {
            await scene.ScrollAsync(0, 350);
            await scene.ClickAsync(scene.Page.Locator("label").Filter(new() { HasTextString = "Shift Lead" }).First, pauseAfter: 700);
            await scene.HighlightAsync(scene.Page.Locator("label").Filter(new() { HasTextString = "Shift Lead" }).First, "Shift Lead");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.HighlightAsync(UserRow(scene, "dana.whitfield"), "Editor and Shift Lead");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task NamesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/debugging");
        await scene.StartCaptureAsync();

        await scene.StepAsync("names-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/userDisplayName"), "Settings > User Display Name");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/Settings/userDisplayName"));
            await scene.HighlightAsync(scene.Page.Locator("#ISite_DisplayNameSettings_Type"), "Display name format");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("names-2", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#ISite_DisplayNameSettings_Type"), "FirstThenLast", pauseAfter: 700);
            await scene.SelectAsync(scene.Page.Locator("#ISite_DisplayNameSettings_FirstName"), "Required", pauseAfter: 700);
            await scene.SelectAsync(scene.Page.Locator("#ISite_DisplayNameSettings_LastName"), "Required", pauseAfter: 700);
        });

        await scene.StepAsync("names-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.NavigateAsync("/Admin/Users/Index");
            await scene.ClickAndWaitForNavigationAsync(UserRow(scene, "dana.whitfield").Locator("a.btn").Filter(new() { HasTextString = "Edit" }).First);
            await scene.TypeAsync(scene.Page.Locator("input[name$='FirstName']").First, "Dana", delay: 60);
            await scene.TypeAsync(scene.Page.Locator("input[name$='LastName']").First, "Whitfield", delay: 60);
        });

        await scene.StepAsync("names-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Filter(new() { HasText = "Dana Whitfield" }).First, "Dana Whitfield");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task AvatarsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/debugging");
        await scene.StartCaptureAsync();

        await scene.StepAsync("avatars-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/avatarOptions"), "Settings > User Avatars");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/Settings/avatarOptions"));
            await scene.HighlightAsync(scene.Page.Locator("input[id$='_Required']").First.Locator("xpath=.."), "Require Avatar");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("input[id$='UseDefaultStyle']").First.Locator("xpath=.."), "Use default CSS styles");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("avatars-2", async () =>
        {
            await scene.NavigateAsync("/Admin/Users/Index");
            await scene.ClickAndWaitForNavigationAsync(UserRow(scene, "Dana Whitfield").Locator("a.btn").Filter(new() { HasTextString = "Edit" }).First);
            await scene.HighlightAsync(scene.Page.Locator("button, a").Filter(new() { HasTextString = "Add media" }).First, "Add media");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task CodeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        // Not signed in: no saved sign-in is used.
        await using var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, Path.Combine(Root, "no-sign-in.json"), Path.GetDirectoryName(directory), durations);

        await scene.OpenAsync("/Login");
        await scene.StartCaptureAsync();

        await scene.StepAsync("code-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("a").Filter(new() { HasTextString = "Email me a sign-in code" }).First, "Email me a sign-in code instead");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a").Filter(new() { HasTextString = "Email me a sign-in code" }).First);
        });

        await scene.StepAsync("code-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#email-sign-in-email"), "dana.whitfield@example.com", delay: 40);
            await scene.HighlightAsync(scene.Page.Locator("button[type=submit]").Filter(new() { HasTextString = "Email me a code" }).First, "Email me a code");
        });

        await scene.ClearHighlightsAsync();
    }
}
