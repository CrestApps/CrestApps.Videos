using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Orders, quotes and disputes video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-settings", SettingsAsync),
        ("04-ask", AskAsync),
        ("05-quote", QuoteAsync),
        ("05b-accept", AcceptAsync),
        ("06-add", AddItemsAsync),
        ("07-export", ExportAsync),
    ];

    /// <summary>
    /// Adds a buyer with an account (not recorded). The site is the one the Orders video prepared: a shop with fake orders.
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var page = await Seed.NewPageAsync(browser);

        await Seed.EnableAsync(page, "OrchardCore.Email.Smtp");

        // Email goes to a small mail catcher on this computer (smtpsink.py next to storyboard.json, port 2525), so the demo store can "send" a quote and nothing leaves it.
        await page.GotoAsync(BaseUrl + "/Admin/Settings/email", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.EvaluateAsync("() => { const set = (id, value) => { const e = document.getElementById(id); e.value = value; e.dispatchEvent(new Event('change', { bubbles: true })); }; const tick = (id, on) => { const e = document.getElementById(id); if (e.checked !== on) { e.click(); } }; tick('ISite_SmtpSettings_IsEnabled', true); set('ISite_SmtpSettings_DefaultSender', 'store@contoso.example'); set('ISite_SmtpSettings_DeliveryMethod', 'Network'); set('ISite_SmtpSettings_Host', 'localhost'); set('ISite_SmtpSettings_Port', '2525'); tick('ISite_SmtpSettings_AutoSelectEncryption', false); set('ISite_SmtpSettings_EncryptionMethod', 'None'); tick('ISite_SmtpSettings_RequireCredentials', false); tick('ISite_SmtpSettings_UseDefaultCredentials', false); tick('ISite_SmtpSettings_IgnoreInvalidSslCertificate', false); }");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("button:has-text('Save')").First.ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });

        try
        {
            await Seed.SubmitAsync(
            page,
            "/Admin/Users/Create",
            new() { ["User.UserName"] = "jamie", ["User.Email"] = "jamie.chen@example.com", ["User.Password"] = "Password1!", ["User.PasswordConfirmation"] = "Password1!" },
            "css:button.btn-primary[type=submit]");
        }
        catch (InvalidOperationException)
        {
        }

        await page.Context.CloseAsync();
    }

    private static async Task WaitAsync(Scene scene, int millis = 4000)
        => await scene.OffCameraAsync(() => scene.Page.WaitForTimeoutAsync(millis));

    private static async Task PickAsync(Scene scene, ILocator select, string contains, int pauseAfter = 700)
    {
        await scene.ClickAsync(select, pauseAfter: 300);
        await select.EvaluateAsync("(select, text) => { const option = [...select.options].find(o => o.text.includes(text)); select.value = option.value; select.dispatchEvent(new Event('change', { bubbles: true })); if (window.jQuery && jQuery(select).selectpicker) { jQuery(select).selectpicker('refresh'); } }", contains);
        await scene.PauseAsync(pauseAfter);
    }

    private static ILocator Fld(Scene scene, string name) => scene.Page.Locator($"[name='{name}']").First;

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/orders");
        await scene.StartCaptureAsync();

        await scene.StepAsync("set-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/Settings/orders"), "Settings > Commerce > Orders", 2000);
            var quotes = Fld(scene, "ISite.OrderSettings.AllowQuoteRequests");

            if (!await quotes.IsCheckedAsync())
            {
                await scene.ClickAsync(quotes, pauseAfter: 900);
            }

            await scene.HighlightAsync(Field(scene, "Take quote requests"), "Take quote requests");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("set-2", async () =>
        {
            var recovery = Fld(scene, "ISite.OrderSettings.SendRecoveryEmails");

            if (!await recovery.IsCheckedAsync())
            {
                await scene.ClickAsync(recovery, pauseAfter: 900);
            }

            await scene.HighlightAsync(Field(scene, "Email buyers who leave a checkout"), "Recovery email");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "Send it after"), "Send it after (hours)");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("set-3", async () =>
        {
            await PickAsync(scene, Fld(scene, "ISite.OrderSettings.OrderBumpProductId"), "Ceramic Mug", 1000);
            await scene.TypeAsync(Fld(scene, "ISite.OrderSettings.OrderBumpPercentOff"), "10", delay: 80);
            await scene.TypeAsync(Fld(scene, "ISite.OrderSettings.OrderBumpHeadline"), "Add a mug for 10% off", delay: 50);
            await scene.HighlightAsync(Field(scene, "Product offered"), "Order bump: one more thing at checkout");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(SaveButton(scene), pauseAfter: 500);
            await WaitAsync(scene, 4000);
        });
    }

    private static async Task AskAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        await scene.OffCameraAsync(async () =>
        {
            await scene.Page.GotoAsync(BaseUrl + "/Login", new() { WaitUntil = WaitUntilState.NetworkIdle });
            await scene.Page.Locator("#LoginForm_UserName").FillAsync("jamie");
            await scene.Page.Locator("#LoginForm_Password").FillAsync("Password1!");
            await scene.Page.RunAndWaitForNavigationAsync(() => scene.Page.Locator("#LoginForm_Password").PressAsync("Enter"), new() { WaitUntil = WaitUntilState.NetworkIdle });
            await scene.Page.GotoAsync(BaseUrl + "/shop", new() { WaitUntil = WaitUntilState.NetworkIdle });
            var card = scene.Page.Locator(".card").Filter(new() { HasTextString = "Hardcover Notebook" }).First;
            await card.Locator("input[type=number]").FillAsync("10");
            await card.Locator("button:has-text('Add to cart')").ClickAsync();
            await scene.Page.WaitForTimeoutAsync(1500);
        });

        await scene.OpenAsync("/cart");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ask-1", async () =>
        {
            await scene.PauseAsync(1500);
            var quote = scene.Page.GetByText("Request a quote").First;
            await quote.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(quote, "Request a quote, under Checkout");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(quote, pauseAfter: 1200);
        });

        await scene.StepAsync("ask-2", async () =>
        {
            var message = scene.Page.Locator("textarea").First;
            await scene.TypeAsync(message, "Ten notebooks for our team, please, and we need them by the 15th.", delay: 35);
            var order = scene.Page.GetByLabel("Purchase order").First;

            if (await order.CountAsync() > 0)
            {
                await scene.TypeAsync(order, "PO-2041", delay: 60);
            }

            await scene.PauseAsync(1500);
            await scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Send the request" }).First, pauseAfter: 600);
            await WaitAsync(scene, 4000);
            await scene.PauseAsync(3500);
        });
    }

    private static async Task QuoteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/orders");
        await scene.StartCaptureAsync();

        await scene.StepAsync("quo-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/orders"), "Commerce > Orders", 1800);
            await scene.ClickAsync(scene.Page.Locator("button, a").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("^ *Placed *$") }).First, pauseAfter: 1000);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".dropdown-menu.show a, .dropdown-menu.show button").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("^ *Drafts *$") }).First);
            var row = scene.Page.Locator("li").Filter(new() { Has = scene.Page.Locator("a[href*='/Admin/orders/']") }).First;
            await scene.HighlightAsync(row, "The quote request, a draft order");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(row.Locator("a[href*='/Admin/orders/']").First);
            await scene.PauseAsync(2500);
            await scene.HighlightAsync(scene.Page.GetByText("Ten notebooks").First, "What the buyer asked for");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a, button").Filter(new() { HasTextString = "Edit" }).First);
        });

        await scene.StepAsync("quo-2", async () =>
        {
            var price = scene.Page.GetByLabel("Price each").First;
            await price.ScrollIntoViewIfNeededAsync();
            await scene.ClickAsync(price, pauseAfter: 200);
            await price.FillAsync("11");
            await scene.HighlightAsync(price, "Price each: your offer");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            var payBy = scene.Page.GetByLabel("Pay by").First;
            await payBy.ScrollIntoViewIfNeededAsync();
            await payBy.FillAsync(DateTime.Today.AddDays(14).ToString("yyyy-MM-dd"));
            await scene.HighlightAsync(payBy, "Pay by: when the quote expires");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("quo-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Update prices" }).First, pauseAfter: 1500);
            await WaitAsync(scene, 2500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Save draft" }).First);
            await scene.PauseAsync(2500);
        });

        await scene.StepAsync("quo-4", async () =>
        {
            var send = scene.Page.Locator("button, a").Filter(new() { HasTextString = "Send quote" }).First;
            await scene.HighlightAsync(send, "Send quote");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(send, pauseAfter: 800);
            await WaitAsync(scene, 4000);
            await scene.PauseAsync(3500);
        });
    }

    // The buyer pays the quote through the checkout, off camera past the first screens.
    private static async Task FinishCheckoutAsync(IPage page)
    {
        for (var step = 0; step < 8 && !page.Url.Contains("/Confirmation", StringComparison.OrdinalIgnoreCase); step++)
        {
            var country = page.Locator("#Shipping_Address_Country");

            if (await country.CountAsync() > 0 && await country.IsVisibleAsync())
            {
                await country.SelectOptionAsync(new SelectOptionValue { Label = "United States" });
                await page.WaitForTimeoutAsync(1200);
                await page.Locator("#Shipping_Address_Name").FillAsync("Jamie Chen");
                await page.Locator("#Shipping_Address_AddressLine1").FillAsync("350 Fifth Avenue");
                await page.Locator("#Shipping_Address_City").FillAsync("New York");
                await page.Locator("#Shipping_Address_Region").SelectOptionAsync(new SelectOptionValue { Label = "New York" });
                await page.Locator("#Shipping_Address_PostalCode").FillAsync("10118");
            }

            var radio = page.Locator("input[type=radio]");

            if (await radio.CountAsync() > 0 && await radio.First.IsVisibleAsync() && !await radio.First.IsCheckedAsync())
            {
                await radio.First.CheckAsync();
            }

            var test = page.GetByText("Test payment").First;

            if (await test.CountAsync() > 0 && await test.IsVisibleAsync())
            {
                await test.ClickAsync();
            }

            await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        }
    }

    private static async Task AcceptAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        await scene.OffCameraAsync(async () =>
        {
            await scene.Page.GotoAsync(BaseUrl + "/Login", new() { WaitUntil = WaitUntilState.NetworkIdle });
            await scene.Page.Locator("#LoginForm_UserName").FillAsync("jamie");
            await scene.Page.Locator("#LoginForm_Password").FillAsync("Password1!");
            await scene.Page.RunAndWaitForNavigationAsync(() => scene.Page.Locator("#LoginForm_Password").PressAsync("Enter"), new() { WaitUntil = WaitUntilState.NetworkIdle });
        });

        await scene.OpenAsync("/account/orders");
        await scene.StartCaptureAsync();

        await scene.StepAsync("acc-1", async () =>
        {
            await scene.PauseAsync(1500);
            var row = scene.Page.Locator("a").Filter(new() { HasTextString = "Quote 26" }).First;
            await scene.HighlightAsync(row, "The quote, under the buyer's orders");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(row);
            await scene.PauseAsync(2500);
            await scene.HighlightAsync(scene.Page.Locator("main, .card").First, "The prices, and until when they are offered");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("acc-2", async () =>
        {
            var pay = scene.Page.Locator("a, button").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("accept|Pay now", System.Text.RegularExpressions.RegexOptions.IgnoreCase) }).First;
            await scene.HighlightAsync(pay, "Accepting a quote means paying it");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(pay);
            await scene.PauseAsync(2500);
            await scene.OffCameraAsync(() => FinishCheckoutAsync(scene.Page));
            await scene.PauseAsync(3500);
        });
    }

    private static async Task AddItemsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/orders");
        await scene.StartCaptureAsync();

        await scene.StepAsync("add-1", async () =>
        {
            var paid = scene.Page.Locator("li, tr").Filter(new() { HasTextString = "Paid" }).Filter(new() { HasNotTextString = "Draft" }).First;
            await scene.ClickAndWaitForNavigationAsync(paid.Locator("a[href*='/Admin/orders/']").First);
            await scene.PauseAsync(1500);
            var add = scene.Page.Locator("a, button").Filter(new() { HasTextString = "Add items" }).First;
            await add.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(add, "Add items");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(add);
        });

        await scene.StepAsync("add-2", async () =>
        {
            await scene.PauseAsync(2000);
            await scene.HighlightAsync(scene.Page.GetByText("These items are added to order").First, "A new order for the same buyer");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task ExportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/orders");
        await scene.StartCaptureAsync();

        await scene.StepAsync("exp-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("input[type=search], input[name*='Search'], input[name*='search']").First, "Search by order number or email");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            var status = scene.Page.Locator("button, a").Filter(new() { HasTextString = "Status" }).First;

            if (await status.CountAsync() > 0)
            {
                await scene.ClickAsync(status, pauseAfter: 1200);
                await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show").Last, "Paid, awaiting payment, draft, canceled");
                await scene.PauseAsync(3500);
                await scene.ClearHighlightsAsync();
                await scene.Page.Keyboard.PressAsync("Escape");
            }
        });

        await scene.StepAsync("exp-2", async () =>
        {
            var export = Link(scene, "Export CSV");
            await scene.HighlightAsync(export, "Export CSV");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();

            try
            {
                await scene.Page.RunAndWaitForDownloadAsync(() => scene.ClickAsync(export, pauseAfter: 500), new() { Timeout = 20000 });
            }
            catch (TimeoutException)
            {
            }

            await scene.PauseAsync(3000);
        });
    }
}
