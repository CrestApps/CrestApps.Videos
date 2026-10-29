using Microsoft.Playwright;

namespace Recorder;

public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-welcome", WelcomeAsync),
        ("04-build", BuildAsync),
        ("05-submenus", SubmenusAsync),
        ("06-tries", TriesAsync),
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

    /// <summary>
    /// Starts from the entry points video's seed (numbers, queues, agents, voice media) and adds the entry point the clips edit.
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await Seed.OpenAsync(browser);
        await Seed.EntryPointAsync(page, "Main line", "Main line", "Support");
        await context.CloseAsync();
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    // ----- Helpers -----

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static ILocator Field(Scene scene, string id) => scene.Page.Locator("#" + id);

    private static ILocator Part(Scene scene, string suffix) => scene.Page.Locator($"[id$='{suffix}']").First;

    private static async Task ScrollToAsync(Scene scene, ILocator locator, double offset = 160)
    {
        var top = await locator.EvaluateAsync<double>("e => e.getBoundingClientRect().top");
        await scene.ScrollAsync(0, top - offset);
    }

    private static async Task OpenMainLineAsync(Scene scene)
    {
        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("ul.admin-list a").Filter(new() { HasTextString = "Main line" }).First);
    }

    private static async Task AddKeyAsync(Scene scene, bool last = true)
    {
        var button = scene.Page.Locator("button").Filter(new() { HasTextString = "Add key" });
        await scene.ClickAsync(last ? button.Last : button.First, pauseAfter: 600);
    }

    // ----- Clips -----

    private static async Task WelcomeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/entry-points");
        await scene.StartCaptureAsync();
        await OpenMainLineAsync(scene);

        await scene.StepAsync("wel-1", async () =>
        {
            await ScrollToAsync(scene, scene.Page.Locator("#heading-card-welcome-and-i-v-r-menu"), 120);
            await scene.HighlightAsync(scene.Page.Locator("#heading-card-welcome-and-i-v-r-menu"), "Welcome and IVR menu");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(Field(scene, "ContactCenterEntryPoint_WelcomeMessage"), "Welcome to Contoso.", delay: 30);
            await scene.HighlightAsync(Section(Field(scene, "ContactCenterEntryPoint_WelcomeMessage")), "Welcome message");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("wel-2", async () =>
        {
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.Locator("text=No IVR menu").First, "No menu: straight to the queue");
        });

        await scene.ClearHighlightsAsync();
        await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
    }

    private static async Task BuildAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/entry-points");
        await scene.StartCaptureAsync();
        await OpenMainLineAsync(scene);
        await ScrollToAsync(scene, scene.Page.Locator("#ContactCenterEntryPoint_WelcomeMessage"), 200);

        await scene.StepAsync("bld-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Build an IVR menu" }).First, pauseAfter: 1000);
            await scene.HighlightAsync(scene.Page.Locator("text=First menu").First, "First menu");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("bld-2", async () =>
        {
            await scene.TypeAsync(Part(scene, "-p-0"), "Thanks for calling Contoso. For sales, press 1. For support, press 2. To talk to Jamie, press 3. For billing, press 4. To leave a message, press 9.", delay: 12);
            await scene.HighlightAsync(Section(Part(scene, "-p-0")), "What callers hear");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("bld-3", async () =>
        {
            await scene.SelectAsync(Part(scene, "-k-0-0"), "Send to a queue", pauseAfter: 300);
            await scene.SelectAsync(Part(scene, "-t-0-0"), "Sales", pauseAfter: 500);
            await AddKeyAsync(scene);
            await scene.SelectAsync(Part(scene, "-d-0-1"), "2", pauseAfter: 300);
            await scene.SelectAsync(Part(scene, "-k-0-1"), "Send to a queue", pauseAfter: 300);
            await scene.SelectAsync(Part(scene, "-t-0-1"), "Support", pauseAfter: 500);
        });

        await scene.StepAsync("bld-4", async () =>
        {
            await AddKeyAsync(scene);
            await scene.SelectAsync(Part(scene, "-d-0-2"), "3", pauseAfter: 300);
            await scene.SelectAsync(Part(scene, "-k-0-2"), "Send to an agent", pauseAfter: 300);
            await scene.SelectAsync(Part(scene, "-t-0-2"), "jamie.chen", pauseAfter: 500);
            await AddKeyAsync(scene);
            await scene.SelectAsync(Part(scene, "-d-0-3"), "9", pauseAfter: 300);
            await scene.SelectAsync(Part(scene, "-k-0-3"), "Send to voicemail", pauseAfter: 600);
        });

        await scene.StepAsync("bld-5", async () =>
        {
            await ScrollToAsync(scene, Part(scene, "-m-0"), 240);
            await scene.SelectAsync(Part(scene, "-m-0"), "Welcome greeting", pauseAfter: 900);
            await scene.HighlightAsync(Section(Part(scene, "-m-0")), "Recorded prompt");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(Part(scene, "-m-0"), "None: speak the text above", pauseAfter: 500);
        });

        await scene.ClearHighlightsAsync();

        // Key 4 is added in the next clip; the menu is saved with four keys.
        await AddKeyAsync(scene);
        await scene.SelectAsync(Part(scene, "-d-0-4"), "4", pauseAfter: 200);
        await scene.SelectAsync(Part(scene, "-k-0-4"), "Send to a queue", pauseAfter: 200);
        await scene.SelectAsync(Part(scene, "-t-0-4"), "Billing", pauseAfter: 200);
        await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
    }

    private static async Task SubmenusAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/entry-points");
        await scene.StartCaptureAsync();
        await OpenMainLineAsync(scene);

        // The Billing key (4) becomes a submenu.
        await ScrollToAsync(scene, Part(scene, "-k-0-4"), 300);

        await scene.StepAsync("sub-1", async () =>
        {
            await scene.SelectAsync(Part(scene, "-k-0-4"), "Open a submenu", pauseAfter: 1200);
            await ScrollToAsync(scene, scene.Page.Locator("span.badge").Filter(new() { HasTextString = "Submenu" }).First, 200);
            await scene.HighlightAsync(scene.Page.Locator("span.badge").Filter(new() { HasTextString = "Submenu" }).First, "Submenu");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("sub-2", async () =>
        {
            await scene.TypeAsync(Part(scene, "-p-1"), "For invoices, press 1. To go back, press 0.", delay: 20);
            await scene.SelectAsync(Part(scene, "-k-1-0"), "Send to a queue", pauseAfter: 300);
            await scene.SelectAsync(Part(scene, "-t-1-0"), "Billing", pauseAfter: 400);
            await AddKeyAsync(scene, last: false);
            await scene.SelectAsync(Part(scene, "-d-1-1"), "0", pauseAfter: 300);
            await scene.SelectAsync(Part(scene, "-k-1-1"), "Go to another menu", pauseAfter: 500);
        });

        await scene.StepAsync("sub-3", async () =>
        {
            await AddKeyAsync(scene, last: false);
            await scene.SelectAsync(Part(scene, "-d-1-2"), "9", pauseAfter: 300);
            await scene.SelectAsync(Part(scene, "-k-1-2"), "Repeat this menu", pauseAfter: 900);
            await scene.HighlightAsync(Part(scene, "-k-1-2"), "Repeat this menu");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
        });

        await scene.ClearHighlightsAsync();
        await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
    }

    private static async Task TriesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/entry-points");
        await scene.StartCaptureAsync();
        await OpenMainLineAsync(scene);
        await ScrollToAsync(scene, Part(scene, "-retries"), 260);

        await scene.StepAsync("try-1", async () =>
        {
            await scene.HighlightAsync(Part(scene, "-retries"), "Tries");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("try-2", async () =>
        {
            await scene.SelectAsync(Part(scene, "-fallback-kind"), "Send to voicemail", pauseAfter: 1800);
            await scene.HighlightAsync(Part(scene, "-fallback-kind"), "When the tries run out");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(Part(scene, "-fallback-kind"), "Route to the entry point target", pauseAfter: 400);
            await ScrollToAsync(scene, scene.Page.Locator("button").Filter(new() { HasTextString = "Add key" }).Last, 400);
            await AddKeyAsync(scene);
            await scene.SelectAsync(Part(scene, "-d-0-5"), "5", pauseAfter: 200);
            await scene.SelectAsync(Part(scene, "-k-0-5"), "Send to a queue", pauseAfter: 400);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("try-3", async () =>
        {
            await ScrollToAsync(scene, scene.Page.Locator("div").Filter(new() { HasTextString = "problem(s) to fix" }).Last, 200);
            await scene.HighlightAsync(scene.Page.Locator("div").Filter(new() { HasTextString = "problem(s) to fix" }).Last, "Problems to fix");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("try-4", async () =>
        {
            await scene.SelectAsync(Part(scene, "-t-0-5"), "Sales", pauseAfter: 600);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }
}
