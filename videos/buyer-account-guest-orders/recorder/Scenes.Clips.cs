using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator LinkNamed(Scene scene, string name) => scene.Page.GetByRole(AriaRole.Link, new() { Name = name, Exact = true }).First;

    private static ILocator Field(Scene scene, string id) => scene.Page.Locator("#" + id);

    private static Task ClearAsync(Scene scene) => scene.ClearHighlightsAsync();

    private static ILocator Main(Scene scene) => scene.Page.Locator("main");

    // ----- Clips -----

    private static async Task AccountAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, State("jamie"));

        await scene.OpenAsync("/account");
        await scene.StartCaptureAsync();

        await scene.StepAsync("acct-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("a[href='/account/orders']").First, "Orders");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator("a[href='/account/wishlist']").First, "Wishlist");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator("a[href='/account/payment-methods']").First, "Payment methods");
        });

        await ClearAsync(scene);

        await scene.StepAsync("acct-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href='/account/orders']").First);
            await scene.PauseAsync(1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='/account/orders/']").First);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Invoice" }), "Print the invoice");
        });

        await ClearAsync(scene);

        await scene.StepAsync("acct-3", async () =>
        {
            await scene.NavigateAsync("/account/payment-methods");
            await scene.PauseAsync(2500);
            await scene.NavigateAsync("/my-transactions");
            await scene.PauseAsync(2000);
        });

        await scene.StepAsync("acct-4", async () =>
        {
            await scene.NavigateAsync("/shop");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='/shop/'], a[href*='/products/']").Filter(new() { HasTextString = "Watch" }).First);
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Save to wishlist" }), pauseAfter: 1500);
            await scene.NavigateAsync("/account/wishlist");
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Add to cart" }).First, "Add to cart");
        });
    }

    private static async Task GuestAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, State("guest"));

        await scene.OpenAsync("/shop");
        await scene.StartCaptureAsync();

        await scene.StepAsync("guest-1", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Add to cart" }).First, pauseAfter: 1500);
            await scene.NavigateAsync("/cart");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Checkout" }).First);
            await FillCheckoutAsync(scene.Page, "Casey Rowe", "casey.rowe@example.com");
        });

        await scene.StepAsync("guest-2", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByText("Bank transfer", new() { Exact = true }).First, pauseAfter: 1200);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Pay now" }));
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("guest-3", async () =>
        {
            var number = await scene.Page.Locator(".font-monospace").First.InnerTextAsync();
            File.WriteAllText(Path.Combine(Root, "guest-order.txt"), number.Trim() + "|casey.rowe@example.com|" + scene.Page.Url);
            await scene.HighlightAsync(scene.Page.Locator(".font-monospace").First, "The order number");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "View your order" }), "A link back to the order");
            await scene.PauseAsync(3000);
        });
    }

    private static (string Number, string Email, string Url) GuestOrder()
    {
        var parts = File.ReadAllText(Path.Combine(Root, "guest-order.txt")).Split('|');
        return (parts[0], parts[1], parts[2]);
    }

    // The newest email of the sink that links to an order, with its quoted-printable encoding undone.
    private static string NewestOrderLink()
    {
        var mail = Directory.GetFiles(MailFolder, "*.eml").OrderBy(file => file).Last();
        var text = File.ReadAllText(mail).Replace("=\r\n", string.Empty).Replace("=\n", string.Empty).Replace("=3D", "=");
        var link = System.Text.RegularExpressions.Regex.Matches(text, @"http://localhost:5375/[^\s""<>]+")
            .Select(m => m.Value.TrimEnd('.', ')'))
            .First(l => l.Contains("/orders/", StringComparison.OrdinalIgnoreCase) && !l.Contains("/orders/track") && !l.Contains("/orders/find"));
        return new Uri(link).PathAndQuery;
    }

    private const string MailFolder = "C:/cavideo/commerce-2/mail";

    private static async Task FindAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var (number, email, _) = GuestOrder();

        // The money for Casey's transfer has arrived, so the order is paid and can be tracked.
        var admin = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var adminPage = await admin.NewPageAsync();
        adminPage.SetDefaultTimeout(60_000);
        await MarkDueOrderPaidAsync(adminPage);
        await admin.CloseAsync();

        await using var scene = await NewSceneAsync(browser, directory, durations, State("guest"));

        await scene.OpenAsync("/orders/track");
        await scene.StartCaptureAsync();

        await scene.StepAsync("find-1", async () =>
        {
            await scene.HighlightAsync(Main(scene).GetByText("Enter your order number"), "The order number and the email");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.TypeAsync(Field(scene, "OrderNumber"), number, delay: 70);
            await scene.TypeAsync(Field(scene, "Email"), email, delay: 40);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Track" }));
            await scene.PauseAsync(2500);
        });

        await scene.StepAsync("find-2", async () =>
        {
            await scene.NavigateAsync("/orders/find");
            await scene.TypeAsync(Field(scene, "Email"), email, delay: 40);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Send me the links" }));
            await scene.PauseAsync(3000);
        });

        await scene.StepAsync("find-3", async () =>
        {
            await Task.Delay(4000);
            var link = NewestOrderLink();
            File.WriteAllText(Path.Combine(Root, "guest-order-link.txt"), link);
            await scene.NavigateAsync(link);
            await scene.PauseAsync(2000);
        });
    }

    private static async Task RegisterAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var link = File.ReadAllText(Path.Combine(Root, "guest-order-link.txt")).Trim();
        await using var scene = await NewSceneAsync(browser, directory, durations, State("guest"));

        await scene.OpenAsync(link);
        await scene.StartCaptureAsync();

        await scene.StepAsync("reg-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Create an account with the same email"), "Create an account with the same email");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Create an account" }));
        });

        await scene.StepAsync("reg-2", async () =>
        {
            await scene.TypeAsync(Field(scene, "RegisterUserForm_UserName"), "casey", delay: 70);
            await scene.TypeAsync(Field(scene, "RegisterUserForm_Email"), "casey.rowe@example.com", delay: 40);
            await scene.TypeAsync(Field(scene, "RegisterUserForm_Password"), "Password1!", delay: 70);
            await scene.TypeAsync(Field(scene, "RegisterUserForm_ConfirmPassword"), "Password1!", delay: 70);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("form button[type=submit]").Last);
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("reg-3", async () =>
        {
            await scene.NavigateAsync("/account/orders");
            await scene.PauseAsync(800);
            await scene.HighlightAsync(scene.Page.Locator("a[href*='/account/orders/']").First, "The guest order, in the account");
        });
    }
}
