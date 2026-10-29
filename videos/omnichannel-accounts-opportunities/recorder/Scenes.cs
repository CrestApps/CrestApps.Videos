using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Omnichannel Accounts and Opportunities video, one method per "clip" of storyboard.json.
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
        ("02-accounts", AccountsAsync),
        ("03-join", JoinAsync),
        ("04-find", FindAsync),
        ("05-opportunity", OpportunityAsync),
        ("06-delete", DeleteAsync),
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
        // The starter recipe turns the CRM on and adds the contact and opportunity types in one run, so the account type's list
        // does not know them yet: tick them, as an administrator does, so an account's page lists its contacts and deals.
        await page.GotoAsync(BaseUrl + "/Admin/ContentTypes/Account/ContentParts/ListPart/Edit");
        await page.Locator("label").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*Contact\s*$") }).First.ClickAsync();
        await page.Locator("label").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*Sales Opportunity\s*$") }).First.ClickAsync();
        await page.RunAndWaitForNavigationAsync(() => page.Locator("button[name='submit.Save']").ClickAsync());
        Console.WriteLine("Configured the account list");

        await ImportCsvAsync(page, "Account", Path.Combine(DataFolder, "accounts.csv"));
        await Task.Delay(6000);
        await ImportCsvAsync(page, "Contact", Path.Combine(DataFolder, "account-contacts.csv"), "US");
        await Task.Delay(6000);
        await ImportCsvAsync(page, "SalesOpportunity", Path.Combine(DataFolder, "opportunities.csv"));
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

    private static ILocator Search(Scene scene) => scene.Page.Locator("input[type=search], #search-box, input[name$='Search']").First;

    // ----- Clips -----

    private static async Task AccountsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/Contact");
        await scene.StartCaptureAsync();

        await scene.StepAsync("accounts-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/Contents/ContentItems/Account"));
            await scene.HighlightAsync(Row(scene, "Summit Fitness Group"), "An account");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Summit Fitness Group").Locator("a.btn").Filter(new() { HasTextString = "List Items" }).First);
        });

        await scene.StepAsync("accounts-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("a.btn").Filter(new() { HasTextString = "Edit Account" }).First, "Edit Account");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 300);
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Filter(new() { HasText = "Priya Patel" }).First, "A contact of the account");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("accounts-3", async () =>
        {
            await scene.ScrollAsync(0, -400);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn").Filter(new() { HasTextString = "List Activities" }).First);
        });
    }

    private static async Task JoinAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/Contact");
        await scene.StartCaptureAsync();

        await scene.StepAsync("join-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Marcus Brooks").Locator("a.btn").Filter(new() { HasTextString = "Edit" }).First);
            await scene.HighlightAsync(scene.Page.Locator("#AccountPicker_AccountContentItemId-selector").Locator("xpath=ancestor::div[contains(@class,'mb-3') or contains(@class,'ocat') or contains(@class,'field')][1]"), "Account");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator(".ts-control, .choices, #AccountPicker_AccountContentItemId-selector").First, pauseAfter: 2500);
            await scene.Page.Keyboard.PressAsync("Escape");
        });

        await scene.StepAsync("join-2", async () =>
        {
            await scene.NavigateAsync("/Admin/Contents/ContentItems/SalesOpportunity?q=closed%3Afalse");
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Harbor espresso machines").Locator("a.btn").Filter(new() { HasTextString = "Edit" }).First);
            await scene.HighlightAsync(scene.Page.Locator("#AccountPicker_AccountContentItemId-selector").Locator("xpath=ancestor::div[contains(@class,'mb-3') or contains(@class,'ocat') or contains(@class,'field')][1]"), "The same picker");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task FindAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/Contact");
        await scene.StartCaptureAsync();

        await scene.StepAsync("find-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/Contents/ContentItems/SalesOpportunity?q=closed%3Afalse"));
            await scene.HighlightAsync(Row(scene, "Summit gym equipment"), "Account, stage, amount, probability, close date");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("find-2", async () =>
        {
            await Search(scene).FillAsync(string.Empty);
            await scene.TypeAsync(Search(scene), "account:\"Summit Fitness Group\"", delay: 55);
            await scene.WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"));
            await scene.PauseAsync(2200);
            await Search(scene).FillAsync(string.Empty);
            await scene.TypeAsync(Search(scene), "closed:false stage:Prospecting", delay: 55);
            await scene.WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"));
            await scene.PauseAsync(1500);
        });
    }

    private static async Task OpportunityAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/SalesOpportunity?q=closed%3Afalse");
        await scene.StartCaptureAsync();

        await scene.StepAsync("opportunity-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn").Filter(new() { HasTextString = "New Sales Opportunity" }).First);
            await scene.ClickAsync(scene.Page.Locator("button[data-id='AccountPicker_AccountContentItemId-selector']"), pauseAfter: 900);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = "Harbor Cafe Co" }).First, pauseAfter: 700);
            await scene.TypeAsync(scene.Page.Locator("#TitlePart_Title"), "Harbor catering ovens", delay: 40);
            await scene.Page.Locator("#OpportunityPart_Amount_Value").FillAsync("7500");
            await scene.Page.Locator("#OpportunityPart_CloseDate_Value").FillAsync("2027-01-15");
        });

        await scene.StepAsync("opportunity-2", async () =>
        {
            await scene.ScrollAsync(0, 300);
            await scene.HighlightAsync(scene.Page.Locator("#OpportunityPart_PrimaryContact_ContentItemIds").Locator("xpath=ancestor::div[contains(@class,'mb-3') or contains(@class,'ocat') or contains(@class,'field')][1]"), "Primary contact");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(scene.Page.Locator("#OpportunityPart_StageId"), "Proposal (50%)", pauseAfter: 900);
            await scene.HighlightAsync(scene.Page.Locator("#OpportunityPart_Probability"), "Starts at the stage's probability");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("opportunity-3", async () =>
        {
            await scene.ScrollAsync(0, 700);
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator("button[name='submit.Publish']").First, pauseAfter: 100));
            await scene.HighlightAsync(Row(scene, "Harbor catering ovens"), "The new opportunity");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task DeleteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/Contact");
        await scene.StartCaptureAsync();

        await scene.StepAsync("delete-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/Contents/ContentItems/Account"));
            await scene.ClickAsync(Row(scene, "Pinecrest Realty").Locator("button.dropdown-toggle").Filter(new() { HasTextString = "Actions" }).First, pauseAfter: 1000);
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show a, .dropdown-menu.show button").Filter(new() { HasTextString = "Delete" }).First, "Delete");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show a, .dropdown-menu.show button").Filter(new() { HasTextString = "Delete" }).First, pauseAfter: 1200);
            await scene.ClickAsync(scene.Page.Locator(".modal.show button, .modal.show a").Filter(new() { HasTextString = "Ok" }).First, pauseAfter: 1500);
        });

        await scene.StepAsync("delete-2", async () =>
        {
            await scene.NavigateAsync("/Admin/Contents/ContentItems/Contact");
            await scene.HighlightAsync(Row(scene, "Elena Rossi"), "Still a contact");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/Contents/ContentItems/SalesOpportunity?q=closed%3Afalse");
            await scene.HighlightAsync(Row(scene, "Pinecrest signage"), "Still an opportunity, without an account");
        });

        await scene.ClearHighlightsAsync();
    }
}
