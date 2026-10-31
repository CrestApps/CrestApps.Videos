using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static ILocator Button(Scene scene, string name)
        => scene.Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).First;

    private static ILocator Field(Scene scene, string key)
        => scene.Page.Locator($".report-designer-field[data-field-key=\"{key}\"]");

    private static ILocator Shelf(Scene scene, string kind)
        => scene.Page.Locator($".report-designer-shelf-zone[data-shelf={kind}]");

    // Collapses the admin menu, so the designers have the whole width.
    private static async Task CompactMenuAsync(Scene scene)
    {
        await scene.Page.EvaluateAsync("() => { if (!document.body.classList.contains('left-sidebar-compact')) document.querySelector('.leftbar-compactor')?.click(); }");
        await Task.Delay(400);
    }

    private static ILocator ModalGroup(Scene scene, string text)
        => scene.Page.Locator(".modal.show .nav-link").Filter(new() { HasTextString = text }).First;

    private static ILocator ModalCard(Scene scene, string title)
        => scene.Page.Locator(".modal.show .card").Filter(new() { Has = scene.Page.Locator(".card-title", new() { HasTextRegex = new System.Text.RegularExpressions.Regex($"^\\s*{title}\\s*$") }) }).First;

    private static async Task ReportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/reports/designs");
        await CompactMenuAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("rep-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a").Filter(new() { HasTextString = "New Report" }).First);
            await scene.PauseAsync(600);
            await scene.ClickAsync(Button(scene, "Add data set"), pauseAfter: 900);
            await scene.HighlightAsync(ModalGroup(scene, "Connected data (live)"));
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(ModalGroup(scene, "Connected data (imported)"));
        });

        await scene.StepAsync("rep-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(ModalGroup(scene, "Connected data (live)"), pauseAfter: 900);
            var card = ModalCard(scene, "Orders");
            await scene.MoveToAsync(card);
            await scene.PauseAsync(1500);
            await scene.ClickAsync(card.Locator(".card-footer button").First, pauseAfter: 1500);
            await scene.HighlightAsync(scene.Page.Locator(".report-designer-data").First);
        });

        await scene.StepAsync("rep-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.DragToAsync(Field(scene, "Orders.region"), Shelf(scene, "columns"));
            await scene.PauseAsync(1500);
            await scene.DragToAsync(Field(scene, "$count"), Shelf(scene, "columns"), 160);
            await scene.Page.Locator(".report-designer-preview").GetByText("Total", new() { Exact = true }).First.WaitForAsync();
            await scene.HighlightAsync(scene.Page.Locator(".report-designer-preview table").First);
        });

        await scene.StepAsync("rep-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Button(scene, "Add data set"), pauseAfter: 900);
            await scene.ClickAsync(ModalGroup(scene, "Connected data (imported)"), pauseAfter: 900);
            await scene.HighlightAsync(ModalCard(scene, "Imported orders"));
        });

        await scene.ClearHighlightsAsync();
        await scene.HoldAsync(1);
    }

    private static async Task PipelineAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/DataPipelines");
        await CompactMenuAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("pipe-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a, button").Filter(new() { HasTextString = "Add Pipeline" }).First);
            await scene.TypeAsync(scene.Page.Locator("input[type=text]:visible").First, "Orders feed", delay: 60);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Create" }).Last);
            await scene.PauseAsync(2000);
        });

        await scene.StepAsync("pipe-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".dpd-toolbox-card.is-source").First);
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator(".dpd-toolbox-card.is-source").First, pauseAfter: 1200);
        });

        await scene.StepAsync("pipe-3", async () =>
        {
            await scene.Page.Locator(".modal.show").WaitForAsync();
            await scene.PauseAsync(1000);
            await scene.ClickAsync(ModalGroup(scene, "Connected data (imported)"), pauseAfter: 900);
            var card = ModalCard(scene, "Imported orders");
            await scene.MoveToAsync(card);
            await scene.PauseAsync(1200);
            await scene.ClickAsync(card.Locator(".card-footer button").First, pauseAfter: 2500);
        });

        await scene.ClearHighlightsAsync();
        await scene.HoldAsync(1);
    }

    private static async Task RolesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/Roles/Edit/Analyst");
        await scene.StartCaptureAsync();

        ILocator PermissionRow(string id)
            => scene.Page.Locator("tr").Filter(new() { Has = scene.Page.Locator($"[id='Checkbox.{id}']") }).First;

        await scene.StepAsync("perm-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "connect", delay: 80);
            await scene.PauseAsync(800);
            await scene.HighlightAsync(PermissionRow("ReadAllConnectedDataSets"));
        });

        await scene.StepAsync("perm-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(PermissionRow("ReadConnectedDataSet_Orders"));
            await scene.PauseAsync(1500);
            await scene.ClickAsync(scene.Page.Locator("[id='Checkbox.ReadConnectedDataSet_Orders']"));
        });

        await scene.StepAsync("perm-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(PermissionRow("ReadConnectedDataSet_Imported_orders"));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(PermissionRow("ReadConnectedDataSet_Customers"));
        });

        await scene.StepAsync("perm-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(PermissionRow("ManageDataConnections"));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(PermissionRow("ManageConnectedDataSets"));
        });

        await scene.ClearHighlightsAsync();
        await scene.HoldAsync(1);
    }
}
