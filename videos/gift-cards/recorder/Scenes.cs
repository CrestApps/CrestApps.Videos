using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Gift cards video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-sell", SellAsync),
        ("04-issue", IssueAsync),
        ("05-card", CardAsync),
        ("06-buy", BuyAsync),
        ("07-pay", PayAsync),
        ("08-list", ListAsync),
    ];

    /// <summary>
    /// Turns on the test payment provider (not recorded). The site is the one the Shipping video prepared: the shop with nine
    /// products and a zone Everywhere with Standard shipping, and the Gift cards feature the Orders video turned on.
    /// The gift card product and the cards are made on camera.
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var page = await Seed.NewPageAsync(browser);

        await Seed.EnableAsync(page, "CrestApps.OrchardCore.Checkout.TestProvider", "CrestApps.OrchardCore.Orders.GiftCards");
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

    // Clicks a button that saves, then waits out of sight: a slow save can outlast the network-idle wait.
    private static async Task SaveAsync(Scene scene, ILocator button)
    {
        await scene.ClickAsync(button, pauseAfter: 500);
        await scene.OffCameraAsync(() => scene.Page.WaitForTimeoutAsync(6000));
    }

    // A guest buys what is in the cart, up to the payment step, off camera.
    private static async Task GuestToPaymentAsync(IPage page, string product, string quantity = "1")
    {
        await page.GotoAsync(BaseUrl + "/shop", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var card = page.Locator(".card").Filter(new() { HasTextString = product }).First;
        await card.Locator("input[type=number]").FillAsync(quantity);
        await card.Locator("button:has-text('Add to cart')").ClickAsync();
        await page.WaitForTimeoutAsync(1500);
        await page.GotoAsync(BaseUrl + "/cart", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.RunAndWaitForNavigationAsync(() => page.Locator(".card").Filter(new() { HasTextString = "Summary" }).Locator("a, button").Filter(new() { HasTextString = "Checkout" }).First.ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.GetByLabel("Full name").FillAsync("Jamie Chen");
        await page.GetByLabel("Email").FillAsync("customer@example.com");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });

        if (await page.Locator("#Shipping_Address_Country").CountAsync() > 0)
        {
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
        }

        await page.Mouse.MoveAsync(700, 500);
    }

    private static async Task SellAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();

        await scene.StepAsync("sell-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/Contents/ContentItems"), "Content > Content Items", 2200);
            await scene.NavigateAsync("/Admin/Contents/ContentTypes/GiftCard/Create");
        });

        await scene.StepAsync("sell-2", async () =>
        {
            await scene.TypeAsync(Fld(scene, "TitlePart.Title"), "Gift card $50", delay: 60);
            await ReplaceAsync(scene, Fld(scene, "ProductPart.Price"), "50", 120);
            await scene.HighlightAsync(Field(scene, "Price"), "Price: what each card holds");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene, Publish(scene));
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("sell-3", async () =>
        {
            await scene.NavigateAsync("/Admin/Contents/ContentItems/ProductListing");
            await scene.Page.RunAndWaitForNavigationAsync(() => Row(scene, "Shop").Locator("a:has-text('Edit')").First.ClickAsync());
            var giftCard = scene.Page.Locator("input[name='ProductListingPart.SelectedContentTypes'][value='GiftCard']");
            await giftCard.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(giftCard.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper') or contains(@class,'mb-3') or contains(@class,'form-group')][1]"), "The product types the Shop lists");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(giftCard, pauseAfter: 1200);
            await SaveAsync(scene, Publish(scene));
            await scene.PauseAsync(1500);
        });
    }

    private static async Task IssueAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/gift-cards");
        await scene.StartCaptureAsync();

        await scene.StepAsync("iss-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/gift-cards"), "Commerce > Gift cards", 2000);
            await scene.ClickAndWaitForNavigationAsync(Link(scene, "Issue gift card"));
            await ReplaceAsync(scene, Fld(scene, "Value"), "25", 120);
            await scene.HighlightAsync(Field(scene, "Currency"), "Currency");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("iss-2", async () =>
        {
            await scene.HighlightAsync(Field(scene, "Code"), "Code: optional");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(Fld(scene, "Code"), "CONTOSO25", delay: 80);
        });

        await scene.StepAsync("iss-3", async () =>
        {
            await scene.TypeAsync(Fld(scene, "RecipientName"), "Alex Rivera", delay: 50);
            await scene.TypeAsync(Fld(scene, "RecipientEmail"), "alex@example.com", delay: 35);
            await ScrollAsync(scene, 0, 400);
            await scene.ClickAsync(Fld(scene, "ExpiresUtc"), pauseAfter: 300);
            await Fld(scene, "ExpiresUtc").FillAsync("2027-12-31T23:59");
            await scene.TypeAsync(Fld(scene, "Message"), "Thank you for being a customer!", delay: 30);
            await scene.TypeAsync(Fld(scene, "Note"), "Goodwill for a late delivery", delay: 30);
        });

        await scene.StepAsync("iss-4", async () =>
        {
            await ScrollAsync(scene, 0, 600);
            await scene.HighlightAsync(Field(scene, "Email the card"), "Email the card");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Fld(scene, "SendEmail"), pauseAfter: 900);
            await SaveAsync(scene, scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Issue" }).Last);
            await scene.PauseAsync(2500);
        });
    }

    private static async Task CardAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/gift-cards");
        await scene.Page.RunAndWaitForNavigationAsync(() => Row(scene, "Alex Rivera").Locator("a").First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("crd-1", async () =>
        {
            await scene.PauseAsync(2500);
            await scene.HighlightAsync(Field(scene, "Enabled"), "Enabled");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("crd-2", async () =>
        {
            var amount = scene.Page.Locator("#adjust-amount");
            await amount.ScrollIntoViewIfNeededAsync();
            await ReplaceAsync(scene, amount, "10", 120);
            await scene.TypeAsync(scene.Page.Locator("#adjust-reason"), "Goodwill top-up", delay: 50);
            await scene.HighlightAsync(scene.Page.Locator(".card").Filter(new() { HasTextString = "Change the balance" }).First, "Change the balance");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Change balance" }).First, pauseAfter: 500);
            await scene.OffCameraAsync(() => scene.Page.WaitForTimeoutAsync(4000));
        });

        await scene.StepAsync("crd-3", async () =>
        {
            var activity = scene.Page.Locator("table").Last;
            await activity.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(activity, "Activity");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".card").Filter(new() { HasTextString = "Email the card" }).First, "Email the card again");
            await scene.PauseAsync(2500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task BuyAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/shop");
        await scene.StartCaptureAsync();

        await scene.StepAsync("buy-1", async () =>
        {
            await ScrollAsync(scene, 0, 250);
            await scene.HighlightAsync(scene.Page.Locator(".card").Filter(new() { HasTextString = "Gift card" }).First, "A gift card in the shop");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("buy-2", async () =>
        {
            await GuestToPaymentAsync(scene.Page, "Gift card");
            await scene.Page.RunAndWaitForNavigationAsync(() => scene.Page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
            await scene.Page.Mouse.MoveAsync(700, 500);
            await scene.HighlightAsync(Summary(scene), "One card for each one bought");
            await scene.PauseAsync(3500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PayAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        await GuestToPaymentAsync(scene.Page, "Canvas Backpack");

        await scene.StartCaptureAsync();

        var page = scene.Page;
        var box = page.Locator(".card").Filter(new() { HasTextString = "Gift card" }).Last;

        await scene.StepAsync("pay-1", async () =>
        {
            await scene.HighlightAsync(box, "Gift card");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(box.Locator("input").First, "CONTOSO25", delay: 80);
            await scene.ClickAsync(box.Locator("button:has-text('Apply')").First, pauseAfter: 3000);
        });

        await scene.StepAsync("pay-2", async () =>
        {
            await scene.HighlightAsync(Summary(scene), "What is still due now");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(box, "The card, what it pays and what is left");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("pay-3", async () =>
        {
            await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
            await page.Mouse.MoveAsync(700, 500);
            await scene.HighlightAsync(Summary(scene), "Paid with gift card");
            await scene.PauseAsync(3000);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/gift-cards");
        await scene.StartCaptureAsync();

        await scene.StepAsync("lst-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("input[type=search], input[name*='earch' i]").First, "Search by code or email");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(scene, "Alex Rivera"), "Last four characters only");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("lst-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button:has-text('Any status'), a:has-text('Any status'), select").First, pauseAfter: 2500);
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.HighlightAsync(scene.Page.Locator("li, tr").Filter(new() { HasTextString = "Bought with order" }).First, "Bought with an order");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("lst-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Alex Rivera").Locator("a").First);
            var activity = scene.Page.Locator("table").Last;
            await activity.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(activity, "Activity");
            await scene.PauseAsync(3500);
        });

        await scene.ClearHighlightsAsync();
    }
}
