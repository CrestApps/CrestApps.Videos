using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The helpers every clip of this video shares: the site, the scene, and the calendar's pages and event window.
/// </summary>
public static partial class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

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

    // The calendar page.
    private static ILocator Modal(Scene scene) => scene.Page.Locator(".modal.show");

    private static async Task OpenCalendarAsync(Scene scene, string view = "Week", int next = 0)
    {
        await scene.OpenAsync("/Admin/calendar");
        await scene.Page.Locator(".fc-view-harness, .fc").First.WaitForAsync();
        await scene.Page.GetByRole(AriaRole.Button, new() { Name = view, Exact = true }).ClickAsync();

        for (var i = 0; i < next; i++)
        {
            await scene.Page.Locator(".fc-next-button").First.ClickAsync();
            await scene.Page.WaitForTimeoutAsync(600);
        }

        await scene.Page.WaitForTimeoutAsync(1500);
    }

    // Scrolls the event window (the page behind it stays put).
    private static async Task ModalScrollAsync(Scene scene, double top)
    {
        await Modal(scene).Locator(".modal-body").EvaluateAsync("(e, t) => e.scrollTo({ top: t, behavior: 'smooth' })", top);
        await scene.PauseAsync(700);
    }

    // In the "New Event" window: picks a kind of event (the nth one of that name, in the order of the calendars listed).
    private static async Task ChooseKindAsync(Scene scene, string kind, int nth = 0)
    {
        var modal = Modal(scene);
        await modal.WaitForAsync();
        await scene.ClickAsync(modal.GetByText(kind, new() { Exact = true }).Filter(new() { Visible = true }).Nth(nth), pauseAfter: 400);
        await scene.PauseAsync(1500);
    }

    private static async Task FillTimesAsync(Scene scene, string date, string start, string end, string endDate = null)
    {
        var modal = Modal(scene);
        await modal.Locator("#CalendarEvent_StartDate").FillAsync(date);
        await modal.Locator("#CalendarEvent_EndDate").FillAsync(endDate ?? date);
        await modal.Locator("#CalendarEvent_StartTime").FillAsync(start);
        await modal.Locator("#CalendarEvent_EndTime").FillAsync(end);
    }

    private static async Task InviteUserAsync(Scene scene, string userName)
    {
        var modal = Modal(scene);
        var invite = modal.GetByRole(AriaRole.Combobox, new() { Name = "Invite users" });
        await invite.ScrollIntoViewIfNeededAsync();
        await scene.ClickAsync(invite, pauseAfter: 500);
        await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show .text").Filter(new() { HasTextString = userName }).First, pauseAfter: 500);
        await scene.Page.Keyboard.PressAsync("Escape");
        await scene.PauseAsync(300);
    }
}
