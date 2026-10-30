using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Memberships video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-sell", SellAsync),
        ("04-members", MembersOnlyAsync),
        ("04-guest", GuestAsync),
        ("05-buy", BuyAsync),
        ("06-member", MemberAsync),
        ("07-manage", ManageMembershipsAsync),
        ("08-after", AfterAsync),
    ];

    public static async Task RecordAsync(IBrowser browser, string outputRoot, IReadOnlyDictionary<string, double> durations, string[] names)
    {
        var selected = names is ["all"] ? s_clips : s_clips.Where(clip => names.Contains(clip.Name)).ToArray();

        foreach (var (name, record) in selected)
        {
            Console.WriteLine($"Recording {name}");
            await record(browser, Path.Combine(outputRoot, name), durations);
        }
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string state = null)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, state ?? StatePath, Path.GetDirectoryName(directory), durations);

    private static ILocator Save(IPage page) => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    // Fills the Contact and Shipping steps of a checkout, and chooses the first shipping rate.
    private static async Task FillCheckoutAsync(IPage page, string name, string email)
    {
        await page.Locator("#Contact_Name").FillAsync(name);
        await page.Locator("#Contact_Email").FillAsync(email);
        await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync());

        await page.Locator("#Shipping_Address_Country").SelectOptionAsync("US");
        await Task.Delay(1000);
        await page.Locator("#Shipping_Address_Name").FillAsync(name);
        await page.Locator("#Shipping_Address_AddressLine1").FillAsync("1 Pioneer Square");
        await page.Locator("#Shipping_Address_City").FillAsync("Seattle");
        await page.Locator("#Shipping_Address_Region").SelectOptionAsync("WA");
        await page.Locator("#Shipping_Address_PostalCode").FillAsync("98104");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync());
        await page.Locator("label[for^='Shipping_RateId']").First.ClickAsync();
        await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync());
    }

    // Buys the first product of the shop and pays by bank transfer, as a buyer (a sign-in) or a guest (none). Returns the confirmation address.
    private static async Task<string> PlaceOrderAsync(IBrowser browser, string state, string name, string email)
    {
        var buyer = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = state, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await buyer.NewPageAsync();
        page.SetDefaultTimeout(60_000);
        await page.GotoAsync(BaseUrl + "/shop");
        await page.GetByRole(AriaRole.Button, new() { Name = "Add to cart" }).First.ClickAsync();
        await Task.Delay(2500);
        await page.GotoAsync(BaseUrl + "/cart");
        await page.RunAndWaitForNavigationAsync(() => page.GetByRole(AriaRole.Button, new() { Name = "Checkout" }).First.ClickAsync());
        await FillCheckoutAsync(page, name, email);
        await page.GetByText("Bank transfer", new() { Exact = true }).First.ClickAsync();
        await page.RunAndWaitForNavigationAsync(() => page.GetByRole(AriaRole.Button, new() { Name = "Pay now" }).ClickAsync());
        Console.WriteLine($"Placed an order for {name}: {page.Url}");
        var url = page.Url;
        await buyer.CloseAsync();
        return url;
    }


    public static async Task PrepareAsync(IBrowser browser)
    {
        var admin = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await admin.NewPageAsync();
        page.SetDefaultTimeout(120_000);

        await EnableFeaturesAsync(page, "OrchardCore.Email.Smtp");
        await admin.CloseAsync();

        await SaveSignInAsync(browser, "jamie");
    }

    // Records the bank transfer of the first order waiting for one as received, as staff do when the money arrives.
    private static async Task MarkDueOrderPaidAsync(IPage page)
    {
        await page.GotoAsync(BaseUrl + "/Admin/orders");
        var order = await page.Locator("li.list-group-item.item").Filter(new() { HasTextString = "Payment due" }).First.Locator("a").First.GetAttributeAsync("href");
        await page.GotoAsync(BaseUrl + order);
        var record = await page.GetByRole(AriaRole.Link, new() { Name = "Record payment" }).First.GetAttributeAsync("href");
        await page.GotoAsync(BaseUrl + record);
        await page.GetByRole(AriaRole.Button, new() { Name = "Mark as paid" }).First.ClickAsync();
        var ok = page.Locator("#modalOkButton");

        if (await ok.CountAsync() > 0)
        {
            await page.RunAndWaitForNavigationAsync(() => ok.ClickAsync());
        }
        else
        {
            await page.WaitForLoadStateAsync();
        }
    }

    private static string State(string user) => Path.Combine(Root, $"state-{user}.json");
}
