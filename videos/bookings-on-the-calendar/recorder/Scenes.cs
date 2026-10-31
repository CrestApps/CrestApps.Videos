using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the "Bookings on the calendar" video, one method per "clip" of storyboard.json.
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
        ("02-calendar", CalendarAsync),
        ("03-block", BlockAsync),
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

    private static async Task CalendarAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/bookings");
        await CompactMenuAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("bc-1", async () =>
        {
            await scene.PauseAsync(1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/bookings/Calendar']").Filter(new() { HasTextString = "Calendar" }).Last);
            await scene.PauseAsync(1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='month=2026-11']").First);
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("bc-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a").Filter(new() { HasTextString = "Open in calendar" }).First);
            await scene.ClickAsync(scene.Page.Locator(".fc-next-button"), pauseAfter: 1500);
            await scene.HighlightAsync(scene.Page.Locator(".fc-event").First);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("bc-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".fc-event").First, pauseAfter: 1500);
            await scene.PauseAsync(3500);
        });

        await scene.StepAsync("bc-4", async () =>
        {
            await scene.NavigateAsync("/Admin/bookings/Calendar");
            await scene.HighlightAsync(scene.Page.Locator("a, button").Filter(new() { HasTextString = "Sync to calendar" }).First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a, button").Filter(new() { HasTextString = "Sync to calendar" }).First);
            await scene.PauseAsync(2500);
        });
    }

    private static async Task BlockAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Contents/ContentItems/4bkngtour0alpinelakes0000a/Edit");
        await CompactMenuAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("blk-1", async () =>
        {
            await scene.PauseAsync(1500);
            await scene.Page.EvaluateAsync("() => document.querySelector('#BookingPart_BlockingCalendarId').scrollIntoView({ block: 'center' })");
            await scene.PauseAsync(800);
            await scene.HighlightAsync(scene.Page.Locator("#BookingPart_BlockingCalendarId"));
            await scene.PauseAsync(2500);
        });

        await scene.StepAsync("blk-2", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#BookingPart_BlockingCalendarId"), "admin");
            await scene.PauseAsync(3000);
        });
    }
}
