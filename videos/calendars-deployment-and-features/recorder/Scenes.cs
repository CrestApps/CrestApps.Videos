using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("01-features", FeaturesAsync),
        ("03-plan", PlanAsync),
    ];

    private static async Task FeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/calendars");
        await scene.StartCaptureAsync();

        await scene.StepAsync("feat-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("nav").GetByText("Tools", new() { Exact = true }).First, pauseAfter: 900);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("nav a[href='/Admin/Features']").First);
            await scene.TypeAsync(scene.Page.Locator("input[type=search], #search-box, input[name*=earch]").First, "Calendars");
            await scene.Page.Keyboard.PressAsync("Enter");
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("feat-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("text=Calendars").Locator("xpath=ancestor::*[contains(@class,'list-group-item') or contains(@class,'card')][1]").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 450);
        });

        await scene.StepAsync("feat-3", async () =>
        {
            await scene.ScrollAsync(0, -450);
            await scene.PauseAsync(1200);
            await scene.ScrollAsync(0, 350);
            await scene.PauseAsync(2500);
            await scene.ScrollAsync(0, 350);
        });

        await scene.StepAsync("feat-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("text=Calendars API").First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task PlanAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/calendars");
        await scene.StartCaptureAsync();

        await scene.StepAsync("plan-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("nav").GetByText("Tools", new() { Exact = true }).First, pauseAfter: 700);
            await scene.ClickAsync(scene.Page.Locator("nav").GetByText("Deployments", new() { Exact = true }).First, pauseAfter: 700);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("nav a[href='/Admin/DeploymentPlan/Index']").First);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href='/Admin/DeploymentPlan/Create']").Filter(new() { Visible = true }).First);
            await scene.TypeAsync(scene.Page.Locator("#Name"), "Copy calendars");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Create" }).First);
        });

        await scene.StepAsync("plan-2", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByText("Add Step").First, pauseAfter: 1500);
            await scene.Page.Locator("a[href*='CalendarsDeploymentStep']").First.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.Locator("a[href*='CalendarsDeploymentStep']").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='CalendarsDeploymentStep']").First);
        });

        await scene.StepAsync("plan-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#CalendarsDeploymentStep_IncludeCategories"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#CalendarsDeploymentStep_IncludeEvents"), pauseAfter: 900);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Create" }).First);
        });

        await scene.StepAsync("plan-4", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByText("Execute").First, pauseAfter: 1800);
            await scene.HighlightAsync(scene.Page.Locator(".modal.show").GetByText("File Download").First);
            await scene.PauseAsync(1500);
        });
    }
}
