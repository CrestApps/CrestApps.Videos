using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Taxation video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-categories", CategoriesAsync),
        ("04-jurisdictions", JurisdictionsAsync),
        ("05-rules", RulesAsync),
        ("06-origin", OriginAsync),
        ("07-classify", ClassifyAsync),
        ("08-checkout", CheckoutAsync),
        ("09-more", MoreAsync),
    ];

    /// <summary>
    /// Turns on the TaxJar feature (not recorded). The site is the one the Orders video prepared;
    /// the tax categories, places and rules are made on camera.
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var page = await Seed.NewPageAsync(browser);

        await Seed.EnableAsync(page, "CrestApps.OrchardCore.Taxation.TaxJar");
        await page.Context.CloseAsync();
    }

    // Picks the option of a drop-down whose text contains a word, with a visible click on the drop-down first.
    private static async Task PickAsync(Scene scene, ILocator select, string contains, int pauseAfter = 700)
    {
        await scene.ClickAsync(select, pauseAfter: 300);
        await select.EvaluateAsync("(select, text) => { const option = [...select.options].find(o => o.text.includes(text)); select.value = option.value; select.dispatchEvent(new Event('change', { bubbles: true })); }", contains);
        await scene.PauseAsync(pauseAfter);
    }

    private static ILocator Fld(Scene scene, string name) => scene.Page.Locator($"[name='{name}']").First;

    private static ILocator Summary(Scene scene) => scene.Page.Locator(".card").Filter(new() { HasTextString = "Order summary" }).First;

    private static async Task CategoriesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/taxation/categories");
        await scene.StartCaptureAsync();

        await scene.StepAsync("cat-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/taxation/categories"), "Commerce > Taxation > Categories", 2400);
            await scene.ClickAndWaitForNavigationAsync(Link(scene, "Add Tax Category"));
        });

        await scene.StepAsync("cat-2", async () =>
        {
            await scene.TypeAsync(Fld(scene, "TaxCategory.Name"), "Clothing", delay: 60);
            await scene.TypeAsync(Fld(scene, "TaxCategory.Code"), "CLOTHING", delay: 50);
            await ShowAsync(scene, Field(scene, "Parent code"), "Parent code", 1500);
            await scene.HighlightAsync(Field(scene, "Description"), "Description");
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.StepAsync("cat-3", async () =>
        {
            await scene.NavigateAsync("/Admin/taxation/types");
            await scene.HighlightAsync(Link(scene, "Add Tax Type"), "Add Tax Type");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task JurisdictionsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/taxation/jurisdictions");
        await scene.StartCaptureAsync();

        await scene.StepAsync("jur-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/taxation/jurisdictions"), "Commerce > Taxation > Jurisdictions", 2400);
            await scene.ClickAndWaitForNavigationAsync(Link(scene, "Add Tax Jurisdiction"));
        });

        await scene.StepAsync("jur-2", async () =>
        {
            await scene.TypeAsync(Fld(scene, "TaxJurisdiction.Name"), "New York", delay: 60);
            await scene.TypeAsync(Fld(scene, "TaxJurisdiction.Code"), "US-NY", delay: 60);
            await PickAsync(scene, Fld(scene, "TaxJurisdiction.Level"), "State");
            await PickAsync(scene, Fld(scene, "TaxJurisdiction.Country"), "United States");
            await scene.HighlightAsync(Field(scene, "Postal code"), "Region, County, City and Postal code");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("jur-3", async () =>
        {
            await ShowAsync(scene, Field(scene, "Parent jurisdiction"), "Parent jurisdiction", 2200);
            await ShowAsync(scene, Field(scene, "Effective from"), "Effective from and to", 2200);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }

    private static async Task RulesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/taxation/rules");
        await scene.StartCaptureAsync();

        await scene.StepAsync("rule-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/taxation/rules"), "Commerce > Taxation > Rules", 2200);
            await scene.ClickAsync(Link(scene, "Add Tax Rule"), pauseAfter: 3000);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/rules/create/Percentage']").First);
        });

        await scene.StepAsync("rule-2", async () =>
        {
            await scene.TypeAsync(Fld(scene, "TaxRule.Name"), "New York sales tax", delay: 40);
            await PickAsync(scene, Fld(scene, "TaxRule.TaxType"), "SalesTax");
            await scene.TypeAsync(Fld(scene, "TaxRule.TaxName"), "Sales tax", delay: 50);
            await ShowAsync(scene, Field(scene, "Jurisdiction"), "Any jurisdiction, or one place", 1800);
            await PickAsync(scene, Fld(scene, "TaxRule.CategoryCode"), "Clothing");
        });

        await scene.StepAsync("rule-3", async () =>
        {
            await ShowAsync(scene, Field(scene, "Customer type"), "Customer type", 1800);
            await ShowAsync(scene, Field(scene, "Minimum amount"), "Minimum and Maximum amount", 2000);
            await scene.HighlightAsync(Field(scene, "Tax is included in the item price"), "Tax is included in the item price");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rule-4", async () =>
        {
            await ShowAsync(scene, Field(scene, "Tax is compound"), "Compound", 1500);
            await ShowAsync(scene, Field(scene, "Reverse charge"), "Reverse charge", 1500);
            await ShowAsync(scene, Field(scene, "Applies to shipping charges"), "Applies to shipping charges", 1500);
            await scene.TypeAsync(Fld(scene, "TaxRule.Rate"), "0.0875", delay: 80);
            await scene.ClickAsync(SaveButton(scene), pauseAfter: 500);

            // Saving a rule takes the site a while: the wait is cut out of the clip.
            await scene.OffCameraAsync(() => scene.Page.WaitForURLAsync("**/Admin/taxation/rules", new() { Timeout = 120_000 }));
            await scene.NavigateAsync("/Admin/taxation/rules");
        });
    }

    private static async Task OriginAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/tax-origin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("org-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/Settings/tax-origin"), "Settings > Commerce > Tax origin", 2200);
            await scene.TypeAsync(Fld(scene, "ISite.TaxOriginSettings.Country"), "US", delay: 90);
            await scene.TypeAsync(Fld(scene, "ISite.TaxOriginSettings.Region"), "NY", delay: 90);
            await scene.HighlightAsync(Field(scene, "Street address"), "Street address");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }

    private static async Task ClassifyAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.StartCaptureAsync();

        await scene.StepAsync("cls-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/ContentTypes/List"), "Content Definition > Content Types", 1800);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/ContentTypes/Edit/Product']").First);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='AddPartsTo/Product']").First);
        });

        await scene.StepAsync("cls-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("div.form-check").Filter(new() { HasTextString = "Taxation" }).First.Locator("input[type=checkbox]"), pauseAfter: 1500);
            await scene.HighlightAsync(scene.Page.Locator("div.form-check").Filter(new() { HasTextString = "Taxation" }).First, "Taxation");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button, input[type=submit]").Filter(new() { HasTextString = "Add" }).Last);
        });

        await scene.StepAsync("cls-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='TaxationPart/Edit']").First);
            await PickAsync(scene, Field(scene, "Default tax category").Locator("select").First, "Clothing");
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.StepAsync("cls-4", async () =>
        {
            await scene.NavigateAsync("/Admin/Contents/ContentItems/Product");
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Trail Runner Shoe").Locator("a:has-text('Edit')").First);
            await ScrollAsync(scene, 0, 3500);
            await scene.HighlightAsync(Field(scene, "Taxable"), "Taxation");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await PickAsync(scene, Field(scene, "Tax category").Locator("select").First, "Clothing", 1200);
            await scene.ClickAndWaitForNavigationAsync(Publish(scene));
        });
    }

    private static async Task CheckoutAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        var page = scene.Page;
        await page.GotoAsync(BaseUrl + "/shop", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator(".card").Filter(new() { HasTextString = "Trail Runner Shoe" }).First.Locator("button:has-text('Add to cart')").ClickAsync();
        await page.WaitForTimeoutAsync(1500);
        await page.GotoAsync(BaseUrl + "/cart", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.RunAndWaitForNavigationAsync(() => page.Locator(".card").Filter(new() { HasTextString = "Summary" }).Locator("a, button").Filter(new() { HasTextString = "Checkout" }).First.ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.GetByLabel("Full name").FillAsync("Jamie Chen");
        await page.GetByLabel("Email").FillAsync("customer@example.com");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#Shipping_Address_Country").SelectOptionAsync(new SelectOptionValue { Label = "United States" });
        await page.WaitForTimeoutAsync(1200);
        await page.Locator("#Shipping_Address_Name").FillAsync("Jamie Chen");
        await page.Locator("#Shipping_Address_AddressLine1").FillAsync("350 Fifth Avenue");
        await page.Locator("#Shipping_Address_City").FillAsync("New York");
        await page.Locator("#Shipping_Address_Region").SelectOptionAsync(new SelectOptionValue { Label = "New York" });
        await page.Locator("#Shipping_Address_PostalCode").FillAsync("10118");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("input[type=radio]").First.CheckAsync();
        await page.RunAndWaitForNavigationAsync(() => page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Mouse.MoveAsync(700, 500);

        await scene.StartCaptureAsync();

        await scene.StepAsync("chk-1", async () =>
        {
            await scene.HighlightAsync(Summary(scene), "Subtotal, tax and total");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("chk-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("div, li, tr").Filter(new() { HasTextString = "Due now" }).Last, "Due now");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task MoreAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/taxation/tables");
        await scene.StartCaptureAsync();

        await scene.StepAsync("more-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/taxation/tables"), "Commerce > Taxation > Tables", 2200);
            await scene.ClickAndWaitForNavigationAsync(Link(scene, "Add Tax Table"));
            await scene.ClickAsync(Link(scene, "Add row"), pauseAfter: 1800);
        });

        await scene.StepAsync("more-2", async () =>
        {
            await scene.NavigateAsync("/Admin/Settings/taxjar");
            await scene.HighlightAsync(scene.Page.Locator("form").Filter(new() { HasTextString = "TaxJar" }).Last, "TaxJar");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("more-3", async () =>
        {
            await scene.NavigateAsync("/Admin/Recipes");
            await scene.TypeAsync(scene.Page.GetByPlaceholder("Search"), "EU VAT", delay: 70);
            await scene.PauseAsync(1500);
        });
    }
}
