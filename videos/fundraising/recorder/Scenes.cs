using Microsoft.Playwright;

namespace Recorder;

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
        ("03-receipts", ReceiptsAsync),
        ("04-campaign", CampaignAsync),
        ("05-give", GiveAsync),
        ("06-orders", OrdersAsync),
        ("07-account", AccountAsync),
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

    // The fake donors: user, full name, amount, who it honors. They give by bank transfer, so no payment provider is involved.
    private static readonly (string User, string Name, string Email, string Amount, string Tribute)[] s_donors =
    [
        ("jamie", "Jamie Chen", "jamie.chen@example.com", "50", "Margaret Hale"),
        ("dana", "Dana Wu", "dana.wu@example.com", "250", ""),
        ("guest", "Casey Rowe", "casey.rowe@example.com", "35", "Walter Rowe"),
        ("guest", "Alex Moreno", "alex.moreno@example.com", "100", ""),
    ];

    // Gives as each donor would: a suggested amount or their own, the checkout, and a bank transfer.
    private static async Task DonateAsync(IBrowser browser)
    {
        foreach (var donor in s_donors)
        {
            var buyer = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = donor.User == "guest" ? null : Path.Combine(Root, $"state-{donor.User}.json"), ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
            var page = await buyer.NewPageAsync();
            page.SetDefaultTimeout(60_000);
            await page.GotoAsync(BaseUrl + "/give/community-library");

            var suggested = page.Locator($"[data-donation-amount='{donor.Amount}']");

            if (await suggested.CountAsync() > 0)
            {
                await suggested.First.ClickAsync();
            }
            else
            {
                await page.Locator("input[name='details[donation-amount]'], input[id$='-amount']").First.FillAsync(donor.Amount);
            }

            if (donor.Tribute.Length > 0)
            {
                await page.Locator("input[id$='-tribute']").First.FillAsync(donor.Tribute);
            }

            await page.GetByRole(AriaRole.Button, new() { Name = "Add to cart" }).First.ClickAsync();
            await Task.Delay(3000);
            await page.GotoAsync(BaseUrl + "/cart");
            await page.RunAndWaitForNavigationAsync(() => page.Locator("a,button").Filter(new() { HasTextString = "Checkout" }).First.ClickAsync());

            var name = page.GetByLabel("Full name");

            if (await name.InputValueAsync() == string.Empty)
            {
                await name.FillAsync(donor.Name);
            }

            var email = page.GetByLabel("Email");

            if (await email.CountAsync() > 0 && await email.First.InputValueAsync() == string.Empty)
            {
                await email.First.FillAsync(donor.Email);
            }

            await page.RunAndWaitForNavigationAsync(() => page.GetByRole(AriaRole.Button, new() { Name = "Continue" }).First.ClickAsync());
            try
            {
                await page.Locator("label[for='payment-method-bank-transfer']").ClickAsync(new() { Timeout = 15_000 });
            }
            catch (TimeoutException)
            {
                Console.WriteLine($"{donor.User} stuck at {page.Url}: {(await page.Locator("body").InnerTextAsync()).Replace(Environment.NewLine, " | ")}");
                throw;
            }

            await page.RunAndWaitForNavigationAsync(() => page.GetByRole(AriaRole.Button, new() { Name = "Pay now" }).First.ClickAsync());
            Console.WriteLine($"{donor.User}: {page.Url} :: {(await page.Locator("#storefront-main").InnerTextAsync()).Replace(Environment.NewLine, " | ")[..Math.Min(400, (await page.Locator("#storefront-main").InnerTextAsync()).Length)]}");
            await buyer.CloseAsync();
        }
    }

    // Records each bank transfer as received, so the gifts count toward the campaign (as staff do when the money arrives).
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
        if (Environment.GetEnvironmentVariable("VIDEO_PREPARE_STAGE") == "receive")
        {
            await ReceiveAsync(browser);
            return;
        }

        if (Environment.GetEnvironmentVariable("VIDEO_PREPARE_STAGE") == "donate")
        {
            await DonateAsync(browser);
            return;
        }

        var admin = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await admin.NewPageAsync();
        page.SetDefaultTimeout(120_000);

        await RunRecipeAsync(page, "Areas/CrestApps.OrchardCore.Fundraising/Recipes", "fundraising-sample.recipe.json");

        // Donors give by bank transfer, and their gifts are recorded as received on the orders.
        await page.GotoAsync(BaseUrl + "/Admin/Settings/paylater");
        await page.Locator("#ISite_BankTransferSettings_Enabled").SetCheckedAsync(true);
        await page.Locator("#ISite_BankTransferSettings_Instructions").FillAsync("Community Library Fund, account 000123456, Springfield Credit Union. Use the order number as the reference.");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
        await admin.CloseAsync();

        foreach (var user in new[] { "jamie", "riley", "morgan", "dana" })
        {
            await SaveSignInAsync(browser, user);
        }
    }
}
