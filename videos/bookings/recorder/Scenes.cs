using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Bookings video, one method per "clip" of storyboard.json.
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
        ("03-product", ProductAsync),
        ("04-dates", DatesAsync),
        ("05-kinds", KindsAsync),
        ("06-rules", RulesAsync),
        ("07-buyer", BuyerAsync),
        ("08-list", ListAsync),
        ("09-detail", DetailAsync),
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

    // The fake bookings: buyer (a user name, or "guest"), name, email, product page, and what is booked (a tour's date, a time, or nights).
    private static readonly (string User, string Name, string Email, string Page, string What)[] s_bookings =
    [
        ("jamie", "Jamie Chen", "jamie.chen@example.com", "/tours/alpine-lakes-tour", "date:1"),
        ("dana", "Dana Wu", "dana.wu@example.com", "/appointments/sports-massage-60-minutes", "date:3"),
        ("guest", "Casey Rowe", "casey.rowe@example.com", "/rooms/lakeview-room", "nights:2026-12-18:2026-12-21"),
    ];

    // Books as each buyer would: the date, time or nights, the cart, the checkout, and a bank transfer.
    private static async Task BookAsync(IBrowser browser)
    {
        foreach (var booking in s_bookings)
        {
            var buyer = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = booking.User == "guest" ? null : Path.Combine(Root, $"state-{booking.User}.json"), ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
            var page = await buyer.NewPageAsync();
            page.SetDefaultTimeout(40_000);
            await page.GotoAsync(BaseUrl + booking.Page);

            var parts = booking.What.Split(':');

            if (parts[0] == "date")
            {
                await page.Locator("select[name='details[booking.date]']").SelectOptionAsync(new SelectOptionValue { Index = int.Parse(parts[1]) });
            }
            else
            {
                await page.Locator("input[name='details[booking.date]']").FillAsync(parts[1]);
                await page.Locator("input[name='details[booking.end]']").FillAsync(parts[2]);
            }

            await page.GetByRole(AriaRole.Button, new() { Name = "Add to cart" }).First.ClickAsync();
            await Task.Delay(3000);
            await page.GotoAsync(BaseUrl + "/cart");
            await page.RunAndWaitForNavigationAsync(() => page.Locator("a,button").Filter(new() { HasTextString = "Checkout" }).First.ClickAsync());

            var travelers = page.Locator("input[name$='FullName.Text']");

            for (var t = 0; t < await travelers.CountAsync(); t++)
            {
                await travelers.Nth(t).FillAsync(t == 0 ? booking.Name : "Sam " + booking.Name.Split(' ')[1]);
            }

            var name = page.Locator("#Contact_Name");

            if (await name.CountAsync() > 0 && await name.InputValueAsync() == string.Empty)
            {
                await name.FillAsync(booking.Name);
            }

            var email = page.Locator("#Contact_Email");

            if (await email.CountAsync() > 0 && await email.InputValueAsync() == string.Empty)
            {
                await email.FillAsync(booking.Email);
            }

            await page.RunAndWaitForNavigationAsync(() => page.GetByRole(AriaRole.Button, new() { Name = "Continue" }).First.ClickAsync());
            await page.Locator("label[for='payment-method-bank-transfer']").ClickAsync();
            await page.RunAndWaitForNavigationAsync(() => page.GetByRole(AriaRole.Button, new() { Name = "Pay now" }).First.ClickAsync());
            Console.WriteLine($"{booking.Name}: {page.Url}");
            await buyer.CloseAsync();
        }
    }

    // Records each bank transfer as received, so the bookings are confirmed (as staff do when the money arrives).
    private static async Task ReceiveAsync(IBrowser browser)
    {
        var admin = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await admin.NewPageAsync();
        page.SetDefaultTimeout(60_000);
        await page.GotoAsync(BaseUrl + "/Admin/orders");

        var orders = await page.Locator("li.list-group-item.item").Filter(new() { HasTextString = "Payment due" }).Locator("a").EvaluateAllAsync<string[]>("links => links.map(a => a.getAttribute('href')).filter(h => h.includes('/orders/4'))");

        foreach (var order in orders.Distinct())
        {
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

            Console.WriteLine($"Received the transfer of {order}");
        }

        await admin.CloseAsync();
    }

    public static async Task PrepareAsync(IBrowser browser)
    {
        var stage = Environment.GetEnvironmentVariable("VIDEO_PREPARE_STAGE");

        if (stage == "book")
        {
            await BookAsync(browser);
            return;
        }

        if (stage == "receive")
        {
            await ReceiveAsync(browser);
            return;
        }

        var admin = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await admin.NewPageAsync();
        page.SetDefaultTimeout(120_000);

        await RunRecipeAsync(page, "Areas/CrestApps.OrchardCore.Bookings/Recipes", "bookings-sample.recipe.json");

        // Buyers pay by bank transfer, and staff record the transfers as received.
        await page.GotoAsync(BaseUrl + "/Admin/Settings/paylater");
        await page.Locator("#ISite_BankTransferSettings_Enabled").SetCheckedAsync(true);
        await page.Locator("#ISite_BankTransferSettings_Instructions").FillAsync("Alpine Lakes Travel, account 000987654, Springfield Credit Union. Use the order number as the reference.");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
        await admin.CloseAsync();

        foreach (var user in new[] { "jamie", "dana" })
        {
            await SaveSignInAsync(browser, user);
        }
    }
}
