using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Omnichannel Load Activities video, one method per "clip" of storyboard.json.
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
        ("03-form", FormAsync),
        ("04-run", RunAsync),
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
        await ImportCsvAsync(page, "Contact", Path.Combine(DataFolder, "omni-contacts.csv"), "US");
        await Task.Delay(6000);
        await ImportTemplateAsync(page, "omni-base-1.recipe.json");
        await ImportTemplateAsync(page, "omni-base-2.recipe.json");
        await ImportTemplateAsync(page, "omni-base-3.recipe.json");
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

    // Imports a recipe file of the video's seed folder whose @disp:Name@, @group:Name@ and @campaign:Name@ tokens stand for the ids of saved entries (not recorded).
    private static async Task ImportTemplateAsync(IPage page, string file)
    {
        var json = await File.ReadAllTextAsync(Path.Combine(Root, "seed", file));

        foreach (var (kind, path) in new[] { ("disp", "dispositions"), ("group", "campaign-groups"), ("campaign", "campaigns") })
        {
            await page.GotoAsync(BaseUrl + "/Admin/omnichannel/" + path);
            var pairs = await page.EvaluateAsync<string[][]>(
                @"() => [...document.querySelectorAll('.list-group-item')].map(li => {
                    const a = li.querySelector(""a[href*='/edit/']"");
                    return a ? [li.innerText.trim().split(String.fromCharCode(10))[0].trim(), a.getAttribute('href').split('/').pop()] : null;
                }).filter(x => x)");

            foreach (var pair in pairs)
            {
                json = json.Replace($"@{kind}:{pair[0]}@", pair[1]);
            }
        }

        var temp = Path.Combine(Path.GetTempPath(), file);
        await File.WriteAllTextAsync(temp, json);
        await ImportRecipeAsync(page, temp);
    }

    // Creates a manual activity load of contacts and loads it (not recorded).
    private static async Task CreateLoadAsync(IPage page, string title, string subject, string campaign, string urgency, string instructions = null, int? limit = null)
    {
        await page.GotoAsync(BaseUrl + "/Admin/omnichannel/activity/batches/create/Manual");
        await page.Locator("[id$='ActivityBatch_DisplayText']").FillAsync(title);
        await page.Locator("[id$='ActivityBatch_SubjectContentType']").SelectOptionAsync(new SelectOptionValue { Label = subject });
        await page.Locator("[id$='ActivityBatch_CampaignId']").SelectOptionAsync(new SelectOptionValue { Label = campaign });
        await page.Locator("[id$='ActivityBatch_Channel']").SelectOptionAsync(new SelectOptionValue { Label = "Phone" });
        await page.Locator("[id$='ActivityBatch_ContactContentType']").SelectOptionAsync(new SelectOptionValue { Label = "Contact" });
        await page.Locator("[id$='ActivityBatch_UrgencyLevel']").SelectOptionAsync(new SelectOptionValue { Label = urgency });

        if (instructions is not null)
        {
            await page.Locator("[id$='ActivityBatch_Instructions']").FillAsync(instructions);
        }

        if (limit is not null)
        {
            await page.Locator("[id$='ActivityBatch_Limit']").FillAsync(limit.Value.ToString());
        }

        await page.Locator("button[data-id='OmnichannelActivityBatch_UserIds_Selector']").ClickAsync();
        await page.Locator(".dropdown-menu.show input[type=search]").FillAsync("admin");
        await Task.Delay(1500);
        await page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = "admin" }).First.ClickAsync();
        await page.Keyboard.PressAsync("Escape");
        await page.Locator("button[name='submit.SaveAndLoad']").ClickAsync();
        await page.RunAndWaitForNavigationAsync(() => page.Locator(".modal.show button").Filter(new() { HasTextString = "Save & Load" }).First.ClickAsync());
        Console.WriteLine($"Loaded {title}");
        await Task.Delay(8000);
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

    private static ILocator Result(Scene scene) => scene.Page.Locator("#activity-load-count-result");

    private static ILocator RowActions(Scene scene, string title) => Row(scene, title).Locator("button.dropdown-toggle").Filter(new() { HasTextString = "Actions" }).First;

    // ----- Clips -----

    private static async Task ListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/Contact");
        await scene.StartCaptureAsync();

        await scene.StepAsync("list-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#left-nav button, #left-nav a").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*Management\s*$") }).First, pauseAfter: 800);
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/omnichannel/activity/batches"));
            await scene.HighlightAsync(scene.Page.Locator("button.create").First, "Add an activity load");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("list-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.create").First, pauseAfter: 1000);
            await scene.HighlightAsync(scene.Page.Locator("#modalAddActivityBatch .modal-content"), "The source of the load");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task FormAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/omnichannel/activity/batches");
        await scene.StartCaptureAsync();

        await scene.StepAsync("form-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.create").First, pauseAfter: 800);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalAddActivityBatch a[href$='/create/Manual']").First);
            await scene.TypeAsync(scene.Page.Locator("[id$='ActivityBatch_DisplayText']"), "Spring sales calls", delay: 50);
            await scene.SelectAsync(scene.Page.Locator("[id$='ActivityBatch_SubjectContentType']"), "Sales Call", pauseAfter: 600);
            await scene.SelectAsync(scene.Page.Locator("[id$='ActivityBatch_CampaignId']"), "Spring Sales Event", pauseAfter: 600);
            await scene.SelectAsync(scene.Page.Locator("[id$='ActivityBatch_Channel']"), "Phone", pauseAfter: 600);
        });

        await scene.StepAsync("form-2", async () =>
        {
            await scene.ScrollAsync(0, 400);
            await scene.ClickAsync(scene.Page.Locator("button[data-id='OmnichannelActivityBatch_UserIds_Selector']"), pauseAfter: 700);
            await scene.Page.Locator(".dropdown-menu.show input[type=search]").FillAsync("admin");
            await scene.PauseAsync(1500);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = "admin" }).First, pauseAfter: 600);
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.SelectAsync(scene.Page.Locator("[id$='ActivityBatch_UrgencyLevel']"), "High", pauseAfter: 600);
            await scene.TypeAsync(scene.Page.Locator("[id$='ActivityBatch_Instructions']"), "Ask about the spring service offer.", delay: 40);
            await scene.HighlightAsync(scene.Page.Locator("[id$='ActivityBatch_PreventDuplicates']").Locator("xpath=ancestor::div[contains(@class,'mb-3') or contains(@class,'form-check')][1]"), "Prevent duplicate activity with the same subject");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("form-3", async () =>
        {
            await scene.ScrollAsync(0, 500);
            await scene.SelectAsync(scene.Page.Locator("[id$='ActivityBatch_ContactContentType']"), "Contact", pauseAfter: 800);
            await scene.ScrollAsync(0, 500);
            await scene.HighlightAsync(scene.Page.Locator("[id$='ActivityBatch_PhoneNumberMatchType']").Locator("xpath=ancestor::div[contains(@class,'mb-3')][1]"), "Phone number filter");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 400);
            await scene.ClickAsync(scene.Page.Locator("#activity-load-count-button"), pauseAfter: 2500);
            await scene.HighlightAsync(Result(scene), "How many records match");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("form-4", async () =>
        {
            await scene.ScrollAsync(0, 600);
            await scene.HighlightAsync(scene.Page.Locator("[id$='ActivityBatch_LastActivityDispositionId']").Locator("xpath=ancestor::div[contains(@class,'mb-3')][1]"), "Last activity disposition");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("[id$='ActivityBatch_LastActivityAttempts']"), "2+", delay: 100);
            await scene.ClickAsync(scene.Page.Locator("#activity-load-count-button"), pauseAfter: 2500);
            await scene.HighlightAsync(Result(scene), "No record has finished an activity yet");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.Page.Locator("[id$='ActivityBatch_LastActivityAttempts']").FillAsync(string.Empty);
        });

        await scene.StepAsync("form-5", async () =>
        {
            await scene.ScrollAsync(0, 600);
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator("button[type=submit]").Filter(new() { HasTextString = "Save" }).First, pauseAfter: 100));
            await scene.HighlightAsync(Row(scene, "Spring sales calls"), "A saved load does nothing yet");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/omnichannel/activity/batches");
        await scene.StartCaptureAsync();

        await scene.StepAsync("run-1", async () =>
        {
            await scene.ClickAsync(RowActions(scene, "Spring sales calls"), pauseAfter: 1000);
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show a, .dropdown-menu.show button").Filter(new() { HasTextString = "Load activities" }).First, "Load activities");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show a, .dropdown-menu.show button").Filter(new() { HasTextString = "Load activities" }).First, pauseAfter: 1000);
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator(".modal.show button, .modal.show a").Filter(new() { HasTextString = "Ok" }).First, pauseAfter: 100));
            await scene.PauseAsync(9000);
            await scene.NavigateAsync("/Admin/omnichannel/activity/batches");
            await scene.HighlightAsync(Row(scene, "Spring sales calls"), "Loaded: how many, and why the others were skipped");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.ClickAsync(RowActions(scene, "Spring sales calls"), pauseAfter: 1000);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".dropdown-menu.show a").Filter(new() { HasTextString = "Clone" }).First);
            await scene.HighlightAsync(scene.Page.Locator("[id$='ActivityBatch_DisplayText']"), "A copy with every setting");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 1500);
            await scene.ClickAsync(scene.Page.Locator("#activity-load-count-button"), pauseAfter: 2500);
            await scene.HighlightAsync(Result(scene), "Already has an open activity");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("run-3", async () =>
        {
            await scene.NavigateAsync("/Admin/omnichannel/activity/batches");
            var loaded = scene.Page.Locator(".list-group-item").Filter(new() { HasText = "Loaded 8 of 8" }).First;
            await scene.HighlightAsync(loaded.Locator("a.btn-danger, button.btn-danger").First, "Delete a finished load");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(loaded.Locator("a.btn-danger, button.btn-danger").First, pauseAfter: 1200);
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator(".modal.show button, .modal.show a").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*(Ok|Delete|Yes)\s*$") }).Last, pauseAfter: 100));
            await scene.PauseAsync(1500);
            await scene.NavigateAsync("/Admin/omnichannel/activities");
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Filter(new() { HasText = "Sales Call" }).First, "The activities stay");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });
    }
}
