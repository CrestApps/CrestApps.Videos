using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the video, recorded as maria.lopez (a member of the Sales Team who is also a manager), and as jamie.chen
/// for the clip that answers an invitation, on the demo site that the Calendars getting started video seeds. The clips run
/// in order: each edits the events of the one before.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5396";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    // The sign-in of maria.lopez (Recorder setup with VIDEO_USER_NAME=maria.lopez), and of jamie.chen.
    public static string StatePath => Path.Combine(Root, "state.json");

    private static string JamieStatePath => Path.Combine(Root, "state-jamie.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("01-repeat", RepeatAsync),
        ("02-series", SeriesAsync),
        ("03-invite", InviteAsync),
        ("04-respond", RespondAsync),
        ("05-status", StatusAsync),
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

    private static async Task NewEventAsync(Scene scene, string calendar, string kind, string title, string date, string start, string end)
    {
        await scene.ClickAsync(scene.Page.Locator("a[data-calendar-new-event]"), pauseAfter: 1500);
        await Cal.ChooseKindAsync(scene, calendar, kind);
        await scene.TypeAsync(Cal.Modal(scene).Locator("#CalendarEvent_Title"), title, delay: 45);
        await Cal.SetAsync(scene, "CalendarEvent_StartDate", date);
        await Cal.SetAsync(scene, "CalendarEvent_StartTime", start);
        await Cal.SetAsync(scene, "CalendarEvent_EndDate", date);
        await Cal.SetAsync(scene, "CalendarEvent_EndTime", end);
    }

    private static async Task SaveAsync(Scene scene)
    {
        await scene.ClickAsync(Cal.ModalButton(scene, "Save"), pauseAfter: 1200);
        await Cal.Modal(scene).WaitForAsync(new() { State = WaitForSelectorState.Hidden });
        await Task.Delay(900);
    }

    private static async Task RepeatAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await Cal.OpenCalendarAsync(scene, weeksAhead: 1);
        await scene.StartCaptureAsync();

        var page = scene.Page;
        var modal = Cal.Modal(scene);

        await scene.StepAsync("rep-1", async () =>
        {
            await NewEventAsync(scene, "maria.lopez", "Meeting", "Coaching session", "2026-11-10", "10:00", "10:30");
            await scene.MoveToAsync(800, 520);
            await scene.ScrollAsync(0, 700);
            await scene.HighlightAsync(modal.Locator("#CalendarEvent_Frequency"));
            await scene.SelectAsync(modal.Locator("#CalendarEvent_Frequency"), "WEEKLY", pauseAfter: 900);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("rep-2", async () =>
        {
            await scene.ClickAsync(modal.Locator("label[for='CalendarEvent_WeekDays-TU']"), pauseAfter: 600);
            await scene.ClickAsync(modal.Locator("label[for='CalendarEvent_WeekDays-TH']"), pauseAfter: 1000);
            await scene.SelectAsync(modal.Locator("#CalendarEvent_Frequency"), "MONTHLY", pauseAfter: 2800);
            await scene.SelectAsync(modal.Locator("#CalendarEvent_Frequency"), "WEEKLY", pauseAfter: 600);
        });

        await scene.StepAsync("rep-3", async () =>
        {
            await scene.ClickAsync(modal.Locator("label[for='CalendarEvent_Ends-after']"), pauseAfter: 500);
            await modal.Locator("#CalendarEvent_Count").FillAsync("8");
            await scene.HighlightAsync(modal.Locator("#CalendarEvent_CustomRule"), "Custom rule");
            await Task.Delay(2000);
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene);
        });

        await scene.StepAsync("rep-4", async () =>
        {
            var views = page.Locator(".fc-header-toolbar button");
            await scene.ClickAsync(views.Filter(new() { HasTextString = "Month" }).First, pauseAfter: 1500);
            await Task.Delay(1500);
        });
    }

    private static async Task SeriesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await Cal.OpenCalendarAsync(scene, weeksAhead: 3);
        await scene.ScrollAsync(0, 400);
        await scene.StartCaptureAsync();

        var page = scene.Page;
        var modal = Cal.Modal(scene);
        var scope = modal.Locator(".card").Filter(new() { HasTextString = "Apply the changes" }).First;

        await scene.StepAsync("ser-1", async () =>
        {
            await scene.ClickAsync(Cal.Event(scene, "Coaching session"), pauseAfter: 1500);
            await scene.ClickAsync(modal.Locator("[data-event-edit]"), pauseAfter: 1500);
            await scene.HighlightAsync(scope);
        });

        await scene.StepAsync("ser-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(modal.Locator("label[for='scope-this']"), pauseAfter: 600);
            await Cal.SetAsync(scene, "CalendarEvent_StartTime", "11:00");
            await Cal.SetAsync(scene, "CalendarEvent_EndTime", "11:30");
            await SaveAsync(scene);
        });

        await scene.StepAsync("ser-3", async () =>
        {
            await scene.ClickAsync(page.Locator(".fc-event").Filter(new() { HasTextRegex = new Regex("10:00.*Coaching", RegexOptions.Singleline) }).First, pauseAfter: 1300);
            await scene.ClickAsync(modal.Locator("[data-event-edit]"), pauseAfter: 1500);
            await scene.ClickAsync(modal.Locator("label[for='scope-following']"), pauseAfter: 600);
            await Cal.SetAsync(scene, "CalendarEvent_EndTime", "11:00");
            await SaveAsync(scene);
        });

        await scene.StepAsync("ser-4", async () =>
        {
            await scene.ClickAsync(page.Locator(".fc-next-button"), pauseAfter: 1800);
            await scene.ClickAsync(Cal.Event(scene, "Coaching session"), pauseAfter: 1300);
            await scene.ClickAsync(modal.Locator("[data-event-edit]"), pauseAfter: 1500);
            await scene.HighlightAsync(scope);
            await Task.Delay(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(modal.Locator("button.btn-warning"), pauseAfter: 1200);
            await modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
            await Task.Delay(800);
        });
    }

    private static async Task InviteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await Cal.OpenCalendarAsync(scene, weeksAhead: 2);
        await scene.StartCaptureAsync();

        var page = scene.Page;
        var modal = Cal.Modal(scene);

        await scene.StepAsync("att-1", async () =>
        {
            await NewEventAsync(scene, "maria.lopez", "Meeting", "Customer workshop", "2026-11-11", "11:00", "12:00");
            await scene.MoveToAsync(800, 520);
            await scene.ScrollAsync(0, 1600);
            await scene.ClickAsync(modal.Locator("button[data-id='CalendarEvent_UserIds']"), pauseAfter: 900);

            foreach (var person in new[] { "jamie", "sam" })
            {
                await scene.TypeAsync(page.Locator(".dropdown-menu.show .bs-searchbox input"), person, delay: 80);
                await Task.Delay(1300);
                await scene.ClickAsync(page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = person }).First, pauseAfter: 500);
                await page.Locator(".dropdown-menu.show .bs-searchbox input").FillAsync("");
            }

            await page.Keyboard.PressAsync("Escape");
        });

        await scene.StepAsync("att-2", async () =>
        {
            await scene.TypeAsync(modal.Locator("#CalendarEvent_Guests"), "Jane Doe <jane.doe@example.com>", delay: 50);
            await scene.HighlightAsync(modal.Locator("#CalendarEvent_Guests"));
            await Task.Delay(1500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("att-3", async () =>
        {
            await SaveAsync(scene);
            await scene.ClickAsync(Cal.Event(scene, "Customer workshop"), pauseAfter: 1400);
            await scene.ClickAsync(modal.Locator("[data-event-edit]"), pauseAfter: 1500);
            await scene.MoveToAsync(800, 520);
            await scene.ScrollAsync(0, 3000);
            await scene.HighlightAsync(modal.Locator(".ocat-wrapper").Filter(new() { HasTextString = "Answers" }).First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task RespondAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, JamieStatePath);
        await Cal.OpenCalendarAsync(scene, weeksAhead: 1);
        await scene.ScrollAsync(0, 450);
        await scene.StartCaptureAsync();

        var modal = Cal.Modal(scene);

        await scene.StepAsync("resp-1", async () =>
        {
            await scene.ClickAsync(Cal.Event(scene, "Pipeline review"), pauseAfter: 1400);
            await scene.ClickAsync(modal.Locator("[data-event-edit]"), pauseAfter: 1500);
            await scene.HighlightAsync(modal.Locator(".alert-info"));
        });

        await scene.StepAsync("resp-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(modal.Locator(".alert-info button").Filter(new() { HasTextString = "Accept" }), pauseAfter: 1800);
            await modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
            await Task.Delay(700);
            await scene.ClickAsync(Cal.Event(scene, "Pipeline review"), pauseAfter: 1200);
            await scene.ClickAsync(modal.Locator("[data-event-edit]"), pauseAfter: 1500);
            await scene.MoveToAsync(800, 520);
            await scene.ScrollAsync(0, 3000);
            await scene.HighlightAsync(modal.Locator(".ocat-wrapper").Filter(new() { HasTextString = "Answers" }).First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task StatusAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await Cal.OpenCalendarAsync(scene, weeksAhead: 2);
        await scene.StartCaptureAsync();

        var modal = Cal.Modal(scene);

        await scene.StepAsync("stat-1", async () =>
        {
            await NewEventAsync(scene, "maria.lopez", "Event", "Optional lunch", "2026-11-12", "12:00", "13:00");
            await scene.MoveToAsync(800, 520);
            await scene.ScrollAsync(0, 560);
            await scene.HighlightAsync(modal.Locator("#CalendarEvent_Transparency"), "Show as");
            await scene.SelectAsync(modal.Locator("#CalendarEvent_Transparency"), "Free", pauseAfter: 1200);
        });

        await scene.StepAsync("stat-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(modal.Locator("#CalendarEvent_Visibility"), "Visibility");
            await scene.SelectAsync(modal.Locator("#CalendarEvent_Visibility"), "Private", pauseAfter: 1500);
        });

        await scene.StepAsync("stat-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(modal.Locator("#CalendarEvent_Status"), "Status");
            await scene.SelectAsync(modal.Locator("#CalendarEvent_Status"), "Tentative", pauseAfter: 1500);
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene);
            await scene.ScrollAsync(0, 400);
            await scene.HighlightAsync(Cal.Event(scene, "Optional lunch"));
        });

        await scene.ClearHighlightsAsync();
    }
}
