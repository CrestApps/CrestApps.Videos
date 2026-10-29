using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator LinkNamed(Scene scene, string name) => scene.Page.GetByRole(AriaRole.Link, new() { Name = name, Exact = true }).First;

    private static ILocator Field(Scene scene, string id) => scene.Page.Locator("#" + id);

    private static Task ClearAsync(Scene scene) => scene.ClearHighlightsAsync();

    private static string State(string user) => Path.Combine(Root, $"state-{user}.json");

    // ----- Clips -----

    private static async Task TermsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/companies");
        await scene.StartCaptureAsync();

        await scene.StepAsync("terms-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/companies"), "Commerce > Companies");
            await scene.PauseAsync(2200);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Northwind Traders"));
            await scene.HighlightAsync(Field(scene, "Buyers_0__User"), "Admin");
            await scene.PauseAsync(1800);
            await ClearAsync(scene);
            await scene.HighlightAsync(Field(scene, "Buyers_1__User"), "Buyer");
        });

        await ClearAsync(scene);

        await scene.StepAsync("terms-2", async () =>
        {
            await Field(scene, "Buyers_1__OrderLimit").FillAsync(string.Empty);
            await scene.TypeAsync(Field(scene, "Buyers_1__OrderLimit"), "300", delay: 90);
            await scene.HighlightAsync(Field(scene, "Buyers_1__OrderLimit"), "Order limit");
        });

        await ClearAsync(scene);

        await scene.StepAsync("terms-3", async () =>
        {
            await scene.HighlightAsync(Field(scene, "NetDays"), "Net days");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await Field(scene, "DepositPercent").FillAsync(string.Empty);
            await scene.TypeAsync(Field(scene, "DepositPercent"), "20", delay: 90);
            await scene.HighlightAsync(Field(scene, "DepositPercent"), "Deposit (%)");
            await scene.PauseAsync(2200);
            await ClearAsync(scene);
            await Field(scene, "CreditLimit").FillAsync(string.Empty);
            await scene.TypeAsync(Field(scene, "CreditLimit"), "5000", delay: 90);
            await scene.HighlightAsync(Field(scene, "CreditLimit"), "Credit limit");
        });

        await ClearAsync(scene);

        await scene.StepAsync("terms-4", async () =>
        {
            await Field(scene, "MonthlyBudget").FillAsync(string.Empty);
            await scene.TypeAsync(Field(scene, "MonthlyBudget"), "2000", delay: 90);
            await scene.HighlightAsync(Field(scene, "MonthlyBudget"), "Monthly budget");
            await scene.PauseAsync(2200);
            await ClearAsync(scene);

            if (!await Field(scene, "RequirePurchaseOrder").IsCheckedAsync())
            {
                await scene.ClickAsync(Field(scene, "RequirePurchaseOrder"), pauseAfter: 600);
            }
            else
            {
                await scene.HighlightAsync(Field(scene, "RequirePurchaseOrder"), "Purchase order required");
                await scene.PauseAsync(1500);
            }
        });

        await scene.StepAsync("terms-5", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Save(scene.Page));
        });
    }

    // Fills the Contact and Shipping steps as a buyer would, quickly (the narration says "as usual").
    private static async Task ContactAndShippingAsync(Scene scene)
    {
        var page = scene.Page;
        await page.Locator("#Contact_Name").FillAsync("Morgan Lee");
        await page.Locator("#Contact_Email").FillAsync("morgan.lee@example.com");
        await scene.ClickAndWaitForNavigationAsync(page.Locator("#checkout-submit"));

        await page.Locator("#Shipping_Address_Country").SelectOptionAsync("US");
        await Task.Delay(1000);
        await page.Locator("#Shipping_Address_Name").FillAsync("Morgan Lee");
        await page.Locator("#Shipping_Address_AddressLine1").FillAsync("1 Pioneer Square");
        await page.Locator("#Shipping_Address_City").FillAsync("Seattle");
        await page.Locator("#Shipping_Address_Region").SelectOptionAsync("WA");
        await page.Locator("#Shipping_Address_PostalCode").FillAsync("98104");
        await scene.ClickAndWaitForNavigationAsync(page.Locator("#checkout-submit"));
        await page.Locator("label[for^='Shipping_RateId']").First.ClickAsync();
        await scene.ClickAndWaitForNavigationAsync(page.Locator("#checkout-submit"));
    }

    private static async Task BuyerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, State("morgan"));

        await scene.OpenAsync("/shop");
        await scene.StartCaptureAsync();

        await scene.StepAsync("buyer-1", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Add to cart" }).First, pauseAfter: 1500);
            await scene.NavigateAsync("/cart");
            await scene.Page.Locator("input[type=number]").First.FillAsync(string.Empty);
            await scene.TypeAsync(scene.Page.Locator("input[type=number]").First, "3", delay: 90);
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Update" }).First, pauseAfter: 2500);
            await scene.HighlightAsync(scene.Page.GetByText("Subtotal (3 items)"), "Over the order limit of $300");
        });

        await ClearAsync(scene);

        await scene.StepAsync("buyer-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Checkout" }).First);
            await ContactAndShippingAsync(scene);
        });

        await scene.StepAsync("buyer-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("You're ordering for Northwind Traders."), "Ordering for the company");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("A company admin has to approve this order first"), "Why it needs approval");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await scene.TypeAsync(Field(scene, "CheckoutFlow_PurchaseOrderNumber"), "PO-2026-0412", delay: 70);
            await scene.HighlightAsync(Field(scene, "CheckoutFlow_PurchaseOrderNumber"), "Purchase order number");
        });

        await ClearAsync(scene);

        await scene.StepAsync("buyer-4", async () =>
        {
            await scene.ClickAsync(Field(scene, "CheckoutFlow_RequestApproval"), pauseAfter: 900);
            await scene.ClickAndWaitForNavigationAsync(Field(scene, "checkout-submit"));
            await scene.HighlightAsync(scene.Page.GetByText("This order is waiting for a company admin's approval"), "Waiting for approval");
        });

        await ClearAsync(scene);

        await scene.StepAsync("buyer-5", async () =>
        {
            await scene.NavigateAsync("/account/orders");
            await scene.PauseAsync(1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='/account/orders/']").First);
            await scene.HighlightAsync(scene.Page.GetByText("Waiting for approval: it's over your order limit"), "Waiting for approval");
        });
    }

    private static async Task ApproveAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, State("riley"));

        await scene.OpenAsync("/account/company");
        await scene.StartCaptureAsync();

        await scene.StepAsync("approve-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("1 order waiting for your approval"), "Waiting for your approval");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("morgan asked"), "Who asked, and why");
        });

        await ClearAsync(scene);

        await scene.StepAsync("approve-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("input[name=note]"), "Approved: it is for the autumn restock.", delay: 45);
            await scene.HighlightAsync(scene.Page.Locator("input[name=note]"), "A note for the buyer");
            await scene.PauseAsync(1500);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Reject" }), "Reject cancels the order");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Approve" }));
        });

        await scene.StepAsync("approve-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Monthly budget"), "The budget, and what is spent");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("The company's orders"), "Every order, with its purchase order");
        });
    }

    private static async Task PayAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, State("morgan"));

        await scene.OpenAsync("/account/orders");
        await scene.StartCaptureAsync();

        await scene.StepAsync("pay-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='/account/orders/']").First);
            await scene.HighlightAsync(scene.Page.GetByText("approved this order"), "Approved: pay it to place it");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Pay now" }));
        });

        await scene.StepAsync("pay-2", async () =>
        {
            await scene.TypeAsync(Field(scene, "CheckoutFlow_PurchaseOrderNumber"), "PO-2026-0412", delay: 70);
            await scene.ClickAndWaitForNavigationAsync(Field(scene, "checkout-submit"));
        });

        await scene.StepAsync("pay-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Pay on account").First, "Pay on account");
            await scene.PauseAsync(4500);
            await ClearAsync(scene);
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Pay on account" }), pauseAfter: 2500);
            await scene.HighlightAsync(scene.Page.GetByText("On account, net 30"), "On account, net 30");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("Due now").First, "Due now: the deposit");
        });

        await ClearAsync(scene);

        await scene.StepAsync("pay-4", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByText("Bank transfer", new() { Exact = true }).First, pauseAfter: 1200);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Pay now" }));
            await scene.PauseAsync(2500);
        });

        await scene.StepAsync("pay-5", async () =>
        {
            await scene.NavigateAsync("/account/company");
            await scene.HighlightAsync(scene.Page.GetByText("Owed on account"), "Owed on account");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.NavigateAsync("/my-transactions");
        });
    }

    private static async Task OwedAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/transactions");
        await scene.StartCaptureAsync();

        await scene.StepAsync("owed-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/transactions"), "Commerce > Transactions");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = "Bank transfer" }).First, "The deposit");
            await scene.PauseAsync(4500);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = "company-terms" }).First, "The rest, on account");
        });

        await ClearAsync(scene);

        await scene.StepAsync("owed-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = "company-terms" }).First.GetByRole(AriaRole.Link, new() { Name = "Manage" }));
            await scene.HighlightAsync(scene.Page.GetByText("Put on Northwind Traders's account"), "On the company's account");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await Field(scene, "Amount").FillAsync(string.Empty);
            await scene.TypeAsync(Field(scene, "Amount"), "316", delay: 90);
            await scene.TypeAsync(Field(scene, "Reference"), "Northwind payment", delay: 50);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Record payment" }), "Record payment");
        });

        await ClearAsync(scene);

        await scene.StepAsync("owed-3", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Record payment" }), pauseAfter: 2500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalOkButton"));
            await scene.PauseAsync(1000);
            await scene.HighlightAsync(scene.Page.GetByText("Outstanding").First, "Paid, and what is outstanding");
        });
    }

    private static async Task QuoteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, State("morgan"));

        await scene.OpenAsync("/shop");
        await scene.StartCaptureAsync();

        await scene.StepAsync("quote-1", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Add to cart" }).First, pauseAfter: 1500);
            await scene.NavigateAsync("/cart");
            await scene.Page.Locator("input[type=number]").First.FillAsync(string.Empty);
            await scene.TypeAsync(scene.Page.Locator("input[type=number]").First, "20", delay: 90);
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Update" }).First, pauseAfter: 2500);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Request a quote" }), "Request a quote");
            await scene.PauseAsync(1500);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Request a quote" }));
        });

        await scene.StepAsync("quote-2", async () =>
        {
            await scene.TypeAsync(Field(scene, "QuoteMessage"), "We need twenty of these by the end of November. Can you do better on the price?", delay: 30);
            await scene.TypeAsync(Field(scene, "QuotePurchaseOrderNumber"), "PO-2026-0450", delay: 70);
            await scene.HighlightAsync(Field(scene, "QuotePurchaseOrderNumber"), "Purchase order number");
            await scene.PauseAsync(2000);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Send the request" }));
            await scene.NavigateAsync("/account/orders");
            await scene.HighlightAsync(scene.Page.Locator("a[href*='/account/orders/']").First, "Asked for");
        });
    }
}
