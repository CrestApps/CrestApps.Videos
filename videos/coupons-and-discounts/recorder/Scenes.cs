using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Coupons and discounts video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-code", CodeAsync),
        ("04-rules", RulesAsync),
        ("05-auto", AutoAsync),
        ("06-share", ShareAsync),
        ("07-checkout", CheckoutAsync),
        ("08-list", ListAsync),
    ];

    /// <summary>
    /// Nothing to prepare (not recorded): the site is the one the Shipping video prepared, the shop with nine products,
    /// a zone Everywhere with Standard shipping and Express shipping, and the test payment provider. The coupons are made on camera.
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var page = await Seed.NewPageAsync(browser);

        await Seed.EnableAsync(page, "CrestApps.OrchardCore.Checkout.TestProvider");
        await page.Context.CloseAsync();
    }

    private static ILocator Fld(Scene scene, string name) => scene.Page.Locator($"[name='{name}']").First;

    private static ILocator Summary(Scene scene) => scene.Page.Locator(".card").Filter(new() { HasTextString = "Order summary" }).First;

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

    // Saves a form without waiting for the network to be idle, which a slow save can outlast.
    private static async Task SaveAsync(Scene scene, ILocator button, string urlPart = "/Admin/coupons")
    {
        await scene.ClickAsync(button, pauseAfter: 500);
        await scene.OffCameraAsync(() => scene.Page.WaitForTimeoutAsync(6000));
    }

    private static ILocator CreateButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Create" }).Last;

    private static async Task CodeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/coupons");
        await scene.StartCaptureAsync();

        await scene.StepAsync("cod-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/coupons"), "Commerce > Coupons", 2200);
            await scene.ClickAndWaitForNavigationAsync(Link(scene, "Add coupon"));
        });

        await scene.StepAsync("cod-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("div").Filter(new() { HasTextString = "How it's applied" }).Filter(new() { HasTextString = "Automatically" }).Last, "How it's applied");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(Fld(scene, "Code"), "WELCOME10", delay: 70);
            await scene.TypeAsync(Fld(scene, "Description"), "Welcome offer: 10% off your first order", delay: 30);
            await ReplaceAsync(scene, Fld(scene, "Percentage"), "10", 120);
        });

        await scene.StepAsync("cod-3", async () =>
        {
            await scene.HighlightAsync(Field(scene, "Applies to"), "Applies to");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await ReplaceAsync(scene, Fld(scene, "MaxRedemptions"), "100", 110);
            await ReplaceAsync(scene, Fld(scene, "MaxRedemptionsPerCustomer"), "1", 110);
            await scene.HighlightAsync(Field(scene, "Uses per customer"), "Usage limit and uses per customer");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("cod-4", async () =>
        {
            await ScrollAsync(scene, 0, 400);
            await scene.ClickAsync(Fld(scene, "EndsUtc"), pauseAfter: 300);
            await Fld(scene, "EndsUtc").FillAsync("2027-12-31T23:59");
            await scene.HighlightAsync(Field(scene, "Valid until"), "Valid until");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "Enabled"), "Enabled");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await ScrollAsync(scene, 0, 1500);
            await SaveAsync(scene, CreateButton(scene));
            await scene.PauseAsync(1500);
        });
    }

    private static async Task RulesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/coupons/Create");
        await scene.StartCaptureAsync();

        await scene.StepAsync("rul-1", async () =>
        {
            await scene.TypeAsync(Fld(scene, "Code"), "SAVE15", delay: 70);
            await scene.TypeAsync(Fld(scene, "Description"), "15 dollars off an order of 100 dollars or more", delay: 25);
            await PickAsync(scene, Fld(scene, "Kind"), "Fixed amount", 900);
            await ReplaceAsync(scene, Fld(scene, "Amount"), "15", 110);
            await PickAsync(scene, Fld(scene, "Currency"), "US Dollar", 900);
        });

        await scene.StepAsync("rul-2", async () =>
        {
            await ReplaceAsync(scene, Fld(scene, "MinimumAmount"), "100", 110);
            await scene.HighlightAsync(Field(scene, "Minimum amount"), "Minimum amount");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "Maximum amount"), "Maximum amount");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "Minimum quantity"), "Minimum quantity");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("rul-3", async () =>
        {
            await ScrollAsync(scene, 0, 500);
            await scene.HighlightAsync(Field(scene, "Combines with"), "Combines with");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Fld(scene, "CombinesWithShippingDiscounts"), pauseAfter: 900);
            await scene.ClickAsync(Fld(scene, "CombinesWithProductDiscounts"), pauseAfter: 900);
        });

        await scene.StepAsync("rul-4", async () =>
        {
            await ScrollAsync(scene, 0, 1500);
            await scene.HighlightAsync(Field(scene, "Products and categories"), "Products and categories");
            await scene.PauseAsync(2600);
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene, CreateButton(scene));
            await scene.NavigateAsync("/Admin/coupons");
            await scene.PauseAsync(2500);
        });
    }

    private static async Task AutoAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/coupons/Create");
        await scene.StartCaptureAsync();

        await scene.StepAsync("aut-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("label").Filter(new() { HasTextString = "Automatically" }).First, pauseAfter: 1200);
            await scene.TypeAsync(Fld(scene, "Description"), "Summer sale: buy 2, get the cheapest free", delay: 30);
        });

        await scene.StepAsync("aut-2", async () =>
        {
            await PickAsync(scene, Fld(scene, "Kind"), "Buy X get Y", 900);
            await ReplaceAsync(scene, Fld(scene, "BuyQuantity"), "2", 120);
            await ReplaceAsync(scene, Fld(scene, "GetQuantity"), "1", 120);
            await ReplaceAsync(scene, Fld(scene, "Percentage"), "100", 100);
            await scene.HighlightAsync(Field(scene, "Times per order"), "Times per order");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("aut-3", async () =>
        {
            await ScrollAsync(scene, 0, 600);
            await scene.ClickAsync(Fld(scene, "CombinesWithShippingDiscounts"), pauseAfter: 900);
            await ScrollAsync(scene, 0, 1500);
            await SaveAsync(scene, CreateButton(scene));
            await scene.NavigateAsync("/Admin/coupons");
            await scene.PauseAsync(2500);
        });
    }

    private static async Task CheckoutAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        var page = scene.Page;
        await page.GotoAsync(BaseUrl + "/shop", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var card = page.Locator(".card").Filter(new() { HasTextString = "Hardcover Notebook" }).First;
        await card.Locator("input[type=number]").FillAsync("3");
        await card.Locator("button:has-text('Add to cart')").ClickAsync();
        await page.WaitForTimeoutAsync(1500);
        await page.GotoAsync(BaseUrl + "/cart", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.RunAndWaitForNavigationAsync(() => page.Locator(".card").Filter(new() { HasTextString = "Summary" }).Locator("a, button").Filter(new() { HasTextString = "Checkout" }).First.ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.GetByLabel("Full name").FillAsync("Jamie Chen");
        await page.GetByLabel("Email").FillAsync("customer@example.com");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#Shipping_Address_Country").SelectOptionAsync(new SelectOptionValue { Label = "United States" });
        await page.WaitForTimeoutAsync(1200);
        await page.Locator("#Shipping_Address_Name").FillAsync("Jamie Chen");
        await page.Locator("#Shipping_Address_AddressLine1").FillAsync("350 Fifth Avenue");
        await page.Locator("#Shipping_Address_City").FillAsync("New York");
        await page.Locator("#Shipping_Address_Region").SelectOptionAsync(new SelectOptionValue { Label = "New York" });
        await page.Locator("#Shipping_Address_PostalCode").FillAsync("10118");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("input[type=radio]").First.CheckAsync();
        await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Mouse.MoveAsync(700, 500);

        await scene.StartCaptureAsync();

        await scene.StepAsync("chk-1", async () =>
        {
            await scene.HighlightAsync(Summary(scene), "Summer sale, taken off by itself");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("chk-2", async () =>
        {
            var box = page.GetByPlaceholder("Enter a code").First;
            await scene.TypeAsync(box, "WELCOME10", delay: 80);
            await scene.ClickAsync(page.Locator("button:has-text('Apply')").First, pauseAfter: 3000);
            await scene.HighlightAsync(page.Locator(".card").Filter(new() { HasTextString = "Promotion code" }).First, "Promotion code");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("chk-3", async () =>
        {
            await scene.HighlightAsync(Summary(scene), "Every discount by name");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
            await page.Mouse.MoveAsync(700, 500);
            await scene.HighlightAsync(Summary(scene), "On the order and its email");
            await scene.PauseAsync(2500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/coupons");
        await scene.StartCaptureAsync();

        await scene.StepAsync("lst-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("input[type=search], input[name*='earch' i]").First, "Search by code or description");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button:has-text('Any status'), a:has-text('Any status'), select").First, pauseAfter: 1800);
            await scene.Page.Keyboard.PressAsync("Escape");
        });

        await scene.StepAsync("lst-2", async () =>
        {
            await scene.HighlightAsync(Row(scene, "Summer sale"), "Discount, applies to, used, valid until");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Summer sale").Locator("a").First);
        });

        await scene.StepAsync("lst-3", async () =>
        {
            var heading = scene.Page.Locator("h1, h2, h3, h4, h5, h6").Filter(new() { HasTextString = "Recent uses" }).First;
            await heading.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(heading.Locator("xpath=.."), "Recent uses");
            await scene.PauseAsync(3500);
        });

        await scene.ClearHighlightsAsync();
    }
}
