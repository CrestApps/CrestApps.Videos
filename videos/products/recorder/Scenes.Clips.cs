using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-currencies", CurrenciesAsync),
        ("04-product", ProductAsync),
        ("05-pricing", PricingAsync),
        ("06-type", ContentTypeAsync),
        ("07-listing", ListingAsync),
        ("08-extras", ExtrasAsync),
        ("09-feeds", FeedsAsync),
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

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    // The block of an editor that holds a label, such as a field with its hint.
    private static ILocator Field(Scene scene, string label)
        => scene.Page.Locator("label, span.ocat-label, h5").Filter(new() { HasTextString = label }).First
            .Locator("xpath=ancestor::div[contains(@class,'ocat-wrapper') or contains(@class,'ocat-limited-wrapper') or contains(@class,'mb-3') or contains(@class,'form-group')][1]");

    private static ILocator Row(Scene scene, string text) => scene.Page.Locator("li, tr").Filter(new() { HasTextString = text }).Last;

    private static ILocator Link(Scene scene, string text) => scene.Page.Locator("a, button").Filter(new() { HasTextString = text }).First;

    private static ILocator Publish(Scene scene) => scene.Page.Locator("button.publish").First;

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    // A wheel turn over a focused number box changes its value: leave the box first.
    private static async Task ScrollAsync(Scene scene, double deltaX, double deltaY)
    {
        await scene.Page.EvaluateAsync("() => document.activeElement && document.activeElement.blur()");
        await scene.ScrollAsync(deltaX, deltaY);
    }

    private static async Task ShowAsync(Scene scene, ILocator target, string caption, int hold = 1800)
    {
        await scene.HighlightAsync(target, caption);
        await scene.PauseAsync(hold);
        await scene.ClearHighlightsAsync();
    }

    // ----- Clips -----

    private static async Task CurrenciesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/commerce/currencies");
        await scene.StartCaptureAsync();

        await scene.StepAsync("cur-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/commerce/currencies"), "Commerce > Currencies", 2500);
            await scene.HighlightAsync(Row(scene, "Euro"), "Currencies you sell in");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("cur-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Link(scene, "Add Currency"));
            await scene.TypeAsync(scene.Page.Locator("#CurrencyEntry_Name"), "CAD", delay: 90);
            await scene.TypeAsync(scene.Page.Locator("#CurrencyEntry_DisplayName"), "Canadian Dollar", delay: 45);
        });

        await scene.StepAsync("cur-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.HighlightAsync(Row(scene, "Canadian Dollar"), "New currency");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ProductAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/Product");
        await scene.StartCaptureAsync();

        await scene.StepAsync("prod-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/Contents/ContentItems/Product"), "Commerce > Products", 2000);
            await ShowAsync(scene, scene.Page.Locator("input[type=search], #search-box, input[name='Options.DisplayText']").First, "Search", 1800);
            await scene.HighlightAsync(Row(scene, "Ceramic Mug").Locator("a:has-text('Edit')").First, "Edit");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("prod-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Link(scene, "New Product"));
            await scene.TypeAsync(scene.Page.Locator("[name='TitlePart.Title']"), "Desk Organizer", delay: 55);
            await scene.HighlightAsync(Field(scene, "Permalink"), "Made from the title");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("prod-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("[name='ProductPart.Price']"), "29", delay: 90);
            await scene.HighlightAsync(scene.Page.Locator("[name='ProductPart.Currency']"), "Currency");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("prod-4", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("[name='ProductPart.CompareAtPrice']"), "35", delay: 90);
            await ScrollAsync(scene, 0, 1500);
            await scene.TypeAsync(scene.Page.Locator("[name='ProductPart.Sku']"), "ORG-DSK-001", delay: 45);
            await scene.ClickAsync(scene.Page.Locator("input[type=checkbox][name='ProductPart.Featured']"), pauseAfter: 600);
        });

        await scene.StepAsync("prod-5", async () =>
        {
            await scene.ClickAsync(Field(scene, "Images").Locator("button.mf-btn-primary").First, pauseAfter: 1500);
            await scene.ClickAsync(scene.Page.Locator(".folder-name").Filter(new() { HasTextString = "products" }).First, pauseAfter: 1200);
            await scene.ClickAsync(scene.Page.Locator(".file-item").Filter(new() { HasTextString = "desk-organizer.jpg" }).First.Locator("input[type=checkbox]"), pauseAfter: 700);
            await scene.ClickAsync(scene.Page.Locator("button.mf-btn-primary").Filter(new() { HasTextString = "OK" }).First, pauseAfter: 900);
            await scene.TypeAsync(Field(scene, "Description").Locator(".CodeMirror").First, "A tidy tray for pens and notes.", delay: 25);
            await scene.TypeAsync(scene.Page.Locator("[name='ShippingPart.Weight']"), "0.6", delay: 90);
        });

        await scene.StepAsync("prod-6", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Publish(scene));
            await scene.HighlightAsync(Row(scene, "Desk Organizer"), "Published");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PricingAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/Product");
        await scene.Page.RunAndWaitForNavigationAsync(() => Row(scene, "Ceramic Mug").Locator("a:has-text('Edit')").First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("rule-1", async () =>
        {
            await ScrollAsync(scene, 0, 400);
            await scene.TypeAsync(scene.Page.Locator("[name='ProductPart.SalePrice']"), "15", delay: 90);
            await scene.HighlightAsync(Field(scene, "Sale runs"), "Sale runs");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rule-2", async () =>
        {
            await scene.HighlightAsync(Field(scene, "Quantity"), "At least, At most, In packs of");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("[name='ProductPart.MinQuantity']"), "2", delay: 90);
            await scene.TypeAsync(scene.Page.Locator("[name='ProductPart.PriceTiers[0].MinQuantity']"), "6", delay: 90);
            await scene.TypeAsync(scene.Page.Locator("[name='ProductPart.PriceTiers[0].PercentOff']"), "10", delay: 90);
        });

        await scene.StepAsync("rule-3", async () =>
        {
            await scene.HighlightAsync(Field(scene, "Role discounts"), "Role discounts");
            await scene.SelectAsync(scene.Page.Locator("[name='ProductPart.RoleDiscounts[0].Role']"), "Wholesale", pauseAfter: 700);
            await scene.TypeAsync(scene.Page.Locator("[name='ProductPart.RoleDiscounts[0].PercentOff']"), "20", delay: 90);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rule-4", async () =>
        {
            await ScrollAsync(scene, 0, 500);
            await scene.TypeAsync(scene.Page.Locator("[name='ProductPart.AddOns[0].Label']"), "Gift wrap", delay: 55);
            await scene.SelectAsync(scene.Page.Locator("[name='ProductPart.AddOns[0].Kind']"), "Ticks a box", pauseAfter: 600);
            await scene.TypeAsync(scene.Page.Locator("[name='ProductPart.AddOns[0].Price']"), "4", delay: 90);
            await scene.SelectAsync(scene.Page.Locator("[name='ProductPart.AddOns[0].PriceBasis']"), "Per unit", pauseAfter: 600);
        });

        await scene.StepAsync("rule-5", async () =>
        {
            await scene.HighlightAsync(Field(scene, "Sold elsewhere"), "Leave empty for a product sold here");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rule-6", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Publish(scene));
            await scene.NavigateAsync("/products/ceramic-mug");
        });
    }

    private static async Task ContentTypeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.StartCaptureAsync();

        await scene.StepAsync("type-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/ContentTypes/List"), "Content Definition > Content Types", 2000);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/ContentTypes/Edit/Product']").First);
        });

        await scene.StepAsync("type-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/ProductPart/Edit']").First);
            await scene.HighlightAsync(Field(scene, "Product Type"), "Product Type");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "Default currency"), "Default currency");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("type-3", async () =>
        {
            await scene.HighlightAsync(Field(scene, "Sales mode"), "Sales mode");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("type-4", async () =>
        {
            await ShowAsync(scene, Field(scene, "Price display"), "Price display", 2500);
            await scene.HighlightAsync(Field(scene, "Inquiry button"), "Inquiry button and link");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("type-5", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Link(scene, "Cancel"));
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='AddPartsTo/Product']").First);
            await ShowAsync(scene, scene.Page.Locator("div.form-check").Filter(new() { HasTextString = "Product prices" }).First, "Product prices", 1800);
            await scene.HighlightAsync(scene.Page.Locator("div.form-check").Filter(new() { HasTextString = "Product variants" }).First, "Product variants");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ListingAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/ProductListing");
        await scene.StartCaptureAsync();

        await scene.StepAsync("list-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/Contents/ContentItems/ProductListing"), "Commerce > Product Listings", 2200);
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Shop").Locator("a:has-text('Edit')").First);
        });

        await scene.StepAsync("list-2", async () =>
        {
            await scene.HighlightAsync(Field(scene, "Only these product types"), "Products");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await ScrollAsync(scene, 0, 250);
            await scene.ClickAsync(Link(scene, "Choose filters"), pauseAfter: 2500);
            await scene.Page.Keyboard.PressAsync("Escape");
        });

        await scene.StepAsync("list-3", async () =>
        {
            await ShowAsync(scene, Field(scene, "Default sort"), "Default sort", 2600);
            await scene.HighlightAsync(Field(scene, "Paging"), "Paging");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("list-4", async () =>
        {
            await ShowAsync(scene, Field(scene, "Image shape"), "Image shape", 2200);
            await ShowAsync(scene, Field(scene, "Page sizes"), "Page sizes", 2000);
            await scene.HighlightAsync(Field(scene, "Keyword search"), "Keyword search");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("list-5", async () =>
        {
            await scene.NavigateAsync("/shop");
            await scene.ClickAsync(scene.Page.Locator("label[for='facet-featured']").First, pauseAfter: 2200);
            await scene.SelectAsync(scene.Page.Locator("select[name='sort']"), "Price: low to high", pauseAfter: 1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn[title='List'], a.btn[aria-label='List']").First);
        });
    }

    private static async Task ExtrasAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/shop");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ext-1", async () =>
        {
            await ScrollAsync(scene, 0, 200);
            var compare = scene.Page.Locator("button").Filter(new() { HasTextString = "Compare" });
            await scene.ClickAsync(compare.Nth(0), pauseAfter: 300);
            await scene.ClickAsync(compare.Nth(1), pauseAfter: 300);
            await scene.ClickAsync(compare.Nth(2), pauseAfter: 300);
        });

        await scene.StepAsync("ext-1b", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a, button").Filter(new() { HasTextString = "Compare" }).Last);
            await ScrollAsync(scene, 0, 450);
        });

        await scene.StepAsync("ext-2", async () =>
        {
            await scene.NavigateAsync("/Admin/Contents/ContentItems/Product");
            await scene.WaitForNavigationAsync(() => Row(scene, "Canvas Backpack").Locator("a:has-text('Edit')").First.ClickAsync());
            await scene.HighlightAsync(Field(scene, "Products in this group"), "Products in this group");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ext-3", async () =>
        {
            await scene.HighlightAsync(Field(scene, "Upsells"), "Upsells");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "Cross-sells"), "Cross-sells");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ext-4", async () =>
        {
            await scene.HighlightAsync(Field(scene, "Made of"), "Bundle");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task FeedsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/product-feeds");
        await scene.StartCaptureAsync();

        await scene.StepAsync("feed-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/Settings/product-feeds"), "Settings > Commerce > Product feeds", 2400);
            await scene.HighlightAsync(Field(scene, "Feed addresses"), "Feed addresses");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("feed-2", async () =>
        {
            await ScrollAsync(scene, 0, 500);
            await scene.HighlightAsync(Field(scene, "Brand"), "Brand");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "Feed key"), "Feed key");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("feed-3", async () =>
        {
            await scene.NavigateAsync("/Admin/Features");
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "Product Search", delay: 60);
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.Locator("li, tr").Filter(new() { HasTextString = "Product Search (Lucene)" }).Last, "Product Search (Lucene)");
        });

        await scene.ClearHighlightsAsync();
    }
}
