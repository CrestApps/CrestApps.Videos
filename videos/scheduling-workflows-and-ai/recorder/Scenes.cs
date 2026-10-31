using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Scheduling "workflows, AI and moving it" video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("01-workflows", WorkflowsAsync),
        ("02-ai", AiAsync),
        ("03-deploy", DeployAsync),
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

    private static async Task CompactMenuAsync(Scene scene)
    {
        await scene.Page.EvaluateAsync("() => { if (!document.body.classList.contains('left-sidebar-compact')) document.querySelector('.leftbar-compactor')?.click(); }");
        await Task.Delay(400);
    }

    private static ILocator Add(Scene scene, string text) => scene.Page.Locator("a.btn").Filter(new() { HasTextString = text }).First;

    private static ILocator StepCard(Scene scene, string title)
        => scene.Page.Locator(".deployment-step-item").Filter(new() { Has = scene.Page.Locator("h4.card-title", new() { HasTextRegex = new System.Text.RegularExpressions.Regex($@"^\s*{title}\s*$") }) }).First;

    private static async Task WorkflowsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Workflows/Types");
        await CompactMenuAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("wf-1", async () =>
        {
            await scene.PauseAsync(1200);
            await scene.ClickAndWaitForNavigationAsync(Add(scene, "Create Workflow"));
            await scene.TypeAsync(scene.Page.Locator("#Name"), "Confirmed appointments", delay: 55);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("Save|Create") }).First);
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("wf-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button, a, div, span, summary, h3, h4, h5").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"^\s*Scheduling\s*\d+\s*$") }).Last, pauseAfter: 1200);
            await scene.ScrollAsync(0, 120);
        });

        await scene.StepAsync("wf-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("^\\s*Appointment Confirmed") }).First, pauseAfter: 1800);
            await scene.HighlightAsync(scene.Page.Locator("text=Appointment types").Last);
            await scene.PauseAsync(3500);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("wf-4", async () =>
        {
            await scene.MoveToAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Find Appointment Slots" }).First);
            await scene.ScrollAsync(0, 600);
            await scene.PauseAsync(1500);
            await scene.ScrollAsync(0, 600);
        });

        await scene.StepAsync("wf-5", async () =>
        {
            await scene.PauseAsync(4000);
        });
    }

    private static async Task AiAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ai/profiles");
        await CompactMenuAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("ai-1", async () =>
        {
            await scene.PauseAsync(1200);
            await scene.ClickAsync(scene.Page.Locator("a, button").Filter(new() { HasTextString = "Add Profile" }).First, pauseAfter: 1800);
            var card = scene.Page.Locator(".modal.show .card").Filter(new() { HasTextString = "Blank profile" }).First;
            await scene.MoveToAsync(card);
            await scene.ClickAndWaitForNavigationAsync(card.Locator("a").First);
        });

        await scene.StepAsync("ai-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".nav-link").Filter(new() { HasTextString = "Capabilities" }).First, pauseAfter: 1200);
            await scene.ScrollAsync(0, 380);
            await scene.HighlightAsync(scene.Page.Locator("h5, h4, h3, legend, strong, .fw-bold").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("^\\s*Scheduling\\s*$") }).First);
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("label").Filter(new() { HasTextString = "Select All in Scheduling" }).First, pauseAfter: 1000);
        });
    }

    private static async Task DeployAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/DeploymentPlan/Index");
        await CompactMenuAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("dep-1", async () =>
        {
            await scene.PauseAsync(1200);
            await scene.ClickAndWaitForNavigationAsync(Add(scene, "Add Deployment Plan"));
            await scene.TypeAsync(scene.Page.Locator("#Name"), "Copy scheduling", delay: 55);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Create" }).First);
            await scene.ClickAsync(scene.Page.Locator("a, button").Filter(new() { HasTextString = "Add Step" }).First, pauseAfter: 1500);
            var row = StepCard(scene, "Calendars");
            await row.EvaluateAsync("e => e.scrollIntoView({ block: 'center' })"); await scene.PauseAsync(600);
            await scene.HighlightAsync(row);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(row.Locator("a").Filter(new() { HasTextString = "Add" }).First);
            await scene.ClickAsync(scene.Page.Locator("label").Filter(new() { HasTextString = "Include events" }).First, pauseAfter: 800);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button[type=submit]").Filter(new() { HasTextString = "Create" }).First);
        });

        await scene.StepAsync("dep-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("a, button").Filter(new() { HasTextString = "Add Step" }).First, pauseAfter: 1500);
            var row = StepCard(scene, "Scheduling");
            await row.EvaluateAsync("e => e.scrollIntoView({ block: 'center' })"); await scene.PauseAsync(600);
            await scene.ClickAndWaitForNavigationAsync(row.Locator("a").Filter(new() { HasTextString = "Add" }).First);
            await scene.ClickAsync(scene.Page.Locator("label").Filter(new() { HasTextString = "Include appointments" }).First, pauseAfter: 900);
            await scene.ClickAsync(scene.Page.Locator("label").Filter(new() { HasTextString = "Include the waitlist" }).First, pauseAfter: 900);
            await scene.HighlightAsync(scene.Page.Locator("#SchedulingDeploymentStep_IncludeAppointments"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button[type=submit]").Filter(new() { HasTextString = "Create" }).First);
        });

        await scene.StepAsync("dep-3", async () =>
        {
            await scene.PauseAsync(4000);
        });
    }
}
