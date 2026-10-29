using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // Opens the order of a buyer from the list, before the capture starts.
    private static async Task OpenOrderOfAsync(Scene scene, string buyer)
    {
        await scene.OpenAsync("/Admin/orders");
        await scene.Page.RunAndWaitForNavigationAsync(() => OrderLink(scene, buyer).ClickAsync());
        await scene.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    // The link of a buyer's order in the list.
    private static ILocator OrderLink(Scene scene, string buyer)
        => scene.Page.Locator($"xpath=((//li[contains(., '{buyer}')][.//a[contains(@href,'/Admin/orders/')]])[last()]//a[contains(@href,'/Admin/orders/')])[1]");

    private static async Task ReplaceAsync(Scene scene, ILocator field, string text)
    {
        await scene.ClickAsync(field, pauseAfter: 200);
        await scene.Page.Keyboard.PressAsync("Control+A");
        await field.PressSequentiallyAsync(text, new() { Delay = 70 });
        await scene.PauseAsync(300);
    }

    private static async Task ListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/orders");
        await scene.StartCaptureAsync();

        await scene.StepAsync("list-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/orders"), "Commerce > Orders", 2200);
            await scene.HighlightAsync(Row(scene, "Ana Torres"), "Paid");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row(scene, "Priya Shah"), "Payment due");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("list-2", async () =>
        {
            await scene.TypeAsync(scene.Page.GetByPlaceholder("Search by order number or customer email"), "priya.shah@example.com", delay: 45);
            await scene.WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"));
        });

        await scene.StepAsync("list-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.dropdown-toggle").Filter(new() { HasTextString = "Every channel" }).First, pauseAfter: 1800);
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.ClickAsync(scene.Page.Locator("button.dropdown-toggle").Filter(new() { HasTextString = "Placed" }).First, pauseAfter: 1800);
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.HighlightAsync(Link(scene, "Export CSV"), "Export CSV");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("list-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(OrderLink(scene, "Priya Shah"));
        });
    }

    private static async Task OrderAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await OpenOrderOfAsync(scene, "Jamie Chen");
        var orderUrl = scene.Page.Url;
        await scene.StartCaptureAsync();

        await scene.StepAsync("ord-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".card").Filter(new() { HasTextString = "Total paid" }).First, "Items and totals");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ord-2", async () =>
        {
            await ShowAsync(scene, scene.Page.Locator(".card").Filter(new() { HasTextString = "Shipments" }).First, "Shipments", 2500);
            await scene.HighlightAsync(Link(scene, "Create shipment"), "Create shipment");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ord-3", async () =>
        {
            await ScrollAsync(scene, 0, -2000);
            await scene.HighlightAsync(Link(scene, "Invoice"), "Invoice");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Link(scene, "Packing slip"), "Packing slip");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Link(scene, "Invoice"));
            await scene.PauseAsync(2500);
            await scene.NavigateAsync(new Uri(orderUrl).AbsolutePath);
        });

        await scene.StepAsync("ord-4", async () =>
        {
            await ScrollAsync(scene, 0, 900);
            await scene.TypeAsync(scene.Page.Locator("#order-note"), "Customer asked for a gift receipt.", delay: 40);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Add note" }).First);
        });

        await scene.StepAsync("ord-5", async () =>
        {
            await ScrollAsync(scene, 0, 1200);
            await scene.HighlightAsync(scene.Page.Locator("div, section, dl").Filter(new() { HasTextString = "Checkout session" }).Filter(new() { HasNotTextString = "Refund" }).Last, "Order details");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ManageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.Dialog += (_, dialog) => _ = dialog.AcceptAsync();

        await OpenOrderOfAsync(scene, "Marcus Lee");
        await scene.StartCaptureAsync();

        await scene.StepAsync("man-1", async () =>
        {
            await ScrollAsync(scene, 0, 400);
            await ReplaceAsync(scene, scene.Page.Locator("#refund-amount"), "20");
            await scene.TypeAsync(scene.Page.Locator("#refund-reason"), "Item arrived scratched", delay: 45);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Refund" }).Last);
        });

        await scene.StepAsync("man-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#refund-restock").Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"), "Only for a full refund");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("man-3", async () =>
        {
            await scene.NavigateAsync("/Admin/orders");
            await scene.ClickAndWaitForNavigationAsync(OrderLink(scene, "Priya Shah"));
            await ScrollAsync(scene, 0, 500);
            await scene.TypeAsync(scene.Page.Locator("#cancel-note"), "Buyer changed their mind", delay: 25);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Cancel order" }).First);
        });

        await scene.StepAsync("man-4", async () =>
        {
            await scene.NavigateAsync("/Admin/orders");
            await scene.ClickAndWaitForNavigationAsync(OrderLink(scene, "Ana Torres"));
            await scene.HighlightAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Send the confirmation again" }).First, "Send the confirmation again");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/orders");
        await scene.StartCaptureAsync();

        await scene.StepAsync("cre-1", async () =>
        {
            await scene.HighlightAsync(Link(scene, "Create order"), "Create order");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Link(scene, "Create order"));
        });

        await scene.StepAsync("cre-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("[name='CustomerEmail']"), "kim.okafor@example.com", delay: 40);
            await scene.TypeAsync(scene.Page.Locator("[name='CustomerName']"), "Kim Okafor", delay: 50);
            await scene.ClickAsync(scene.Page.Locator("button.dropdown-toggle").Filter(new() { HasTextString = "Find products" }).First, pauseAfter: 900);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show li, .dropdown-menu.show a").Filter(new() { HasTextString = "Desk Lamp" }).First, pauseAfter: 600);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show li, .dropdown-menu.show a").Filter(new() { HasTextString = "Wool Beanie" }).First, pauseAfter: 600);
            await scene.Page.Keyboard.PressAsync("Escape");
        });

        await scene.StepAsync("cre-3", async () =>
        {
            await ScrollAsync(scene, 0, 600);
            await scene.HighlightAsync(Field(scene, "Note for the buyer"), "Note for the buyer");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "Pay by"), "Pay by");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("cre-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Save draft" }).First);
        });
    }

    private static async Task ReturnsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/returns");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ret-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/returns"), "Commerce > Returns", 2500);
            await scene.HighlightAsync(scene.Page.Locator("h1").First, "Return requests");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ret-2", async () =>
        {
            await scene.NavigateAsync("/Admin/Settings/returns");
            await ShowAsync(scene, Field(scene, "Return window"), "Return window", 1800);
            await ShowAsync(scene, Field(scene, "Reasons"), "Reasons", 1800);
            await ShowAsync(scene, Field(scene, "Let buyers ask for something else"), "Exchanges", 1800);
            await scene.HighlightAsync(Field(scene, "Return instructions"), "Return instructions");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task GiftAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/gift-cards");
        await scene.StartCaptureAsync();

        await scene.StepAsync("gift-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/gift-cards"), "Commerce > Gift cards", 2500);
            await scene.ClickAndWaitForNavigationAsync(Link(scene, "Issue gift card"));
        });

        await scene.StepAsync("gift-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("[name='Value']"), "25", delay: 90);
            await scene.HighlightAsync(Field(scene, "Code"), "Leave empty: the site makes one");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("[name='RecipientName']"), "Taylor Reed", delay: 45);
            await scene.TypeAsync(scene.Page.Locator("[name='RecipientEmail']"), "taylor.reed@example.com", delay: 30);
            await scene.TypeAsync(scene.Page.Locator("[name='Message']"), "Thanks for your patience!", delay: 30);
        });

        await scene.StepAsync("gift-3", async () =>
        {
            await scene.HighlightAsync(Field(scene, "Email the card"), "Email the card");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("[name='SendEmail']").First, pauseAfter: 600);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.btn-primary").Filter(new() { HasTextString = "Issue" }).First);
        });
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/orders");
        await scene.StartCaptureAsync();

        await scene.StepAsync("set-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/Settings/orders"), "Settings > Commerce > Orders", 2200);
            await ShowAsync(scene, Field(scene, "Send order confirmations"), "Send order confirmations", 2000);
            await scene.HighlightAsync(Field(scene, "Take quote requests"), "Take quote requests");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("set-2", async () =>
        {
            await ShowAsync(scene, Field(scene, "Email buyers who leave a checkout"), "Email buyers who leave a checkout", 2200);
            await ScrollAsync(scene, 0, 500);
            await scene.HighlightAsync(Field(scene, "Product offered"), "Order bump");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("set-3", async () =>
        {
            await scene.NavigateAsync("/Admin/Settings/order-emails");
            await ShowAsync(scene, Field(scene, "Layout"), "Layout", 2200);
            await scene.HighlightAsync(scene.Page.Locator("[name='ISite.OrderEmailTemplateSettings.Templates[0].IsEnabled']").Locator("xpath=ancestor::div[contains(@class,'card') or contains(@class,'ocat-wrapper')][1]"), "Use my template, Subject, Body");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("set-4", async () =>
        {
            await scene.NavigateAsync("/Admin/Settings/receipts");
            await ShowAsync(scene, Field(scene, "Header title"), "Header title", 1800);
            await ShowAsync(scene, Field(scene, "Business address"), "Business address", 1800);
            await scene.HighlightAsync(Field(scene, "Footer text"), "Footer text");
        });

        await scene.ClearHighlightsAsync();
    }
}
