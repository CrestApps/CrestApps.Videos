using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-start", StartAsync),
        ("04-order", OrderAsync),
        ("05-request", RequestAsync),
        ("05b-waiting", WaitingAsync),
        ("06-receive", ReceiveAsync),
        ("07-cancel", CancelAsync),
        ("08-alerts", AlertsAsync),
    ];

    // Turns on stock alerts and purchase orders, counts the Product type, gives the products their counts and adds a location (not recorded).
    public static async Task PrepareAsync(IBrowser browser)
    {
        var page = await Seed.NewPageAsync(browser);

        await Seed.EnableAsync(page, "CrestApps.OrchardCore.Inventory.StockAlerts", "CrestApps.OrchardCore.Inventory.PurchaseOrders", "CrestApps.OrchardCore.Checkout.TestProvider");

        // The Inventory part on the Product type.
        await page.GotoAsync(BaseUrl + "/Admin/ContentTypes/AddPartsTo/Product", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var row = page.Locator("div.form-check").Filter(new() { HasTextString = "Counts how many units" }).First;

        if (await row.CountAsync() > 0)
        {
            await row.Locator("input[type=checkbox]").CheckAsync();
            await page.RunAndWaitForNavigationAsync(() => page.Locator("button, input[type=submit]").Filter(new() { HasTextString = "Add" }).Last.ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        }

        await page.GotoAsync(BaseUrl + "/Admin/inventory", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var find = page.Locator("a, button").Filter(new() { HasTextString = "Find products to count" }).First;

        if (await find.CountAsync() > 0)
        {
            await page.RunAndWaitForNavigationAsync(() => find.ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        }

        foreach (var (product, count) in new[] { ("Ceramic Mug", 4), ("Wool Beanie", 0), ("Canvas Backpack", 3), ("Trail Runner Shoe", 25), ("Wireless Earbuds", 18), ("Leather Wallet", 12), ("Hardcover Notebook", 30), ("Desk Lamp", 9), ("Insulated Water Bottle", 22) })
        {
            await CountToAsync(page, product, count);
        }

        await Seed.SubmitAsync(page, "/Admin/Settings/inventory", new()
        {
            ["ISite.InventorySettings.Locations[1].Name"] = "Main warehouse",
            ["ISite.InventorySettings.Locations[1].Address"] = "12 Harbor Road, Portland",
        }, "css:form button.btn-primary");

        await page.Context.CloseAsync();
    }

    private static async Task CountToAsync(IPage page, string product, int count)
    {
        await page.GotoAsync(BaseUrl + "/Admin/inventory", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var change = page.Locator("li, tr").Filter(new() { HasTextString = product }).Last.Locator("a:has-text('Change')").First;

        if (await change.CountAsync() == 0)
        {
            return;
        }

        await page.RunAndWaitForNavigationAsync(() => change.ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("[name='Count.Count']").FillAsync(count.ToString());
        await page.RunAndWaitForNavigationAsync(() => page.Locator("button").Filter(new() { HasTextString = "Record count" }).First.ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
    }
}
