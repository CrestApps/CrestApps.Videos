using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator LinkNamed(Scene scene, string name) => scene.Page.GetByRole(AriaRole.Link, new() { Name = name, Exact = true }).First;

    private static ILocator Field(Scene scene, string id) => scene.Page.Locator("#" + id);

    private static ILocator Row(Scene scene, string text) => scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = text }).First;

    private static Task ClearAsync(Scene scene) => scene.ClearHighlightsAsync();

    private const string CanceledWorkflow = "Email the store when an order is canceled";

    // ----- Clips -----

    private static async Task RecipesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Recipes");
        await scene.StartCaptureAsync();

        await scene.StepAsync("rec-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Automation: thank-you email").First, "Automation: thank-you email");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("Automation: canceled order alert").First, "Automation: canceled order alert");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("Automation: low stock alert").First, "Automation: low stock alert");
        });

        await ClearAsync(scene);

        await scene.StepAsync("rec-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#btn-run-CommerceCanceledOrderAlert"), pauseAfter: 1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalOkButton"));
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("rec-3", async () =>
        {
            await scene.NavigateAsync("/Admin/Recipes");
            await scene.ClickAsync(scene.Page.Locator("#btn-run-CommerceLowStockAlert"), pauseAfter: 1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalOkButton"));
            await scene.PauseAsync(1500);
            await scene.NavigateAsync("/Admin/Workflows/Types");
            await scene.HighlightAsync(Row(scene, CanceledWorkflow), "Added turned off");
        });
    }

    private static async Task EditorAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Workflows/Types");
        await scene.StartCaptureAsync();

        await scene.StepAsync("edit-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Row(scene, CanceledWorkflow).GetByRole(AriaRole.Link, new() { Name = "Edit" }));
            await scene.HighlightAsync(scene.Page.GetByText("Order canceled", new() { Exact = true }).First, "The event that starts it");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("Tell the store").First, "The task that runs");
        });

        await ClearAsync(scene);

        await scene.StepAsync("edit-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("a, button").Filter(new() { HasTextString = "Add Event" }).First, pauseAfter: 1500);
            await scene.ClickAsync(scene.Page.Locator(".modal.show").GetByText("Commerce", new() { Exact = true }).First, pauseAfter: 1500);
            await scene.HighlightAsync(scene.Page.Locator(".modal.show").GetByText("Order Paid").First, "Order Paid");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator(".modal.show").GetByText("Low Stock").First, "Low Stock");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator(".modal.show").GetByText("Shipment Shipped").First, "Shipment Shipped");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.ClickAsync(scene.Page.Locator(".modal.show").GetByText("Stripe", new() { Exact = true }).First, pauseAfter: 1500);
            await scene.PauseAsync(3000);
        });

        await scene.StepAsync("edit-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".modal.show").GetByRole(AriaRole.Button, new() { Name = "Cancel" }).First, pauseAfter: 800);
            await scene.Page.GetByText("Tell the store").First.DblClickAsync();
            await scene.Page.WaitForURLAsync(url => url.Contains("/Activity/", StringComparison.OrdinalIgnoreCase));
            await Field(scene, "EmailTask_RecipientsExpression").FillAsync(string.Empty);
            await scene.TypeAsync(Field(scene, "EmailTask_RecipientsExpression"), "ops@contoso.example", delay: 50);
            await scene.HighlightAsync(Field(scene, "EmailTask_RecipientsExpression"), "Who it goes to");
            await scene.PauseAsync(2000);
            await ClearAsync(scene);
            await scene.HighlightAsync(Field(scene, "EmailTask_SubjectExpression"), "The subject, in Liquid");
        });

        await ClearAsync(scene);

        await scene.StepAsync("edit-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Save(scene.Page));
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).First);
        });
    }

    private static async Task FireAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Workflows/Types");
        await scene.StartCaptureAsync();

        await scene.StepAsync("fire-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Row(scene, CanceledWorkflow).GetByRole(AriaRole.Link, new() { Name = "Properties" }));
            await scene.ClickAsync(Field(scene, "IsEnabled"), pauseAfter: 800);
            await scene.HighlightAsync(Field(scene, "IsEnabled"), "Turn it on");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.ClickAsync(Field(scene, "DeleteFinishedWorkflows"), pauseAfter: 800);
            await scene.HighlightAsync(Field(scene, "DeleteFinishedWorkflows"), "Keep finished runs, to see them");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(Save(scene.Page));
        });

        await scene.StepAsync("fire-2", async () =>
        {
            await scene.NavigateAsync("/Admin/orders");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("li.list-group-item.item").Filter(new() { HasTextString = "Awaiting payment" }).Locator("a[href*='/Admin/orders/']").First);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Cancel order" }), "Cancel order");
            await scene.PauseAsync(1500);
            await ClearAsync(scene);
            await scene.TypeAsync(scene.Page.GetByLabel("Why"), "Out of stock", delay: 50);
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Cancel order" }), pauseAfter: 3000);
            await scene.HighlightAsync(scene.Page.GetByText("Canceled", new() { Exact = true }).First, "Canceled");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
        });

        await scene.StepAsync("fire-3", async () =>
        {
            await scene.NavigateAsync("/Admin/Workflows/Types");
            await scene.HighlightAsync(Row(scene, CanceledWorkflow).GetByRole(AriaRole.Link, new() { Name = "Instances" }), "Instances");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(Row(scene, CanceledWorkflow).GetByRole(AriaRole.Link, new() { Name = "Instances" }));
            await scene.HighlightAsync(scene.Page.GetByText("Finished", new() { Exact = true }).Last, "Finished");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("li.list-group-item a[href*='/Workflow/Details/']").First);
            await scene.PauseAsync(2000);
        });
    }
}
