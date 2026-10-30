using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Content Transfer Columns video, one method per "clip" of storyboard.json.
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
        ("02-table", TableAsync),
        ("03-upload", UploadAsync),
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

        await ImportRecipeAsync(page, Path.Combine(Root, "seed", "showcase.recipe.json"));
        await Task.Delay(8000);
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

    private static ILocator Col(Scene scene, string column) => scene.Page.Locator("table.table tbody tr").Filter(new() { HasText = column }).First;

    private static async Task OpenImportAsync(Scene scene)
    {
        await scene.ClickAsync(scene.Page.Locator("#new-dropdown"), pauseAfter: 800);
        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".dropdown-menu a.dropdown-item").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*Showcase\s*$") }).First);
    }

    // ----- Clips -----

    private static async Task TableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/content-transfer-entries");
        await scene.StartCaptureAsync();

        await scene.StepAsync("table-1", async () =>
        {
            await OpenImportAsync(scene);
            await scene.ScrollAsync(0, 500);
            await scene.HighlightAsync(scene.Page.Locator("a").Filter(new() { HasTextString = "Download template" }).First, "A template with every column");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 400);
            await scene.HighlightAsync(Col(scene, "ContentItemId"), "Fill it in to update an existing item");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Col(scene, "TitlePart_Title"), "Required");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("table-2", async () =>
        {
            await scene.ScrollAsync(0, 300);
            await scene.HighlightAsync(Col(scene, "HtmlBodyPart_Html"), "HTML and Markdown bodies");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Col(scene, "AliasPart_Alias"), "Alias and URL path");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Col(scene, "ArchiveLaterPart_ScheduledArchiveUtc"), "Scheduled dates, in UTC");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Col(scene, "LiquidPart_Liquid"), "A Liquid template");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("table-3", async () =>
        {
            await scene.ScrollAsync(0, 350);
            await scene.HighlightAsync(Col(scene, "Showcase_Location_Text"), "Text");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Col(scene, "Showcase_Seats_Value"), "Number");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Col(scene, "Showcase_StartsAt_Value"), "Date and time");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Col(scene, "Showcase_Featured_Value"), "True or False");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("table-4", async () =>
        {
            await scene.ScrollAsync(0, 500);
            await scene.HighlightAsync(Col(scene, "Showcase_Speakers_Values"), "Several values separated by a bar");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Col(scene, "Showcase_Website_Url"), "A link has a URL, a text and a target");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Col(scene, "Showcase_RelatedEvents_ContentItemIds"), "Pickers use ids");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task UploadAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/content-transfer-entries");
        await scene.StartCaptureAsync();

        await scene.StepAsync("upload-1", async () =>
        {
            await OpenImportAsync(scene);
            await scene.Page.Locator("input[type=file]").SetInputFilesAsync(Path.Combine(DataFolder, "events.csv"));
            await scene.PauseAsync(2500);
            await scene.HighlightAsync(scene.Page.Locator("input[type=file]"), "Two events");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("upload-2", async () =>
        {
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator("#upload-actions button"), pauseAfter: 100));
            await scene.PauseAsync(6000);
            await scene.NavigateAsync("/Admin/content-transfer-entries");
            await scene.PauseAsync(2500);
        });

        await scene.StepAsync("upload-3", async () =>
        {
            await scene.NavigateAsync("/Admin/Contents/ContentItems/Showcase");
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Filter(new() { HasText = "Spring Open House" }).First, "The imported event");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
        });
    }
}
