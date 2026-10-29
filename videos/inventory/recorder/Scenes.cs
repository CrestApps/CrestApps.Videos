using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Inventory video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-count", CountAsync),
        ("04-stock", StockAsync),
        ("05-import", ImportAsync),
        ("06-product", ProductAsync),
        ("07-locations", LocationsAsync),
        ("08-purchase", PurchaseAsync),
        ("09-shop", ShopAsync),
    ];

    /// <summary>
    /// Turns on the stock alerts and purchase orders features (not recorded). The site is the one the Orders video prepared:
    /// the shop with nine products and four fake orders. The Inventory part, the counts and the locations are made on camera.
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var page = await Seed.NewPageAsync(browser);

        await Seed.EnableAsync(page, "CrestApps.OrchardCore.Inventory.StockAlerts", "CrestApps.OrchardCore.Inventory.PurchaseOrders");
        await page.Context.CloseAsync();
    }

    private static ILocator Fld(Scene scene, string name) => scene.Page.Locator($"[name='{name}']").First;

    private static async Task ReplaceAsync(Scene scene, ILocator field, string text, int delay = 60)
    {
        await scene.ClickAsync(field, pauseAfter: 200);
        await scene.Page.Keyboard.PressAsync("Control+A");
        await field.PressSequentiallyAsync(text, new() { Delay = delay });
        await scene.PauseAsync(300);
    }

    private static async Task CountAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.StartCaptureAsync();

        await scene.StepAsync("cnt-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/ContentTypes/List"), "Content Definition > Content Types", 1800);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/ContentTypes/Edit/Product']").First);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='AddPartsTo/Product']").First);
        });

        await scene.StepAsync("cnt-2", async () =>
        {
            var row = scene.Page.Locator("div.form-check").Filter(new() { HasTextString = "Counts how many units" }).First;
            await scene.ClickAsync(row.Locator("input[type=checkbox]"), pauseAfter: 1200);
            await scene.HighlightAsync(row, "Inventory");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button, input[type=submit]").Filter(new() { HasTextString = "Add" }).Last);
        });

        await scene.StepAsync("cnt-3", async () =>
        {
            await scene.NavigateAsync("/Admin/inventory");
            await scene.ClickAndWaitForNavigationAsync(Link(scene, "Find products to count"));
        });
    }
}
