using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Omnichannel Contacts video, one method per "clip" of storyboard.json.
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
        ("02-type", TypeAsync),
        ("03-create", CreateAsync),
        ("04-find", FindAsync),
        ("05-work", WorkAsync),
        ("06-transfer", TransferAsync),
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
            "CrestApps.OrchardCore.ContentTransfer",
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

        

        await RunRecipeAsync(page, "Omnichannel CRM starter");
        await ImportCsvAsync(page, "Contact", Path.Combine(DataFolder, "contacts.csv"), "US");
        await Task.Delay(6000);

        await context.CloseAsync();
    }

    // ----- Helpers -----

    // Runs a recipe of Tools > Recipes by its name (not recorded).
    private static async Task RunRecipeAsync(IPage page, string name)
    {
        await page.GotoAsync(BaseUrl + "/Admin/Recipes");
        var run = page.Locator($"xpath=//*[contains(normalize-space(text()),'{name}')]/ancestor::*[.//a[starts-with(@id,'btn-run-')]][1]//a[starts-with(@id,'btn-run-')]").First;
        await run.ClickAsync();
        await page.Locator(".modal.show button, .modal.show a").Filter(new() { HasTextString = "Yes" }).First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(12000);
        Console.WriteLine($"Ran the {name} recipe");
    }

    // Imports a CSV through Content > Import (not recorded).
    private static async Task ImportCsvAsync(IPage page, string contentType, string path, string leadCountry = null)
    {
        await page.GotoAsync(BaseUrl + $"/Admin/import/contents/{contentType}");
        await page.Locator("input[type=file]").SetInputFilesAsync(path);

        if (leadCountry is not null)
        {
            var country = page.Locator("select[id*='LeadCountry'], select[name*='LeadCountry'], select[name*='Country']").First;
            if (await country.CountAsync() > 0)
            {
                await country.SelectOptionAsync(leadCountry);
            }
        }

        await page.RunAndWaitForNavigationAsync(() => page.Locator("#upload-actions button").ClickAsync());
        Console.WriteLine($"Uploaded {Path.GetFileName(path)}");
    }

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

    private static ILocator Row(Scene scene, string text) => scene.Page.Locator(".list-group-item").Filter(new() { HasText = text }).First;

    // ----- Clips -----

    private static async Task TypeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.StartCaptureAsync();

        await scene.StepAsync("type-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ContentTypes/List"), "Design > Content Definition > Content Types");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/ContentTypes/Edit/Contact']").Last);
            await scene.ScrollAsync(0, 700);
        });

        await scene.StepAsync("type-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/ContentTypes/Contact/ContentParts/OmnichannelContactPart/Edit']").First);
            await scene.HighlightAsync(scene.Page.Locator("input[id$='_AutoDetectTimeZone']").Locator("xpath=.."), "Auto detect time zone");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("input[id$='_RequireTimeZone']").Locator("xpath=.."), "Require time zone");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("type-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("input[id$='_UseDoNotCall']").Locator("xpath=.."), "Use Do not call");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("input[id$='_UseDoNotSms']").Locator("xpath=.."), "Use Do not SMS");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("input[id$='_UseDoNotEmail']").Locator("xpath=.."), "Use Do not email");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("type-4", async () =>
        {
            await scene.NavigateAsync("/Admin/ContentTypes/Edit/Contact");
            await scene.ScrollAsync(0, 700);
            await scene.HighlightAsync(scene.Page.Locator("li, .list-group-item, tr, div.row").Filter(new() { HasTextString = "Contact Methods" }).Last, "Contact Methods");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/Contact");
        await scene.StartCaptureAsync();

        await scene.StepAsync("create-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Contents/ContentItems/Contact"), "Interaction Center > Contacts");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(scene, "Noah Fischer"), "A contact");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn").Filter(new() { HasTextString = "New Contact" }).First);
        });

        await scene.StepAsync("create-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#Contact_FirstName_Text"), "Dana", delay: 50);
            await scene.TypeAsync(scene.Page.Locator("#Contact_LastName_Text"), "Whitfield", delay: 40);
            await scene.TypeAsync(scene.Page.Locator("#Contact_JobTitle_Text"), "Purchasing Agent", delay: 25);
            await scene.HighlightAsync(scene.Page.Locator("#AccountPicker_AccountContentItemId-selector").Locator("xpath=ancestor::div[contains(@class,'mb-3') or contains(@class,'ocat')][1]"), "Account");
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("create-3", async () =>
        {
            await scene.ScrollAsync(0, 500);
            await scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Add Item" }).First, pauseAfter: 700);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show a").Filter(new() { HasTextString = "Phone Number" }).First, pauseAfter: 1200);
            await scene.TypeAsync(scene.Page.Locator("input[type=tel]").First, "602 555 0142", delay: 60);
            await scene.SelectAsync(scene.Page.Locator("select[id$='PhoneNumberInfoPart_Type_Text']").First, "Cell", pauseAfter: 600);
        });

        await scene.StepAsync("create-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#OmnichannelContactPart_TimeZoneId"), "Contact time zone");
            await scene.PauseAsync(3200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#OmnichannelContactPart_DoNotCall"), pauseAfter: 700);
            await scene.HighlightAsync(scene.Page.Locator("#OmnichannelContactPart_DoNotCall").Locator("xpath=.."), "Do not call");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#OmnichannelContactPart_DoNotCall"), pauseAfter: 300);
        });

        await scene.StepAsync("create-5", async () =>
        {
            await scene.ScrollAsync(0, 700);
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator("button[name='submit.Publish']").First, pauseAfter: 100));
            await scene.HighlightAsync(Row(scene, "Dana Whitfield"), "The new contact");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task FindAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/Contact");
        await scene.StartCaptureAsync();

        var search = scene.Page.Locator("input[type=search], #search-box, input[name$='Search']").First;

        await scene.StepAsync("find-1", async () =>
        {
            await scene.TypeAsync(search, "Elena", delay: 90);
            await scene.WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"));
            await scene.HighlightAsync(Row(scene, "Elena Rossi"), "Found by name");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("find-2", async () =>
        {
            await scene.Page.Locator("input[type=search], #search-box, input[name$='Search']").First.FillAsync(string.Empty);
            await scene.TypeAsync(scene.Page.Locator("input[type=search], #search-box, input[name$='Search']").First, "702555", delay: 120);
            await scene.WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"));
            await scene.HighlightAsync(Row(scene, "Noah Fischer"), "Begins with 702555");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("find-3", async () =>
        {
            await scene.Page.Locator("input[type=search], #search-box, input[name$='Search']").First.FillAsync(string.Empty);
            await scene.TypeAsync(scene.Page.Locator("input[type=search], #search-box, input[name$='Search']").First, "phone-ends:0104", delay: 80);
            await scene.WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"));
            await scene.HighlightAsync(Row(scene, "Elena Rossi"), "Ends with 0104");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button:has(i[class*='filter'])").First, pauseAfter: 1500);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show a, .dropdown-menu.show button").Filter(new() { HasTextString = "Filter syntax" }).First, pauseAfter: 3500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task WorkAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/Contact");
        await scene.StartCaptureAsync();

        await scene.StepAsync("work-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Elena Rossi").Locator("a.btn").Filter(new() { HasTextString = "Edit" }).First);
            await scene.HighlightAsync(scene.Page.Locator("a.btn, button.btn").Filter(new() { HasTextString = "List Activities" }).First, "List Activities");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("work-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn, button.btn").Filter(new() { HasTextString = "List Activities" }).First);
        });

        await scene.StepAsync("work-3", async () =>
        {
            await scene.Page.GoBackAsync();
            await scene.PauseAsync(800);
            await scene.ClickAsync(scene.Page.Locator("button, a").Filter(new() { HasTextString = "Add Activity" }).First, pauseAfter: 1000);
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show").First, "Outbound or Inbound");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task TransferAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/content-transfer-entries");
        await scene.StartCaptureAsync();

        await scene.StepAsync("transfer-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#new-dropdown"), pauseAfter: 900);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".dropdown-menu a.dropdown-item").Filter(new() { HasTextString = "Contact" }).First);
            await scene.ScrollAsync(0, 150);
            await scene.HighlightAsync(scene.Page.Locator("label").Filter(new() { HasTextString = "Ignore duplicate by phone number" }).First, "Ignore duplicate by phone number");
            await scene.PauseAsync(2800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#ImportContent_DuplicateScope").Locator("xpath=ancestor::div[1]"), "Compare with");
            await scene.PauseAsync(2800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#ImportContent_SelectedCountryCode").Locator("xpath=ancestor::div[1]"), "Lead country");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("transfer-2", async () =>
        {
            await scene.NavigateAsync("/Admin/export/contents");
            await scene.ClickAsync(scene.Page.Locator(".action-bar button.btn-secondary").Last, pauseAfter: 800);
            await scene.SelectAsync(scene.Page.Locator("#contentTypeId"), "Contact", pauseAfter: 1200);
            await scene.ScrollAsync(0, 250);
            await scene.HighlightAsync(scene.Page.Locator("label").Filter(new() { HasTextString = "Include last completed activity information" }).First, "CRM last activity");
        });

        await scene.ClearHighlightsAsync();
    }
}
