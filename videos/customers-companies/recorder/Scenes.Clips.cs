using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Row(Scene scene, string text) => scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = text }).First;

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("button.save").First;

    private static ILocator LinkNamed(Scene scene, string name) => scene.Page.GetByRole(AriaRole.Link, new() { Name = name, Exact = true }).First;

    private static ILocator Labeled(Scene scene, string label) => scene.Page.GetByLabel(label).First;

    private static Task ClearAsync(Scene scene) => scene.ClearHighlightsAsync();

    // ----- Clips -----

    private static async Task FindAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/customers");
        await scene.StartCaptureAsync();

        await scene.StepAsync("find-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/customers"), "Commerce > Customers");
            await scene.PauseAsync(2600);
            await ClearAsync(scene);
            await scene.HighlightAsync(Row(scene, "Wingtip Hardware"), "Badges, email, last activity");
        });

        await ClearAsync(scene);

        await scene.StepAsync("find-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#Options_Search"), "Jamie", delay: 90);
            await scene.WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"));
        });

        await scene.StepAsync("find-3", async () =>
        {
            await scene.NavigateAsync("/Admin/customers");
            await scene.ClickAsync(scene.Page.Locator(".bootstrap-select button.dropdown-toggle").First, pauseAfter: 2200);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".bootstrap-select .dropdown-menu.show").GetByText("Businesses"));
        });
    }

    private static async Task EditAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/customers");
        await scene.StartCaptureAsync();

        await scene.StepAsync("edit-1", async () =>
        {
            await scene.HighlightAsync(LinkNamed(scene, "Add customer"));
            await scene.PauseAsync(1500);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Add customer"));
        });

        await scene.StepAsync("edit-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#Kind"), "Kind");
            await scene.PauseAsync(1800);
            await ClearAsync(scene);
            await scene.TypeAsync(scene.Page.Locator("#FirstName"), "Pat", delay: 70);
            await scene.TypeAsync(scene.Page.Locator("#LastName"), "Rivera", delay: 70);
            await scene.HighlightAsync(scene.Page.Locator("#DisplayName"), "Display name");
        });

        await ClearAsync(scene);

        await scene.StepAsync("edit-3", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#Kind"), "Business", pauseAfter: 800);
            await scene.TypeAsync(scene.Page.Locator("#CompanyName"), "Tailspin Toys", delay: 50);
            await scene.TypeAsync(scene.Page.Locator("#TaxId"), "TX-555-0199", delay: 50);
            await scene.ClickAsync(scene.Page.Locator("#IsTaxExempt"), pauseAfter: 500);
            await scene.TypeAsync(scene.Page.Locator("#TaxExemptionReason"), "Resale certificate 555-0100", delay: 40);
        });

        await scene.StepAsync("edit-4", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#Email"), "pat.rivera@example.com", delay: 45);
            await scene.TypeAsync(scene.Page.Locator("#Phone"), "555-0123", delay: 70);
            await scene.ClickAsync(scene.Page.Locator("#AcceptsMarketing"), pauseAfter: 500);
        });

        await scene.StepAsync("edit-5", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1200);
            await scene.ScrollAsync(0, 700);
        });
    }

    private static async Task AddressesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/customers");
        await scene.StartCaptureAsync();

        await scene.StepAsync("addr-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Sam Carter"));
            await scene.PauseAsync(600);
            await scene.Page.GetByText("Default billing").First.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(scene.Page.GetByText("Default billing").First, "Default billing");
        });

        await ClearAsync(scene);

        await scene.StepAsync("addr-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Add address"));
            await scene.HighlightAsync(scene.Page.Locator("#Address_Country"), "Country");
        });

        await ClearAsync(scene);

        await scene.StepAsync("addr-3", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#Address_Country"), "United Kingdom", pauseAfter: 300);
            await scene.Page.Locator("#Address_Country").DispatchEventAsync("change");
            await scene.PauseAsync(3000);
            await scene.SelectAsync(scene.Page.Locator("#Address_Country"), "United States", pauseAfter: 300);
            await scene.Page.Locator("#Address_Country").DispatchEventAsync("change");
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("addr-4", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#Address_AddressLine1"), "200 Pine Street", delay: 20);
            await scene.TypeAsync(scene.Page.Locator("#Address_City"), "Seattle", delay: 20);
            await scene.SelectAsync(scene.Page.Locator("#Address_Region"), "Washington", pauseAfter: 150);
            await scene.TypeAsync(scene.Page.Locator("#Address_PostalCode"), "98101", delay: 30);
            await scene.TypeAsync(scene.Page.Locator("#Label"), "Warehouse", delay: 20);
            await scene.ClickAsync(scene.Page.Locator("#IsDefaultShipping"), pauseAfter: 200);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.StepAsync("addr-5", async () =>
        {
            await scene.Page.GetByText("Warehouse").First.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(scene.Page.Locator(".btn-danger").Filter(new() { HasTextString = "Delete" }).First, "Delete");
        });
    }

    private static async Task CreditAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/customers?Options.Filter=WithAccount");
        await scene.StartCaptureAsync();

        await scene.StepAsync("credit-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Jamie Chen"));
            await scene.PauseAsync(500);
            await scene.Page.Locator("#store-credit-amount").ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(400);
            await scene.HighlightAsync(scene.Page.Locator("#store-credit-amount"), "Store credit");
        });

        await ClearAsync(scene);

        await scene.StepAsync("credit-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#store-credit-amount"), "25", delay: 90);
            await scene.HighlightAsync(scene.Page.Locator("#store-credit-currency"), "Currency");
            await scene.PauseAsync(1200);
            await ClearAsync(scene);
            await scene.TypeAsync(scene.Page.Locator("#store-credit-reason"), "Late delivery", delay: 50);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("form").Filter(new() { Has = scene.Page.Locator("#store-credit-amount") }).GetByRole(AriaRole.Button, new() { Name = "Change" }));
        });

        await scene.StepAsync("credit-3", async () =>
        {
            await scene.Page.Locator("#store-credit-amount").ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(scene.Page.GetByText("Late delivery").First, "The change, with its reason");
        });

        await ClearAsync(scene);

        await scene.StepAsync("credit-4", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#loyalty-points"), "500", delay: 90);
            await scene.TypeAsync(scene.Page.Locator("#loyalty-reason"), "Welcome bonus", delay: 50);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("form").Filter(new() { Has = scene.Page.Locator("#loyalty-points") }).GetByRole(AriaRole.Button, new() { Name = "Change" }));
            await scene.Page.Locator("#loyalty-points").ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(scene.Page.GetByText("points worth").First, "Points and their worth");
        });
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/customers");
        await scene.StartCaptureAsync();

        await scene.StepAsync("set-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/customers"), "Settings > Commerce > Customers");
            await scene.PauseAsync(2600);
            await ClearAsync(scene);
            await scene.HighlightAsync(Labeled(scene, "Billing address at checkout"), "Billing address at checkout");
        });

        await ClearAsync(scene);

        await scene.StepAsync("set-2", async () =>
        {
            await scene.HighlightAsync(Labeled(scene, "Erase inactive guests after"), "Erase inactive guests");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.HighlightAsync(Labeled(scene, "Anonymize guest orders after"), "Anonymize guest orders");
        });

        await ClearAsync(scene);

        await scene.StepAsync("set-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/Settings/address-suggestions"));
            await scene.HighlightAsync(Labeled(scene, "Google Places API key"), "Google Places API key");
        });

        await ClearAsync(scene);

        await scene.StepAsync("set-4", async () =>
        {
            await scene.NavigateAsync("/Admin/Settings/loyalty");
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/loyalty"), "Settings > Commerce > Rewards");
            await scene.PauseAsync(2000);
            await ClearAsync(scene);
            await scene.HighlightAsync(Labeled(scene, "Program name"));
            await scene.PauseAsync(1300);
            await ClearAsync(scene);
            await scene.HighlightAsync(Labeled(scene, "Points per unit spent"));
            await scene.PauseAsync(1300);
            await ClearAsync(scene);
            await scene.HighlightAsync(Labeled(scene, "What a point is worth"));
            await scene.PauseAsync(1300);
            await ClearAsync(scene);
            await scene.HighlightAsync(Labeled(scene, "Fewest points to spend"));
        });

        await ClearAsync(scene);

        await scene.StepAsync("set-5", async () =>
        {
            await scene.NavigateAsync("/Admin/Settings/referrals");
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/referrals"), "Settings > Commerce > Referrals");
            await scene.PauseAsync(2000);
            await ClearAsync(scene);
            await scene.HighlightAsync(Labeled(scene, "What a friend gets"), "What a friend gets");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.HighlightAsync(Labeled(scene, "What the customer gets"), "What the customer gets");
        });

        await ClearAsync(scene);

        await scene.StepAsync("set-6", async () =>
        {
            await scene.HighlightAsync(Labeled(scene, "Least a friend's first order spends"), "Least a friend's first order spends");
        });
    }

    private static async Task PrivacyAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/customers");
        await scene.StartCaptureAsync();

        await scene.StepAsync("priv-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Avery Stone"));
            await scene.PauseAsync(500);
            await scene.Page.Locator(".btn").Filter(new() { HasTextString = "Download data" }).ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(scene.Page.Locator(".btn").Filter(new() { HasTextString = "Download data" }), "Download data");
        });

        await ClearAsync(scene);

        await scene.StepAsync("priv-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Erase personal data" }), "Erase personal data");
        });

        await ClearAsync(scene);

        await scene.StepAsync("priv-3", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Erase personal data" }), pauseAfter: 1800);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalOkButton"));
        });

        await scene.StepAsync("priv-4", async () =>
        {
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.GetByText("This customer's personal data was erased").First);
            await scene.PauseAsync(4500);
            await ClearAsync(scene);
            await scene.ScrollAsync(0, 1500);
        });
    }

    private static async Task AccountAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, Path.Combine(Root, "state-jamie.json"));

        await scene.OpenAsync("/account/details");
        await scene.StartCaptureAsync();

        await scene.StepAsync("acct-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#Kind"), "I buy for");
            await scene.PauseAsync(3200);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator("#FirstName"), "Name");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator("#account-email"), "Changed on the account");
        });

        await ClearAsync(scene);

        await scene.StepAsync("acct-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Addresses"));
        });

        await scene.StepAsync("acct-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Store credit"));
            await scene.PauseAsync(3500);
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Rewards"));
        });

        await scene.StepAsync("acct-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Refer a friend"));
        });
    }

    private static async Task PriceListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/companies/price-lists");
        await scene.StartCaptureAsync();

        await scene.StepAsync("pl-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/companies/price-lists"), "Commerce > Price lists");
            await scene.PauseAsync(2200);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Add price list"));
        });

        await scene.StepAsync("pl-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#Name"), "Wholesale", delay: 70);
            await scene.TypeAsync(scene.Page.Locator("#PercentOff"), "10", delay: 90);
            await scene.HighlightAsync(scene.Page.Locator("#PercentOff"), "Percentage off everything else");
        });

        await ClearAsync(scene);

        await scene.StepAsync("pl-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#Prices"), "T-S-86309, 14.50\nHOO-60268, 44.00", delay: 45);
            await scene.HighlightAsync(scene.Page.Locator("#Prices"), "Prices by SKU");
        });

        await ClearAsync(scene);

        await scene.StepAsync("pl-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }

    private static async Task CompanyAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/companies");
        await scene.StartCaptureAsync();

        await scene.StepAsync("co-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/companies"), "Commerce > Companies");
            await scene.PauseAsync(2200);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Add company"));
        });

        await scene.StepAsync("co-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#Name"), "Adventure Works Outfitters", delay: 45);
            await scene.TypeAsync(scene.Page.Locator("#TaxId"), "TAX-555-0100", delay: 50);
            await scene.TypeAsync(scene.Page.Locator("#BillingEmail"), "ap@adventure-works.example", delay: 40);
        });

        await scene.StepAsync("co-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#Buyers_0__User"), "jamie", delay: 70);
            await scene.TypeAsync(scene.Page.Locator("#Buyers_0__OrderLimit"), "500", delay: 90);
            await scene.TypeAsync(scene.Page.Locator("#Buyers_1__User"), "dana", delay: 70);
            await scene.SelectAsync(scene.Page.Locator("#Buyers_1__Role"), "Admin", pauseAfter: 600);
            await scene.HighlightAsync(scene.Page.Locator("#Buyers_0__OrderLimit"), "Order limit");
        });

        await ClearAsync(scene);

        await scene.StepAsync("co-4", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#NetDays"), "30", delay: 90);
            await scene.TypeAsync(scene.Page.Locator("#DepositPercent"), "20", delay: 90);
            await scene.TypeAsync(scene.Page.Locator("#CreditLimit"), "5000", delay: 90);
        });

        await scene.StepAsync("co-5", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#MonthlyBudget"), "2000", delay: 90);
            await scene.ClickAsync(scene.Page.Locator("#RequirePurchaseOrder"), pauseAfter: 600);
        });

        await scene.StepAsync("co-6", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#PriceListId"), "Wholesale", pauseAfter: 700);
            await scene.TypeAsync(scene.Page.Locator("#Locations_0__Name"), "Head office", delay: 50);
            await scene.SelectAsync(scene.Page.Locator("#Locations_0__Address_Country"), "United States", pauseAfter: 700);
            await scene.TypeAsync(scene.Page.Locator("#Locations_0__Address_AddressLine1"), "1 Pioneer Square", delay: 40);
            await scene.TypeAsync(scene.Page.Locator("#Locations_0__Address_City"), "Seattle", delay: 50);
            await scene.SelectAsync(scene.Page.Locator("#Locations_0__Address_Region"), "Washington", pauseAfter: 400);
            await scene.TypeAsync(scene.Page.Locator("#Locations_0__Address_PostalCode"), "98104", delay: 70);
        });

        await scene.StepAsync("co-7", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#IsEnabled"), "On");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }

    private static async Task PunchoutAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/companies");
        await scene.StartCaptureAsync();

        await scene.StepAsync("punch-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Northwind Traders"));
            await scene.PauseAsync(500);
            await scene.Page.Locator("legend").Filter(new() { HasTextString = "Punchout" }).ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(scene.Page.Locator("legend").Filter(new() { HasTextString = "Punchout" }), "Punchout (OCI)");
        });

        await ClearAsync(scene);

        await scene.StepAsync("punch-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#PunchoutEnabled"), pauseAfter: 700);
            await scene.TypeAsync(scene.Page.Locator("#PunchoutUser"), "morgan", delay: 70);
            await scene.HighlightAsync(scene.Page.Locator("#PunchoutUser"), "Punchout buyer");
        });

        await ClearAsync(scene);

        await scene.StepAsync("punch-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#NewPunchoutKey"), "Make a new key");
        });
    }
}
