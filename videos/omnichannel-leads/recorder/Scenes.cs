using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Omnichannel Leads video, one method per "clip" of storyboard.json.
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
        ("02-list", ListAsync),
        ("03-add", AddAsync),
        ("04-import", ImportAsync),
        ("05-work", WorkAsync),
        ("06-convert", ConvertAsync),
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

        

        await ImportCsvAsync(page, "Lead", Path.Combine(DataFolder, "leads.csv"), "US");
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

    private static async Task ListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/Contact");
        await scene.StartCaptureAsync();

        await scene.StepAsync("list-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/Contents/ContentItems/Lead?q=converted%3Afalse"));
            await scene.HighlightAsync(Row(scene, "Aaron Blake"), "Status, rating, company, source and list");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("list-2", async () =>
        {
            await scene.TypeAsync(Search(scene), "rating:hot", delay: 80);
            await scene.WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"));
            await scene.HighlightAsync(Row(scene, "Aaron Blake"), "The hot leads");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("list-3", async () =>
        {
            await Search(scene).FillAsync(string.Empty);
            await scene.TypeAsync(Search(scene), "list:\"Spring Import\" source:\"Trade show\"", delay: 55);
            await scene.WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"));
            await scene.PauseAsync(1500);
        });
    }

    private static async Task AddAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/Lead?q=converted%3Afalse");
        await scene.StartCaptureAsync();

        await scene.StepAsync("add-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn").Filter(new() { HasTextString = "New Lead" }).First);
            await scene.TypeAsync(scene.Page.Locator("#Lead_FirstName_Text"), "Rosa", delay: 60);
            await scene.TypeAsync(scene.Page.Locator("#Lead_LastName_Text"), "Delgado", delay: 45);
            await scene.HighlightAsync(scene.Page.Locator("#LeadPart_StatusId"), "Lead status");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("add-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#LeadPart_Company_Text"), "Delgado Auto Repair", delay: 30);
            await scene.HighlightAsync(scene.Page.Locator("#LeadPart_Source_ContentItemIds").Locator("xpath=ancestor::div[contains(@class,'mb-3') or contains(@class,'field-wrapper')][1]"), "Lead source");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#LeadPart_ListName_Text"), "Spring Import", delay: 30);
            await scene.SelectAsync(scene.Page.Locator("#LeadPart_Rating_Text"), "Warm", pauseAfter: 800);
            await scene.ScrollAsync(0, 900);
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator("button[name='submit.Publish']").First, pauseAfter: 100));
            await scene.HighlightAsync(Row(scene, "Rosa Delgado"), "The new lead");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ImportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/content-transfer-entries");
        await scene.StartCaptureAsync();

        await scene.StepAsync("import-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#new-dropdown"), pauseAfter: 800);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".dropdown-menu a.dropdown-item").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("^\\s*Lead\\s*$") }).First);
            await scene.ScrollAsync(0, 700);
            await scene.HighlightAsync(scene.Page.Locator("table.table tbody tr").Filter(new() { HasText = "LeadStatus" }).First, "LeadStatus");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("table.table tbody tr").Filter(new() { HasText = "LeadOwner" }).First, "LeadOwner");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("import-2", async () =>
        {
            await scene.ScrollAsync(0, -900);
            await scene.HighlightAsync(scene.Page.Locator("label").Filter(new() { HasTextString = "Skip numbers that already belong to a contact" }).First, "Skip numbers that belong to a contact");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("label").Filter(new() { HasTextString = "Skip numbers that already belong to an open lead" }).First, "Skip numbers that belong to an open lead");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("import-3", async () =>
        {
            await scene.ScrollAsync(0, 200);
            await scene.HighlightAsync(scene.Page.Locator("label").Filter(new() { HasTextString = "List name" }).First.Locator("xpath=ancestor::div[1]"), "List name");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("label").Filter(new() { HasTextString = "Lead owner" }).First.Locator("xpath=ancestor::div[1]"), "Lead owner");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task WorkAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/Lead?q=converted%3Afalse");
        await scene.StartCaptureAsync();

        await scene.StepAsync("work-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Mei Tanaka").Locator("a.btn").Filter(new() { HasTextString = "Edit" }).First);
            await scene.HighlightAsync(scene.Page.Locator("a.btn, button.btn").Filter(new() { HasTextString = "List Activities" }).First, "List Activities");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("work-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button, a").Filter(new() { HasTextString = "Add Activity" }).First, pauseAfter: 1000);
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show").First, "Outbound or Inbound");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ConvertAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/Lead?q=converted%3Afalse");
        await scene.StartCaptureAsync();

        await scene.StepAsync("convert-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Priya Patel").Locator("a.btn").Filter(new() { HasTextString = "Convert" }).First);
            await scene.HighlightAsync(scene.Page.Locator("label").Filter(new() { HasTextString = "Merge into" }).First, "Merge into the contact that has the same email");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("convert-2", async () =>
        {
            await scene.ScrollAsync(0, 400);
            await scene.HighlightAsync(scene.Page.Locator("input[name='AccountName'], #AccountName").First, "The account starts from the company");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("label[for='CreateOpportunity'], label").Filter(new() { HasTextString = "Create an opportunity" }).First, "Create an opportunity");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("convert-3", async () =>
        {
            await scene.ScrollAsync(0, 700);
            await scene.HighlightAsync(scene.Page.Locator("div.card, fieldset").Filter(new() { HasTextString = "Move the open activities" }).Last, "Open activities");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Convert" }).Last, pauseAfter: 100));
        });

        await scene.StepAsync("convert-4", async () =>
        {
            await scene.NavigateAsync("/Admin/Contents/ContentItems/Lead?q=converted%3Afalse");
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Aaron Blake").Locator("a.btn").Filter(new() { HasTextString = "Convert" }).First);
            await scene.SelectAsync(scene.Page.Locator("#ContactContentType"), "Contact", pauseAfter: 600);
            await scene.ScrollAsync(0, 400);
            await scene.ClickAsync(scene.Page.Locator("#CreateOpportunity"), pauseAfter: 800);
            await scene.SelectAsync(scene.Page.Locator("#OpportunityContentType"), "Sales Opportunity", pauseAfter: 500);
            await scene.TypeAsync(scene.Page.Locator("#OpportunityName"), "Riverbend dental chairs", delay: 30);
            await scene.Page.Locator("#OpportunityAmount").FillAsync("18500");
            await scene.Page.Locator("#OpportunityCloseDate").FillAsync("2026-12-15");
        });

        await scene.StepAsync("convert-5", async () =>
        {
            await scene.ScrollAsync(0, 900);
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Convert" }).Last, pauseAfter: 100));
            await scene.PauseAsync(1500);
        });
    }
}
