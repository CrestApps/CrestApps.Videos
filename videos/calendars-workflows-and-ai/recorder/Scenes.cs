using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-editor", EditorAsync),
        ("03-run", RunAsync),
        ("04-tools", ToolsAsync),
    ];

    private static ILocator Toolbox(Scene scene) => scene.Page.GetByRole(AriaRole.Complementary, new() { Name = "Toolbox" });

    private static ILocator Drawer(Scene scene) => scene.Page.Locator("text=Available data").First;

    private static async Task EditorAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Workflows/Types");
        await scene.StartCaptureAsync();

        await scene.StepAsync("wf-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Edit", Exact = true }).First);
            await scene.Page.GetByRole(AriaRole.Group, new() { Name = "Calendar Event Created, Calendar Event Created, start activity" }).WaitForAsync();
            await scene.PauseAsync(800);
            await scene.TypeAsync(scene.Page.GetByRole(AriaRole.Searchbox, new() { Name = "Search activities" }), "Calendar");
            await scene.PauseAsync(800);
        });

        await scene.StepAsync("wf-2", async () =>
        {
            await scene.ClickAsync(Toolbox(scene).GetByText("Events", new() { Exact = true }).First, pauseAfter: 2500);
            await scene.ScrollAsync(0, 150);
        });

        await scene.StepAsync("wf-3", async () =>
        {
            await scene.ClickAsync(Toolbox(scene).GetByText("Tasks", new() { Exact = true }).First, pauseAfter: 2500);
            await scene.ScrollAsync(0, 150);
        });

        await scene.StepAsync("wf-4", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Fit to content" }), pauseAfter: 800);
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Open the settings of Calendar Event Created" }), pauseAfter: 1500);
            await scene.MoveToAsync(820, 700);
            await scene.ScrollAsync(0, 250);
            await scene.PauseAsync(1200);
            await scene.ScrollAsync(0, 250);
        });

        await scene.StepAsync("wf-5", async () =>
        {
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Open the settings of Create Calendar Event" }), pauseAfter: 1500);
            await scene.MoveToAsync(820, 700);
            await scene.ScrollAsync(0, 250);
            await scene.PauseAsync(1200);
            await scene.ScrollAsync(0, 250);
        });
    }

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenCalendarAsync(scene, "Week", next: 3);
        await scene.StartCaptureAsync();

        await scene.StepAsync("run-1", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByText("New event").First, pauseAfter: 1200);
            await ChooseKindAsync(scene, "Out of office", nth: 1);
            await scene.TypeAsync(Modal(scene).Locator("#CalendarEvent_Title"), "Riley: vacation");
            await Modal(scene).Locator("#CalendarEvent_StartDate").FillAsync("2026-11-20");
            await Modal(scene).Locator("#CalendarEvent_EndDate").FillAsync("2026-11-20");
            await ModalScrollAsync(scene, 5000);
            await InviteUserAsync(scene, "riley");
            await scene.ClickAsync(Modal(scene).GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }), pauseAfter: 2500);
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.PauseAsync(3000);
            await scene.NavigateAsync("/Admin/Workflows/Instances");
            await scene.HighlightAsync(scene.Page.Locator("text=Plan cover for time off").Last);
        });

        await scene.StepAsync("run-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/calendar");
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Month", Exact = true }), pauseAfter: 600);
            await scene.ClickAsync(scene.Page.Locator(".fc-next-button").First, pauseAfter: 1500);
            await scene.HighlightAsync(scene.Page.Locator(".fc-event").Filter(new() { HasTextString = "Cover plan" }).First);
            await scene.PauseAsync(1500);
        });
    }

    private static async Task ToolsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/ai/profiles");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ai-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='/Admin/ai/profile/edit/']").First);
            await scene.ClickAsync(scene.Page.GetByText("Capabilities", new() { Exact = true }).First, pauseAfter: 1000);
            await scene.Page.GetByText("Select All in Calendar", new() { Exact = true }).First.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.Locator("text=Cancel calendar event").First.Locator("xpath=ancestor::div[1]/parent::*"));
        });

        await scene.StepAsync("ai-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.GetByText("Select All in Calendar", new() { Exact = true }).First, pauseAfter: 2500);
            await scene.Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ScrollIntoViewIfNeededAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }));
        });
    }
}
