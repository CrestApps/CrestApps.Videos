using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Carts and Checkout video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-cart", CartAsync),
        ("04-contact", ContactAsync),
        ("05-shipping", ShippingAsync),
        ("06-payment", PaymentAsync),
        ("07-settings", SettingsAsync),
    ];

    /// <summary>
    /// Turns on the test payment method, and adds a shipping zone with two flat rates and a 10% promotion code (not recorded).
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var page = await Seed.NewPageAsync(browser);

        await Seed.EnableAsync(page, "CrestApps.OrchardCore.Checkout.TestProvider", "CrestApps.OrchardCore.Customers.CheckoutDetails");

        await page.GotoAsync(BaseUrl + "/Admin/shipping/zones");

        if (await page.Locator("a[href*='/zones/edit/']").CountAsync() == 0)
        {
            await Seed.SubmitAsync(page, "/Admin/shipping/zones/create", new() { ["ShippingZone.Name"] = "Everywhere", ["ShippingZone.RestOfWorld"] = "true" }, "css:button.btn-primary[type=submit]");
            await page.GotoAsync(BaseUrl + "/Admin/shipping/zones");
        }

        var zone = (await page.Locator("a[href*='/zones/edit/']").First.GetAttributeAsync("href")).Split('/').Last();
        await page.GotoAsync(BaseUrl + $"/Admin/shipping/zones/edit/{zone}");

        if (!(await page.Locator("body").InnerTextAsync()).Contains("Standard shipping"))
        {
            foreach (var (name, amount) in new[] { ("Standard shipping", "6"), ("Express shipping", "15") })
            {
                await Seed.SubmitAsync(page, $"/Admin/shipping/zones/{zone}/methods/create/Flat", new() { ["ShippingMethod.Name"] = name, ["ShippingMethod.Amount"] = amount }, "css:button.btn-primary[type=submit]");
            }
        }

        await page.GotoAsync(BaseUrl + "/Admin/coupons");

        if (!(await page.Locator("body").InnerTextAsync()).Contains("WELCOME10"))
        {
            await Seed.SubmitAsync(page, "/Admin/coupons/Create", new() { ["Code"] = "WELCOME10", ["Description"] = "10% off a first order", ["Percentage"] = "10" }, "css:button.btn-primary[type=submit]");
        }

        await page.Context.CloseAsync();
    }

    // Puts a guest's cart at a step of the checkout without recording: 1 is Contact, 2 is Shipping, 3 is Payment.
    private static async Task ReachAsync(Scene scene, int step)
    {
        var page = scene.Page;
        await page.GotoAsync(BaseUrl + "/shop", new() { WaitUntil = WaitUntilState.NetworkIdle });

        foreach (var index in new[] { 0, 2 })
        {
            await page.Locator("button:has-text('Add to cart')").Nth(index).ClickAsync();
            await page.WaitForTimeoutAsync(1500);
            await page.Keyboard.PressAsync("Escape");
            await page.WaitForTimeoutAsync(700);
        }

        await page.GotoAsync(BaseUrl + "/cart", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.RunAndWaitForNavigationAsync(() => CartCheckout(page).ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });

        if (step >= 2)
        {
            await page.GetByLabel("Full name").FillAsync("Jamie Chen");
            await page.GetByLabel("Email").FillAsync("customer@example.com");
            await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        }

        if (step >= 3)
        {
            await FillShippingAsync(page);
            await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
            await page.Locator("input[type=radio]").First.CheckAsync();
            await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        }

        await page.Mouse.MoveAsync(700, 500);
    }

    private static async Task FillShippingAsync(IPage page)
    {
        await page.Locator("#Shipping_Address_Country").SelectOptionAsync(new SelectOptionValue { Label = "United States" });
        await page.WaitForTimeoutAsync(1200);
        await page.Locator("#Shipping_Address_Name").FillAsync("Jamie Chen");
        await page.Locator("#Shipping_Address_AddressLine1").FillAsync("350 Fifth Avenue");
        await page.Locator("#Shipping_Address_City").FillAsync("New York");
        await page.Locator("#Shipping_Address_Region").SelectOptionAsync(new SelectOptionValue { Label = "New York" });
        await page.Locator("#Shipping_Address_PostalCode").FillAsync("10118");
    }

    private static ILocator Steps(Scene scene) => scene.Page.Locator(".checkout-stepper").First;

    // The Checkout button of the cart page's summary.
    private static ILocator CartCheckout(IPage page) => page.Locator(".card").Filter(new() { HasTextString = "Summary" }).Locator("a, button").Filter(new() { HasTextString = "Checkout" }).First;

    private static ILocator Summary(Scene scene) => scene.Page.Locator(".card").Filter(new() { HasTextString = "Order summary" }).First;

    private static async Task CartAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/shop");
        await scene.StartCaptureAsync();

        var add = scene.Page.Locator("button:has-text('Add to cart')");
        var miniCart = scene.Page.Locator("#mini-cart");

        await scene.StepAsync("cart-1", async () =>
        {
            await scene.HighlightAsync(add.Nth(0), "Add to cart");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(add.Nth(0), pauseAfter: 1500);
        });

        await scene.StepAsync("cart-2", async () =>
        {
            await ShowAsync(scene, miniCart.Locator("a, button").Filter(new() { HasTextString = "Remove" }).First, "Remove", 1800);
            await ShowAsync(scene, miniCart.Locator("a, button").Filter(new() { HasTextString = "Checkout" }).First, "Checkout", 1800);
            await ShowAsync(scene, miniCart.Locator("a, button").Filter(new() { HasTextString = "View cart" }).First, "View cart", 1800);
            await scene.HighlightAsync(scene.Page.Locator("a[href$='/cart']").First, "Cart");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("cart-3", async () =>
        {
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.PauseAsync(600);
            await scene.ClickAsync(add.Nth(2), pauseAfter: 1500);
            await scene.ClickAndWaitForNavigationAsync(miniCart.Locator("a").Filter(new() { HasTextString = "View cart" }).First);
        });

        await scene.StepAsync("cart-4", async () =>
        {
            await ShowAsync(scene, scene.Page.Locator(".card").Filter(new() { HasTextString = "Summary" }).First, "Summary", 2800);
            await scene.ClickAndWaitForNavigationAsync(CartCheckout(scene.Page));
        });
    }

    private static async Task ContactAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        await ReachAsync(scene, 1);
        await scene.StartCaptureAsync();

        await scene.StepAsync("con-1", async () =>
        {
            await ShowAsync(scene, Steps(scene), "Steps", 2800);
            await scene.HighlightAsync(Summary(scene), "Order summary");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("con-2", async () =>
        {
            await scene.TypeAsync(scene.Page.GetByLabel("Full name"), "Jamie Chen", delay: 55);
            await scene.TypeAsync(scene.Page.GetByLabel("Email"), "customer@example.com", delay: 35);
            await scene.HighlightAsync(scene.Page.GetByLabel("Phone (optional)"), "Optional");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("con-3", async () =>
        {
            await ShowAsync(scene, scene.Page.Locator("select").Filter(new() { HasTextString = "Choose a country" }).First, "Billing address", 2500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#checkout-submit"));
        });
    }

    private static async Task ShippingAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        await ReachAsync(scene, 2);
        await scene.StartCaptureAsync();

        await scene.StepAsync("ship-1", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#Shipping_Address_Country"), "United States", pauseAfter: 900);
            await scene.TypeAsync(scene.Page.Locator("#Shipping_Address_Name"), "Jamie Chen", delay: 35);
            await scene.TypeAsync(scene.Page.Locator("#Shipping_Address_AddressLine1"), "350 Fifth Avenue", delay: 25);
            await scene.TypeAsync(scene.Page.Locator("#Shipping_Address_City"), "New York", delay: 30);
            await scene.SelectAsync(scene.Page.Locator("#Shipping_Address_Region"), "New York", pauseAfter: 500);
            await scene.TypeAsync(scene.Page.Locator("#Shipping_Address_PostalCode"), "10118", delay: 60);
        });

        await scene.StepAsync("ship-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#checkout-submit"));
            await scene.HighlightAsync(scene.Page.Locator("h2, h3, h4, h5").Filter(new() { HasTextString = "Shipping method" }).First.Locator("xpath=.."), "Shipping method");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ship-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("input[type=radio]").First, pauseAfter: 900);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#checkout-submit"));
            await scene.HighlightAsync(scene.Page.Locator("li, tr, div").Filter(new() { HasTextString = "Shipping: Standard shipping" }).Last, "Shipping");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PaymentAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        await ReachAsync(scene, 3);
        await scene.StartCaptureAsync();

        await scene.StepAsync("pay-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("div").Filter(new() { HasTextString = "Payment method" }).Filter(new() { HasNotTextString = "Order summary" }).Last, "Payment method");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("pay-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#CheckoutFlow_Code"), "WELCOME10", delay: 70);
            await scene.ClickAsync(scene.Page.Locator("button.checkout-action, button:has-text('Apply')").First, pauseAfter: 1200);
        });

        await scene.StepAsync("pay-3", async () =>
        {
            await scene.HighlightAsync(Summary(scene), "Discount and Due now");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("pay-4", async () =>
        {
            await scene.PauseAsync(1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#checkout-submit"));
        });

        await scene.StepAsync("pay-5", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("div").Filter(new() { HasTextString = "Order number" }).Filter(new() { HasNotTextString = "Order summary" }).Last, "Order number");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".card").Filter(new() { HasTextString = "Order summary" }).First, "What was bought");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/checkout");
        await scene.StartCaptureAsync();

        await scene.StepAsync("set-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/Settings/checkout"), "Settings > Commerce > Checkout", 2400);
            await scene.HighlightAsync(Field(scene, "Checkout layout"), "Checkout layout");
            await scene.PauseAsync(1500);
            await scene.SelectAsync(scene.Page.Locator("[name='ISite.CheckoutSettings.Layout']"), "One page", pauseAfter: 1500);
            await scene.SelectAsync(scene.Page.Locator("[name='ISite.CheckoutSettings.Layout']"), "A page per step", pauseAfter: 600);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("set-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/Settings/customers"));
            await scene.HighlightAsync(Field(scene, "Billing address at checkout"), "Billing address at checkout");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("set-3", async () =>
        {
            await scene.NavigateAsync("/Admin/Features");
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "Checkout details", delay: 60);
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.Locator("li, tr").Filter(new() { HasTextString = "Checkout details" }).Last, "Checkout details");
        });

        await scene.ClearHighlightsAsync();
    }
}
