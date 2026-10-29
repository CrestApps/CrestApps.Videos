using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Field(Scene scene, string id) => scene.Page.Locator("#" + id);

    private static ILocator Row(Scene scene, string text) => scene.Page.Locator("li.list-group-item.item").Filter(new() { HasTextString = text }).First;

    private static Task ClearAsync(Scene scene) => scene.ClearHighlightsAsync();

    // ----- Clips -----

    private static async Task BrandAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/receipts");
        await scene.StartCaptureAsync();

        await scene.StepAsync("brand-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/receipts"), "Settings > Commerce > Receipts");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.TypeAsync(Field(scene, "ISite_ReceiptSettings_BusinessName"), "Contoso Outfitters", delay: 50);
            await scene.TypeAsync(Field(scene, "ISite_ReceiptSettings_BusinessAddress"), "100 Main Street, Springfield", delay: 40);
            await scene.HighlightAsync(Field(scene, "ISite_ReceiptSettings_LogoUrl"), "Logo URL");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.TypeAsync(Field(scene, "ISite_ReceiptSettings_ContactEmail"), "billing@contoso.example", delay: 40);
            await scene.TypeAsync(Field(scene, "ISite_ReceiptSettings_ContactPhone"), "555-0100", delay: 60);
            await scene.TypeAsync(Field(scene, "ISite_ReceiptSettings_FooterText"), "Thank you for your business.", delay: 40);
        });

        await scene.StepAsync("brand-2", async () =>
        {
            await scene.HighlightAsync(Field(scene, "ISite_ReceiptSettings_HeaderTitle"), "Header title");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await scene.HighlightAsync(Field(scene, "ISite_ReceiptSettings_ShowTestPaymentBadge"), "Test payment badge");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(Save(scene.Page));
        });
    }

    private static async Task InvoiceAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/transactions");
        await scene.StartCaptureAsync();

        await scene.StepAsync("inv-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/transactions"), "Commerce > Transactions");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Outstanding").GetByRole(AriaRole.Link, new() { Name = "Manage" }));
            await scene.HighlightAsync(scene.Page.GetByText("Send the invoice", new() { Exact = true }), "Send the invoice");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Send invoice" }), pauseAfter: 1500);

            if (await scene.Page.Locator("#modalOkButton").CountAsync() > 0)
            {
                await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalOkButton"));
            }

            await scene.PauseAsync(1500);
        });

        await InvoiceStepsAsync(scene);
    }

    private static string PayLinkFile => Path.Combine(Root, "pay-link.txt");

    private static async Task InvoiceStepsAsync(Scene scene)
    {
        await scene.StepAsync("inv-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Invoice", new() { Exact = true }).First, "The invoice number");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='/Admin/transaction-invoices/']").First);
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.GetByText("Billed to", new() { Exact = false }).First, "Who it is billed to");
            await scene.PauseAsync(4000);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("Due on").First, "The due date");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
        });

        await scene.StepAsync("inv-3", async () =>
        {
            File.WriteAllText(PayLinkFile, await Field(scene, "invoice-pay-link").InputValueAsync());
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Print invoice" }), "Print invoice");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await Field(scene, "invoice-pay-link").ScrollIntoViewIfNeededAsync();
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Copy" }), pauseAfter: 1200);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Copied" }), "Copied");
        });
    }

    private static async Task PayAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var link = new Uri(File.ReadAllText(PayLinkFile).Trim()).PathAndQuery;
        await using var scene = await NewSceneAsync(browser, directory, durations, State("guest"));

        await scene.OpenAsync(link);
        await scene.StartCaptureAsync();

        await scene.StepAsync("pay-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Amount due").First, "Amount due");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("Due on").First, "Due on");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Pay" }).Or(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Pay" })).First, "Pay");
        });

        await ClearAsync(scene);

        await scene.StepAsync("pay-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Pay" }).Or(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Pay" })).First);
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.GetByText("Back to the payment page").First, "Back to the payment page");
            await scene.PauseAsync(3000);
        });
    }

    private static async Task ReceiptAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/transactions");
        await scene.StartCaptureAsync();

        await scene.StepAsync("rcpt-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Outstanding").GetByRole(AriaRole.Link, new() { Name = "Manage" }));
            await Field(scene, "Amount").FillAsync(string.Empty);
            await scene.TypeAsync(Field(scene, "Amount"), "135", delay: 90);
            await scene.TypeAsync(Field(scene, "Reference"), "Transfer 5521", delay: 50);
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Record payment" }), pauseAfter: 1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalOkButton"));
        });

        await scene.StepAsync("rcpt-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Payment received", new() { Exact = true }).First, "Payment received, with its receipt");
            await scene.PauseAsync(4000);
            await ClearAsync(scene);
        });

        await scene.StepAsync("rcpt-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "View receipt" }).First);
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.GetByText("Receipt:", new() { Exact = false }).First, "The receipt number");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Print receipt" }), "Print receipt");
        });
    }
}
