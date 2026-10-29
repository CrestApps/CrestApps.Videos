using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Store credit, loyalty points and referrals video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    // A throwaway account of the demo site, made up for the recording.
    public const string CustomerUser = "ana";

    public const string CustomerPassword = "Demo-Passw0rd!";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-credit", CreditAsync),
        ("04-refund", RefundAsync),
        ("05-spend", SpendAsync),
        ("06-points", PointsAsync),
        ("07-spend", PointsSpendAsync),
        ("08-setup", SetupAsync),
        ("09-refer", ReferAsync),
        ("09-friend", FriendAsync),
        ("09-reward", RewardAsync),
    ];

    /// <summary>
    /// Turns on Store credit, Loyalty points and Referrals, adds a customer with an account (Ana Rivera), a coupon for friends
    /// of referrals, and one paid order of Ana's (not recorded). The site is the one the Shipping video prepared: the shop
    /// with nine products and a zone Everywhere with Standard shipping, and the test payment provider.
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var page = await Seed.NewPageAsync(browser);

        await Seed.EnableAsync(page, "CrestApps.OrchardCore.Checkout.TestProvider", "CrestApps.OrchardCore.Customers.StoreCredit", "CrestApps.OrchardCore.Customers.Loyalty", "CrestApps.OrchardCore.Customers.Referrals");

        await page.GotoAsync(BaseUrl + "/Admin/customers", new() { WaitUntil = WaitUntilState.NetworkIdle });

        if (await page.GetByText("ana.rivera@example.com").CountAsync() == 0)
        {
            await Seed.SubmitAsync(page, "/Admin/Users/Create", new()
            {
                ["User.UserName"] = CustomerUser,
                ["User.Email"] = "ana.rivera@example.com",
                ["User.Password"] = CustomerPassword,
                ["User.PasswordConfirmation"] = CustomerPassword,
            }, "css:form button.btn-primary");

            await Seed.SubmitAsync(page, "/Admin/coupons/Create", new()
            {
                ["Code"] = "FRIEND15",
                ["OnlyGeneratedCodes"] = "true",
                ["Description"] = "15% off your first order",
                ["Kind"] = "Percentage",
                ["Percentage"] = "15",
            }, "css:form button.btn-primary");

            var context = await browser.NewContextAsync(new() { ViewportSize = new() { Width = 1920, Height = 1080 } });
            var buyer = await context.NewPageAsync();
            buyer.SetDefaultTimeout(90_000);
            await SignInAsync(buyer);
            await BuyAsync(buyer, "Leather Wallet", "1");
            await context.CloseAsync();
        }

        // The customer record starts with the user name: give it a name.
        await page.GotoAsync(BaseUrl + "/Admin/customers", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var href = await page.Locator("a").Filter(new() { HasTextString = "ana" }).Filter(new() { HasNotTextString = "Torres" }).First.GetAttributeAsync("href");

        if (await page.Locator("a", new() { HasTextString = "Ana Rivera" }).CountAsync() == 0)
        {
            await Seed.SubmitAsync(page, href, new()
            {
                ["FirstName"] = "Ana",
                ["LastName"] = "Rivera",
                ["DisplayName"] = "Ana Rivera",
            }, "css:form button.btn-primary");
        }

        await page.Context.CloseAsync();
    }

    private static async Task SignInAsync(IPage page)
    {
        await page.GotoAsync(BaseUrl + "/Login", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("[name='LoginForm.UserName']").FillAsync(CustomerUser);
        await page.Locator("[name='LoginForm.Password']").FillAsync(CustomerPassword);
        await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Last.ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
    }

    // A signed-in buyer's order with the test payment, off camera.
    private static async Task BuyAsync(IPage page, string product, string quantity)
    {
        await ToPaymentAsync(page, product, quantity);
        await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
    }

    // Puts a product in the cart and goes through the checkout steps up to the payment step.
    private static async Task ToPaymentAsync(IPage page, string product, string quantity, string name = "Ana Rivera", string email = "ana.rivera@example.com")
    {
        await page.GotoAsync(BaseUrl + "/shop", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var card = page.Locator(".card").Filter(new() { HasTextString = product }).First;
        await card.Locator("input[type=number]").FillAsync(quantity);
        await card.Locator("button:has-text('Add to cart')").ClickAsync();
        await page.WaitForTimeoutAsync(1500);
        await page.GotoAsync(BaseUrl + "/cart", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.RunAndWaitForNavigationAsync(() => page.Locator(".card").Filter(new() { HasTextString = "Summary" }).Locator("a, button").Filter(new() { HasTextString = "Checkout" }).First.ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });

        for (var step = 0; step < 3; step++)
        {
            if (await page.Locator("#checkout-submit").CountAsync() == 0 || (await page.Locator("#checkout-submit").InnerTextAsync()).Contains("Pay", StringComparison.OrdinalIgnoreCase) || (await page.Locator("#checkout-submit").InnerTextAsync()).Contains("Place", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (await page.GetByLabel("Full name").CountAsync() > 0 && await page.GetByLabel("Full name").IsVisibleAsync())
            {
                if (string.IsNullOrEmpty(await page.GetByLabel("Full name").InputValueAsync()))
                {
                    await page.GetByLabel("Full name").FillAsync(name);
                }

                if (await page.GetByLabel("Email").CountAsync() > 0 && string.IsNullOrEmpty(await page.GetByLabel("Email").First.InputValueAsync()))
                {
                    await page.GetByLabel("Email").First.FillAsync(email);
                }
            }

            if (await page.Locator("#Shipping_Address_Country").CountAsync() > 0 && await page.Locator("#Shipping_Address_Country").IsVisibleAsync())
            {
                await page.Locator("#Shipping_Address_Country").SelectOptionAsync(new SelectOptionValue { Label = "United States" });
                await page.WaitForTimeoutAsync(1200);
                await page.Locator("#Shipping_Address_Name").FillAsync(name);
                await page.Locator("#Shipping_Address_AddressLine1").FillAsync("350 Fifth Avenue");
                await page.Locator("#Shipping_Address_City").FillAsync("New York");
                await page.Locator("#Shipping_Address_Region").SelectOptionAsync(new SelectOptionValue { Label = "New York" });
                await page.Locator("#Shipping_Address_PostalCode").FillAsync("10118");
            }

            if (await page.Locator("input[type=radio]").CountAsync() > 0 && await page.Locator("input[type=radio]").First.IsVisibleAsync() && await page.Locator("input[type=radio]:checked").CountAsync() == 0)
            {
                await page.Locator("input[type=radio]").First.CheckAsync();
            }

            await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        }
    }
}
