using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Row(Scene scene, string text) => scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = text }).First;

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("button.save").First;

    private static ILocator LinkNamed(Scene scene, string name) => scene.Page.GetByRole(AriaRole.Link, new() { Name = name, Exact = true }).First;

    private static Task ClearAsync(Scene scene) => scene.ClearHighlightsAsync();

    // ----- Clips -----

    private static async Task RegistersAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/point-of-sale/registers");
        await scene.StartCaptureAsync();

        await scene.StepAsync("reg-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/point-of-sale/registers"), "Commerce > Point of sale > Registers");
            await scene.PauseAsync(2800);
            await ClearAsync(scene);
            await scene.HighlightAsync(Row(scene, "Downtown till 1"), "On, address, Shifts, Edit");
        });

        await ClearAsync(scene);

        await scene.StepAsync("reg-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Add register"));
            await scene.TypeAsync(scene.Page.Locator("#Name"), "Uptown till 1", delay: 40);
            await scene.SelectAsync(scene.Page.Locator("#LocationId"), "Uptown store", pauseAfter: 600);
        });

        await scene.StepAsync("reg-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#AddressLine1"), "45 Hill Road", delay: 20);
            await scene.TypeAsync(scene.Page.Locator("#City"), "Springfield", delay: 20);
            await scene.TypeAsync(scene.Page.Locator("#Region"), "OR", delay: 40);
            await scene.TypeAsync(scene.Page.Locator("#PostalCode"), "97477", delay: 30);
            await scene.TypeAsync(scene.Page.Locator("#Country"), "US", delay: 40);
        });

        await scene.StepAsync("reg-4", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#Currency"), "USD", delay: 90);
            await scene.HighlightAsync(scene.Page.Locator("#Currency"), "Currency");
        });

        await ClearAsync(scene);

        await scene.StepAsync("reg-5", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#role-Administrator"), "Who may sell here");
            await scene.PauseAsync(4000);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator("#IsEnabled"), "On");
        });

        await ClearAsync(scene);

        await scene.StepAsync("reg-6", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }

    private static async Task ShiftsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/point-of-sale/shifts");
        await scene.StartCaptureAsync();

        await scene.StepAsync("shift-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/point-of-sale/shifts"), "Commerce > Point of sale > Shifts");
            await scene.PauseAsync(2800);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = "Closed" }).First, "Open or Closed, and the difference");
        });

        await ClearAsync(scene);

        await scene.StepAsync("shift-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".bootstrap-select button.dropdown-toggle").First, pauseAfter: 3500);
            await scene.Page.Keyboard.PressAsync("Escape");
        });

        await scene.StepAsync("shift-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = "Closed" }).First.GetByRole(AriaRole.Link, new() { Name = "View" }));
            await scene.HighlightAsync(scene.Page.GetByText("Sales", new() { Exact = true }).First, "Sales");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("Refunded", new() { Exact = true }).First, "Refunded");
        });

        await ClearAsync(scene);

        await scene.StepAsync("shift-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Opening float", new() { Exact = true }).First, "Opening float");
            await scene.PauseAsync(2800);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("Expected", new() { Exact = true }).First, "Expected");
            await scene.PauseAsync(2800);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("Banked at noon").First, "A cash movement, with its reason");
        });

        await ClearAsync(scene);

        await scene.StepAsync("shift-5", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Counted", new() { Exact = true }).First, "Counted");
            await scene.PauseAsync(2200);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("Difference", new() { Exact = true }).First, "Difference");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("a coin dropped behind the till").First, "The note");
        });
    }

    private static async Task OrdersAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/orders");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ord-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/orders"), "Commerce > Orders");
            await scene.PauseAsync(2400);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item.item").First, "A register sale, with its register");
        });

        await ClearAsync(scene);

        await scene.StepAsync("ord-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".bootstrap-select button.dropdown-toggle").First, pauseAfter: 2600);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".bootstrap-select .dropdown-menu.show").GetByText("Point of sale"));
        });

        await scene.StepAsync("ord-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("li.list-group-item.item").First.GetByRole(AriaRole.Link, new() { Name = "View" }));
            await scene.HighlightAsync(scene.Page.GetByText("Shipping (In store)").First, "In store: nothing is shipped");
        });

        await ClearAsync(scene);

        await scene.StepAsync("ord-4", async () =>
        {
            await scene.Page.GetByText("Ships to").First.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.GetByText("Ships to").First, "The register's store");
        });
    }
}
