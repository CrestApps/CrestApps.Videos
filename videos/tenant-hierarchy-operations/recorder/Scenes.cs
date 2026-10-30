using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Tenant Hierarchy Operations video, one method per "clip" of storyboard.json.
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
        ("03-failed", FailedAsync),
        ("04-bulk", BulkAsync),
        ("05-switch", SwitchAsync),
        ("06-platform", PlatformAsync),
        ("07-changed", ChangedAsync),
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

        {
            foreach (var feature in new[]
            {
                "OrchardCore.Tenants",
                "CrestApps.OrchardCore.TenantHierarchy.Platform",
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

            await Task.Delay(4000);

            // The firm: a tenant at its own address, set up like any new site, then made a parent.
            await page.GotoAsync(BaseUrl + "/Admin/Tenants/Create");
            await page.Locator("#Name").FillAsync("firm");
            await page.Locator("#RequestUrlHost").FillAsync("firm.localhost");
            await page.Locator("#RecipeName").SelectOptionAsync(new SelectOptionValue { Label = "Blank site" });
            await page.Locator("#DatabaseProvider").SelectOptionAsync(new SelectOptionValue { Label = "Sqlite" });
            await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("Create|Save") }).Last.ClickAsync());
            await page.Locator("#SiteName").FillAsync("Hartwell Bookkeeping");
            await page.Locator("#UserName").FillAsync("admin");
            await page.Locator("#Email").FillAsync("admin@example.com");
            await page.Locator("#Password").FillAsync(Password);
            await page.Locator("#PasswordConfirmation").FillAsync(Password);
            await page.Locator("form button[type=submit], form input[type=submit]").Last.ClickAsync();
            await Task.Delay(30000);
            Console.WriteLine("Set up the firm");

            await page.GotoAsync(BaseUrl + "/Admin/tenant-hierarchy/make-parent");
            await page.Locator("#TenantName").SelectOptionAsync(new SelectOptionValue { Label = "firm (Running)" });
            await Task.Delay(800);
            await page.Locator("#DisplayName").FillAsync("Hartwell Bookkeeping");
            await page.Locator("#MaxChildren").FillAsync("10");
            await page.Locator("#ParentLabel").FillAsync("Practice");
            await page.Locator("#ChildLabel").FillAsync("Client");
            await page.Locator("#ChildrenLabel").FillAsync("Clients");
            await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Make parent" }).First.ClickAsync());
            Console.WriteLine("Made the firm a parent");

            // The firm's own sign-in, for the clips that work in the firm.
            var firm = await browser.NewContextAsync(new BrowserNewContextOptions { IgnoreHTTPSErrors = true });
            var login = await firm.NewPageAsync();
            await login.GotoAsync("https://firm.localhost:5376/Login");
            await login.Locator("#LoginForm_UserName").FillAsync(UserName);
            await login.Locator("#LoginForm_Password").FillAsync(Password);
            await login.Locator("#LoginForm_Password").PressAsync("Enter");
            await login.WaitForURLAsync(url => !url.Contains("/Login", StringComparison.OrdinalIgnoreCase));
            await firm.StorageStateAsync(new BrowserContextStorageStateOptions { Path = Path.Combine(Root, "state-firm.json") });
            await firm.CloseAsync();
            Console.WriteLine("Signed in to the firm");
        }

        {
            // The firm's clients, added the way the firm's team adds them.
            var firm = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = Path.Combine(Root, "state-firm.json"), IgnoreHTTPSErrors = true, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
            var team = await firm.NewPageAsync();
            team.SetDefaultTimeout(60_000);

            foreach (var (name, recipe) in new[]
            {
                ("Northwind Traders", "Blank"),
                ("Bluebird Cafe", "Blank"),
                ("Maple Dental", "Blank"),
                ("Riverside Gym", "Blank"),
                ("Summit Realty", "Blank"),
                ("Failing Co", "SaaS"),
            })
            {
                await team.GotoAsync("https://firm.localhost:5376/Admin/children/create");
                await team.Locator("#DisplayName").FillAsync(name);
                await Task.Delay(1500);
                await team.Locator("#recipe-" + recipe).CheckAsync(new() { Force = true });
                await team.RunAndWaitForNavigationAsync(() => team.Locator("form button[type=submit]").Filter(new() { HasTextString = "Create" }).Last.ClickAsync());
                Console.WriteLine($"Added {name}");
                await Task.Delay(5000);
            }

            await Task.Delay(60000);
            await team.GotoAsync("https://firm.localhost:5376/Admin/children");
            Console.WriteLine(await team.Locator("body").InnerTextAsync());
            await firm.CloseAsync();
        }

        // Then make-states.py (seed folder) marks one client as failed and removes the lakeside parent, with the site stopped.
        {
            // A second parent with one child tenant: the clips leave that child without its parent.
            await page.GotoAsync(BaseUrl + "/Admin/Tenants/Create");
            await page.Locator("#Name").FillAsync("lakeside");
            await page.Locator("#RequestUrlHost").FillAsync("lakeside.localhost");
            await page.Locator("#RecipeName").SelectOptionAsync(new SelectOptionValue { Label = "Blank site" });
            await page.Locator("#DatabaseProvider").SelectOptionAsync(new SelectOptionValue { Label = "Sqlite" });
            await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("Create|Save") }).Last.ClickAsync());
            await page.Locator("#SiteName").FillAsync("Lakeside Accounting");
            await page.Locator("#UserName").FillAsync("admin");
            await page.Locator("#Email").FillAsync("admin@example.com");
            await page.Locator("#Password").FillAsync(Password);
            await page.Locator("#PasswordConfirmation").FillAsync(Password);
            await page.Locator("form button[type=submit], form input[type=submit]").Last.ClickAsync();
            await Task.Delay(30000);
            await page.GotoAsync(BaseUrl + "/Admin/tenant-hierarchy/make-parent");
            await page.Locator("#TenantName").SelectOptionAsync(new SelectOptionValue { Label = "lakeside (Running)" });
            await Task.Delay(800);
            await page.Locator("#DisplayName").FillAsync("Lakeside Accounting");
            await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Make parent" }).First.ClickAsync());
            Console.WriteLine("Made lakeside a parent");

            var lake = await browser.NewContextAsync(new BrowserNewContextOptions { IgnoreHTTPSErrors = true });
            var lakePage = await lake.NewPageAsync();
            lakePage.SetDefaultTimeout(60_000);
            await lakePage.GotoAsync("https://lakeside.localhost:5376/Login");
            await lakePage.Locator("#LoginForm_UserName").FillAsync(UserName);
            await lakePage.Locator("#LoginForm_Password").FillAsync(Password);
            await lakePage.Locator("#LoginForm_Password").PressAsync("Enter");
            await lakePage.WaitForURLAsync(url => !url.Contains("/Login", StringComparison.OrdinalIgnoreCase));
            await lakePage.GotoAsync("https://lakeside.localhost:5376/Admin/children/create");
            await lakePage.Locator("#DisplayName").FillAsync("Cedar Florist");
            await Task.Delay(1500);
            await lakePage.Locator("#recipe-Blank").CheckAsync(new() { Force = true });
            await lakePage.RunAndWaitForNavigationAsync(() => lakePage.Locator("form button[type=submit]").Filter(new() { HasTextString = "Create" }).Last.ClickAsync());
            await Task.Delay(40000);
            Console.WriteLine("Added Cedar Florist");
            await lake.CloseAsync();

            // Two more parents with no clients: the clips move a client to the first, and make the second an ordinary tenant again.
            foreach (var (tenant, title) in new[] { ("northridge", "Northridge Tax"), ("emptyco", "Empty Practice") })
            {
                await page.GotoAsync(BaseUrl + "/Admin/Tenants/Create");
                await page.Locator("#Name").FillAsync(tenant);
                await page.Locator("#RequestUrlHost").FillAsync(tenant + ".localhost");
                await page.Locator("#RecipeName").SelectOptionAsync(new SelectOptionValue { Label = "Blank site" });
                await page.Locator("#DatabaseProvider").SelectOptionAsync(new SelectOptionValue { Label = "Sqlite" });
                await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("Create|Save") }).Last.ClickAsync());
                await page.Locator("#SiteName").FillAsync(title);
                await page.Locator("#UserName").FillAsync("admin");
                await page.Locator("#Email").FillAsync("admin@example.com");
                await page.Locator("#Password").FillAsync(Password);
                await page.Locator("#PasswordConfirmation").FillAsync(Password);
                await page.Locator("form button[type=submit], form input[type=submit]").Last.ClickAsync();
                await Task.Delay(30000);
                await page.GotoAsync(BaseUrl + "/Admin/tenant-hierarchy/make-parent");
                await page.Locator("#TenantName").SelectOptionAsync(new SelectOptionValue { Label = tenant + " (Running)" });
                await Task.Delay(800);
                await page.Locator("#DisplayName").FillAsync(title);
                await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Make parent" }).First.ClickAsync());
                Console.WriteLine($"Made {tenant} a parent");
            }
        }
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

    private const string FirmUrl = "https://firm.localhost:5376";

    private static Task<Scene> NewFirmSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), FirmUrl, Path.Combine(Root, "state-firm.json"), Path.GetDirectoryName(directory), durations);

    // Confirms a dialog by its main button (not recorded as a choice of the narration).
    private static async Task ConfirmAsync(Scene scene)
    {
        await scene.PauseAsync(1500);
        var button = scene.Page.Locator(".modal.show .btn-primary, .modal.show .btn-danger, .modal.show .btn-success").Last;
        await scene.WaitForNavigationAsync(() => scene.ClickAsync(button, pauseAfter: 100));
    }

    private static async Task OpenClientsMenuAsync(Scene scene)
    {
        if (!await Menu(scene, "/Admin/children").IsVisibleAsync())
        {
            await scene.ClickAsync(scene.Page.Locator("#left-nav button").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*Clients\s*$") }).First, pauseAfter: 900);
        }
    }

    private static ILocator Client(Scene scene, string name) => scene.Page.Locator(".list-group-item").Filter(new() { HasText = name }).First;

    private static ILocator ToolbarMenu(Scene scene, string name) => scene.Page.Locator("button.dropdown-toggle").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*" + name + @"\s*$") }).First;

    private static async Task ChooseAsync(Scene scene, string menu, string choice)
    {
        await scene.ClickAsync(ToolbarMenu(scene, menu), pauseAfter: 1000);
        await scene.WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show a, .dropdown-menu.show button, .dropdown-menu.show label, .dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = choice }).First, pauseAfter: 100));
        await scene.PauseAsync(2200);
    }

    private static Task ChooseStateAsync(Scene scene, string state) => ChooseAsync(scene, "State", state);

    // ----- Clips -----

    private static async Task ListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewFirmSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("list-1", async () =>
        {
            await OpenClientsMenuAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/children"));
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Nth(1), "Name, address, state");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(ToolbarMenu(scene, "State"), "Filter by state");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("list-2", async () =>
        {
            await scene.ClickAsync(ToolbarMenu(scene, "Sort"), pauseAfter: 1000);
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show").First, "Name, newest first, or state");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.Page.Keyboard.PressAsync("Escape");
            await ChooseStateAsync(scene, "Setup failed");
            await scene.HighlightAsync(Client(scene, "Failing Co"), "Retry or Discard");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task FailedAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewFirmSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/children?Options.State=Failed");
        await scene.StartCaptureAsync();

        await scene.StepAsync("failed-1", async () =>
        {
            await scene.HighlightAsync(Client(scene, "Failing Co").Locator("a").Filter(new() { HasTextString = "Retry" }).First, "Try the setup again");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Client(scene, "Failing Co").Locator("a").Filter(new() { HasTextString = "Discard" }).First, "Give up on it");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("failed-2", async () =>
        {
            await scene.ClickAsync(Client(scene, "Failing Co").Locator("a").Filter(new() { HasTextString = "Discard" }).First, pauseAfter: 1500);
            await ConfirmAsync(scene);
            await scene.PauseAsync(2500);
        });
    }

    private static async Task BulkAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewFirmSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/children");
        await scene.StartCaptureAsync();

        await scene.StepAsync("bulk-1", async () =>
        {
            await Client(scene, "Bluebird Cafe").Locator("input[type=checkbox]").First.CheckAsync(new() { Force = true });
            await Client(scene, "Maple Dental").Locator("input[type=checkbox]").First.CheckAsync(new() { Force = true });
            await scene.PauseAsync(1500);
            await scene.ClickAsync(scene.Page.Locator("#bulk-action-menu-button"), pauseAfter: 1200);
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu").Filter(new() { HasText = "Reload" }).First, "Suspend, Resume, Reload, Remove");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("bulk-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu a, .dropdown-menu button").Filter(new() { HasTextString = "Suspend" }).First, pauseAfter: 1500);
            await ConfirmAsync(scene);
            await scene.PauseAsync(4000);
            await ChooseStateAsync(scene, "Suspended");
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Nth(1), "Both are suspended");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("bulk-3", async () =>
        {
            await scene.Page.Locator("#select-all").CheckAsync(new() { Force = true });
            await scene.PauseAsync(1200);
            await scene.ClickAsync(scene.Page.Locator("#bulk-action-menu-button"), pauseAfter: 1200);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu a, .dropdown-menu button").Filter(new() { HasTextString = "Resume" }).First, pauseAfter: 1500);
            await ConfirmAsync(scene);
            await scene.PauseAsync(4000);
            await ChooseStateAsync(scene, "Running");
        });
    }

    private static async Task SwitchAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewFirmSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("switch-1", async () =>
        {
            await OpenClientsMenuAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/children/switch"));
            await scene.HighlightAsync(scene.Page.Locator("a[href*='/delegated-access/open/']").First, "Open it, signed in");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button[title='Add to favorites']").Nth(2), pauseAfter: 1500);
            await scene.PauseAsync(2500);
        });

        await scene.StepAsync("switch-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Sign out of every" }).First, "Ends every session, on every device");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task PlatformAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/tenant-hierarchy");
        await scene.StartCaptureAsync();

        await scene.StepAsync("platform-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item, .card").Filter(new() { HasText = "Hartwell Bookkeeping" }).First, "A parent with its clients");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 600);
            await scene.HighlightAsync(scene.Page.Locator("a, button").Filter(new() { HasTextString = "Make ordinary" }).First, "An orphan: Move or Make ordinary");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("platform-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("a, button").Filter(new() { HasTextString = "Make ordinary" }).First, pauseAfter: 1500);
            await ConfirmAsync(scene);
            await scene.PauseAsync(3000);
        });

        await scene.StepAsync("platform-3", async () =>
        {
            await scene.NavigateAsync("/Admin/tenant-hierarchy/parents/firm");
            var row = scene.Page.Locator(".list-group-item").Filter(new() { HasText = "Riverside Gym" }).First;
            await scene.HighlightAsync(row, "Move it to another parent");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(row.Locator("select[name='newParent']"), "Northridge Tax", pauseAfter: 1000);
            scene.Page.Dialog += async (_, dialog) => await dialog.AcceptAsync();
            await scene.MoveToAsync(row.Locator("button[type=submit]").First);
            await scene.PauseAsync(800);
            await scene.WaitForNavigationAsync(() => row.Locator("button[type=submit]").First.ClickAsync());
            await scene.PauseAsync(1500);
            if (await scene.Page.Locator(".modal.show").CountAsync() > 0)
            {
                await ConfirmAsync(scene);
            }

            await scene.PauseAsync(3000);
        });

        await scene.StepAsync("platform-4", async () =>
        {
            await scene.NavigateAsync("/Admin/tenant-hierarchy/parents/emptyco");
            await scene.HighlightAsync(scene.Page.Locator("a, button").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*Make ordinary\s*$") }).First, "Only for a parent with no clients");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("a, button").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*Make ordinary\s*$") }).First, pauseAfter: 1500);
            await ConfirmAsync(scene);
            await scene.PauseAsync(3000);
        });
    }

    private static async Task ChangedAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewFirmSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/children");
        await scene.StartCaptureAsync();

        await scene.StepAsync("changed-1", async () =>
        {
            await ChooseStateAsync(scene, "Changed by platform");
            await scene.HighlightAsync(Client(scene, "Summit Realty"), "Changed by the platform");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("changed-2", async () =>
        {
            await scene.ClickAsync(Client(scene, "Summit Realty").Locator("a").Filter(new() { HasTextString = "Remove from list" }).First, pauseAfter: 1500);
            await ConfirmAsync(scene);
            await scene.PauseAsync(3000);
        });
    }
}
