using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the video, recorded as maria.lopez (a member of the Sales Team who is also a manager) on the demo site that
/// the Calendars getting started video seeds. Run <c>Recorder prepare</c> first: it adds one tentative event for sam.patel.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5396";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    // The sign-in of maria.lopez (Recorder setup with VIDEO_USER_NAME=maria.lopez); state-admin.json is admin's, for prepare.
    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("01-schedule", ScheduleAsync),
        ("03-team", TeamAsync),
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

    /// <summary>Adds a tentative event to sam.patel's calendar, as admin, outside the capture. The editor takes UTC times.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = Path.Combine(Root, "state-admin.json"), ViewportSize = new ViewportSize { Width = 1536, Height = 864 } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        await page.GotoAsync($"{BaseUrl}/Admin/calendar/events/create?calendarId=samcalendar00001&eventType=Meeting&start=2026-11-04T20:00:00Z&end=2026-11-04T21:00:00Z");
        await page.Locator("#CalendarEvent_Title").FillAsync("Dentist");
        await page.Locator("#CalendarEvent_Status").SelectOptionAsync("Tentative");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).First.ClickAsync());
        Console.WriteLine("Event Dentist");
        await context.CloseAsync();
    }

    private static async Task ScheduleAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await Cal.OpenCalendarAsync(scene, weeksAhead: 1);
        await scene.StartCaptureAsync();

        var page = scene.Page;

        await scene.StepAsync("sch-1", async () =>
        {
            await scene.ClickAsync(page.Locator("label[for='schedule-everyone']"), pauseAfter: 900);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Show" }).First);
            await Task.Delay(1200);
            await scene.ClickAsync(page.Locator(".fc-next-button"), pauseAfter: 1800);
            await scene.MoveToAsync(1000, 600);
            await scene.ScrollAsync(0, 450);
        });

        await scene.StepAsync("sch-2", async () =>
        {
            await scene.ScrollAsync(0, -450);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("a").Filter(new() { HasTextString = "Back to my calendars" }).First);
            await scene.ClickAsync(page.Locator("label[for='schedule-people']"), pauseAfter: 900);
            await scene.ClickAsync(page.Locator("button[data-id='schedule-people-picker']"), pauseAfter: 800);
            await scene.TypeAsync(page.Locator(".dropdown-menu.show .bs-searchbox input"), "jamie", delay: 80);
            await Task.Delay(1300);
            await scene.ClickAsync(page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = "jamie" }).First, pauseAfter: 500);
            await page.Keyboard.PressAsync("Escape");
            await Task.Delay(500);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Show" }).First);
            await Task.Delay(1000);
            await scene.ClickAsync(page.Locator(".fc-next-button"), pauseAfter: 1600);
            await scene.MoveToAsync(1000, 600);
            await scene.ScrollAsync(0, 450);
        });

        await scene.StepAsync("sch-3", async () =>
        {
            await scene.HighlightAsync(page.Locator(".fc-timegrid-event").Filter(new() { HasTextString = "Busy" }).First, "Busy: no title");
            await Task.Delay(4000);
            await scene.ClearHighlightsAsync();
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task TeamAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await Cal.OpenCalendarAsync(scene, weeksAhead: 0);
        await scene.StartCaptureAsync();

        var page = scene.Page;

        await scene.StepAsync("team-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(page.Locator("#left-nav a[href='/Admin/calendar/availability']").First);
            await Task.Delay(1000);
            await scene.HighlightAsync(page.Locator("#calendarId"));
            await Task.Delay(1000);
            await scene.ClearHighlightsAsync();
            await Cal.SetAsync(scene, "date", "2026-11-04");
            await page.Locator("#minutes").FillAsync("60");
            await scene.ClickAndWaitForNavigationAsync(page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Show" }).First);
            await Task.Delay(800);
        });

        await scene.StepAsync("team-2", async () =>
        {
            await scene.HighlightAsync(page.Locator("table").First);
            await Task.Delay(5000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("team-3", async () =>
        {
            await scene.HighlightAsync(page.Locator(".card, .list-group").Filter(new() { HasTextString = "When everyone is free" }).Last);
            await Task.Delay(3500);
            await scene.ClearHighlightsAsync();
            await Cal.SetAsync(scene, "minutes", "120");
            await scene.ClickAndWaitForNavigationAsync(page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Show" }).First);
            await Task.Delay(1200);
            await scene.HighlightAsync(page.Locator(".card, .list-group").Filter(new() { HasTextString = "When everyone is free" }).Last);
        });

        await scene.StepAsync("team-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await Cal.SetAsync(scene, "date", "2026-11-02");
            await scene.ClickAndWaitForNavigationAsync(page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Show" }).First);
            await Task.Delay(1500);
        });
    }
}
