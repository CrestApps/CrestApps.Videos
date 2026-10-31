using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the video, on the demo site that the Calendars getting started video seeds. <c>01-create</c> and
/// <c>03-direct</c> are recorded as admin (state.json), <c>02-book</c> as maria.lopez (state-maria.json). The clips run in
/// order: the rooms are made by the first one.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5396";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("01-create", CreateAsync),
        ("02-book", BookAsync),
        ("03-direct", DirectAsync),
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

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string statePath = null)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, statePath ?? StatePath, Path.GetDirectoryName(directory), durations);

    private static async Task AddCalendarAsync(Scene scene, string type)
    {
        var page = scene.Page;
        await scene.ClickAsync(page.Locator("a, button").Filter(new() { HasTextString = "Add calendar" }).First, pauseAfter: 1000);
        await scene.ClickAndWaitForNavigationAsync(page.Locator($".dropdown-menu.show a[href='/Admin/calendars/create/{type}']"));
        await Task.Delay(700);
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/calendars");
        await scene.StartCaptureAsync();

        var page = scene.Page;

        await scene.StepAsync("room-1", async () =>
        {
            await AddCalendarAsync(scene, "Room");
            await scene.TypeAsync(page.Locator("#Calendar_Name"), "Boardroom", delay: 60);
            await scene.TypeAsync(page.Locator("#Calendar_Location"), "Second floor, north wing", delay: 35);
            await scene.ClickAsync(page.Locator("#Calendar_Seats"), pauseAfter: 200);
            await page.Locator("#Calendar_Seats").FillAsync("12");
            await Task.Delay(500);
            await scene.TypeAsync(page.Locator("#Calendar_Amenities"), "Projector, whiteboard, video conferencing", delay: 30);
        });

        await scene.StepAsync("room-2", async () =>
        {
            await scene.MoveToAsync(1000, 600);
            await scene.ScrollAsync(0, 350);
            await scene.HighlightAsync(page.Locator("#Calendar_Visibility"));
            await Task.Delay(2500);
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(1000, 600);
            await scene.ScrollAsync(0, 900);
            await scene.HighlightAsync(page.Locator("#Calendar_Capacity"));
            await Task.Delay(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last);
            await Task.Delay(1000);
        });

        await scene.StepAsync("equip-1", async () =>
        {
            await AddCalendarAsync(scene, "Equipment");
            await scene.TypeAsync(page.Locator("#Calendar_Name"), "Demo projector", delay: 60);
            await scene.TypeAsync(page.Locator("#Calendar_Amenities"), "HDMI and USB-C cables", delay: 30);
            await scene.MoveToAsync(1000, 600);
            await scene.ScrollAsync(0, 1200);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last);
            await Task.Delay(1500);
        });
    }

    private static async Task PickRoomAsync(Scene scene, string room)
    {
        var page = scene.Page;
        await scene.MoveToAsync(800, 520);
        await scene.ScrollAsync(0, 1600);
        await scene.ClickAsync(page.Locator("button[data-id='CalendarEvent_ResourceIds']"), pauseAfter: 900);
        await scene.ClickAsync(page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = room }).First, pauseAfter: 600);
        await page.Keyboard.PressAsync("Escape");
        await Task.Delay(600);
    }

    private static async Task BookAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, Path.Combine(Root, "state-maria.json"));
        await Cal.OpenCalendarAsync(scene, weeksAhead: 2);
        await scene.StartCaptureAsync();

        var page = scene.Page;
        var modal = Cal.Modal(scene);

        async Task NewMeetingAsync(string title, string start, string end)
        {
            await scene.ClickAsync(page.Locator("a[data-calendar-new-event]"), pauseAfter: 1500);
            await Cal.ChooseKindAsync(scene, "maria.lopez", "Meeting");
            await scene.TypeAsync(modal.Locator("#CalendarEvent_Title"), title, delay: 45);
            await Cal.SetAsync(scene, "CalendarEvent_StartDate", "2026-11-10");
            await Cal.SetAsync(scene, "CalendarEvent_StartTime", start);
            await Cal.SetAsync(scene, "CalendarEvent_EndDate", "2026-11-10");
            await Cal.SetAsync(scene, "CalendarEvent_EndTime", end);
        }

        await scene.StepAsync("book-1", async () =>
        {
            await NewMeetingAsync("Pipeline planning", "10:00", "11:00");
            await PickRoomAsync(scene, "Boardroom");
            await scene.ClickAsync(Cal.ModalButton(scene, "Save"), pauseAfter: 1200);
            await modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
            await Task.Delay(800);
        });

        await scene.StepAsync("book-2", async () =>
        {
            await scene.ClickAsync(page.Locator("label[for^='calendar-']").Filter(new() { HasTextString = "Boardroom" }).First, pauseAfter: 1500);
            await scene.MoveToAsync(1000, 600);
            await scene.ScrollAsync(0, 400);
            await Task.Delay(1500);
            await scene.HighlightAsync(page.Locator(".fc-timegrid-event").Filter(new() { HasTextString = "Pipeline planning" }).Last, "The room's booking");
            await Task.Delay(1500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, -400);
        });

        await scene.StepAsync("book-3", async () =>
        {
            await NewMeetingAsync("Budget check-in", "10:30", "11:30");
            await PickRoomAsync(scene, "Boardroom");
            await scene.ClickAsync(Cal.ModalButton(scene, "Save"), pauseAfter: 1800);
            await scene.MoveToAsync(800, 520);
            await scene.ScrollAsync(0, -4000);
            await Task.Delay(500);
            await scene.HighlightAsync(modal.Locator(".validation-summary-errors, .alert-danger, .text-danger").First);
            await Task.Delay(2500);
            await scene.ClearHighlightsAsync();
            await Cal.SetAsync(scene, "CalendarEvent_StartTime", "11:00");
            await Cal.SetAsync(scene, "CalendarEvent_EndTime", "12:00");
            await scene.ClickAsync(Cal.ModalButton(scene, "Save"), pauseAfter: 1200);
            await modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
        });
    }

    private static async Task DirectAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await Cal.OpenCalendarAsync(scene, weeksAhead: 2);
        await scene.StartCaptureAsync();

        var page = scene.Page;
        var modal = Cal.Modal(scene);

        await scene.StepAsync("dir-1", async () =>
        {
            await scene.ClickAsync(page.Locator("a[data-calendar-new-event]"), pauseAfter: 1800);
            await scene.HighlightAsync(modal.Locator(".card").Filter(new() { HasTextString = "Boardroom" }).First);
            await Task.Delay(2500);
            await scene.ClearHighlightsAsync();
            await Cal.ChooseKindAsync(scene, "Boardroom", "Maintenance");
            await scene.TypeAsync(modal.Locator("#CalendarEvent_Title"), "Projector service", delay: 45);
            await Cal.SetAsync(scene, "CalendarEvent_StartDate", "2026-11-11");
            await Cal.SetAsync(scene, "CalendarEvent_StartTime", "14:00");
            await Cal.SetAsync(scene, "CalendarEvent_EndDate", "2026-11-11");
            await Cal.SetAsync(scene, "CalendarEvent_EndTime", "16:00");
            await scene.ClickAsync(Cal.ModalButton(scene, "Save"), pauseAfter: 1200);
            await modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
            await Task.Delay(800);
            await scene.MoveToAsync(1000, 600);
            await scene.ScrollAsync(0, 450);
            await scene.HighlightAsync(page.Locator(".fc-timegrid-event").Filter(new() { HasTextString = "Projector service" }).First, "Maintenance");
            await Task.Delay(2500);
            await scene.ClearHighlightsAsync();
        });
    }
}
