using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-set", SetAsync),
        ("03-show", ShowAsync),
    ];

    private static ILocator Toast(Scene scene) => scene.Page.Locator("[data-calendar-reminders-container] .toast");

    private static async Task SetAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenCalendarAsync(scene, "Week");
        await scene.StartCaptureAsync();

        await scene.StepAsync("set-1", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByText("New event").First, pauseAfter: 1200);
            await ChooseKindAsync(scene, "Meeting", nth: 0);
        });

        await scene.StepAsync("set-2", async () =>
        {
            // Seven minutes from now, in the calendar's time zone (the site's, here New York).
            var zone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var start = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone).DateTime.AddMinutes(7);
            await scene.TypeAsync(Modal(scene).Locator("#CalendarEvent_Title"), "Review the launch plan");
            await FillTimesAsync(scene, start.ToString("yyyy-MM-dd"), start.ToString("HH:mm"), start.AddMinutes(30).ToString("HH:mm"));
            await Modal(scene).Locator("#CalendarEvent_ReminderMinutes").ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(Modal(scene).Locator("#CalendarEvent_ReminderMinutes"));
        });

        await scene.StepAsync("set-3", async () =>
        {
            await scene.SelectAsync(Modal(scene).Locator("#CalendarEvent_ReminderMinutes"), "5 minutes before");
        });

        await scene.StepAsync("set-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ModalScrollAsync(scene, 5000);
            await scene.ClickAsync(Modal(scene).GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }), pauseAfter: 2500);
        });
    }

    private static async Task ShowAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/calendars");
        await scene.StartCaptureAsync();

        await scene.StepAsync("show-1", async () =>
        {
            // The pages check once a minute: wait off camera until the reminder is shown.
            await scene.OffCameraAsync(async () =>
            {
                for (var i = 0; i < 36 && await Toast(scene).CountAsync() == 0; i++)
                {
                    await scene.Page.WaitForTimeoutAsync(5000);
                }

                await Toast(scene).First.WaitForAsync(new() { Timeout = 20000 });
            });
            await scene.HighlightAsync(Toast(scene).First);
        });

        await scene.StepAsync("show-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/calendar-categories");
            await Toast(scene).First.WaitForAsync(new() { Timeout = 30000 });
            await scene.PauseAsync(1200);
        });

        await scene.StepAsync("show-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Toast(scene).First.GetByText("Open calendar"));
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("show-4", async () =>
        {
            await Toast(scene).First.WaitForAsync(new() { Timeout = 30000 });
            await scene.ClickAsync(Toast(scene).First.GetByRole(AriaRole.Button, new() { Name = "Dismiss" }), pauseAfter: 2500);
        });
    }
}
