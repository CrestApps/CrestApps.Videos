using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static async Task StripeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/stripe");
        await scene.StartCaptureAsync();

        await scene.StepAsync("stripe-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/Settings/stripe"), "Settings > Payments > Stripe", 2400);
            await scene.HighlightAsync(Field(scene, "Enable Production"), "Enable Production");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("stripe-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("[name='ISite.StripeSettings.TestPublishableKey']"), "pk_test_demo_not_a_real_key", delay: 30);
            await scene.TypeAsync(scene.Page.Locator("[name='ISite.StripeSettings.TestPrivateSecret']"), "sk_test_demo_not_a_real_key", delay: 30);
            await scene.HighlightAsync(Field(scene, "Test Publishable Key"), "Test keys");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("stripe-3", async () =>
        {
            await scene.Page.EvaluateAsync("() => { const d = [...document.querySelectorAll('details')].find(x => /Advanced/.test(x.textContent)); if (d) { d.open = true; } }");
            await scene.PauseAsync(1000);
            await ShowAsync(scene, scene.Page.Locator("details").Filter(new() { HasTextString = "Advanced" }).First, "Webhook signing secret", 2600);
            await scene.ClickAsync(scene.Page.Locator("[name='ISite.StripeSettings.IsLive']").First, pauseAfter: 900);
            await ShowAsync(scene, Field(scene, "Production Publishable Key"), "Production keys", 2600);
            await scene.ClickAsync(scene.Page.Locator("[name='ISite.StripeSettings.IsLive']").First, pauseAfter: 600);
        });
    }

    private static async Task OfflineAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/paylater");
        await scene.StartCaptureAsync();

        await scene.StepAsync("off-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/Settings/paylater"), "Settings > Offline payments", 2200);
            await ShowAsync(scene, Field(scene, "Offer Pay Later at checkout"), "Pay Later", 2200);
            await scene.HighlightAsync(Field(scene, "Net term (days)"), "Net term");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("off-2", async () =>
        {
            await ShowAsync(scene, Field(scene, "Offer Bank transfer at checkout"), "Bank transfer", 2000);
            await ShowAsync(scene, Field(scene, "Bank details"), "Bank details", 2000);
            await scene.HighlightAsync(Field(scene, "Due within (days)"), "Due within");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("off-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("[name='ISite.CashOnDeliverySettings.Enabled']").First, pauseAfter: 700);
            await scene.TypeAsync(scene.Page.Locator("[name='ISite.CashOnDeliverySettings.Instructions']"), "Our courier takes cash and cards.", delay: 30);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }

    private static async Task TransactionsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/transactions");
        await scene.StartCaptureAsync();

        await scene.StepAsync("trn-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/transactions"), "Commerce > Transactions", 2200);
            await scene.HighlightAsync(Link(scene, "Manage"), "Manage");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Link(scene, "Manage"));
        });

        await scene.StepAsync("trn-2", async () =>
        {
            await ShowAsync(scene, scene.Page.Locator(".card").Filter(new() { HasTextString = "Outstanding" }).Filter(new() { HasTextString = "Owner" }).First, "Summary", 2800);
            await scene.HighlightAsync(scene.Page.Locator(".card").Filter(new() { HasTextString = "Timeline" }).Last, "Timeline");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("trn-3", async () =>
        {
            await ShowAsync(scene, scene.Page.Locator(".card").Filter(new() { HasTextString = "Send a reminder" }).Last, "Send reminder", 2800);
            await scene.HighlightAsync(scene.Page.Locator(".card").Filter(new() { HasTextString = "Send the invoice" }).Last, "Send invoice");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("trn-4", async () =>
        {
            await ScrollAsync(scene, 0, 500);
            await ShowAsync(scene, scene.Page.Locator(".card").Filter(new() { HasTextString = "Adjust or waive" }).Last, "Adjust or waive", 3000);
            await scene.HighlightAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Mark as paid" }).First, "Mark as paid");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("trn-5", async () =>
        {
            await ScrollAsync(scene, 0, -300);
            await scene.TypeAsync(scene.Page.GetByPlaceholder("Check number or transfer reference (optional)"), "Transfer 20261029", delay: 25);
            await scene.TypeAsync(scene.Page.GetByPlaceholder("How the payment was received"), "Received by wire", delay: 25);
            await scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Record payment" }).First, pauseAfter: 1200);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".modal.show .btn-danger").First);
        });

        await scene.StepAsync("trn-6", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".card").Filter(new() { HasTextString = "Timeline" }).Last, "Timeline");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task RemindersAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/transactions");
        await scene.StartCaptureAsync();

        await scene.StepAsync("rem-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/Settings/transactions"), "Settings > Commerce > Transactions", 2400);
            await scene.HighlightAsync(Field(scene, "Enable scheduled reminders"), "Enable scheduled reminders");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rem-2", async () =>
        {
            await ShowAsync(scene, Field(scene, "First reminder delay"), "First reminder delay", 2000);
            await ShowAsync(scene, Field(scene, "Reminder interval"), "Reminder interval", 2000);
            await ShowAsync(scene, Field(scene, "Maximum reminders"), "Maximum reminders", 2000);
            await scene.HighlightAsync(Field(scene, "Remind before the due date"), "Remind before the due date");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PaymentsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/payments");
        await scene.StartCaptureAsync();

        await scene.StepAsync("pay-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/payments"), "Commerce > Payments", 2200);
            await scene.HighlightAsync(Row(scene, "USD 124.00"), "Amount, state, method, reference");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button.dropdown-toggle").Filter(new() { HasTextString = "Any state" }).First, pauseAfter: 1500);
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.ClickAsync(scene.Page.Locator("button.dropdown-toggle").Filter(new() { HasTextString = "Any method" }).First, pauseAfter: 1500);
            await scene.Page.Keyboard.PressAsync("Escape");
        });

        await scene.StepAsync("pay-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "USD 124.00").Locator("a, button").Filter(new() { HasTextString = "Refund" }).First);
            await scene.ClickAsync(scene.Page.Locator("input[type=number]").First, pauseAfter: 200);
            await scene.Page.Keyboard.PressAsync("Control+A");
            await scene.Page.Locator("input[type=number]").First.PressSequentiallyAsync("20", new() { Delay = 50 });
            await scene.TypeAsync(scene.Page.GetByLabel("Reason"), "Damage in transit", delay: 20);
            await scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Refund" }).Last, pauseAfter: 1000);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".modal.show .btn-danger").First);
        });

        await scene.StepAsync("pay-3", async () =>
        {
            await scene.NavigateAsync("/Admin/refunds");
            await scene.HighlightAsync(scene.Page.Locator("h1").First, "Refunds");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task WebhooksAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/commerce/webhooks");
        await scene.StartCaptureAsync();

        await scene.StepAsync("hook-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/commerce/webhooks"), "Commerce > Webhooks", 2400);
            await scene.ClickAndWaitForNavigationAsync(Link(scene, "Add webhook"));
        });

        await scene.StepAsync("hook-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("[name='Name']"), "Warehouse", delay: 55);
            await scene.TypeAsync(scene.Page.Locator("[name='Url']"), "https://example.com/hooks/orders", delay: 30);
            await scene.HighlightAsync(Field(scene, "On"), "On");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("hook-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("input[name='Events'][value='order.paid']"), pauseAfter: 700);
            await scene.ClickAsync(scene.Page.Locator("input[name='Events'][value='shipment.shipped']"), pauseAfter: 700);
            await scene.HighlightAsync(Field(scene, "Every event"), "Every event, those added later too");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("hook-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.StepAsync("hook-5", async () =>
        {
            // The signing secret is generated for this demo webhook, and is hidden all the same.
            await scene.Page.EvaluateAsync("() => document.querySelectorAll('input').forEach(i => { if (/^whsec_/.test(i.value)) { i.type = 'password'; } })");
            await ScrollAsync(scene, 0, 1500);
            await scene.HighlightAsync(scene.Page.Locator("button, a").Filter(new() { HasTextString = "Deliveries" }).First, "Deliveries");
        });

        await scene.ClearHighlightsAsync();
    }
}
