using Microsoft.Playwright;

namespace Recorder;

/// <summary>Helpers shared by the clips of the Calendars videos.</summary>
internal static class Cal
{
    public static ILocator Modal(Scene scene) => scene.Page.Locator(".modal.show");

    /// <summary>Opens the calendar view before the capture, on the week the given number of weeks from this one.</summary>
    public static async Task OpenCalendarAsync(Scene scene, int weeksAhead = 0, string path = "/Admin/calendar")
    {
        await scene.OpenAsync(path);

        for (var i = 0; i < weeksAhead; i++)
        {
            await scene.Page.Locator(".fc-next-button").ClickAsync();
            await Task.Delay(1200);
        }

        await scene.Page.Mouse.MoveAsync(900, 500);
    }

    /// <summary>The middle of the cell of a day and a time in the week view (viewport coordinates).</summary>
    public static async Task<(double X, double Y)> SlotAsync(Scene scene, string date, string time)
    {
        var column = await scene.Page.Locator($".fc-col-header-cell[data-date='{date}']").BoundingBoxAsync();
        var row = await scene.Page.Locator($".fc-timegrid-slot-lane[data-time='{time}']").BoundingBoxAsync();

        return (column.X + column.Width / 2, row.Y + row.Height / 2);
    }

    /// <summary>Drags across the cells of the week view, from one time to another on the same day.</summary>
    public static async Task DragAcrossAsync(Scene scene, string date, string from, string to)
    {
        var (x, y1) = await SlotAsync(scene, date, from);
        var (_, y2) = await SlotAsync(scene, date, to);
        await scene.MoveToAsync(x, y1 + 6);
        await scene.Page.Mouse.DownAsync();
        await scene.MoveToAsync(x, y2 + 6, 30);
        await Task.Delay(250);
        await scene.Page.Mouse.UpAsync();
        await Task.Delay(900);
    }

    /// <summary>The event of the week view with the given title.</summary>
    public static ILocator Event(Scene scene, string title) => scene.Page.Locator(".fc-event").Filter(new() { HasTextString = title }).First;

    /// <summary>A button of the open window.</summary>
    public static ILocator ModalButton(Scene scene, string text) => Modal(scene).Locator("button, a.btn").Filter(new() { HasTextString = text }).First;

    /// <summary>Fills a date or time field of the editor, clicking it first so the cursor shows where.</summary>
    public static async Task SetAsync(Scene scene, string fieldId, string value)
    {
        var field = scene.Page.Locator("#" + fieldId);
        await scene.ClickAsync(field, pauseAfter: 150);
        await field.FillAsync(value);
        await Task.Delay(250);
    }

    /// <summary>Chooses a calendar's kind of event in the "New event" window.</summary>
    public static async Task ChooseKindAsync(Scene scene, string calendar, string kind)
    {
        var card = Modal(scene).Locator(".card").Filter(new() { HasTextString = calendar }).First;
        await scene.ClickAsync(card.Locator("a").Filter(new() { HasTextString = kind }).First, pauseAfter: 700);
        await Modal(scene).Locator("#CalendarEvent_Title").WaitForAsync();
        await Task.Delay(500);
    }
}
