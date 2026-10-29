using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Omnichannel CRM Setup video, one method per "clip" of storyboard.json.
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
        ("03-recipe", RecipeAsync),
        ("04-statuses", StatusesAsync),
        ("05-stages", StagesAsync),
        ("06-sources", SourcesAsync),
        ("07-types", TypesAsync),
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

        foreach (var feature in new string[]
        {
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

    // ----- Clips -----

    private static async Task RecipeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();

        await scene.StepAsync("recipe-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/Recipes"));
            await scene.TypeAsync(scene.Page.Locator("input[type=search], input[name$='Search'], #search-box").First, "CRM", delay: 90);
            await scene.HighlightAsync(scene.Page.Locator("a[id^='btn-run-']:visible").First.Locator("xpath=ancestor::*[contains(@class,'list-group-item') or contains(@class,'card')][1]"), "Omnichannel CRM starter");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("recipe-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("a[id^='btn-run-']:visible").First, pauseAfter: 1200);
            await scene.ClickAsync(scene.Page.Locator(".modal.show button, .modal.show a").Filter(new() { HasTextString = "Yes" }).First, pauseAfter: 500);
            await scene.OffCameraAsync(async () =>
            {
                await scene.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                await Task.Delay(8000);
            });
        });

        await scene.StepAsync("recipe-3", async () =>
        {
            await scene.NavigateAsync("/Admin/Recipes");
            await scene.PauseAsync(800);
            await scene.HighlightAsync(scene.Page.Locator("#left-nav a.item-label, #left-nav .title").Filter(new() { HasTextString = "Interaction Center" }).First, "Interaction Center");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task StatusesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/omnichannel/cadences");
        await scene.StartCaptureAsync();

        await scene.StepAsync("statuses-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/omnichannel/lead-statuses"), "Interaction Center > Management > Lead Statuses");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/omnichannel/lead-statuses"));
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Filter(new() { HasText = "Open - Not Contacted" }).First, "Open, and the default");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("statuses-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Filter(new() { HasText = "Closed - Not Converted" }).First, "Closed");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Filter(new() { HasText = "A lead takes this status when it is converted" }).First, "Converted");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("statuses-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn").Filter(new() { HasTextString = "Add Lead Status" }).First);
            await scene.TypeAsync(scene.Page.Locator("#LeadStatus_Name"), "Meeting Booked", delay: 45);
            await scene.TypeAsync(scene.Page.Locator("#LeadStatus_Description"), "A meeting with the lead is on the calendar.", delay: 12);
            await scene.Page.Locator("#LeadStatus_Order").FillAsync("25");
            await scene.HighlightAsync(scene.Page.Locator("#LeadStatus_Kind"), "Type");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }

    private static async Task StagesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/omnichannel/cadences");
        await scene.StartCaptureAsync();

        await scene.StepAsync("stages-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/omnichannel/opportunity-stages"), "Interaction Center > Management > Opportunity Stages");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/omnichannel/opportunity-stages"));
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Nth(1), "Name, order and probability");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("stages-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Filter(new() { HasText = "Won" }).First, "Closed and won");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Filter(new() { HasText = "Lost" }).First, "Closed, not won");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("stages-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn").Filter(new() { HasTextString = "Add Opportunity Stage" }).First);
            await scene.TypeAsync(scene.Page.Locator("#OpportunityStage_Name"), "Contract Review", delay: 45);
            await scene.Page.Locator("#OpportunityStage_Order").FillAsync("45");
            await scene.Page.Locator("#OpportunityStage_Probability").FillAsync("75");
            await scene.HighlightAsync(scene.Page.Locator("#OpportunityStage_Probability"), "Probability");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }

    private static async Task SourcesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/omnichannel/cadences");
        await scene.StartCaptureAsync();

        await scene.StepAsync("sources-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Contents/ContentItems/LeadSource"), "Interaction Center > Management > Lead Sources");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/Contents/ContentItems/LeadSource"));
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Filter(new() { HasText = "Trade show" }).First, "Trade show");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("sources-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn, button.btn").Filter(new() { HasTextString = "New" }).First);
            await scene.TypeAsync(scene.Page.Locator("#TitlePart_Title"), "Partner event", delay: 50);
            await scene.ClickAsync(scene.Page.Locator("button[name='submit.Publish']").First, pauseAfter: 100);
            await scene.PauseAsync(2500);
        });
    }

    private static async Task TypesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.StartCaptureAsync();

        await scene.StepAsync("types-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/ContentTypes/Edit/Lead']").Last);
            await scene.ScrollAsync(0, 700);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/ContentTypes/Lead/ContentParts/LeadPart/Edit']").First);
        });

        await scene.StepAsync("types-2", async () =>
        {
            await scene.ScrollAsync(0, 300);
            await scene.HighlightAsync(scene.Page.Locator("select").First, "A setting of the Lead part");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("types-3", async () =>
        {
            await scene.NavigateAsync("/Admin/ContentTypes/List");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/ContentTypes/Edit/SalesOpportunity']").Last);
            await scene.ScrollAsync(0, 700);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/ContentTypes/SalesOpportunity/ContentParts/OpportunityPart/Edit']").First);
            await scene.ScrollAsync(0, 300);
        });
    }
}
