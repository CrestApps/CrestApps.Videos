using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Site Wizard and Recipes video, one method per "clip" of storyboard.json.
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
        ("03-type", TypeAsync),
        ("04-steps", StepsAsync),
        ("05-run", RunAsync),
        ("06-results", ResultsAsync),
        ("07-recipes", RecipesAsync),
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
            "CrestApps.OrchardCore.Resources",
            "CrestApps.OrchardCore.Recipes",
            "CrestApps.OrchardCore.Wizard",
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

        await ImportRecipeAsync(page, Path.Combine(Root, "seed", "registration.recipe.json"));

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

    private static string HostIdPath => Path.Combine(Root, "state-host-id.json");

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
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/ContentTypes/Edit/WorkshopRegistration']").Last);
            await scene.ScrollAsync(0, 500);
        });

        await scene.StepAsync("type-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/ContentTypes/AddPartsTo/WorkshopRegistration']"));
        });

        await scene.StepAsync("type-3", async () =>
        {
            var row = scene.Page.Locator("li, .list-group-item, div.row").Filter(new() { HasTextString = "Wizard" }).Last;
            await scene.ClickAndWaitForNavigationAsync(row.Locator("a, button").Filter(new() { HasTextString = "Add & Configure" }).First);
            await scene.ScrollAsync(0, 300);
            await scene.ClickAsync(scene.Page.Locator("label").Filter(new() { HasTextString = "Visitor Details" }).First, pauseAfter: 500);
            await scene.ClickAsync(scene.Page.Locator("label").Filter(new() { HasTextString = "Visit Preferences" }).First, pauseAfter: 500);
        });

        await scene.StepAsync("type-4", async () =>
        {
            await scene.ScrollAsync(0, 400);
            await scene.HighlightAsync(scene.Page.Locator("input[id$='RequiresAuthenticatedUser']").Locator("xpath=.."), "Requires an authenticated user");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("select[id$='CompletionPolicy']"), "Completion policy");
            await scene.PauseAsync(2800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button[name='submit.Save']"));
        });
    }

    private static async Task StepsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();

        await scene.StepAsync("steps-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.dropdown-toggle, a.dropdown-toggle").Filter(new() { HasTextString = "New" }).First, pauseAfter: 700);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".dropdown-menu.show a").Filter(new() { HasTextString = "Workshop Registration" }).First);
            await scene.TypeAsync(scene.Page.Locator("#TitlePart_Title"), "Spring workshop", delay: 45);
        });

        await scene.StepAsync("steps-2", async () =>
        {
            await scene.ScrollAsync(0, 300);
            await scene.ClickAsync(scene.Page.Locator(".btn-wizard-add button").First, pauseAfter: 800);
            await scene.ClickAsync(scene.Page.Locator(".btn-wizard-add .dropdown-item").Filter(new() { HasTextString = "Visitor Details" }).First, pauseAfter: 1200);
            await scene.ClickAsync(scene.Page.Locator(".btn-wizard-add button").First, pauseAfter: 800);
            await scene.ClickAsync(scene.Page.Locator(".btn-wizard-add .dropdown-item").Filter(new() { HasTextString = "Visit Preferences" }).First, pauseAfter: 1200);
        });

        await scene.StepAsync("steps-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".widget-template-placeholder").First, "Cards in order: the steps");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 900);
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator("button[name='submit.Publish']").First, pauseAfter: 100));
            var href = await scene.Page.Locator(".list-group-item").Filter(new() { HasText = "Spring workshop" }).First.Locator("a[href*='/Edit']").First.GetAttributeAsync("href");
            var parts = href.Split('?')[0].Split('/', StringSplitOptions.RemoveEmptyEntries);
            var id = parts[Array.IndexOf(parts, "ContentItems") + 1];
            await File.WriteAllTextAsync(HostIdPath, id);
        });
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        // A visitor who is not signed in.
        await using var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, Path.Combine(Root, "no-sign-in.json"), Path.GetDirectoryName(directory), durations);
        var id = (await File.ReadAllTextAsync(HostIdPath)).Trim();

        await scene.OpenAsync($"/Wizard/Start/Content?definitionId={id}");
        await scene.StartCaptureAsync();

        await scene.StepAsync("run-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("nav.wizard-stepper"), "Progress");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("run-2", async () =>
        {
            var boxes = scene.Page.Locator("#wizard-form input[type=text], #wizard-form input:not([type]), #wizard-form input[type=email]").Filter(new() { HasNot = scene.Page.Locator("[type=hidden]") });
            await scene.TypeAsync(scene.Page.Locator("#wizard-form input[name*='FullName']").First, "Priya Raman", delay: 55);
            await scene.TypeAsync(scene.Page.Locator("#wizard-form input[name*='Email']").First, "priya.raman@example.com", delay: 30);
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator("#Wizard_Next_Button"), pauseAfter: 100));
        });

        await scene.StepAsync("run-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#wizard-form input[name*='Workshop']").First, "Spring workshop", delay: 45);
            await scene.HighlightAsync(scene.Page.Locator("#Wizard_Previous_Button"), "Previous");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator("#Wizard_Next_Button"), pauseAfter: 100));
        });

        await scene.StepAsync("run-4", async () =>
        {
            await scene.PauseAsync(1500);
        });
    }

    private static async Task ResultsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();

        await scene.StepAsync("results-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item-action").Filter(new() { HasText = "VisitorDetails" }).First, "Visitor Details");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item-action").Filter(new() { HasText = "VisitPreferences" }).First, "Visit Preferences");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task RecipesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();

        await scene.StepAsync("recipes-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Recipes"), "Tools > Recipes");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/Recipes"));
            var first = scene.Page.Locator("a.btn, button.btn").Filter(new() { HasTextString = "Run" }).Nth(1).Locator("xpath=ancestor::*[contains(@class,'list-group-item') or contains(@class,'card')][1]");
            await scene.HighlightAsync(first, "Name, tags and description");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("recipes-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("input[type=search], input[name$='Search'], #search-box").First, "classifieds", delay: 40);
            await scene.HighlightAsync(scene.Page.Locator("a[id^='btn-run-']:visible").First, "Run");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("recipes-3", async () =>
        {
            await scene.NavigateAsync("/Admin/DeploymentPlan/Import/Json");
            await scene.PauseAsync(1500);
            await scene.ClickAsync(scene.Page.Locator(".monaco-editor").First, pauseAfter: 400);
            await scene.Page.Keyboard.PressAsync("Control+A");
            await scene.Page.Keyboard.PressAsync("Delete");
            await scene.Page.Keyboard.TypeAsync("{ \"steps\": [ { \"name\": \"", new() { Delay = 45 });
            await scene.Page.Keyboard.PressAsync("Control+Space");
            await scene.PauseAsync(2500);
        });
    }
}
