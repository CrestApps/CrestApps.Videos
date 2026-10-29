using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Contact Center Roles and Permissions video, one method per "clip" of storyboard.json.
/// The site holds the Contact Center with a few queues and agents, the Roles feature, and a user named taylor.brooks; it has no
/// Agent or Supervisor role yet (see README.md).
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-roles", RolesAsync),
        ("03-agent", AgentAsync),
        ("04-supervisor", SupervisorAsync),
        ("05-users", UsersAsync),
        ("06-entitlement", EntitlementAsync),
        ("07-gated", GatedAsync),
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

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Button(Scene scene, string name) => scene.Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).First;

    private static ILocator Row(Scene scene, string text) => scene.Page.Locator("tr").Filter(new() { HasText = text }).First;

    // Types into the permission search box, replacing what was there.
    private static async Task SearchAsync(Scene scene, string text)
    {
        var box = scene.Page.Locator("#search-box");
        await scene.ClearHighlightsAsync();
        await box.FillAsync(string.Empty);
        await scene.TypeAsync(box, text, delay: 70);
        await scene.PauseAsync(600);
    }

    private static async Task ShowRowAsync(Scene scene, string text, string caption, int hold = 2200, bool keep = false)
    {
        await scene.HighlightAsync(Row(scene, text), caption);
        await scene.PauseAsync(hold);

        if (!keep)
        {
            await scene.ClearHighlightsAsync();
        }
    }

    private static async Task RolesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync("/Admin/Roles/Index");
        await scene.StartCaptureAsync();

        await scene.StepAsync("roles-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("a", new() { HasTextString = "Administrator" }).First, "Every permission");
            await scene.PauseAsync(4500);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("roles-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a", new() { HasTextString = "Add Role" }).First);
            await scene.TypeAsync(scene.Page.Locator("input[name='RoleName']"), "Agent");
            await scene.ClickAndWaitForNavigationAsync(Button(scene, "Create"));
        });

        await scene.StepAsync("roles-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a", new() { HasTextString = "Add Role" }).First);
            await scene.TypeAsync(scene.Page.Locator("input[name='RoleName']"), "Supervisor");
            await scene.ClickAndWaitForNavigationAsync(Button(scene, "Create"));
            await scene.HighlightAsync(scene.Page.Locator("a", new() { HasTextString = "Supervisor" }).First, "Supervisor");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task AgentAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync("/Admin/Roles/Edit/Agent");
        await scene.StartCaptureAsync();

        await scene.StepAsync("agent-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#search-box"), "Search permissions");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await SearchAsync(scene, "queues");
            await ShowRowAsync(scene, "Sign in to Contact Center queues and campaigns", "Allowed", 3000);
            await ShowRowAsync(scene, "Manage Contact Center queues", "Not allowed", 2000, keep: true);
        });

        await scene.StepAsync("agent-2", async () =>
        {
            await SearchAsync(scene, "live interactions");
            await ShowRowAsync(scene, "Pause recording on own live interactions", "Own live calls only", 2200);
            await ShowRowAsync(scene, "Initiate secure capture on own live interactions", "Own live calls only", 2200);
            await SearchAsync(scene, "own call recordings");
            await ShowRowAsync(scene, "Listen to own call recordings", "Allowed", 2000, keep: true);
        });

        await scene.StepAsync("agent-3", async () =>
        {
            await SearchAsync(scene, "soft phone");
            await ShowRowAsync(scene, "Use the telephony soft phone", "Not in the Agent role", 2600, keep: true);
            await scene.ClickAsync(Row(scene, "Use the telephony soft phone").Locator("input[type=checkbox]").First, pauseAfter: 1000);
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(Button(scene, "Save"), pauseAfter: 400));
        });
    }

    private static async Task SupervisorAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync("/Admin/Roles/Edit/Supervisor");
        await scene.StartCaptureAsync();

        await scene.StepAsync("sup-1", async () =>
        {
            await SearchAsync(scene, "monitor");
            await ShowRowAsync(scene, "Monitor the Contact Center in real time", "Opens the live dashboard", 3500);
            await SearchAsync(scene, "contact center reports");
            await ShowRowAsync(scene, "View Contact Center reports", "Allowed", 2500, keep: true);
        });

        await scene.StepAsync("sup-2", async () =>
        {
            await SearchAsync(scene, "take over");
            await ShowRowAsync(scene, "Take over, end, transfer and record live Contact Center calls", "Step into live calls", 3500);
            await SearchAsync(scene, "recordings");
            await ShowRowAsync(scene, "Listen to anyone's call recordings", "Allowed", 2500, keep: true);
        });

        await scene.StepAsync("sup-3", async () =>
        {
            await SearchAsync(scene, "queues");
            await ShowRowAsync(scene, "Sign in to Contact Center queues and campaigns", "Not allowed: add the Agent role", 4500, keep: true);
        });
    }

    private static async Task UsersAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync("/Admin/Users/Index");
        await scene.StartCaptureAsync();

        await scene.StepAsync("user-1", async () =>
        {
            var user = scene.Page.Locator("li.list-group-item, tr").Filter(new() { HasText = "taylor.brooks" }).First;
            await scene.ClickAndWaitForNavigationAsync(user.Locator("a", new() { HasTextString = "Edit" }).First);
            await scene.HighlightAsync(scene.Page.Locator("label", new() { HasTextString = "Supervisor" }).First.Locator("xpath=../.."), "Roles");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("label", new() { HasTextString = "Supervisor" }).First, pauseAfter: 800);
        });

        await scene.StepAsync("user-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("label", new() { HasTextString = "Agent" }).First, pauseAfter: 1500);
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(Button(scene, "Save"), pauseAfter: 400));
        });
    }

    private static async Task EntitlementAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync("/Admin/contact-center/dashboard");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ent-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("No queues are configured.").First, "No entitlement record, no queues");
            await scene.PauseAsync(7000);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ent-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#left-nav").GetByText("Management", new() { Exact = true }).First, pauseAfter: 900);
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/agent-entitlements"));
            await scene.PauseAsync(1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a", new() { HasTextString = "Add agent entitlements" }).First);
        });

        await scene.StepAsync("ent-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button[data-id='agent-entitlement-user-selector']"), pauseAfter: 600);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show .dropdown-item", new() { HasTextString = "taylor.brooks" }).First, pauseAfter: 500);
            await scene.ClickAsync(scene.Page.Locator("button[data-id='AllowedQueueIds']"), pauseAfter: 500);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show .dropdown-item", new() { HasTextString = "Support" }).First, pauseAfter: 400);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show .dropdown-item", new() { HasTextString = "Billing" }).First, pauseAfter: 400);
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.PauseAsync(300);
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(Button(scene, "Save"), pauseAfter: 400));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task GatedAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync("/Admin/Roles/Edit/Supervisor");
        await scene.StartCaptureAsync();

        await scene.StepAsync("gate-1", async () =>
        {
            await SearchAsync(scene, "call recordings");
            await ShowRowAsync(scene, "Listen to own call recordings", "Own calls", 3000);
            await ShowRowAsync(scene, "Listen to anyone's call recordings", "Anyone's calls", 3000);
            await ShowRowAsync(scene, "Delete call recordings", "Administrators only", 2500, keep: true);
        });

        await scene.StepAsync("gate-2", async () =>
        {
            await SearchAsync(scene, "shared queue voicemail");
            await ShowRowAsync(scene, "Access shared queue voicemail", "Work the box", 4500);
            await ShowRowAsync(scene, "Manage shared queue voicemail", "Delete and take over", 4000, keep: true);
        });

        await scene.StepAsync("gate-3", async () =>
        {
            await SearchAsync(scene, "View Contact Center reports");
            await ShowRowAsync(scene, "View Contact Center reports", "Contact Center reports", 3000, keep: true);
        });
    }
}
