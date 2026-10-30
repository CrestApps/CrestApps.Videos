using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Omnichannel Automated SMS video, one method per "clip" of storyboard.json.
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
        ("02-profile", ProfileAsync),
        ("03-load", LoadAsync),
        ("04-result", ResultAsync),
        ("06-inbound", InboundAsync),
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

    private static ILocator AiField(Scene scene, string suffix) => scene.Page.Locator($"[id$='{suffix}']").First;

    private static ILocator Load(Scene scene, string suffix) => scene.Page.Locator($"#OmnichannelActivityBatch_{suffix}");

    // Opens a searchable picker and clicks the option, as a person would.
    private static async Task PickAsync(Scene scene, string id, string text)
    {
        var button = scene.Page.Locator($"#{id}").Locator("xpath=ancestor::div[contains(@class,'bootstrap-select')][1]//button").First;
        await scene.ClickAsync(button, pauseAfter: 700);
        await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show li").Filter(new() { HasText = text }).First, pauseAfter: 500);
        await scene.Page.Keyboard.PressAsync("Escape");
        await scene.PauseAsync(500);
    }

    // ----- Clips -----

    private static async Task ProfileAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.StartCaptureAsync();

        await scene.StepAsync("profile-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button, a").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*Add Profile\s*$") }).First, pauseAfter: 1200);
            await scene.ScrollAsync(0, 600);
            await scene.HighlightAsync(scene.Page.Locator("#newProfileModal .card").Filter(new() { HasText = "Qualify leads by text" }).First, "Text messaging starting points");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("profile-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#newProfileModal a[href$='/new/sms-qualify-leads']").First);
            await scene.HighlightAsync(scene.Page.Locator("#DisplayText"), "Title");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(scene.Page.Locator("#ChatDeploymentName"), "demo-chat (demo-chat-model)", pauseAfter: 900);
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator("button[type=submit]").Filter(new() { HasTextString = "Create profile" }).First, pauseAfter: 100));
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("profile-3", async () =>
        {
            await scene.HighlightAsync(AiField(scene, "AddInitialPrompt").Locator("xpath=ancestor::div[contains(@class,'mb-3') or contains(@class,'form-check')][1]"), "The AI starts the conversation");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(AiField(scene, "InitialPrompt").Locator("xpath=ancestor::div[contains(@class,'mb-3')][1]"), "The opening text the customer receives");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task LoadAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/omnichannel/activity/batches");
        await scene.StartCaptureAsync();

        await scene.StepAsync("load-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.create").First, pauseAfter: 1000);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalAddActivityBatch a[href$='/create/Automatic']").First);
            await scene.TypeAsync(Load(scene, "DisplayText"), "Spring lead texts", delay: 50);
            await scene.SelectAsync(Load(scene, "SubjectContentType"), "Sales Call", pauseAfter: 600);
            await scene.SelectAsync(Load(scene, "CampaignId"), "Spring Sales Event", pauseAfter: 600);
            await scene.SelectAsync(Load(scene, "AIProfileId"), "Qualify leads by text", pauseAfter: 900);
        });

        await scene.StepAsync("load-2", async () =>
        {
            await scene.ScrollAsync(0, 400);
            await scene.HighlightAsync(Load(scene, "AllowAIToUpdateContact").Locator("xpath=ancestor::div[contains(@class,'mb-3') or contains(@class,'form-check')][1]"), "What the AI may update");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(Load(scene, "ResponseDelayMode"), "Fixed", pauseAfter: 900);
            await Load(scene, "ResponseDelaySeconds").FillAsync("20");
            await scene.HighlightAsync(Load(scene, "ResponseDelayMode").Locator("xpath=ancestor::div[contains(@class,'mb-3')][1]"), "A pause before each reply");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("load-3", async () =>
        {
            await scene.ScrollAsync(0, 400);
            await scene.SelectAsync(Load(scene, "CadenceId"), "Spring re-engagement", pauseAfter: 900);
            await scene.SelectAsync(Load(scene, "BusinessHoursCalendarId"), "Showroom hours", pauseAfter: 900);
            await scene.HighlightAsync(Load(scene, "CadenceId").Locator("xpath=ancestor::div[contains(@class,'mb-3')][1]"), "Follow-ups when the customer goes quiet");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(Load(scene, "Channel"), "SMS", pauseAfter: 900);
            await scene.SelectAsync(Load(scene, "ChannelEndpointId"), "Sales line (+15125550111)", pauseAfter: 900);
            await scene.HighlightAsync(Load(scene, "ChannelEndpointId").Locator("xpath=ancestor::div[contains(@class,'mb-3')][1]"), "The number the texts come from");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("load-4", async () =>
        {
            await scene.ScrollAsync(0, 600);
            await scene.SelectAsync(Load(scene, "ContactContentType"), "Contact", pauseAfter: 900);
            await scene.ScrollAsync(0, 1500);
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator("button[type=submit]").Filter(new() { HasTextString = "Save" }).First, pauseAfter: 100));
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Filter(new() { HasText = "Spring lead texts" }).First, "Saved, not started");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task ResultAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/omnichannel/activity/batches");
        await scene.StartCaptureAsync();

        await scene.StepAsync("result-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".list-group-item").Filter(new() { HasText = "Spring lead texts" }).First.Locator("button.dropdown-toggle").First, pauseAfter: 1000);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu a, .dropdown-menu button").Filter(new() { HasTextString = "Load activities" }).First, pauseAfter: 1000);
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator(".modal.show button, .modal.show a").Filter(new() { HasTextString = "Ok" }).First, pauseAfter: 100));
            await scene.PauseAsync(9000);
            await scene.NavigateAsync("/Admin/omnichannel/activity/batches");
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Filter(new() { HasText = "Spring lead texts" }).First, "Automated activities, one per contact");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("result-2", async () =>
        {
            await scene.NavigateAsync("/Admin/omnichannel/manage-activities");
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").First, "Automated (AI), with the profile");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task InboundAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/entry-points");
        await scene.StartCaptureAsync();

        await scene.StepAsync("inbound-1", async () =>
        {
            await scene.NavigateAsync("/Admin/contact-center/entry-points/create?channel=SMS");
            await scene.TypeAsync(scene.Page.Locator("#ContactCenterEntryPoint_Name"), "Sales texts", delay: 55);
            await PickAsync(scene, "ContactCenterEntryPoint_AddressIds", "Sales line");
            await scene.HighlightAsync(scene.Page.Locator("#ContactCenterEntryPoint_AddressIds").Locator("xpath=ancestor::div[contains(@class,'mb-3')][1]"), "The number customers text");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("inbound-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#ContactCenterEntryPoint_TargetType").Locator("xpath=ancestor::div[contains(@class,'mb-3')][1]"), "Route to: Queue, Specific agent or AI agent");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(scene.Page.Locator("#ContactCenterEntryPoint_TargetType"), "AI agent", pauseAfter: 900);
            await PickAsync(scene, "ContactCenterEntryPoint_TargetAIProfileId", "Qualify leads by text");
            await scene.HighlightAsync(scene.Page.Locator("#ContactCenterEntryPoint_TargetAIProfileId").Locator("xpath=ancestor::div[contains(@class,'mb-3')][1]"), "The AI profile that answers");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 1500);
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator("button[type=submit]").Filter(new() { HasTextString = "Save" }).First, pauseAfter: 100));
            await scene.PauseAsync(2500);
        });
    }
}
