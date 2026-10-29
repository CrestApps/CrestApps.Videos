using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Customers and Companies video, one method per "clip" of storyboard.json.
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
        ("03-find", FindAsync),
        ("04-edit", EditAsync),
        ("05-addresses", AddressesAsync),
        ("06-credit", CreditAsync),
        ("07-settings", SettingsAsync),
        ("08-privacy", PrivacyAsync),
        ("09-account", AccountAsync),
        ("10-pricelist", PriceListAsync),
        ("11-company", CompanyAsync),
        ("13-punchout", PunchoutAsync),
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

    // The fake buyers (user name, full name, email): they all sign in with the demo password.
    private static readonly (string User, string First, string Last, string Email)[] s_people =
    [
        ("jamie", "Jamie", "Chen", "jamie.chen@example.com"),
        ("riley", "Riley", "Park", "riley.park@example.com"),
        ("morgan", "Morgan", "Lee", "morgan.lee@example.com"),
        ("dana", "Dana", "Wu", "dana.wu@example.com"),
    ];

    /// <summary>
    /// Enables the features, creates the fake buyers' accounts, and makes the two customers with an account (not recorded).
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = StatePath,
            ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height },
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        await page.GotoAsync(BaseUrl + "/Admin/Features");

        foreach (var feature in new[]
        {
            "CrestApps.OrchardCore.Companies",
            "CrestApps.OrchardCore.Customers.StoreCredit",
            "CrestApps.OrchardCore.Customers.Loyalty",
            "CrestApps.OrchardCore.Customers.Referrals",
            "CrestApps.OrchardCore.Customers.CheckoutDetails",
            "CrestApps.OrchardCore.Addresses.GooglePlaces",
        })
        {
            var status = await page.EvaluateAsync<int>(
                "async id => { const token = document.querySelector('input[name=__RequestVerificationToken]').value; const r = await fetch('/Admin/Features/' + id + '/Enable', { method: 'POST', credentials: 'include', headers: { RequestVerificationToken: token } }); return r.status; }",
                feature);
            Console.WriteLine($"Enabled {feature}: {status}");
        }

        foreach (var (user, first, last, email) in s_people)
        {
            await page.GotoAsync(BaseUrl + "/Admin/Users/Create");

            if (await page.Locator("#User_UserName").CountAsync() == 0)
            {
                continue;
            }

            await page.Locator("#User_UserName").FillAsync(user);
            await page.Locator("#User_Email").FillAsync(email);
            await page.Locator("#User_Password").FillAsync(Password);
            await page.Locator("#User_PasswordConfirmation").FillAsync(Password);
            await page.Locator("button.save").First.ClickAsync();

            try
            {
                await page.WaitForURLAsync(url => !url.Contains("/Create", StringComparison.OrdinalIgnoreCase), new() { Timeout = 10_000 });
                Console.WriteLine($"Created {user}");
            }
            catch (TimeoutException)
            {
                Console.WriteLine($"{user} exists");
            }
        }

        // More customers for the list: a guest, and a second business.
        await AddCustomerAsync(page, "Person", "Avery", "Stone", null, "avery.stone@example.com", "555-0142");
        await AddCustomerAsync(page, "Business", "Casey", "Murphy", "Wingtip Hardware", "orders@wingtip.example", "555-0177");

        // A company with two buyers, for the list.
        await page.GotoAsync(BaseUrl + "/Admin/companies");

        if (await page.GetByText("Northwind Traders").CountAsync() == 0)
        {
        await page.GotoAsync(BaseUrl + "/Admin/companies/create");
        await page.Locator("#Name").FillAsync("Northwind Traders");
        await page.Locator("#BillingEmail").FillAsync("accounts@northwind.example");
        await page.Locator("#Buyers_0__User").FillAsync("riley");
        await page.Locator("#Buyers_0__Role").SelectOptionAsync("Admin");
        await page.Locator("#Buyers_1__User").FillAsync("morgan");
        await page.Locator("#NetDays").FillAsync("30");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("button.save").First.ClickAsync());
        Console.WriteLine("Created company Northwind Traders");
        }

        await context.CloseAsync();

        // Signing in and opening the account page makes a customer with an account, as the first order does.
        foreach (var user in new[] { "jamie", "riley", "dana" })
        {
            var buyer = await browser.NewContextAsync(new BrowserNewContextOptions { ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
            var buyerPage = await buyer.NewPageAsync();
            buyerPage.SetDefaultTimeout(60_000);
            await buyerPage.GotoAsync(BaseUrl + "/Login");
            await buyerPage.Locator("#LoginForm_UserName").FillAsync(user);
            await buyerPage.Locator("#LoginForm_Password").FillAsync(Password);
            await buyerPage.Locator("#LoginForm_Password").PressAsync("Enter");
            await buyerPage.WaitForURLAsync(url => !url.Contains("/Login", StringComparison.OrdinalIgnoreCase));
            await buyerPage.GotoAsync(BaseUrl + "/account/details");
            Console.WriteLine($"{user}: {await buyerPage.TitleAsync()}");
            await buyer.StorageStateAsync(new() { Path = Path.Combine(Root, $"state-{user}.json") });
            await buyer.CloseAsync();
        }

        // Their customers start with the user name: give them their names.
        var admin = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var adminPage = await admin.NewPageAsync();
        adminPage.SetDefaultTimeout(60_000);

        foreach (var (user, first, last, _) in new[] { ("jamie", "Jamie", "Chen", ""), ("riley", "Riley", "Park", ""), ("dana", "Dana", "Wu", ""), ("admin", "Taylor", "Brooks", "") })
        {
            await adminPage.GotoAsync(BaseUrl + "/Admin/customers");

            if (await adminPage.Locator($"a:text-is('{user}')").CountAsync() == 0)
            {
                continue;
            }

            await adminPage.RunAndWaitForNavigationAsync(() => adminPage.Locator($"a:text-is('{user}')").First.ClickAsync());
            await adminPage.Locator("#FirstName").FillAsync(first);
            await adminPage.Locator("#LastName").FillAsync(last);
            await adminPage.Locator("#DisplayName").FillAsync(string.Empty);
            await adminPage.Locator("#Phone").FillAsync("555-0110");
            await adminPage.RunAndWaitForNavigationAsync(() => adminPage.Locator("button.save").First.ClickAsync());
            Console.WriteLine($"Named {first} {last}");
        }

        await admin.CloseAsync();
    }

    private static async Task AddCustomerAsync(IPage page, string kind, string first, string last, string company, string email, string phone)
    {
        await page.GotoAsync(BaseUrl + "/Admin/customers");

        if (await page.GetByText(company ?? $"{first} {last}").CountAsync() > 0)
        {
            return;
        }

        await page.GotoAsync(BaseUrl + "/Admin/customers/create");
        await page.Locator("#Kind").SelectOptionAsync(kind);
        await page.Locator("#FirstName").FillAsync(first);
        await page.Locator("#LastName").FillAsync(last);

        if (company is not null)
        {
            await page.Locator("#CompanyName").FillAsync(company);
        }

        await page.Locator("#Email").FillAsync(email);
        await page.Locator("#Phone").FillAsync(phone);
        await page.RunAndWaitForNavigationAsync(() => page.Locator("button.save").First.ClickAsync());
        Console.WriteLine($"Added customer {first} {last}");
    }
}
