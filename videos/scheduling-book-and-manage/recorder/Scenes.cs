using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Scheduling "book and manage appointments" video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // A Wednesday and the Thursday after it (days after the day of the take).
    private const string Wednesday = "2026-11-04";
    private const string Thursday = "2026-11-05";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-book", BookAsync),
        ("03-manage", ManageAsync),
        ("04-calendar", CalendarAsync),
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

    private static ILocator Time(Scene scene, string text) => scene.Page.Locator("label.btn-outline-primary").Filter(new() { HasTextString = text }).First;

    private static ILocator Row(Scene scene, string text) => scene.Page.Locator(".list-group-item").Filter(new() { HasTextString = text }).First;

    private static async Task BookAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/scheduling/types");
        await CompactMenuAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("book-1", async () =>
        {
            await scene.PauseAsync(1200);
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Ultrasound scan").Locator("a").Filter(new() { HasTextString = "Book" }).First);
            await scene.Page.Locator("#Date").FillAsync(Wednesday);
            await scene.PauseAsync(500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Show times" }).First);
        });

        await scene.StepAsync("book-2", async () =>
        {
            await scene.ClickAsync(Time(scene, "10:00 AM"), pauseAfter: 600);
            await scene.TypeAsync(scene.Page.Locator("#CustomerName"), "Riley Brooks", delay: 55);
            await scene.TypeAsync(scene.Page.Locator("#CustomerEmail"), "riley.brooks@example.com", delay: 35);
            await scene.TypeAsync(scene.Page.Locator("#CustomerPhone"), "555-0167", delay: 55);
            await scene.TypeAsync(scene.Page.Locator("#Notes"), "Follow-up scan, bring the earlier report.", delay: 30);
        });

        await scene.StepAsync("book-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Book" }).Last);
            await scene.HighlightAsync(scene.Page.Locator(".card, .badge").Filter(new() { HasTextString = "Requested" }).First);
            await scene.PauseAsync(2000);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("book-4", async () =>
        {
            await scene.NavigateAsync("/Admin/scheduling/appointments");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn").Filter(new() { HasTextString = "Book an appointment" }).First);
            await scene.HighlightAsync(scene.Page.Locator("#AppointmentTypeId"));
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#Date"));
        });
    }

    private static async Task ManageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/scheduling/appointments");
        await CompactMenuAsync(scene);
        var open = await scene.Page.Locator(".list-group-item").Filter(new() { HasTextString = "Riley Brooks" }).First.Locator("a").First.GetAttributeAsync("href");
        await scene.OpenAsync(open);
        await scene.StartCaptureAsync();

        await scene.StepAsync("manage-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".card").First);
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("code").First);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("manage-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button[value=Confirm]"));
            await scene.HighlightAsync(scene.Page.Locator(".card").First);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("manage-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("button[value=Complete]"));
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("button[value=NoShow]"));
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("input[name=reason]"));
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("manage-4", async () =>
        {
            await scene.ScrollAsync(0, 500);
            var day = scene.Page.Locator("input[type=date]").Last;
            await scene.ClickAsync(day, pauseAfter: 300);
            await scene.WaitForNavigationAsync(async () => await day.FillAsync(Thursday));
            await scene.ScrollAsync(0, 500);
            await scene.ClickAsync(scene.Page.Locator("label.btn-outline-primary").Filter(new() { HasTextString = "11:00 AM" }).First, pauseAfter: 800);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button[value=Reschedule]"));
        });

        await scene.StepAsync("manage-5", async () =>
        {
            var reason = scene.Page.Locator("input[name=reason]");
            await scene.TypeAsync(reason, "Customer called to cancel.", delay: 50);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button[value=Cancel]"));
            await scene.HighlightAsync(scene.Page.Locator(".card").First);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("manage-6", async () =>
        {
            await scene.NavigateAsync("/Admin/scheduling/appointments");
            await scene.HighlightAsync(scene.Page.Locator("input[name=search]"));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("select[name=status]"));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Filter(new() { HasTextString = "Jordan Blake" }).First);
        });
    }

    private static async Task CalendarAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/calendar");
        await CompactMenuAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("cal-1", async () =>
        {
            await scene.PauseAsync(1500);
            await scene.ClickAsync(scene.Page.Locator(".fc-next-button"), pauseAfter: 1500);
            await scene.ScrollAsync(0, 380);
            await scene.PauseAsync(2000);
        });

        await scene.StepAsync("cal-2", async () =>
        {
            await scene.PauseAsync(3000);
        });
    }
}
