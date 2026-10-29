using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Product variants and prices video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-options", OptionsAsync),
        ("04-values", ValuesAsync),
        ("05-shop", ShopAsync),
        ("06-prices", PricesAsync),
    ];

    /// <summary>Nothing to prepare: the site is the one the Shipping video prepared (the shop with nine products). The parts are added on camera.</summary>
    public static Task PrepareAsync(IBrowser browser) => Task.CompletedTask;

    private const string BackpackPath = "/Admin/Contents/ContentItems/4swccm1c6r9fa4v1hjqgpfsn4h/Edit";

    private static ILocator Fld(Scene scene, string name) => scene.Page.Locator($"[name='{name}']").First;

    private static async Task ReplaceAsync(Scene scene, ILocator field, string text, int delay = 60)
    {
        await scene.ClickAsync(field, pauseAfter: 200);
        await scene.Page.Keyboard.PressAsync("Control+A");
        await field.PressSequentiallyAsync(text, new() { Delay = delay });
        await scene.PauseAsync(300);
    }

    private static async Task PickAsync(Scene scene, ILocator select, string contains, int pauseAfter = 700)
    {
        await scene.ClickAsync(select, pauseAfter: 300);
        await select.EvaluateAsync("(select, text) => { const option = [...select.options].find(o => o.text.includes(text)); select.value = option.value; select.dispatchEvent(new Event('change', { bubbles: true })); }", contains);
        await scene.PauseAsync(pauseAfter);
    }

    private static async Task WaitAsync(Scene scene, int millis = 6000)
        => await scene.OffCameraAsync(() => scene.Page.WaitForTimeoutAsync(millis));

    private static async Task AddPartAsync(Scene scene, string description)
    {
        await scene.NavigateAsync("/Admin/ContentTypes/AddPartsTo/Product");
        var row = scene.Page.Locator("div.form-check").Filter(new() { HasTextString = description }).First;
        await scene.ClickAsync(row.Locator("input[type=checkbox]"), pauseAfter: 1200);
        await scene.HighlightAsync(row, "The part");
        await scene.PauseAsync(1500);
        await scene.ClearHighlightsAsync();
        await scene.ClickAsync(scene.Page.Locator("button, input[type=submit]").Filter(new() { HasTextString = "Add" }).Last, pauseAfter: 500);
        await WaitAsync(scene, 3500);
    }

    private static async Task OptionsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.StartCaptureAsync();

        await scene.StepAsync("opt-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/ContentTypes/List"), "Content Definition > Content Types", 2000);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/ContentTypes/Edit/Product']").First);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='AddPartsTo/Product']").First);
            var row = scene.Page.Locator("div.form-check").Filter(new() { HasTextString = "Sells a product in versions" }).First;
            await scene.ClickAsync(row.Locator("input[type=checkbox]"), pauseAfter: 1200);
            await scene.HighlightAsync(row, "Product variants");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button, input[type=submit]").Filter(new() { HasTextString = "Add" }).Last, pauseAfter: 500);
            await WaitAsync(scene, 3500);
        });

        await scene.StepAsync("opt-2", async () =>
        {
            await scene.NavigateAsync("/Admin/ContentTypes/Product/ContentParts/ProductVariantsPart/Edit");
            const string Prefix = "Product.ProductVariantsPart.ProductVariantsPartSettingsDisplayDriver.Options";
            await scene.TypeAsync(Fld(scene, $"{Prefix}[0].Name"), "Size", delay: 80);
            await scene.ClickAsync(Fld(scene, $"{Prefix}[0].Filterable"), pauseAfter: 900);
            await scene.TypeAsync(Fld(scene, $"{Prefix}[1].Name"), "Color", delay: 80);
            await scene.HighlightAsync(Fld(scene, $"{Prefix}[0].Name").Locator("xpath=ancestor::div[contains(@class,'row') or contains(@class,'card') or contains(@class,'mb-3')][1]"), "Options");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button[name='submit.Save']").First, pauseAfter: 500);
            await WaitAsync(scene, 4000);
        });
    }

    private static async Task ValuesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync(BackpackPath);
        await scene.StartCaptureAsync();

        await scene.StepAsync("val-1", async () =>
        {
            var sizes = Fld(scene, "ProductVariantsPart.Options[0].Values");
            await sizes.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(sizes.Locator("xpath=ancestor::div[contains(@class,'ocat') or contains(@class,'card') or contains(@class,'mb-3')][last()]"), "Variants");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(sizes, "S, M, L", delay: 80);
            await scene.TypeAsync(Fld(scene, "ProductVariantsPart.Options[1].Values"), "Green, Navy", delay: 70);
            await scene.ClickAsync(Publish(scene), pauseAfter: 500);
            await WaitAsync(scene, 5000);
            await scene.NavigateAsync(BackpackPath);
        });

        await scene.StepAsync("val-2", async () =>
        {
            var first = Fld(scene, "ProductVariantsPart.Variants[0].Enabled");
            await first.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(first.Locator("xpath=ancestor::table[1]"), "A variant for each combination");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(first, pauseAfter: 900);
            await scene.TypeAsync(Fld(scene, "ProductVariantsPart.Variants[5].Sku"), "BAG-CNV-L-NVY", delay: 50);
            await ReplaceAsync(scene, Fld(scene, "ProductVariantsPart.Variants[5].Price"), "74", 110);
            await ReplaceAsync(scene, Fld(scene, "ProductVariantsPart.Variants[5].CompareAtPrice"), "89", 110);
        });

        await scene.StepAsync("val-3", async () =>
        {
            await scene.HighlightAsync(Fld(scene, "ProductVariantsPart.Variants[5].Price").Locator("xpath=ancestor::tr[1]"), "Its own SKU and price");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Publish(scene), pauseAfter: 500);
            await WaitAsync(scene, 5000);
        });
    }

    private static async Task ShopAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/shop");
        await scene.StartCaptureAsync();

        await scene.StepAsync("shp-1", async () =>
        {
            await ScrollAsync(scene, 0, 250);
            await scene.HighlightAsync(scene.Page.Locator(".card").Filter(new() { HasTextString = "Canvas Backpack" }).First, "From the lowest price");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("div, section").Filter(new() { HasTextString = "Size" }).Filter(new() { HasTextString = "Filters" }).Last, "A filter on an option");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("shp-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".card").Filter(new() { HasTextString = "Canvas Backpack" }).First.Locator("a").First);
            await PickAsync(scene, scene.Page.Locator("select").Filter(new() { Has = scene.Page.Locator("option", new() { HasTextString = "L" }) }).First, "L", 1200);
            await scene.HighlightAsync(scene.Page.Locator("select").First.Locator("xpath=ancestor::*[self::form or contains(@class,'card')][1]"), "One list for each option");
            await scene.PauseAsync(3500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PricesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems?q=Ceramic");
        await scene.StartCaptureAsync();

        await scene.StepAsync("prc-1", async () =>
        {
            await AddPartAsync(scene, "Offers a product at more than one price");
            await scene.NavigateAsync("/Admin/Contents/ContentItems?q=Ceramic");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a").Filter(new() { HasTextString = "Ceramic Mug" }).First);
            var name = Fld(scene, "ProductPricePart.Prices[0].Name");
            await name.ScrollIntoViewIfNeededAsync();
            await scene.TypeAsync(name, "Single mug", delay: 60);
            await ReplaceAsync(scene, Fld(scene, "ProductPricePart.Prices[0].Amount"), "18", 120);
        });

        await scene.StepAsync("prc-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button, a").Filter(new() { HasTextString = "Add a price" }).First, pauseAfter: 1500);
            await scene.TypeAsync(Fld(scene, "ProductPricePart.Prices[1].Name"), "Mug club", delay: 60);
            await ReplaceAsync(scene, Fld(scene, "ProductPricePart.Prices[1].Amount"), "12", 120);
            await PickAsync(scene, Fld(scene, "ProductPricePart.Prices[1].Kind"), "Recurring", 1200);
            await ReplaceAsync(scene, Fld(scene, "ProductPricePart.Prices[1].BillingDuration"), "1", 120);
            await PickAsync(scene, Fld(scene, "ProductPricePart.Prices[1].Interval"), "Month", 900);
            await ReplaceAsync(scene, Fld(scene, "ProductPricePart.Prices[1].TrialDays"), "7", 120);
            await scene.HighlightAsync(Fld(scene, "ProductPricePart.Prices[1].Name").Locator("xpath=ancestor::*[contains(@class,'card') or contains(@class,'border') or contains(@class,'price')][1]"), "Charged: Recurring");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("prc-3", async () =>
        {
            var offered = scene.Page.GetByText("Offered from").First;
            await offered.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(offered.Locator("xpath=ancestor::div[2]"), "Offered from and until");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            var named = scene.Page.GetByText("Buyer names the amount").First;
            await named.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(named.Locator("xpath=ancestor::div[2]"), "Buyer names the amount");
            await scene.PauseAsync(3000);
        });

        await scene.ClearHighlightsAsync();
    }
}
