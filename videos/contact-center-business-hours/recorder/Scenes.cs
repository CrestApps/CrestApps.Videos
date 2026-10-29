using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Business Hours video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string ListPath = "/Admin/contact-center/business-hours";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-create", CreateAsync),
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

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static ILocator Day(Scene scene, int index) => scene.Page.Locator($"#BusinessHoursCalendar_Days_{index}__IsOpen").Locator("xpath=ancestor::tr[1]");

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync(ListPath);
        await scene.StartCaptureAsync();

        await scene.StepAsync("cal-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, ListPath), "Interaction Center > Management > Business hours");
            await scene.PauseAsync(2800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create"));
        });

        await scene.StepAsync("cal-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#BusinessHoursCalendar_Name"), "Support hours", delay: 55);
            await scene.TypeAsync(scene.Page.Locator("#BusinessHoursCalendar_Description"), "Weekdays, and Saturday mornings", delay: 35);
        });

        await scene.StepAsync("cal-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button[data-id='BusinessHoursCalendar_TimeZoneId']"), pauseAfter: 700);
            await scene.TypeAsync(scene.Page.Locator(".dropdown-menu.show input[type=search]").First, "New_York", delay: 80);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show .dropdown-item").First, pauseAfter: 700);
            await scene.HighlightAsync(Section(scene.Page.Locator("#BusinessHoursCalendar_TimeZoneId")), "Time zone");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("cal-4", async () =>
        {
            await scene.Page.Locator("#BusinessHoursCalendar_Days_1__IsOpen").ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 200);
            await scene.HighlightAsync(scene.Page.Locator("#BusinessHoursCalendar_Days_1__IsOpen").Locator("xpath=ancestor::table[1]"), "Weekly schedule");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("cal-5", async () =>
        {
            await scene.Page.Locator("#BusinessHoursCalendar_Days_5__CloseTime").FillAsync("13:00");
            await scene.HighlightAsync(Day(scene, 5), "Friday closes at 1:00 PM");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#BusinessHoursCalendar_Days_6__IsOpen"), pauseAfter: 600);
            await scene.Page.Locator("#BusinessHoursCalendar_Days_6__CloseTime").FillAsync("12:00");
            await scene.HighlightAsync(Day(scene, 6), "Saturday, 9:00 AM to 12:00 PM");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("cal-6", async () =>
        {
            await scene.Page.Locator("#BusinessHoursCalendar_HolidaysText").ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 300);
            await scene.TypeAsync(scene.Page.Locator("#BusinessHoursCalendar_HolidaysText"), "2026-12-25\n2027-01-01", delay: 60);
            await scene.HighlightAsync(Section(scene.Page.Locator("#BusinessHoursCalendar_HolidaysText")), "Holidays");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("cal-7", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#BusinessHoursCalendar_Enabled")), "Enabled");
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1500);
        });
    }
}
