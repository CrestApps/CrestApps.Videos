using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the video, recorded as maria.lopez (a member of the Sales Team who is also a manager) on the demo site
/// that <see cref="PrepareAsync"/> seeds. The clips run in order: the change clip edits the events the add clip creates.
/// </summary>
public static partial class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5396";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    // The sign-in of the person the clips are recorded as (Recorder setup with VIDEO_USER_NAME=maria.lopez).
    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-open", OpenAsync),
        ("03-add", AddAsync),
        ("04-change", ChangeAsync),
        ("05-schedule", ScheduleAsync),
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

    private static async Task OpenAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        var page = scene.Page;

        await scene.StepAsync("open-1", async () =>
        {
            await scene.ClickAsync(page.Locator("#left-nav button").Filter(new() { HasTextString = "Calendars" }).First, pauseAfter: 700);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("#left-nav a[href='/Admin/calendar']").First);
            await Task.Delay(1500);
            await scene.HighlightAsync(page.Locator(".calendar-list").First, "Your calendars");
        });

        await scene.StepAsync("open-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var company = page.Locator("label[for^='calendar-']").Filter(new() { HasTextString = "Company" }).First;
            await scene.ClickAsync(company, pauseAfter: 2500);
            await scene.ClickAsync(company, pauseAfter: 1500);
        });

        await scene.StepAsync("open-3", async () =>
        {
            var views = page.Locator(".fc-header-toolbar button");
            await scene.ClickAsync(views.Filter(new() { HasTextString = "Month" }).First, pauseAfter: 2200);
            await scene.ClickAsync(views.Filter(new() { HasTextString = "Agenda" }).First, pauseAfter: 2200);
            await scene.ClickAsync(views.Filter(new() { HasTextString = "Week" }).First, pauseAfter: 1000);
        });

        await scene.StepAsync("open-4", async () =>
        {
            await scene.ClickAsync(page.Locator(".fc-next-button"), pauseAfter: 1800);
            await scene.ClickAsync(page.Locator(".fc-prev-button"), pauseAfter: 1200);
            await scene.ClickAsync(page.Locator(".fc-today-button"), pauseAfter: 1000);
        });
    }

    private static async Task AddAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await Cal.OpenCalendarAsync(scene, weeksAhead: 1);
        await scene.StartCaptureAsync();

        var page = scene.Page;
        var modal = Cal.Modal(scene);

        await scene.StepAsync("add-1", async () =>
        {
            await scene.ClickAsync(page.Locator("a[data-calendar-new-event]"), pauseAfter: 2200);
            await Cal.ChooseKindAsync(scene, "maria.lopez", "Meeting");
        });

        await scene.StepAsync("add-2", async () =>
        {
            await scene.TypeAsync(modal.Locator("#CalendarEvent_Title"), "Budget review with finance", delay: 45);
            await scene.HighlightAsync(modal.Locator("#CalendarEvent_EventType"));
            await Task.Delay(900);
            await scene.ClearHighlightsAsync();
            await Cal.SetAsync(scene, "CalendarEvent_StartDate", "2026-11-06");
            await Cal.SetAsync(scene, "CalendarEvent_StartTime", "14:00");
            await Cal.SetAsync(scene, "CalendarEvent_EndDate", "2026-11-06");
            await Cal.SetAsync(scene, "CalendarEvent_EndTime", "15:00");
        });

        await scene.StepAsync("add-3", async () =>
        {
            await scene.ScrollAsync(0, 560);
            await scene.HighlightAsync(modal.Locator("#CalendarEvent_Status"), "Status");
            await Task.Delay(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(modal.Locator("#CalendarEvent_Transparency"), "Show as");
            await Task.Delay(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(modal.Locator("#CalendarEvent_Visibility"), "Visibility");
        });

        await scene.StepAsync("add-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Cal.ModalButton(scene, "Save"), pauseAfter: 1500);
            await modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
            await Task.Delay(1000);
            await scene.MoveToAsync(1000, 600);
            await scene.ScrollAsync(0, 450);
            await scene.HighlightAsync(Cal.Event(scene, "Budget review"));
        });

        await scene.StepAsync("add-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await Cal.DragAcrossAsync(scene, "2026-11-04", "15:00:00", "16:00:00");
            await modal.WaitForAsync();
            await Task.Delay(900);
            await Cal.ChooseKindAsync(scene, "maria.lopez", "Event");
            await scene.TypeAsync(modal.Locator("#CalendarEvent_Title"), "Call with the bank", delay: 45);
            await scene.ClickAsync(Cal.ModalButton(scene, "Save"), pauseAfter: 1200);
            await modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
        });
    }

    private static async Task ChangeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await Cal.OpenCalendarAsync(scene, weeksAhead: 1);
        await scene.ScrollAsync(0, 450);
        await scene.StartCaptureAsync();

        var page = scene.Page;
        var modal = Cal.Modal(scene);

        await scene.StepAsync("change-1", async () =>
        {
            await scene.ClickAsync(Cal.Event(scene, "Budget review"), pauseAfter: 1800);
            await scene.ClickAsync(modal.Locator("[data-event-edit]"), pauseAfter: 1500);
        });

        await scene.StepAsync("change-2", async () =>
        {
            await Cal.SetAsync(scene, "CalendarEvent_StartTime", "15:00");
            await Cal.SetAsync(scene, "CalendarEvent_EndTime", "16:00");
            await scene.ClickAsync(Cal.ModalButton(scene, "Save"), pauseAfter: 1200);
            await modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
            await Task.Delay(800);
        });

        await scene.StepAsync("change-3", async () =>
        {
            var call = Cal.Event(scene, "Call with the bank");
            var box = await call.BoundingBoxAsync();
            var (thursdayX, _) = await Cal.SlotAsync(scene, "2026-11-05", "15:00:00");
            await scene.MoveToAsync(box.X + box.Width / 2, box.Y + 14);
            await page.Mouse.DownAsync();
            await scene.MoveToAsync(thursdayX, box.Y + 14, 35);
            await Task.Delay(200);
            await page.Mouse.UpAsync();
            await Task.Delay(1800);

            // Drag its bottom edge down to make it an hour longer.
            box = await Cal.Event(scene, "Call with the bank").BoundingBoxAsync();
            await scene.MoveToAsync(box.X + box.Width / 2, box.Y + box.Height - 3);
            await page.Mouse.DownAsync();
            await scene.MoveToAsync(box.X + box.Width / 2, box.Y + box.Height + 47, 25);
            await Task.Delay(200);
            await page.Mouse.UpAsync();
            await Task.Delay(1500);
        });

        await scene.StepAsync("change-4", async () =>
        {
            await scene.ClickAsync(Cal.Event(scene, "Call with the bank"), pauseAfter: 1200);
            await scene.ClickAsync(modal.Locator("[data-event-edit]"), pauseAfter: 1500);
            await scene.HighlightAsync(Cal.ModalButton(scene, "Cancel event"));
            await Task.Delay(1200);
            await scene.ClickAsync(Cal.ModalButton(scene, "Cancel event"), pauseAfter: 1200);
            await modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
            await scene.ClearHighlightsAsync();
            await Task.Delay(1000);
            await scene.ClickAsync(Cal.Event(scene, "Budget review"), pauseAfter: 1000);
            await scene.ClickAsync(modal.Locator("[data-event-edit]"), pauseAfter: 1500);
            await scene.MoveToAsync(800, 520);
            await scene.ScrollAsync(0, 3000);
            await scene.HighlightAsync(Cal.ModalButton(scene, "Delete"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ScheduleAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await Cal.OpenCalendarAsync(scene, weeksAhead: 1);
        await scene.StartCaptureAsync();

        var page = scene.Page;

        await scene.StepAsync("sched-1", async () =>
        {
            await scene.ClickAsync(page.Locator("label[for='schedule-people']"), pauseAfter: 1000);
            await scene.ClickAsync(page.Locator("button[data-id='schedule-people-picker']"), pauseAfter: 900);

            foreach (var person in new[] { "jamie", "sam" })
            {
                await scene.TypeAsync(page.Locator(".dropdown-menu.show .bs-searchbox input"), person, delay: 80);
                await Task.Delay(1400);
                await scene.ClickAsync(page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = person }).First, pauseAfter: 500);
                await page.Locator(".dropdown-menu.show .bs-searchbox input").FillAsync("");
            }

            await page.Keyboard.PressAsync("Escape");
            await Task.Delay(600);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Show" }).First);
            await Task.Delay(1500);
            await scene.ClickAsync(page.Locator(".fc-next-button"), pauseAfter: 1800);
            await scene.MoveToAsync(1000, 600);
            await scene.ScrollAsync(0, 450);
        });

        await scene.StepAsync("sched-2", async () =>
        {
            await scene.HighlightAsync(page.Locator(".fc-timegrid-event").Filter(new() { HasTextString = "Busy" }).First, "Busy: no title");
            await Task.Delay(3500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, -450);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("a").Filter(new() { HasTextString = "Back to my calendars" }).First);
        });
    }
}
