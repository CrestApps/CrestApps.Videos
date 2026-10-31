using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Scheduling "how open times are worked out" video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // The Tuesday and the Monday the video books on (a weekday after the day of the take), and a closed day.
    private const string Tuesday = "2026-11-03";
    private const string Monday = "2026-11-02";
    private const string ClosedDay = "2026-12-24";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-hours", HoursAsync),
        ("03-busy", BusyAsync),
        ("04-limits", LimitsAsync),
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

    private static ILocator Button(Scene scene, string text) => scene.Page.Locator("a.btn, button.btn").Filter(new() { HasTextString = text }).First;

    // The id of an appointment type, from the booking page's list.
    private static async Task<string> TypeIdAsync(Scene scene, string name)
        => await scene.Page.EvaluateAsync<string>("async (name) => { const html = await (await fetch('/Admin/scheduling/appointments/new')).text(); const doc = new DOMParser().parseFromString(html, 'text/html'); return [...doc.querySelectorAll('#AppointmentTypeId option')].find(o => o.textContent.trim() === name)?.value ?? ''; }", name);

    // Chooses the type and the day on the booking page, and shows the times.
    private static async Task ShowTimesAsync(Scene scene, string type, string date)
    {
        await scene.SelectAsync(scene.Page.Locator("#AppointmentTypeId"), type);
        await scene.Page.Locator("#Date").FillAsync(date);
        await scene.PauseAsync(400);
        await scene.ClickAndWaitForNavigationAsync(Button(scene, "Show times"));
    }

    private static ILocator Times(Scene scene) => scene.Page.Locator(".card-body .d-flex").First;

    private static ILocator Time(Scene scene, string text) => scene.Page.Locator("label.btn-outline-primary").Filter(new() { HasTextString = text }).First;

    private static async Task HoursAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/scheduling/appointments");
        await CompactMenuAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("hours-1", async () =>
        {
            await scene.PauseAsync(1500);
            await scene.ClickAndWaitForNavigationAsync(Button(scene, "Book an appointment"));
            await ShowTimesAsync(scene, "Initial consultation", Tuesday);
        });

        await scene.StepAsync("hours-2", async () =>
        {
            await scene.HighlightAsync(Times(scene));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Time(scene, "11:30 AM"));
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Time(scene, "1:00 PM"));
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("hours-3", async () =>
        {
            await scene.HighlightAsync(Time(scene, "11:30 AM"));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Time(scene, "4:30 PM"));
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("hours-4", async () =>
        {
            await scene.Page.Locator("#Date").FillAsync(ClosedDay);
            await scene.PauseAsync(500);
            await scene.ClickAndWaitForNavigationAsync(Button(scene, "Show times"));
            await scene.HighlightAsync(scene.Page.Locator(".card-body .text-muted").First);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("hours-5", async () =>
        {
            await ShowTimesAsync(scene, "Physiotherapy session", Monday);
            await scene.HighlightAsync(Times(scene));
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("hours-6", async () =>
        {
            await scene.Page.Locator("#Date").FillAsync(Tuesday);
            await scene.PauseAsync(500);
            await scene.ClickAndWaitForNavigationAsync(Button(scene, "Show times"));
            await scene.HighlightAsync(scene.Page.Locator(".card-body .text-muted").First);
        });
    }

    private static async Task BusyAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/calendar");
        await CompactMenuAsync(scene);
        var typeId = await TypeIdAsync(scene, "Initial consultation");
        await scene.StartCaptureAsync();

        await scene.StepAsync("busy-1", async () =>
        {
            await scene.PauseAsync(1500);
            await scene.ClickAsync(scene.Page.Locator("a, button").Filter(new() { HasTextString = "New event" }).First, pauseAfter: 1800);
            var modal = scene.Page.Locator(".modal.show");
            var card = modal.Locator(".card").Filter(new() { HasTextString = "Dr. Maya Chen" }).First;
            await scene.HighlightAsync(card);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(card.Locator("a").Filter(new() { HasTextString = "Blocked time" }).First, pauseAfter: 2500);
        });

        await scene.StepAsync("busy-2", async () =>
        {
            var modal = scene.Page.Locator(".modal.show");
            await scene.TypeAsync(modal.Locator("#CalendarEvent_Title"), "Staff meeting", delay: 60);
            await modal.Locator("#CalendarEvent_StartDate").FillAsync(Tuesday);
            await modal.Locator("#CalendarEvent_StartTime").FillAsync("14:00");
            await modal.Locator("#CalendarEvent_EndDate").FillAsync(Tuesday);
            await modal.Locator("#CalendarEvent_EndTime").FillAsync("15:00");
            await scene.HighlightAsync(modal.Locator("#CalendarEvent_StartTime"));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(modal.Locator("button").Filter(new() { HasTextString = "Save" }).Last, pauseAfter: 2500);
        });

        await scene.StepAsync("busy-3", async () =>
        {
            await scene.NavigateAsync("/Admin/scheduling/appointments/new");
            await ShowTimesAsync(scene, "Initial consultation", Tuesday);
            await scene.HighlightAsync(Time(scene, "1:00 PM"));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Time(scene, "3:00 PM"));
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("busy-4", async () =>
        {
            await scene.ClickAsync(Time(scene, "10:00 AM"), pauseAfter: 600);
            await scene.TypeAsync(scene.Page.Locator("#CustomerName"), "Jordan Blake", delay: 55);
            await scene.TypeAsync(scene.Page.Locator("#CustomerEmail"), "jordan.blake@example.com", delay: 35);
            await scene.TypeAsync(scene.Page.Locator("#CustomerPhone"), "555-0142", delay: 55);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Book" }).Last);
        });

        await scene.StepAsync("busy-5", async () =>
        {
            await scene.NavigateAsync("/Admin/scheduling/appointments/new");
            await ShowTimesAsync(scene, "Initial consultation", Tuesday);
            await scene.HighlightAsync(Time(scene, "9:00 AM"));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Time(scene, "11:00 AM"));
        });

        await scene.ClearHighlightsAsync();
        _ = typeId;
    }

    private static async Task LimitsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/scheduling/types");
        await CompactMenuAsync(scene);
        var yoga = await TypeIdAsync(scene, "Yoga class");
        await scene.StartCaptureAsync();

        await scene.StepAsync("limits-1", async () =>
        {
            await scene.PauseAsync(1200);
            var row = scene.Page.Locator(".list-group-item").Filter(new() { HasTextString = "Initial consultation" }).First;
            await scene.ClickAndWaitForNavigationAsync(row.Locator("a").Filter(new() { HasTextString = "Edit" }).First);
            await scene.ScrollAsync(0, 1500);
            await scene.HighlightAsync(scene.Page.Locator("#MinimumNoticeMinutes"));
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("limits-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#HorizonDays"));
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#MaxPerDay"));
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("limits-3", async () =>
        {
            await scene.NavigateAsync($"/Admin/scheduling/appointments/new?AppointmentTypeId={yoga}&Date={Tuesday}");
            await scene.HighlightAsync(Times(scene));
            await scene.PauseAsync(2500);
        });
    }
}
