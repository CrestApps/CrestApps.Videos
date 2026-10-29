using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Orders video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-list", ListAsync),
        ("04-order", OrderAsync),
        ("05-manage", ManageAsync),
        ("06-create", CreateAsync),
        ("07-returns", ReturnsAsync),
        ("08-gift", GiftAsync),
        ("09-settings", SettingsAsync),
    ];

    // The made-up buyers of the demo store: name, email, what they buy (title and quantity), and how they pay.
    private static readonly (string Name, string Email, (string Product, int Quantity)[] Items, string Payment)[] s_orders =
    [
        ("Jamie Chen", "customer@example.com", [("Trail Runner Shoe", 1), ("Wool Beanie", 1)], "Test payment"),
        ("Priya Shah", "priya.shah@example.com", [("Canvas Backpack", 1)], "Bank transfer"),
        ("Marcus Lee", "marcus.lee@example.com", [("Wireless Earbuds", 2)], "Test payment"),
        ("Ana Torres", "ana.torres@example.com", [("Desk Lamp", 1), ("Hardcover Notebook", 2)], "Test payment"),
    ];

    /// <summary>
    /// Turns on the test and bank transfer payment methods, a shipping zone and the order features, and places four orders (not recorded).
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var page = await Seed.NewPageAsync(browser);

        await Seed.EnableAsync(page, "CrestApps.OrchardCore.Checkout.TestProvider", "CrestApps.OrchardCore.Orders.Returns", "CrestApps.OrchardCore.Orders.GiftCards");

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

        await Seed.SubmitAsync(
            page,
            "/Admin/Settings/paylater",
            new()
            {
                ["ISite.BankTransferSettings.Enabled"] = "true",
                ["ISite.BankTransferSettings.Instructions"] = "Contoso Ltd, account 0000 1111 2222, reference: your order number.",
                ["ISite.BankTransferSettings.NetTermDays"] = "7",
            },
            "css:button.btn-primary[type=submit]");

        await page.Context.CloseAsync();

        foreach (var order in s_orders)
        {
            await PlaceOrderAsync(browser, order);
        }
    }

    private static async Task PlaceOrderAsync(IBrowser browser, (string Name, string Email, (string Product, int Quantity)[] Items, string Payment) order)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        await page.GotoAsync(BaseUrl + "/shop", new() { WaitUntil = WaitUntilState.NetworkIdle });

        foreach (var (product, quantity) in order.Items)
        {
            var card = page.Locator(".card").Filter(new() { HasTextString = product }).First;
            await card.Locator("input[name='quantity']").FillAsync(quantity.ToString());
            await card.Locator("button:has-text('Add to cart')").ClickAsync();
            await page.WaitForTimeoutAsync(1500);
            await page.Keyboard.PressAsync("Escape");
            await page.WaitForTimeoutAsync(700);
        }

        await page.GotoAsync(BaseUrl + "/cart", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.RunAndWaitForNavigationAsync(() => page.Locator(".card").Filter(new() { HasTextString = "Summary" }).Locator("a, button").Filter(new() { HasTextString = "Checkout" }).First.ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });

        await page.GetByLabel("Full name").FillAsync(order.Name);
        await page.GetByLabel("Email").FillAsync(order.Email);
        await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });

        await page.Locator("#Shipping_Address_Country").SelectOptionAsync(new SelectOptionValue { Label = "United States" });
        await page.WaitForTimeoutAsync(1200);
        await page.Locator("#Shipping_Address_Name").FillAsync(order.Name);
        await page.Locator("#Shipping_Address_AddressLine1").FillAsync("350 Fifth Avenue");
        await page.Locator("#Shipping_Address_City").FillAsync("New York");
        await page.Locator("#Shipping_Address_Region").SelectOptionAsync(new SelectOptionValue { Label = "New York" });
        await page.Locator("#Shipping_Address_PostalCode").FillAsync("10118");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("input[type=radio]").First.CheckAsync();
        await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });

        await page.GetByText(order.Payment, new() { Exact = false }).First.ClickAsync();
        await page.WaitForTimeoutAsync(800);
        await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });

        Console.WriteLine($"Placed the order of {order.Name}: {page.Url}");
        await context.CloseAsync();
    }
}
