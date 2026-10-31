using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Scheduling "online booking and the waitlist" video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // The public pages are recorded without signing in.
    public static string AnonymousPath => Path.Combine(Root, "anon.json");

    // A Monday (physiotherapy) and a Wednesday (ultrasound), days after the day of the take.
    private const string Monday = "2026-11-09";
    private const string Wednesday = "2026-11-04";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-setup", SetupAsync),
        ("03-customer", CustomerAsync),
        ("04-waitlist", WaitlistAsync),
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

    private static Task<Scene> NewAnonymousSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        File.WriteAllText(AnonymousPath, "{\"cookies\":[],\"origins\":[]}");

        return Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, AnonymousPath, Path.GetDirectoryName(directory), durations);
    }

    private static async Task CompactMenuAsync(Scene scene)
    {
        await scene.Page.EvaluateAsync("() => { if (!document.body.classList.contains('left-sidebar-compact')) document.querySelector('.leftbar-compactor')?.click(); }");
        await Task.Delay(400);
    }

    private static ILocator Time(Scene scene, string text) => scene.Page.Locator("label.btn-outline-primary").Filter(new() { HasTextString = text }).First;

    private static ILocator Row(Scene scene, string text) => scene.Page.Locator(".list-group-item").Filter(new() { HasTextString = text }).First;

    private static async Task SetupAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Features");
        await CompactMenuAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("setup-1", async () =>
        {
            await scene.PauseAsync(800);
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "Scheduling", delay: 80);
            await scene.PauseAsync(1200);
            await scene.HighlightAsync(Row(scene, "Scheduling Self-Service"));
            await scene.PauseAsync(2500);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("setup-2", async () =>
        {
            await scene.NavigateAsync("/Admin/scheduling/types");
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Physiotherapy session").Locator("a").Filter(new() { HasTextString = "Edit" }).First);
            await scene.ScrollAsync(0, 1500);
            await scene.HighlightAsync(scene.Page.Locator(".form-check").Filter(new() { HasTextString = "Customers can book it themselves" }).First);
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#Slug"));
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("setup-3", async () =>
        {
            await scene.NavigateAsync("/book/physiotherapy");
            await scene.PauseAsync(2500);
        });
    }

    private static async Task CustomerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewAnonymousSceneAsync(browser, directory, durations);
        scene.Page.Dialog += (_, dialog) => dialog.AcceptAsync();
        await scene.OpenAsync("/book/physiotherapy");
        await scene.StartCaptureAsync();

        await scene.StepAsync("cust-1", async () =>
        {
            await scene.PauseAsync(2500);
            await scene.Page.Locator("#date").FillAsync(Monday);
            await scene.PauseAsync(500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Show times" }).First);
            await scene.ScrollAsync(0, 250);
        });

        await scene.StepAsync("cust-2", async () =>
        {
            await scene.ClickAsync(Time(scene, "10:00 AM"), pauseAfter: 600);
            await scene.ScrollAsync(0, 300);
            await scene.TypeAsync(scene.Page.Locator("#CustomerName"), "Casey Morgan", delay: 55);
            await scene.TypeAsync(scene.Page.Locator("#CustomerEmail"), "casey.morgan@example.com", delay: 35);
            await scene.TypeAsync(scene.Page.Locator("#CustomerPhone"), "555-0188", delay: 55);
            await scene.TypeAsync(scene.Page.Locator("#Notes"), "Sore shoulder.", delay: 40);
        });

        await scene.StepAsync("cust-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Book" }).Last);
            await scene.PauseAsync(2500);
            await scene.HighlightAsync(scene.Page.Locator("p.text-muted").Filter(new() { HasTextString = "Keep this page" }).First);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("cust-4", async () =>
        {
            await scene.ScrollAsync(0, 500);
            await scene.HighlightAsync(scene.Page.Locator("button[value=Cancel]"));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("label.btn-outline-primary").Filter(new() { HasTextString = "11:00 AM" }).First, pauseAfter: 800);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button[value=Reschedule]"));
        });

        await scene.StepAsync("cust-5", async () =>
        {
            await scene.NavigateAsync("/book/ultrasound");
            await scene.Page.Locator("#date").FillAsync(Wednesday);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Show times" }).First);
            await scene.ScrollAsync(0, 250);
            await scene.ClickAsync(Time(scene, "1:00 PM"), pauseAfter: 600);
            await scene.ScrollAsync(0, 300);
            await scene.TypeAsync(scene.Page.Locator("#CustomerName"), "Riley Brooks", delay: 20);
            await scene.TypeAsync(scene.Page.Locator("#CustomerEmail"), "riley.brooks@example.com", delay: 10);
        });

        await scene.StepAsync("cust-6", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Book" }).Last);
            await scene.HighlightAsync(scene.Page.Locator(".alert-warning"));
            await scene.PauseAsync(2500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task WaitlistAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.Dialog += (_, dialog) => dialog.AcceptAsync();
        await scene.OpenAsync("/Admin/scheduling/waitlist");
        await CompactMenuAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("wait-1", async () =>
        {
            await scene.PauseAsync(1200);
            await scene.SelectAsync(scene.Page.Locator("#appointmentTypeId"), "Physiotherapy session");
            await scene.Page.Locator("#from").FillAsync(Monday);
            await scene.Page.Locator("#to").FillAsync(Monday);
            await scene.HighlightAsync(scene.Page.Locator("#from"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#customerName"), "Taylor Reed", delay: 55);
            await scene.TypeAsync(scene.Page.Locator("#customerEmail"), "taylor.reed@example.com", delay: 30);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button[type=submit]").Filter(new() { HasTextString = "Add" }).First);
        });

        await scene.StepAsync("wait-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").First);
            await scene.PauseAsync(2500);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("wait-3", async () =>
        {
            await scene.NavigateAsync("/Admin/scheduling/appointments");
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Casey Morgan").Locator("a").First);
            await scene.TypeAsync(scene.Page.Locator("input[name=reason]"), "The customer can't come.", delay: 45);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button[value=Cancel]"));
        });

        await scene.StepAsync("wait-4", async () =>
        {
            await scene.NavigateAsync("/Admin/scheduling/waitlist");
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").First);
            await scene.PauseAsync(2500);
        });

        await scene.ClearHighlightsAsync();
    }
}
