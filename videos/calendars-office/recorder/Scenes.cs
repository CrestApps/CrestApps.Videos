using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-setup", SetupClipAsync),
        ("03-book", BookAsync),
        ("04-closed", ClosedAsync),
    ];

    private static async Task SetupClipAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/calendars");
        await scene.StartCaptureAsync();

        await scene.StepAsync("setup-1", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Add calendar" }), pauseAfter: 1500);
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show a").Filter(new() { HasTextString = "Office" }).First);
        });

        await scene.StepAsync("setup-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.Page.Keyboard.PressAsync("Escape");
            var row = scene.Page.Locator(".list-group-item, tr, li").Filter(new() { HasTextString = "Lisbon office" }).First;
            await scene.ClickAndWaitForNavigationAsync(row.GetByRole(AriaRole.Link, new() { Name = "Edit", Exact = true }));
            await scene.Page.EvaluateAsync("window.scrollTo({ top: 520, behavior: 'smooth' })");
            await scene.PauseAsync(900);
            await scene.TypeAsync(scene.Page.Locator("#Calendar_Address"), "Rua da Prata 100, 1100-420 Lisbon");
            await scene.TypeAsync(scene.Page.Locator("#Calendar_Contact"), "Ana Costa, office manager");
        });

        await scene.StepAsync("setup-3", async () =>
        {
            await scene.Page.EvaluateAsync("window.scrollTo({ top: 1700, behavior: 'smooth' })");
            await scene.PauseAsync(1200);
            await scene.HighlightAsync(scene.Page.Locator("#Calendar_Capacity"));
        });

        await scene.StepAsync("setup-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.Page.EvaluateAsync("window.scrollTo({ top: 520, behavior: 'smooth' })");
            await scene.PauseAsync(900);
            await scene.HighlightAsync(scene.Page.Locator("#Calendar_Visibility"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.Page.EvaluateAsync("window.scrollTo({ top: 5000, behavior: 'smooth' })");
            await scene.PauseAsync(900);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }));
        });
    }

    private static async Task BookAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenCalendarAsync(scene, "Week", next: 1);
        await scene.StartCaptureAsync();

        await scene.StepAsync("book-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".fc-event").Filter(new() { HasTextString = "Jamie Chen" }).First);
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("book-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.GetByText("New event").First, pauseAfter: 1200);
            await ChooseKindAsync(scene, "Desk booking");
        });

        await scene.StepAsync("book-3", async () =>
        {
            await scene.TypeAsync(Modal(scene).Locator("#CalendarEvent_Title"), "Riley at a desk");
            await FillTimesAsync(scene, "2026-11-02", "10:00", "14:00");
            await ModalScrollAsync(scene, 5000);
            await InviteUserAsync(scene, "riley");
        });

        await scene.StepAsync("book-4", async () =>
        {
            await scene.ClickAsync(Modal(scene).GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }), pauseAfter: 2500);
        });

        await scene.StepAsync("book-5", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByText("New event").First, pauseAfter: 1200);
            await ChooseKindAsync(scene, "Visitor");
            await scene.TypeAsync(Modal(scene).Locator("#CalendarEvent_Title"), "Visitor: Dana from Northwind");
            await FillTimesAsync(scene, "2026-11-02", "11:00", "12:00");
        });

        await scene.StepAsync("book-6", async () =>
        {
            await ModalScrollAsync(scene, 5000);
            await scene.ClickAsync(Modal(scene).GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }), pauseAfter: 1500);
            await ModalScrollAsync(scene, 0);
            await scene.HighlightAsync(Modal(scene).Locator(".alert, .text-danger").Filter(new() { HasTextString = "already full" }).First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ClosedAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenCalendarAsync(scene, "Month", next: 2);
        await scene.StartCaptureAsync();

        await scene.StepAsync("closed-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".fc-event").Filter(new() { HasTextString = "Office closed" }).First);
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("closed-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator(".fc-event").Filter(new() { HasTextString = "Office closed" }).First, pauseAfter: 1500);
        });
    }
}
