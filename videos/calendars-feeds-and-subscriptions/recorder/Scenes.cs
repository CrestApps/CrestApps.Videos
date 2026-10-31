using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-address", AddressAsync),
        ("03-private", PrivateAsync),
        ("04-export", ExportAsync),
        ("05-import", ImportAsync),
    ];

    // The calendar files the import steps choose (see the README).
    private const string ImportFolder = "C:/cavideo/cal2/import";

    private static ILocator ActionsOf(Scene scene, string calendar)
        => scene.Page.Locator("li, tr, .list-group-item").Filter(new() { HasTextString = calendar }).First.GetByText("Actions");

    private static ILocator MenuItem(Scene scene, string text)
        => scene.Page.Locator(".dropdown-menu.show a").Filter(new() { HasTextString = text }).First;

    private static async Task AddressAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/calendars");
        await scene.StartCaptureAsync();

        await scene.StepAsync("addr-1", async () =>
        {
            await scene.ClickAsync(ActionsOf(scene, "Company"), pauseAfter: 900);
            await scene.HighlightAsync(MenuItem(scene, "Subscription address"));
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(MenuItem(scene, "Subscription address"));
        });

        await scene.StepAsync("addr-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Make an address" }));
            await scene.HighlightAsync(scene.Page.Locator("#feed-url"));
        });

        await scene.StepAsync("addr-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Copy" }), pauseAfter: 1500);
        });

        await scene.StepAsync("addr-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Make a new address" }));
            await scene.PauseAsync(2500);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Remove the address" }));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Make a new address" }));
            await scene.HighlightAsync(scene.Page.Locator("#feed-url"));
        });
    }

    private static async Task PrivateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/calendar/events/edit/ev-launch00000001");
        await scene.StartCaptureAsync();

        await scene.StepAsync("priv-1", async () =>
        {
            await scene.Page.Locator("#CalendarEvent_Visibility").ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.Locator("#CalendarEvent_Visibility"));
            await scene.SelectAsync(scene.Page.Locator("#CalendarEvent_Visibility"), "Private: others see only that the time is busy");
            await scene.PauseAsync(2000);
        });
    }

    private static async Task ExportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/calendars");
        await scene.StartCaptureAsync();

        await scene.StepAsync("exp-1", async () =>
        {
            await scene.ClickAsync(ActionsOf(scene, "Company"), pauseAfter: 900);
            await scene.HighlightAsync(MenuItem(scene, "Export .ics"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.Page.RunAndWaitForDownloadAsync(() => scene.ClickAsync(MenuItem(scene, "Export .ics"), pauseAfter: 800));
            await scene.PauseAsync(1500);
        });
    }

    private static async Task ShowImportedAsync(Scene scene)
    {
        await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Month", Exact = true }), pauseAfter: 600);
        await scene.ClickAsync(scene.Page.Locator(".fc-next-button").First, pauseAfter: 1200);
    }

    private static async Task ImportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/calendars/cal-company0000001/import");
        await scene.StartCaptureAsync();

        await scene.StepAsync("imp-1", async () =>
        {
            await scene.Page.Locator("input[type=file]").SetInputFilesAsync($"{ImportFolder}/v1/partner-events.ics");
            await scene.HighlightAsync(scene.Page.Locator("input[type=file]"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.PauseAsync(4500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Import", Exact = true }));
        });

        await scene.StepAsync("imp-2", async () =>
        {
            await scene.PauseAsync(3500);
            await ShowImportedAsync(scene);
        });

        await scene.StepAsync("imp-3", async () =>
        {
            await scene.NavigateAsync("/Admin/calendars/cal-company0000001/import");
            await scene.Page.Locator("input[type=file]").SetInputFilesAsync($"{ImportFolder}/v2/partner-events.ics");
            await scene.PauseAsync(3500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Import", Exact = true }));
        });

        await scene.StepAsync("imp-4", async () =>
        {
            await scene.PauseAsync(2500);
            await ShowImportedAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator(".fc-event").Filter(new() { HasTextString = "Vendor demo" }).First);
            await scene.PauseAsync(1500);
        });
    }
}
