using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Markets video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-market", MarketAsync),
        ("04-picker", PickerAsync),
        ("05-notsold", NotSoldAsync),
        ("06-shipping", ShippingAsync),
        ("07-checkout", CheckoutAsync),
    ];

    /// <summary>
    /// Turns on the Markets feature and adds the EU VAT rates (not recorded). The site is the one the Shipping video
    /// prepared: the shop with nine products, a zone Everywhere with Standard shipping and Express shipping, and the
    /// test payment provider. The market itself, its shipping method and the product left out are made on camera.
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var page = await Seed.NewPageAsync(browser);

        await Seed.EnableAsync(page, "CrestApps.OrchardCore.Markets", "CrestApps.OrchardCore.Checkout.TestProvider");

        await page.GotoAsync(BaseUrl + "/Admin/Recipes", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#btn-run-EuVatStandardRates").ClickAsync();
        await page.RunAndWaitForNavigationAsync(() => page.Locator(".modal.show .btn-primary").First.ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Context.CloseAsync();
    }

    private static ILocator Fld(Scene scene, string name) => scene.Page.Locator($"[name='{name}']").First;

    private static ILocator Summary(Scene scene) => scene.Page.Locator(".card").Filter(new() { HasTextString = "Order summary" }).First;

    private static ILocator Card(Scene scene, string product) => scene.Page.Locator(".card").Filter(new() { HasTextString = product }).First;

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

    // Picks a market in the storefront's picker without showing it.
    private static async Task ChooseMarketAsync(IPage page, string market)
    {
        await page.Locator("#market-picker").ClickAsync();
        await page.RunAndWaitForNavigationAsync(
            () => page.Locator(".dropdown-menu.show button.dropdown-item").Filter(new() { HasTextString = market }).First.ClickAsync(),
            new() { WaitUntil = WaitUntilState.NetworkIdle });
    }

    private static async Task MarketAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/markets");
        await scene.StartCaptureAsync();

        await scene.StepAsync("mkt-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/markets"), "Commerce > Markets", 2200);
            await scene.ClickAndWaitForNavigationAsync(Link(scene, "Add market"));
        });

        await scene.StepAsync("mkt-2", async () =>
        {
            await scene.TypeAsync(Fld(scene, "Name"), "Europe", delay: 70);
            await scene.TypeAsync(Fld(scene, "Countries"), "DE, FR, IT, ES, NL, AT", delay: 50);
            await scene.HighlightAsync(Field(scene, "Countries"), "Countries");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("mkt-3", async () =>
        {
            await scene.TypeAsync(Fld(scene, "Currency"), "EUR", delay: 90);
            await ReplaceAsync(scene, Fld(scene, "ExchangeRate"), "0.92", 110);
            await scene.HighlightAsync(Field(scene, "Exchange rate"), "Exchange rate");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("mkt-4", async () =>
        {
            await ScrollAsync(scene, 0, 300);
            await PickAsync(scene, Fld(scene, "Rounding"), "Up to .99", 900);
            await scene.HighlightAsync(Field(scene, "Language"), "Language");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Fld(scene, "PricesIncludeTax"), pauseAfter: 800);
            await scene.HighlightAsync(Field(scene, "Prices include tax"), "Prices include tax");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "On"), "On");
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1800);
        });
    }

    private static async Task PickerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/shop");
        await scene.StartCaptureAsync();

        await scene.StepAsync("pick-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#market-picker"), "Market picker");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#market-picker"), pauseAfter: 1200);
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show"), "Shop in, and Everywhere else");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("pick-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".dropdown-menu.show button.dropdown-item").Filter(new() { HasTextString = "Europe" }).First);
            await ScrollAsync(scene, 0, 250);
            await scene.HighlightAsync(Card(scene, "Trail Runner Shoe"), "Prices in euros");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("pick-3", async () =>
        {
            await scene.ClickAsync(Card(scene, "Trail Runner Shoe").Locator("button:has-text('Add to cart')").First, pauseAfter: 1500);
            await scene.NavigateAsync("/cart");
            await scene.HighlightAsync(scene.Page.Locator(".card").Filter(new() { HasTextString = "Subtotal" }).First, "The cart, in euros");
            await scene.PauseAsync(2000);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task NotSoldAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/Product");
        await scene.Page.RunAndWaitForNavigationAsync(() => Row(scene, "Ceramic Mug").Locator("a:has-text('Edit')").First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("out-1", async () =>
        {
            await scene.HighlightAsync(Field(scene, "Not sold in"), "Not sold in");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("label.form-check-label").Filter(new() { HasTextString = "Europe" }).First, pauseAfter: 1200);
            await scene.ClickAsync(Publish(scene), pauseAfter: 500);
            await scene.OffCameraAsync(() => scene.Page.WaitForTimeoutAsync(8000));
        });

        await scene.StepAsync("out-2", async () =>
        {
            await scene.NavigateAsync("/shop");
            await scene.OffCameraAsync(() => ChooseMarketAsync(scene.Page, "Europe"));
            await ScrollAsync(scene, 0, 250);
            await scene.HighlightAsync(Card(scene, "Ceramic Mug"), "Not sold in Europe.");
            await scene.PauseAsync(3000);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ShippingAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/shipping/zones");
        await scene.Page.RunAndWaitForNavigationAsync(() => Row(scene, "Everywhere").Locator("a:has-text('Edit')").First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("shp-1", async () =>
        {
            await scene.HighlightAsync(Link(scene, "Add method"), "Add method");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Link(scene, "Add method"), pauseAfter: 1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/methods/create/Flat']").First);
        });

        await scene.StepAsync("shp-2", async () =>
        {
            await ReplaceAsync(scene, Fld(scene, "ShippingMethod.Name"), "Standard shipping in Europe", 45);
            await ReplaceAsync(scene, Fld(scene, "ShippingMethod.Amount"), "6.9", 100);
            await PickAsync(scene, scene.Page.Locator("select[name$='Currency']").First, "EUR", 900);
            await scene.HighlightAsync(Field(scene, "Currency"), "Currency");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1500);
        });
    }

    private static async Task CheckoutAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        var page = scene.Page;
        await page.GotoAsync(BaseUrl + "/shop", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await ChooseMarketAsync(page, "Europe");
        await page.Locator(".card").Filter(new() { HasTextString = "Trail Runner Shoe" }).First.Locator("button:has-text('Add to cart')").ClickAsync();
        await page.WaitForTimeoutAsync(1500);
        await page.GotoAsync(BaseUrl + "/cart", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.RunAndWaitForNavigationAsync(() => page.Locator(".card").Filter(new() { HasTextString = "Summary" }).Locator("a, button").Filter(new() { HasTextString = "Checkout" }).First.ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.GetByLabel("Full name").FillAsync("Jamie Chen");
        await page.GetByLabel("Email").FillAsync("customer@example.com");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#Shipping_Address_Country").SelectOptionAsync(new SelectOptionValue { Label = "Germany" });
        await page.WaitForTimeoutAsync(1200);
        await page.Locator("#Shipping_Address_Name").FillAsync("Jamie Chen");
        await page.Locator("#Shipping_Address_AddressLine1").FillAsync("Unter den Linden 77");
        await page.Locator("#Shipping_Address_City").FillAsync("Berlin");
        await page.Locator("#Shipping_Address_PostalCode").FillAsync("10117");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("input[type=radio]").First.CheckAsync();
        await page.WaitForTimeoutAsync(800);

        await scene.StartCaptureAsync();

        await scene.StepAsync("chk-1", async () =>
        {
            await scene.HighlightAsync(Summary(scene), "The order and its shipping, in euros");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("chk-2", async () =>
        {
            await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
            await page.Mouse.MoveAsync(700, 500);
            await scene.HighlightAsync(Summary(scene), "VAT, included in the price");
            await scene.PauseAsync(3000);
        });

        await scene.ClearHighlightsAsync();
    }
}
