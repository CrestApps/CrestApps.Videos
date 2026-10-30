using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Omnichannel AI Handoff video, one method per "clip" of storyboard.json.
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
        ("02-queue", QueueAsync),
        ("03-subject", SubjectAsync),
        ("05-inbox", InboxAsync),
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
            "CrestApps.OrchardCore.OpenAI",
            "CrestApps.OrchardCore.AI.Chat",
            "CrestApps.OrchardCore.AI.ConnectionManagement",
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

        // The log SMS provider stands in for a real one: nothing is sent anywhere.
        await page.GotoAsync(BaseUrl + "/Admin/Settings/sms");
        await page.Locator("#ISite_SmsSettings_DefaultProvider").SelectOptionAsync(new SelectOptionValue { Label = "Log" });
        await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("Save") }).Last.ClickAsync());
        Console.WriteLine("Set the SMS provider");

        // An AI connection and deployment that point nowhere (no model is ever called while recording).
        await page.GotoAsync(BaseUrl + "/Admin/ai/provider/connection/create/OpenAI");
        await page.Locator("#AIProviderConnection_DisplayText").FillAsync("Demo connection");
        await page.Locator("#AIProviderConnection_Name").FillAsync("demo");
        await page.Locator("#AIProviderConnection_Endpoint").FillAsync("http://127.0.0.1:9/v1");
        await page.Locator("#AIProviderConnection_ApiKey").FillAsync("demo-not-a-real-key");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("Create|Save") }).Last.ClickAsync());
        await page.GotoAsync(BaseUrl + "/Admin/ai/deployment/create/OpenAI");
        await page.Locator("#AIDeployment_ModelName").FillAsync("demo-chat-model");
        await page.Locator("#AIDeployment_Name").FillAsync("demo-chat");
        await page.Locator("#AIDeployment_ConnectionName").SelectOptionAsync(new SelectOptionValue { Label = "Demo connection" });
        await page.Locator("#SelectedFeatures_textGeneration").CheckAsync(new() { Force = true });
        await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("Create|Save") }).Last.ClickAsync());
        Console.WriteLine("Added the AI connection and deployment");

        // Showroom hours, for the load's business hours.
        await page.GotoAsync(BaseUrl + "/Admin/contact-center/business-hours/create");
        await page.Locator("#BusinessHoursCalendar_Name").FillAsync("Showroom hours");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("Save|Create") }).Last.ClickAsync());
        Console.WriteLine("Added the business hours");

        // A cadence of two follow-ups.
        await page.GotoAsync(BaseUrl + "/Admin/omnichannel/cadences/create");
        await page.Locator("#Cadence_DisplayText").FillAsync("Spring re-engagement");
        await page.Locator("#Cadence_Description").FillAsync("Two nudges for customers who stop replying.");
        await page.Locator("#cadence-step-add").ClickAsync();
        await page.Locator("#cadence-step-0-delay").FillAsync("60");
        await page.Locator("#cadence-step-0-message").FillAsync("Hi, just checking in. Are you still interested in a test drive?");
        await page.Locator("#cadence-step-add").ClickAsync();
        await page.Locator("#cadence-step-1-delay").FillAsync("1440");
        await page.Locator("#cadence-step-1-type").SelectOptionAsync(new SelectOptionValue { Label = "AI-generated" });
        await page.Locator("#cadence-step-1-message").FillAsync("Offer two test-drive times.");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
        Console.WriteLine("Added the cadence");

        // The AI profile the subject uses.
        await page.GotoAsync(BaseUrl + "/Admin/ai/profile/new/sms-qualify-leads");
        await page.Locator("#ChatDeploymentName").SelectOptionAsync(new SelectOptionValue { Label = "demo-chat (demo-chat-model)" });
        await page.RunAndWaitForNavigationAsync(() => page.Locator("button[type=submit]").Filter(new() { HasTextString = "Create profile" }).First.ClickAsync());
        Console.WriteLine("Added the AI profile");
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

    private const string SubjectPrefix = "DealershipInquiry_OmnichannelSubjectPart_";

    private static ILocator SubjectSetting(Scene scene, string name) => scene.Page.Locator($"#{SubjectPrefix}OmnichannelSubjectPartSettingsDisplayDriver_{name}");

    private static ILocator AiSetting(Scene scene, string name) => scene.Page.Locator($"#{SubjectPrefix}OmnichannelSubjectAISettingsDisplayDriver_{name}");

    private static ILocator Group(ILocator field) => field.Locator("xpath=ancestor::div[contains(@class,'mb-3') or contains(@class,'ocat-wrapper') or contains(@class,'form-check')][1]");

    // Opens a searchable picker and clicks the option, as a person would.
    private static async Task PickAsync(Scene scene, ILocator select, string text)
    {
        var button = select.Locator("xpath=ancestor::div[contains(@class,'bootstrap-select')][1]//button").First;
        await scene.ClickAsync(button, pauseAfter: 700);
        await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show li").Filter(new() { HasText = text }).First, pauseAfter: 500);
        await scene.Page.Keyboard.PressAsync("Escape");
        await scene.PauseAsync(500);
    }

    // ----- Clips -----

    private static async Task QueueAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/queues");
        await scene.StartCaptureAsync();

        await scene.StepAsync("queue-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("Add|New") }).First);
            await scene.TypeAsync(scene.Page.Locator("#ActivityQueue_Name"), "Sales queue", delay: 55);
            await scene.HighlightAsync(Group(scene.Page.Locator("#ActivityQueue_Name")), "The queue the AI hands to");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("queue-2", async () =>
        {
            await scene.ScrollAsync(0, 900);
            await PickAsync(scene, scene.Page.Locator("#ActivityQueue_BusinessHoursCalendarId"), "Showroom hours");
            await scene.HighlightAsync(Group(scene.Page.Locator("#ActivityQueue_BusinessHoursCalendarId")), "Closed outside these hours");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 3000);
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator("button[type=submit]").Filter(new() { HasTextString = "Save" }).Last, pauseAfter: 100));
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Filter(new() { HasText = "Sales queue" }).First, "Saved");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task SubjectAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/omnichannel/subject-flows");
        await scene.StartCaptureAsync();

        await scene.StepAsync("subject-1", async () =>
        {
            await scene.NavigateAsync("/Admin/ContentTypes/Edit/DealershipInquiry");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/ContentParts/OmnichannelSubjectPart/Edit']").First);
            await scene.HighlightAsync(Group(SubjectSetting(scene, "Direction")), "Inbound: the customer starts it");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(SubjectSetting(scene, "InteractionType"), "Automated", pauseAfter: 900);
            await scene.SelectAsync(SubjectSetting(scene, "Channel"), "SMS", pauseAfter: 900);
            await scene.SelectAsync(SubjectSetting(scene, "ChannelEndpointId"), "Sales line (+15125550111)", pauseAfter: 900);
        });

        await scene.StepAsync("subject-2", async () =>
        {
            await scene.ScrollAsync(0, 600);
            await scene.SelectAsync(AiSetting(scene, "ProfileId"), "Qualify leads by text", pauseAfter: 900);
            await scene.TypeAsync(AiSetting(scene, "SubjectGoal"), "Find out which car the customer wants and book a test drive.", delay: 25);
            await AiSetting(scene, "NoResponseTimeoutInMinutes").FillAsync("30");
            await AiSetting(scene, "SmsResponseDelayInSeconds").FillAsync("10");
            await scene.HighlightAsync(Group(AiSetting(scene, "NoResponseTimeoutInMinutes")), "Give up on a silent customer");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("subject-3", async () =>
        {
            await scene.ScrollAsync(0, 700);
            await AiSetting(scene, "EnableAgentHandoff").CheckAsync(new() { Force = true });
            await scene.PauseAsync(900);
            await scene.SelectAsync(AiSetting(scene, "HandoffQueueId"), "Sales queue", pauseAfter: 900);
            await AiSetting(scene, "HandoffOnQualifiedLead").CheckAsync(new() { Force = true });
            await AiSetting(scene, "HandoffOnFrustration").CheckAsync(new() { Force = true });
            await scene.HighlightAsync(Group(AiSetting(scene, "HandoffQueueId")), "The queue a person answers");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Group(AiSetting(scene, "HandoffOnFrustration")), "Tick the cases");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 2500);
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last, pauseAfter: 100));
            await scene.PauseAsync(1500);
        });
    }

    private static async Task InboxAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("inbox-1", async () =>
        {
            await scene.NavigateAsync("/Admin/messaging");
            await scene.PauseAsync(2500);
            await scene.HighlightAsync(scene.Page.Locator("text=Nothing here yet").First, "Handed-over texts appear here");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
        });
    }
}
