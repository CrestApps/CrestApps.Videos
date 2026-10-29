using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Content Fields video, one method per "clip" of storyboard.json.
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
        ("03-phone", PhoneAsync),
        ("04-address", AddressAsync),
        ("05-enter", EnterAsync),
        ("06-countries", CountriesAsync),
        ("07-suggestions", SuggestionsAsync),
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
            "CrestApps.OrchardCore.ContentFields",
            "CrestApps.OrchardCore.ContentFields.Address",
            "CrestApps.OrchardCore.Addresses.Countries",
            "CrestApps.OrchardCore.Addresses.GooglePlaces",
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

        await ImportRecipeAsync(page, Path.Combine(Root, "seed", "branch-office.recipe.json"));

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

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

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

    private static ILocator Radio(Scene scene, string fieldType) => scene.Page.Locator($"input[type=radio][value='{fieldType}']");

    // ----- Clips -----

    private static async Task PhoneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.StartCaptureAsync();

        await scene.StepAsync("phone-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ContentTypes/List"), "Design > Content Definition > Content Types");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/ContentTypes/Edit/BranchOffice']").Last);
            await scene.ScrollAsync(0, 500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='/Admin/ContentTypes/AddFieldsTo/BranchOffice']").First);
        });

        await scene.StepAsync("phone-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#DisplayName"), "Main phone", delay: 35);
            await scene.ClickAsync(scene.Page.Locator("label[for='Fields_12_']"), pauseAfter: 700);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button[name='submit.Save']").Filter(new() { HasTextString = "Configure" }));
        });

        await scene.StepAsync("phone-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("input[id$='Required']").First, pauseAfter: 500);
            await scene.TypeAsync(scene.Page.Locator("input[id$='Hint']").First, "Include the area code.", delay: 40);
        });

        await scene.StepAsync("phone-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("select[id$='InitialCountryMode']").Locator("xpath=.."), "Initial country");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(scene.Page.Locator("select[id$='InitialCountryMode']"), "Specific", pauseAfter: 900);
        });

        await scene.StepAsync("phone-5", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("select[id$='SpecificCountryCode']"), "US", pauseAfter: 900);
            await scene.HighlightAsync(scene.Page.Locator("select[id$='SpecificCountryCode']").Locator("xpath=.."), "Country");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }

    private static async Task AddressAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.Page.Locator("a[href$='/Admin/ContentTypes/Edit/BranchOffice']").Last.ClickAsync();
        await scene.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await scene.Page.Locator("a[href*='/Admin/ContentTypes/AddFieldsTo/BranchOffice']").First.ClickAsync();
        await scene.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await scene.StartCaptureAsync();

        await scene.StepAsync("address-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#DisplayName"), "Location", delay: 40);
            await scene.ClickAsync(scene.Page.Locator("label[for='Fields_0_']"), pauseAfter: 700);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button[name='submit.Save']").Filter(new() { HasTextString = "Configure" }));
        });

        await scene.StepAsync("address-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("input[id$='Required']").First, pauseAfter: 400);
            await scene.TypeAsync(scene.Page.Locator("input[id$='Hint']").First, "Where the branch is.", delay: 25);
            await scene.ScrollAsync(0, 250);
            await scene.ClickAsync(scene.Page.Locator("input[id$='ShowName']"), pauseAfter: 400);
            await scene.HighlightAsync(Section2(scene.Page.Locator("input[id$='ShowName']")), "Extra lines");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("address-3", async () =>
        {
            await scene.ScrollAsync(0, 300);
            var countries = scene.Page.Locator("select[id$='_Countries']");
            await scene.Page.EvaluateAsync("() => document.querySelector(\"select[id$='_Countries']\").scrollIntoView({block: 'center'})");
            await scene.ClickAsync(countries.Locator("option", new() { HasTextString = "United States" }).First, pauseAfter: 600);
            await scene.Page.Keyboard.DownAsync("Control");
            await scene.ClickAsync(countries.Locator("option", new() { HasTextString = "Canada" }).First, pauseAfter: 600);
            await scene.Page.Keyboard.UpAsync("Control");
            await scene.HighlightAsync(countries, "Countries offered");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(scene.Page.Locator("select[id$='DefaultCountry']"), "US", pauseAfter: 600);
        });

        await scene.StepAsync("address-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1000);
        });
    }

    private static ILocator Section2(ILocator input) => input.Locator("xpath=ancestor::div[contains(@class,'mb-3')][1]");

    private static async Task EnterAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();

        await scene.StepAsync("enter-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.dropdown-toggle, a.dropdown-toggle").Filter(new() { HasTextString = "New" }).First, pauseAfter: 700);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".dropdown-menu.show a").Filter(new() { HasTextString = "Branch Office" }).First);
            await scene.TypeAsync(scene.Page.Locator("#TitlePart_Title"), "Toronto branch", delay: 45);
        });

        await scene.StepAsync("enter-2", async () =>
        {
            await scene.ScrollAsync(0, 250);
            await scene.ClickAsync(scene.Page.Locator(".iti__selected-country, .iti__selected-flag").First, pauseAfter: 500);
            await scene.ClickAsync(scene.Page.Locator(".iti__country-name:text-is('Canada'), .iti__country[data-country-code='ca']").First, pauseAfter: 400);
            await scene.TypeAsync(scene.Page.Locator("input[type=tel]").First, "416 555 0134", delay: 40);
        });

        await scene.StepAsync("enter-3", async () =>
        {
            await scene.ScrollAsync(0, 350);
            await scene.HighlightAsync(scene.Page.Locator("select[id$='_Country']").First.Locator("xpath=ancestor::div[contains(@class,'content-field-address')][1]"), "Address Field: United States");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("enter-4", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("select[id$='_Country']").First, "Canada", pauseAfter: 600);
            await scene.HighlightAsync(scene.Page.Locator("select[id$='_Country']").First.Locator("xpath=ancestor::div[contains(@class,'content-field-address')][1]"), "Province and Postal code");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("input[id$='_AddressLine1']").First, "200 Bay Street", delay: 25);
            await scene.TypeAsync(scene.Page.Locator("input[id$='_City']").First, "Toronto", delay: 25);
            await scene.SelectAsync(scene.Page.Locator("select[id$='_Region']").First, "British Columbia", pauseAfter: 300);
            await scene.TypeAsync(scene.Page.Locator("input[id$='_PostalCode']").First, "M5J 2J3", delay: 30);
        });

        await scene.StepAsync("enter-5", async () =>
        {
            await scene.ScrollAsync(0, 400);
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator("button[name='submit.Publish']").First, pauseAfter: 100));
            await scene.PauseAsync(500);
            await scene.HighlightAsync(scene.Page.Locator(".alert-danger").First, "Refused: the postal code is not in that province");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("enter-6", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("select[id$='_Region']").First, "Ontario", pauseAfter: 400);
            await scene.ScrollAsync(0, 500);
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator("button[name='submit.Publish']").First, pauseAfter: 100));
            await scene.NavigateAsync("/toronto-branch");
            await scene.PauseAsync(600);
            await scene.ScrollAsync(0, 420);
        });
    }

    private static async Task CountriesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();

        await scene.StepAsync("countries-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button, a").Filter(new() { HasTextString = "Content Type" }).Last, pauseAfter: 900);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".dropdown-menu.show a").Filter(new() { HasTextString = "Country" }).First);
        });

        await scene.StepAsync("countries-2", async () =>
        {
            await scene.Page.Locator("input[type=search], input[name$='DisplayText']").First.PressAsync("End");
            await scene.TypeAsync(scene.Page.Locator("input[type=search], input[name$='DisplayText']").First, " Canada", delay: 70);
            await scene.WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"));
            await scene.HighlightAsync(scene.Page.Locator("button.dropdown-toggle, a.dropdown-toggle").Filter(new() { HasTextString = "New" }).First, "Add a country");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn").Filter(new() { HasTextString = "Edit" }).First);
        });
    }

    private static async Task SuggestionsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/general");
        await scene.StartCaptureAsync();

        await scene.StepAsync("suggestions-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/address-suggestions"), "Settings > Address suggestions");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/Settings/address-suggestions"));
            await scene.HighlightAsync(scene.Page.Locator("form p.text-muted").First, "Google Places");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("suggestions-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("input[type=password]").First, "demo-key-not-a-real-key", delay: 40);
            await scene.HighlightAsync(scene.Page.Locator("input[type=password]").First, "Google Places API key");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }
}
