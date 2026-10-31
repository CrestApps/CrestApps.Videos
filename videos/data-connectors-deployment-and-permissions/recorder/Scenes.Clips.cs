using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static ILocator Row(Scene scene, string name) => scene.Page.Locator("li.item").Filter(new() { HasText = name }).First;

    private static async Task OpenActionsAsync(Scene scene, string rowName)
        => await scene.ClickAsync(Row(scene, rowName).Locator("button.actions"));

    private static ILocator ModalGroup(Scene scene, string text)
        => scene.Page.Locator(".modal.show .nav-link").Filter(new() { HasTextRegex = new Regex($"^\\s*{Regex.Escape(text)}\\s*(\\d+)?\\s*$") }).First;

    private static ILocator ModalCard(Scene scene, string title)
        => scene.Page.Locator(".modal.show .card").Filter(new() { HasText = title }).First;

    private static async Task RolesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/Roles/Edit/Analyst");
        await scene.Page.Locator("[id='Checkbox.ManageDataConnections']").ScrollIntoViewIfNeededAsync();
        await scene.Page.EvaluateAsync("() => window.scrollBy(0, 120)");
        await scene.StartCaptureAsync();

        ILocator PermissionRow(string id)
            => scene.Page.Locator("tr").Filter(new() { Has = scene.Page.Locator($"[id='Checkbox.{id}']") }).First;

        ILocator Allow(string id) => scene.Page.Locator($"[id='Checkbox.{id}']");

        await scene.StepAsync("roles-1", async () =>
        {
            await scene.HighlightAsync(PermissionRow("ManageDataConnections"));
        });

        await scene.StepAsync("roles-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(PermissionRow("ManageConnectedDataSets"));
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Allow("ManageConnectedDataSets"));
            await scene.PauseAsync(600);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.btn-primary").Filter(new() { HasTextString = "Save" }).First);
            await scene.NavigateAsync("/Admin/Roles/Edit/Analyst");
            await scene.Page.Locator("[id='Checkbox.ManageDataConnections']").ScrollIntoViewIfNeededAsync();
            await scene.Page.EvaluateAsync("() => window.scrollBy(0, 120)");
            await scene.PauseAsync(600);
            await scene.HighlightAsync(PermissionRow("RefreshConnectedDataSets"));
        });

        await scene.StepAsync("roles-3", async () =>
        {
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Allow("ManageConnectedDataSets"));
            await scene.ClickAsync(Allow("RefreshConnectedDataSets"));
            await scene.HighlightAsync(PermissionRow("RefreshConnectedDataSets"));
        });

        await scene.StepAsync("roles-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Allow("ReadAllConnectedDataSets"));
            await scene.PauseAsync(800);
            await scene.HighlightAsync(PermissionRow("ReadConnectedDataSet_Orders"));
        });

        await scene.ClearHighlightsAsync();
        await scene.HoldAsync(1);
    }

    private static async Task PlanAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/DeploymentPlan/Index");
        await scene.StartCaptureAsync();

        await scene.StepAsync("plan-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a, button").Filter(new() { HasTextString = "Add Deployment Plan" }).First);
            await scene.TypeAsync(scene.Page.Locator("#Name"), "Copy connections to live", delay: 50);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Create" }).First);
        });

        await scene.StepAsync("plan-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button, a").Filter(new() { HasTextString = "Add Step" }).First);
            await scene.Page.Locator(".modal.show").WaitForAsync();
            await scene.PauseAsync(800);
            await scene.ClickAsync(ModalGroup(scene, "Data"), pauseAfter: 1200);
            var card = ModalCard(scene, "Data Connections");
            await scene.MoveToAsync(card);
            await scene.PauseAsync(900);
            await scene.ClickAndWaitForNavigationAsync(card.Locator("a").First);
            await scene.HighlightAsync(scene.Page.Locator("#DataConnectionDeploymentStep_IncludeAll"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Create" }).First);
        });

        await scene.StepAsync("plan-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button, a").Filter(new() { HasTextString = "Add Step" }).First);
            await scene.Page.Locator(".modal.show").WaitForAsync();
            await scene.PauseAsync(600);
            await scene.ClickAsync(ModalGroup(scene, "Data"), pauseAfter: 900);
            var card = ModalCard(scene, "Connected Data Sets");
            await scene.MoveToAsync(card);
            await scene.PauseAsync(600);
            await scene.ClickAndWaitForNavigationAsync(card.Locator("a").First);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Create" }).First);
            await scene.HighlightAsync(scene.Page.Locator("table, .list-group").First);
        });

        await scene.StepAsync("plan-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("button, a").Filter(new() { HasTextString = "Execute" }).First);
        });

        await scene.StepAsync("plan-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/DeploymentPlan/Import/Index");
            await scene.HighlightAsync(scene.Page.Locator("input[type=file]").First);
        });

        await scene.ClearHighlightsAsync();
        await scene.HoldAsync(1);
    }

    private static async Task CheckAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/data-connectors/data-sets");
        await scene.StartCaptureAsync();

        await scene.StepAsync("check-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Orders").GetByRole(AriaRole.Link, new() { Name = "Edit" }).First);
            await scene.Page.Locator("#ConnectedDataSet_IsEnabled").ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(scene.Page.Locator("#ConnectedDataSet_IsEnabled"));
        });

        await scene.StepAsync("check-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("button[name='submit.DiscoverFields']"));
        });

        await scene.StepAsync("check-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/data-connectors/data-sets");
            await OpenActionsAsync(scene, "Imported orders");
            await scene.PauseAsync(800);
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Imported orders").Locator("a[href*='/History/']"));
            await scene.PauseAsync(1500);
        });

        await scene.ClearHighlightsAsync();
        await scene.HoldAsync(1);
    }
}
