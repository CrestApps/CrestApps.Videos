using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Returns video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-settings", SettingsAsync),
        ("04-ask", AskAsync),
        ("05-handle", HandleAsync),
        ("06-exchange", ExchangeAsync),
    ];

    /// <summary>
    /// Turns on the Returns feature and the test payment provider (not recorded). The site is the one the Shipping video
    /// prepared: the shop with nine products and a zone Everywhere with Standard shipping. The settings are made on camera;
    /// the buyer's orders and requests are made with the shop's own pages.
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var page = await Seed.NewPageAsync(browser);

        await Seed.EnableAsync(page, "CrestApps.OrchardCore.Checkout.TestProvider", "CrestApps.OrchardCore.Orders.Returns");
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

    private static async Task SaveAsync(Scene scene, ILocator button)
    {
        await scene.ClickAsync(button, pauseAfter: 500);
        await scene.OffCameraAsync(() => scene.Page.WaitForTimeoutAsync(5000));
    }

    // A guest buys something with the test payment, and ends on the order's page (the "View your order" link).
    private static async Task GuestOrderAsync(IPage page, string product, string quantity)
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
        await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        // The link carries the site's configured address; follow its path on the demo site.
        var href = await page.GetByRole(AriaRole.Link, new() { Name = "View your order" }).GetAttributeAsync("href");
        var path = Uri.TryCreate(href, UriKind.Absolute, out var uri) ? uri.PathAndQuery : href;
        await page.GotoAsync(BaseUrl + path, new() { WaitUntil = WaitUntilState.NetworkIdle });
    }

    // From the order's page: opens the return form and fills it in. Does not send it.
    private static async Task FillReturnAsync(IPage page, bool exchange, string exchangeFor = "")
    {
        await page.RunAndWaitForNavigationAsync(() => page.GetByRole(AriaRole.Link, new() { Name = "Return items" }).ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#return-quantity-0").SelectOptionAsync("1");
        await page.Locator("#return-reason-0").SelectOptionAsync(new SelectOptionValue { Index = 1 });

        if (exchange)
        {
            await page.Locator("#return-exchange").CheckAsync();
            await page.Locator("#return-exchange-for").FillAsync(exchangeFor);
        }

        await page.Locator("#return-comment").FillAsync(exchange ? "The first one is too small." : "It does not fit me.");
    }

    // A buyer's request that the clips of the admin then handle, made off camera in a browser of its own.
    private static async Task BuyerAsksAsync(IBrowser browser, string product, bool exchange, string exchangeFor = "")
    {
        var context = await browser.NewContextAsync(new() { ViewportSize = new() { Width = 1920, Height = 1080 } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(90_000);
        await GuestOrderAsync(page, product, "2");
        await FillReturnAsync(page, exchange, exchangeFor);
        await page.RunAndWaitForNavigationAsync(() => page.GetByRole(AriaRole.Button, new() { Name = "Ask to return" }).ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        await context.CloseAsync();
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/returns");
        await scene.StartCaptureAsync();

        await scene.StepAsync("set-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/Settings/returns"), "Settings > Commerce > Returns", 2200);
            await scene.HighlightAsync(Field(scene, "Return window"), "Return window (days)");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("set-2", async () =>
        {
            await scene.HighlightAsync(Field(scene, "Reasons"), "Reasons, one for each line");
            await scene.PauseAsync(2800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "Let buyers ask for something else"), "Exchanges");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("set-3", async () =>
        {
            await scene.TypeAsync(Fld(scene, "ISite.ReturnSettings.Instructions"), "Send the items to 12 Harbor Road, Portland, and put the return number in the parcel.", delay: 25);
            await scene.HighlightAsync(Field(scene, "Return instructions"), "Return instructions");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene, SaveButton(scene));
            await scene.PauseAsync(1500);
        });
    }

    private static async Task AskAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        var page = scene.Page;
        await GuestOrderAsync(page, "Leather Wallet", "2");
        await page.Mouse.MoveAsync(700, 500);

        await scene.StartCaptureAsync();

        await scene.StepAsync("ask-1", async () =>
        {
            await scene.HighlightAsync(page.Locator(".card, div").Filter(new() { HasTextString = "Returns" }).Filter(new() { HasTextString = "Return items" }).Last, "The Returns box");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(page.GetByRole(AriaRole.Link, new() { Name = "Return items" }));
        });

        await scene.StepAsync("ask-2", async () =>
        {
            await scene.HighlightAsync(page.Locator("#return-quantity-0").Locator("xpath=ancestor::*[contains(@class,'card') or contains(@class,'row')][1]"), "How many, and why");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await page.Locator("#return-quantity-0").SelectOptionAsync("1");
            await scene.PauseAsync(800);
            await page.Locator("#return-reason-0").SelectOptionAsync(new SelectOptionValue { Index = 1 });
            await scene.PauseAsync(1200);
        });

        await scene.StepAsync("ask-3", async () =>
        {
            await scene.HighlightAsync(page.Locator("fieldset").First, "My money back, or something else");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(page.Locator("#return-comment"), "It does not fit me.", delay: 45);
            await scene.ClickAsync(page.GetByRole(AriaRole.Button, new() { Name = "Ask to return" }), pauseAfter: 500);
            await scene.OffCameraAsync(() => page.WaitForLoadStateAsync(LoadState.NetworkIdle));
        });

        await scene.StepAsync("ask-4", async () =>
        {
            await scene.HighlightAsync(page.Locator("div").Filter(new() { HasTextString = "Requested" }).Last, "Requested");
            await scene.PauseAsync(3500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task HandleAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/returns");
        await scene.StartCaptureAsync();

        await scene.StepAsync("hnd-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/returns"), "Commerce > Returns", 2200);
            await scene.HighlightAsync(Row(scene, "Requested"), "A requested return");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Requested").Locator("a").First);
            await scene.HighlightAsync(scene.Page.Locator("table").First, "What is coming back");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("hnd-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#approve-message"), "Thank you. We are sorry it did not fit.", delay: 40);
            await SaveAsync(scene, scene.Page.Locator("button").Filter(new() { HasTextString = "Approve" }).First);
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("hnd-3", async () =>
        {
            var received = scene.Page.Locator("div.card").Filter(new() { HasTextString = "The items arrived" }).First;
            await received.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(received, "The items arrived");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene, scene.Page.Locator("button").Filter(new() { HasTextString = "Mark received" }).First);
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("hnd-4", async () =>
        {
            var refund = scene.Page.Locator("div.card").Filter(new() { HasTextString = "Refund and close" }).First;
            await refund.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(refund, "Refund");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Refund and close" }).First, pauseAfter: 1500);
            var confirm = scene.Page.Locator(".modal.show .btn-danger, .modal.show .btn-primary").First;

            try
            {
                await confirm.WaitForAsync(new() { Timeout = 2500 });
                await scene.ClickAsync(confirm, pauseAfter: 500);
            }
            catch (TimeoutException)
            {
                // No confirmation was asked for: the refund went ahead.
            }

            await scene.OffCameraAsync(() => scene.Page.WaitForTimeoutAsync(6000));
            await scene.PauseAsync(2500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ExchangeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        // A second buyer asks for an exchange, off camera.
        await BuyerAsksAsync(browser, "Ceramic Mug", exchange: true, exchangeFor: "The same mug in a larger size");

        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/returns");
        await scene.Page.RunAndWaitForNavigationAsync(() => Row(scene, "Requested").Locator("a").First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("exc-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("dl").First, "What the buyer wants");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            var decline = scene.Page.Locator("div.card").Filter(new() { HasTextString = "Decline the return" }).First;
            await decline.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(decline, "Decline");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("exc-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#approve-message"), "Thank you. A larger one is on its way.", delay: 40);
            await SaveAsync(scene, scene.Page.Locator("button").Filter(new() { HasTextString = "Approve" }).First);
            var exchange = scene.Page.Locator("div.card").Filter(new() { HasTextString = "The replacement is on its way" }).First;
            await exchange.ScrollIntoViewIfNeededAsync();
            await scene.TypeAsync(scene.Page.Locator("#exchange-message"), "Tracking number 1Z999AA10123456784", delay: 40);
            await scene.HighlightAsync(exchange, "Exchange");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene, scene.Page.Locator("button").Filter(new() { HasTextString = "The replacement is on its way" }).First);
            await scene.PauseAsync(2500);
        });

        await scene.ClearHighlightsAsync();
    }
}
