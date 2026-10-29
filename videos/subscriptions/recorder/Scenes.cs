using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Subscriptions video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-plan", PlanAsync),
        ("04-signup", SignupAsync),
        ("05-agreements", AgreementsAsync),
        ("06-settings", SettingsAsync),
        ("07-memberships", MembershipsAsync),
    ];

    /// <summary>
    /// Turns on the subscription features and the test payment method, runs the Commerce starter and Memberships sample recipes
    /// (a Membership plan, a Plans page, a Member role, an Annual Membership), and adds a customer account (not recorded).
    /// The site is the one the Orders video prepared.
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var page = await Seed.NewPageAsync(browser);

        if (Environment.GetEnvironmentVariable("VIDEO_STAGE") == "guest")
        {
            await Seed.SubmitAsync(page, "/Admin/Settings/subscriptions", new() { ["ISite.SubscriptionSettings.AllowGuestSignup"] = "true" }, "css:button.btn-primary[type=submit]");
            await page.Context.CloseAsync();

            return;
        }

        if (Environment.GetEnvironmentVariable("VIDEO_STAGE") == "buy")
        {
            await SignUpAsync(browser, "/membership/annual-membership", "jamie", "Password1!");
            await Seed.SubmitAsync(page, "/Admin/Settings/subscriptions", new() { ["ISite.SubscriptionSettings.AllowGuestSignup"] = "true" }, "css:button.btn-primary[type=submit]");
            await page.Context.CloseAsync();

            return;
        }

        if (Environment.GetEnvironmentVariable("VIDEO_STAGE") != "members")
        {
            await Seed.EnableAsync(page, "CrestApps.OrchardCore.Subscriptions", "CrestApps.OrchardCore.Subscriptions.Installments", "CrestApps.OrchardCore.Memberships", "CrestApps.OrchardCore.Checkout.TestProvider");
            await RunRecipeAsync(page, "commerce-starter");
        }

        await RunRecipeAsync(page, "memberships-sample");
        await Seed.SubmitAsync(page, "/Admin/Settings/subscriptions", new() { ["ISite.SubscriptionSettings.AllowGuestSignup"] = "true" }, "css:button.btn-primary[type=submit]");

        await Seed.SubmitAsync(
            page,
            "/Admin/Users/Create",
            new() { ["User.UserName"] = "jamie", ["User.Email"] = "jamie.chen@example.com", ["User.Password"] = "Password1!", ["User.PasswordConfirmation"] = "Password1!" },
            "css:button.btn-primary[type=submit]");

        await page.Context.CloseAsync();
    }

    // Signs a customer in and signs them up to a plan: through the checkout, paying with the test payment method.
    private static async Task SignUpAsync(IBrowser browser, string path, string user, string password)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        await page.GotoAsync(BaseUrl + "/Login");
        await page.Locator("#LoginForm_UserName").FillAsync(user);
        await page.Locator("#LoginForm_Password").FillAsync(password);
        await page.RunAndWaitForNavigationAsync(() => page.Locator("#LoginForm_Password").PressAsync("Enter"));

        await page.GotoAsync(BaseUrl + path, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.RunAndWaitForNavigationAsync(() => page.Locator("button:has-text('Sign up'), a:has-text('Sign up')").First.ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });

        for (var step = 0; step < 6 && !page.Url.Contains("/Confirmation", StringComparison.OrdinalIgnoreCase); step++)
        {
            Console.WriteLine("  checkout step: " + page.Url);

            var test = page.GetByText("Test payment").First;

            if (await test.CountAsync() > 0 && await test.IsVisibleAsync())
            {
                await test.ClickAsync();
            }

            await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        }

        Console.WriteLine("Signed up " + user + ": " + page.Url);
        await context.CloseAsync();
    }

    private static async Task RunRecipeAsync(IPage page, string fileName)
    {
        await page.GotoAsync(BaseUrl + "/Admin/Recipes");
        await page.Locator($"a[href*='{fileName}']").First.ClickAsync();
        await page.WaitForTimeoutAsync(1500);

        var confirm = page.Locator(".modal.show .btn-primary, .modal.show .btn-danger");

        if (await confirm.CountAsync() > 0)
        {
            await page.RunAndWaitForNavigationAsync(() => confirm.First.ClickAsync());
        }

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Console.WriteLine($"Ran the {fileName} recipe");
    }
}
