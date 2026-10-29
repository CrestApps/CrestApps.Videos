using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static async Task PickAsync(Scene scene, ILocator select, string contains, int pauseAfter = 700)
    {
        await scene.ClickAsync(select, pauseAfter: 300);
        await select.EvaluateAsync("(select, text) => { const option = [...select.options].find(o => o.text.includes(text)); select.value = option.value; select.dispatchEvent(new Event('change', { bubbles: true })); }", contains);
        await scene.PauseAsync(pauseAfter);
    }

    private static async Task StockAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/inventory");
        await scene.StartCaptureAsync();

        await scene.StepAsync("stk-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/inventory"), "Commerce > Inventory", 2000);
            await scene.HighlightAsync(Row(scene, "Ceramic Mug"), "On hand, Reserved, Available");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Ceramic Mug").Locator("a:has-text('Change')").First);
        });

        await scene.StepAsync("stk-2", async () =>
        {
            await ReplaceAsync(scene, Fld(scene, "Adjust.Quantity"), "30", 90);
            await scene.TypeAsync(Fld(scene, "Adjust.Reason"), "Delivery from the supplier", delay: 25);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Record" }).First);
        });

        await scene.StepAsync("stk-3", async () =>
        {
            await PickAsync(scene, Fld(scene, "Adjust.Kind"), "Adjusted", 400);
            await ReplaceAsync(scene, Fld(scene, "Adjust.Quantity"), "-2", 90);
            await scene.TypeAsync(Fld(scene, "Adjust.Reason"), "Damaged", delay: 30);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Record" }).First);
        });

        await scene.StepAsync("stk-3b", async () =>
        {
            await ReplaceAsync(scene, Fld(scene, "Count.Count"), "4", 90);
            await scene.TypeAsync(Fld(scene, "Count.Reason"), "Stocktake", delay: 30);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Record count" }).First);
        });

        await scene.StepAsync("stk-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("table").First, "History");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ImportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/inventory");
        await scene.StartCaptureAsync();

        await scene.StepAsync("imp-1", async () =>
        {
            await scene.HighlightAsync(Link(scene, "Export CSV"), "Export CSV");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("imp-2", async () =>
        {
            await scene.ClickAsync(Link(scene, "Import CSV"), pauseAfter: 1500);
            await scene.Page.Locator("#inventory-import-file").SetInputFilesAsync(Path.Combine(Root, "stock-counts.csv"));
            await scene.PauseAsync(1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".modal.show button, button").Filter(new() { HasTextString = "Import" }).Last);
        });
    }

    private static async Task ProductAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/Product");
        await scene.Page.RunAndWaitForNavigationAsync(() => Row(scene, "Ceramic Mug").Locator("a:has-text('Edit')").First.ClickAsync());
        await ScrollAsync(scene, 0, 4200);
        await scene.StartCaptureAsync();

        await scene.StepAsync("prd-1", async () =>
        {
            await scene.HighlightAsync(Field(scene, "Count stock"), "Count stock");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("prd-2", async () =>
        {
            await ShowAsync(scene, Field(scene, "Allow backorders"), "Allow backorders", 2400);
            await ShowAsync(scene, Field(scene, "Low-stock threshold"), "Low-stock threshold", 2400);
            await scene.HighlightAsync(Field(scene, "Ships from"), "Ships from");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("prd-3", async () =>
        {
            await ScrollAsync(scene, 0, 300);
            await scene.HighlightAsync(scene.Page.Locator("div").Filter(new() { HasTextString = "On hand" }).Filter(new() { HasTextString = "Change" }).Last, "On hand");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task LocationsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/inventory");
        await scene.StartCaptureAsync();

        await scene.StepAsync("loc-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/Settings/inventory"), "Settings > Commerce > Inventory", 2200);
            await ShowAsync(scene, Field(scene, "Low-stock threshold"), "Low-stock threshold", 2200);
            await scene.HighlightAsync(Field(scene, "Hold stock for"), "Hold stock for");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("loc-2", async () =>
        {
            await scene.HighlightAsync(Fld(scene, "ISite.InventorySettings.LowStockNotificationRoles").Locator("xpath=ancestor::div[contains(@class,'ocat-wrapper') or contains(@class,'mb-3') or contains(@class,'card')][1]"), "Notify about low stock");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("loc-3", async () =>
        {
            await scene.TypeAsync(Fld(scene, "ISite.InventorySettings.Locations[1].Name"), "Main warehouse", delay: 50);
            await scene.TypeAsync(Fld(scene, "ISite.InventorySettings.Locations[1].Address"), "12 Harbor Road, Portland", delay: 30);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.StepAsync("loc-4", async () =>
        {
            await scene.NavigateAsync("/Admin/inventory");
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Ceramic Mug").Locator("a:has-text('Change')").First);
            await scene.HighlightAsync(scene.Page.Locator("div, section").Filter(new() { HasTextString = "Move between locations" }).Last, "By location and Move between locations");
            await ScrollAsync(scene, 0, 500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PurchaseAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/purchase-orders");
        await scene.StartCaptureAsync();

        await scene.StepAsync("po-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/purchase-orders"), "Commerce > Purchase orders", 2400);
            await scene.ClickAndWaitForNavigationAsync(Link(scene, "Add purchase order"));
        });

        await scene.StepAsync("po-2", async () =>
        {
            await scene.TypeAsync(Fld(scene, "Supplier"), "Northwind Supply", delay: 45);
            await scene.TypeAsync(Fld(scene, "SupplierEmail"), "orders@example.com", delay: 30);
            await scene.HighlightAsync(Field(scene, "Receive at"), "Receive at");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "Start with the items that are low on stock"), "Start with low-stock items");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Start the order" }).First);
        });

        await scene.StepAsync("po-3", async () =>
        {
            await ScrollAsync(scene, 0, 700);
            await scene.HighlightAsync(scene.Page.Locator("table").First, "Items: Ordered and Unit cost");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.Page.Locator("input[name*='UnitCost' i]").First.FillAsync("4.5");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Save" }).First);
            await ScrollAsync(scene, 0, 700);
            await scene.HighlightAsync(scene.Page.Locator("button, a").Filter(new() { HasTextString = "Mark as ordered" }).First, "Mark as ordered");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Mark as ordered" }).First);
        });

        await scene.StepAsync("po-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Receive everything" }).First, "Receive everything");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Receive everything" }).First);
        });
    }

    // Counts a product's stock to a number, from the admin, outside the capture.
    private static async Task CountToAsync(IBrowser browser, string product, int count)
    {
        var page = await Seed.NewPageAsync(browser);
        await page.GotoAsync(BaseUrl + "/Admin/inventory", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.RunAndWaitForNavigationAsync(() => page.Locator("li, tr").Filter(new() { HasTextString = product }).Last.Locator("a:has-text('Change')").First.ClickAsync());
        await page.Locator("[name='Count.Count']").FillAsync(count.ToString());
        await page.RunAndWaitForNavigationAsync(() => page.Locator("button").Filter(new() { HasTextString = "Record count" }).First.ClickAsync());
        await page.Context.CloseAsync();
    }

    private static async Task ShopAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        // Something running low, and something gone, whatever the purchase order received.
        await CountToAsync(browser, "Ceramic Mug", 4);
        await CountToAsync(browser, "Wool Beanie", 0);

        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/shop");
        await scene.StartCaptureAsync();

        await scene.StepAsync("shop-1", async () =>
        {
            await ScrollAsync(scene, 0, 250);
            await scene.HighlightAsync(scene.Page.Locator(".card").Filter(new() { HasTextString = "Ceramic Mug" }).First, "Only a few left");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".card").Filter(new() { HasTextString = "Wool Beanie" }).First, "Out of stock");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("shop-2", async () =>
        {
            await ScrollAsync(scene, 0, -400);
            await scene.ClickAsync(scene.Page.Locator("label[for='facet-in-stock']").First, pauseAfter: 2500);
        });
    }
}
