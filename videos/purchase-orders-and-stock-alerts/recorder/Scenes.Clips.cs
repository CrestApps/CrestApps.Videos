using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static ILocator Fld(Scene scene, string name) => scene.Page.Locator($"[name='{name}']").First;

    private static async Task ReplaceAsync(Scene scene, ILocator field, string text, int delay = 60)
    {
        await scene.ClickAsync(field, pauseAfter: 200);
        await scene.Page.Keyboard.PressAsync("Control+A");
        await field.PressSequentiallyAsync(text, new() { Delay = delay });
        await scene.PauseAsync(300);
    }

    private static async Task SaveAsync(Scene scene, ILocator button)
    {
        await scene.ClickAsync(button, pauseAfter: 500);
        await scene.OffCameraAsync(() => scene.Page.WaitForLoadStateAsync(LoadState.NetworkIdle));
        await scene.OffCameraAsync(() => scene.Page.WaitForTimeoutAsync(2500));
    }

    private static ILocator Btn(Scene scene, string text) => scene.Page.Locator("button, a.btn").Filter(new() { HasTextString = text }).First;

    // Opens the purchase order of a supplier from the list.
    private static async Task OpenOrderAsync(Scene scene, string supplier)
    {
        await scene.NavigateAsync("/Admin/purchase-orders");
        await scene.ClickAndWaitForNavigationAsync(Row(scene, supplier).Locator("a").First);
    }

    private static async Task StartAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/purchase-orders");
        await scene.StartCaptureAsync();

        await scene.StepAsync("sta-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/purchase-orders"), "Commerce > Purchase orders", 2200);
            await scene.ClickAndWaitForNavigationAsync(Link(scene, "Add purchase order"));
        });

        await scene.StepAsync("sta-2", async () =>
        {
            await scene.TypeAsync(Fld(scene, "Supplier"), "Northwind Supply", delay: 45);
            await scene.TypeAsync(Fld(scene, "SupplierEmail"), "orders@example.com", delay: 30);
            await scene.HighlightAsync(Field(scene, "Receive at"), "Receive at");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "Costs in"), "Costs in");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await Fld(scene, "ExpectedDate").FillAsync("2026-11-12");
            await scene.TypeAsync(Fld(scene, "Note"), "Weekly reorder", delay: 50);
        });

        await scene.StepAsync("sta-3", async () =>
        {
            await scene.HighlightAsync(Field(scene, "Start with the items that are low on stock"), "Start with low stock");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Btn(scene, "Start the order"));
            await scene.PauseAsync(2500);
        });
    }

    private static async Task OrderAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/purchase-orders");
        await scene.Page.RunAndWaitForNavigationAsync(() => Row(scene, "Northwind Supply").Locator("a").First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("ord-1", async () =>
        {
            await ScrollAsync(scene, 0, 500);
            await scene.HighlightAsync(scene.Page.Locator("table").First, "Ordered and Unit cost");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            var costs = scene.Page.Locator("input[name*='UnitCost' i]");
            var count = await costs.CountAsync();

            for (var i = 0; i < count; i++)
            {
                await ReplaceAsync(scene, costs.Nth(i), (4 + i * 1.5).ToString("0.00"), 100);
            }

            await SaveAsync(scene, Btn(scene, "Save"));
        });

        await scene.StepAsync("ord-2", async () =>
        {
            await ScrollAsync(scene, 0, 600);
            await scene.HighlightAsync(Btn(scene, "Mark as ordered"), "Mark as ordered");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene, Btn(scene, "Mark as ordered"));
            await scene.PauseAsync(1500);
        });

        // A second order, for the cancel clip, made off camera.
        await scene.OffCameraAsync(async () =>
        {
            await scene.NavigateAsync("/Admin/purchase-orders/create");
            await Fld(scene, "Supplier").FillAsync("Harbor Goods");
            await Fld(scene, "SupplierEmail").FillAsync("sales@example.com");
            await scene.Page.RunAndWaitForNavigationAsync(() => Btn(scene, "Start the order").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
            await scene.Page.RunAndWaitForNavigationAsync(() => Btn(scene, "Mark as ordered").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        });
    }

    private static async Task RequestAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/shop");
        await scene.Page.RunAndWaitForNavigationAsync(() => scene.Page.Locator(".card").Filter(new() { HasTextString = "Wool Beanie" }).First.Locator("a").First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("req-1", async () =>
        {
            var box = scene.Page.Locator("div, form").Filter(new() { HasTextString = "Sold out" }).Filter(new() { HasTextString = "Notify me" }).Last;
            await scene.HighlightAsync(box, "Sold out: get an email when it is back");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(box.Locator("input[type=email], input[name*='mail' i]").First, "shopper@example.com", delay: 45);
            await scene.ClickAsync(box.Locator("button").Filter(new() { HasTextString = "Notify me" }).First, pauseAfter: 3000);
        });
    }

    private static async Task WaitingAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/inventory");
        await scene.Page.RunAndWaitForNavigationAsync(() => Row(scene, "Wool Beanie").Locator("a:has-text('Change')").First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("req-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("waiting").First, "Shoppers waiting");
            await scene.PauseAsync(4000);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ReceiveAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/purchase-orders");
        await scene.Page.RunAndWaitForNavigationAsync(() => Row(scene, "Northwind Supply").Locator("a").First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("rcv-1", async () =>
        {
            await ScrollAsync(scene, 0, 500);
            var arrived = scene.Page.Locator("input[name$='.Receive']").First;
            await arrived.ScrollIntoViewIfNeededAsync();
            await ReplaceAsync(scene, arrived, "2", 120);
            await scene.HighlightAsync(Btn(scene, "Receive what arrived"), "Receive what arrived");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene, Btn(scene, "Receive what arrived"));
            await scene.HighlightAsync(scene.Page.GetByText("Partly received").First, "Partly received");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("rcv-2", async () =>
        {
            await scene.HighlightAsync(Btn(scene, "Receive everything"), "Receive everything");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene, Btn(scene, "Receive everything"));
            await scene.HighlightAsync(scene.Page.GetByText("Received").First, "Received");
            await scene.PauseAsync(3000);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task CancelAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/purchase-orders");
        await scene.Page.RunAndWaitForNavigationAsync(() => Row(scene, "Harbor Goods").Locator("a").First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("can-1", async () =>
        {
            await scene.HighlightAsync(Btn(scene, "Cancel order"), "Cancel order");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Btn(scene, "Cancel order"), pauseAfter: 1200);
            var confirm = scene.Page.Locator(".modal.show .btn-danger, .modal.show .btn-primary").First;

            try
            {
                await confirm.WaitForAsync(new() { Timeout = 2500 });
                await scene.ClickAsync(confirm, pauseAfter: 500);
            }
            catch (TimeoutException)
            {
            }

            await scene.OffCameraAsync(() => scene.Page.WaitForTimeoutAsync(4000));
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("can-2", async () =>
        {
            await scene.NavigateAsync("/Admin/purchase-orders");
            var filter = scene.Page.Locator("button.dropdown-toggle").Filter(new() { HasTextString = "Open" }).First;
            await scene.HighlightAsync(filter, "Open, Drafts, Ordered, Canceled and more");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(filter, pauseAfter: 1500);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show a, .dropdown-menu.show button, .dropdown-menu.show label").Filter(new() { HasTextString = "Canceled" }).First, pauseAfter: 3000);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task AlertsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/inventory");
        await scene.StartCaptureAsync();

        await scene.StepAsync("alt-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/Settings/inventory"), "Settings > Commerce > Inventory", 2200);
            await scene.HighlightAsync(Field(scene, "Low-stock threshold"), "Low-stock threshold");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Fld(scene, "ISite.InventorySettings.LowStockNotificationRoles").Locator("xpath=ancestor::div[contains(@class,'ocat-wrapper') or contains(@class,'mb-3') or contains(@class,'card')][1]"), "Notify about low stock");
            await scene.PauseAsync(4000);
        });

        await scene.ClearHighlightsAsync();
    }
}
