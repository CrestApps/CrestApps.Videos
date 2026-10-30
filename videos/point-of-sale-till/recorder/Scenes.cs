using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Point of Sale: A Day at the Till video, one method per "clip" of storyboard.json.
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
        ("03-open", OpenShiftAsync),
        ("04-closed", ClosedShiftAsync),
        ("05-sales", SalesAsync),
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


    private static string State(string user) => Path.Combine(Root, $"state-{user}.json");
}
