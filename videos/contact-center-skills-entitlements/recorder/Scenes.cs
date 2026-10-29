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
        ("03-skill", SkillAsync),
        ("04-queues", QueuesAsync),
        ("05-skills", SkillsAsync),
        ("06-list", ListAsync),
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
    /// Seeds two skills, three queues and one agent's entitlements (not recorded). Fake data only.
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var (context, page) = await Seed.OpenAsync(browser);

        await Seed.SkillAsync(page, "Spanish", "Speaks Spanish with customers");
        await Seed.SkillAsync(page, "Billing", "Can resolve invoice and payment questions");
        await Seed.QueueAsync(page, "Support", "Customers who need help with an order");
        await Seed.QueueAsync(page, "Sales", "New customers and quotes");
        await Seed.QueueAsync(page, "Billing", "Invoices and payments");
        await Seed.EntitlementAsync(page, "jamie.chen", ["Support", "Sales"]);
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

    private static async Task PickAsync(Scene scene, string selectId, string text, bool close = false)
    {
        await scene.ClickAsync(scene.Page.Locator($"button[data-id='{selectId}']"), pauseAfter: 400);
        await scene.ClickAsync(scene.Page.Locator($"div.bootstrap-select:has(button[data-id='{selectId}']) .dropdown-item").Filter(new() { HasTextString = text }).First, pauseAfter: 400);

        if (close)
        {
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.PauseAsync(300);
        }
    }

    private static async Task SetAsync(Scene scene, ILocator field, string text, int delay = 40)
    {
        await scene.ClickAsync(field, pauseAfter: 150);
        await field.FillAsync(string.Empty);
        await field.PressSequentiallyAsync(text, new() { Delay = delay });
        await scene.PauseAsync(250);
    }

    // ----- Clips -----

    private static async Task SkillAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/skills");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ski-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("ul.admin-list").First, "Skills that exist");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a").Filter(new() { HasTextString = "Add Skill" }).First);
        });

        await scene.StepAsync("ski-2", async () =>
        {
            await scene.TypeAsync(Field(scene, "ContactCenterSkill_Name"), "Tier 2 support", delay: 55);
            await scene.TypeAsync(Field(scene, "ContactCenterSkill_Description"), "Handles the hardest support questions", delay: 25);
        });

        await scene.StepAsync("ski-3", async () =>
        {
            await scene.HighlightAsync(Section(Field(scene, "ContactCenterSkill_Enabled")), "Enabled");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }

    private static async Task QueuesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/agent-entitlements");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ent-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a").Filter(new() { HasTextString = "Add agent entitlements" }).First);
        });

        await scene.StepAsync("ent-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button[data-id='agent-entitlement-user-selector']"), pauseAfter: 1500);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = "maria.lopez" }).First, pauseAfter: 500);
        });

        await scene.StepAsync("ent-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button[data-id='AllowedQueueIds']"), pauseAfter: 500);
            await scene.ClickAsync(scene.Page.Locator("div.bootstrap-select:has(button[data-id='AllowedQueueIds']) .dropdown-item").Filter(new() { HasTextString = "Support" }).First, pauseAfter: 400);
            await scene.ClickAsync(scene.Page.Locator("div.bootstrap-select:has(button[data-id='AllowedQueueIds']) .dropdown-item").Filter(new() { HasTextString = "Billing" }).First, pauseAfter: 400);
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.PauseAsync(500);
        });

        await scene.StepAsync("ent-4", async () =>
        {
            await SetAsync(scene, scene.Page.Locator("input[name$='.Priority']").Nth(1), "2");
            await SetAsync(scene, scene.Page.Locator("input[name$='.DelaySeconds']").Nth(1), "30");
            await scene.HighlightAsync(scene.Page.Locator("input[name$='.Priority']").Nth(1), "Priority and delay");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SkillsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        // The same form as the previous clip, with the user and queues already chosen (not recorded).
        await scene.OpenAsync("/Admin/contact-center/agent-entitlements/create");
        await scene.Page.Locator("button[data-id='agent-entitlement-user-selector']").ClickAsync();
        await scene.Page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = "maria.lopez" }).First.ClickAsync();
        await scene.Page.Locator("button[data-id='AllowedQueueIds']").ClickAsync();
        await scene.Page.Locator("div.bootstrap-select:has(button[data-id='AllowedQueueIds']) .dropdown-item").Filter(new() { HasTextString = "Support" }).First.ClickAsync();
        await scene.Page.Locator("div.bootstrap-select:has(button[data-id='AllowedQueueIds']) .dropdown-item").Filter(new() { HasTextString = "Billing" }).First.ClickAsync();
        await scene.Page.Keyboard.PressAsync("Escape");
        await scene.PauseAsync(500);
        await ScrollToAsync(scene, scene.Page.Locator("button[data-id='AllowedCampaignIds']"), 200);
        await scene.StartCaptureAsync();

        await scene.StepAsync("agk-1", async () =>
        {
            await scene.HighlightAsync(Section(Field(scene, "AllowedCampaignIds")), "Allowed campaigns");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("agk-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Add skill" }).First, pauseAfter: 700);
            await scene.SelectAsync(Part(scene, "-skill"), "Spanish", pauseAfter: 400);
        });

        await scene.StepAsync("agk-3", async () =>
        {
            await scene.SelectAsync(Part(scene, "-proficiency"), "4", pauseAfter: 600);
            await scene.HighlightAsync(Part(scene, "-proficiency"), "Proficiency 1 to 5");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("agk-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }

    private static async Task ListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/agent-entitlements");
        await scene.StartCaptureAsync();

        await scene.StepAsync("lst-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("ul.admin-list").First, "Queues and campaigns per agent");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("lst-2", async () =>
        {
            await scene.PauseAsync(500);
        });
    }
}
