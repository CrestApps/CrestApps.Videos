using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Product listings and attributes video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-field", FieldAsync),
        ("04-value", ValueAsync),
        ("05-listing", ListingAsync),
        ("06-visitors", VisitorsAsync),
    ];

    /// <summary>Nothing to prepare: the site is the one the Shipping video prepared (the shop with nine products and its Shop listing).</summary>
    public static Task PrepareAsync(IBrowser browser) => Task.CompletedTask;

    private const string BackpackPath = "/Admin/Contents/ContentItems/4swccm1c6r9fa4v1hjqgpfsn4h/Edit";

    private const string ShopPath = "/Admin/Contents/ContentItems/4jzxbym2j0f3psgb8s1jh63xcs/Edit";

    private const string AttributePrefix = "Product.Brand.ProductAttributeSettingsDisplayDriver.";

    private static ILocator Fld(Scene scene, string name) => scene.Page.Locator($"[name='{name}']").First;

    private static async Task ReplaceAsync(Scene scene, ILocator field, string text, int delay = 60)
    {
        await scene.ClickAsync(field, pauseAfter: 200);
        await scene.Page.Keyboard.PressAsync("Control+A");
        await field.PressSequentiallyAsync(text, new() { Delay = delay });
        await scene.PauseAsync(300);
    }

    private static async Task PickAsync(Scene scene, ILocator select, string contains, int pauseAfter = 700)
    {
        await scene.ClickAsync(select, pauseAfter: 300);
        await select.EvaluateAsync("(select, text) => { const option = [...select.options].find(o => o.text.includes(text)); select.value = option.value; select.dispatchEvent(new Event('change', { bubbles: true })); }", contains);
        await scene.PauseAsync(pauseAfter);
    }

    private static async Task WaitAsync(Scene scene, int millis = 5000)
        => await scene.OffCameraAsync(() => scene.Page.WaitForTimeoutAsync(millis));

    private static async Task FieldAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.StartCaptureAsync();

        await scene.StepAsync("fld-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/ContentTypes/List"), "Content Definition > Content Types", 2000);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/ContentTypes/Edit/Product']").First);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='AddFieldsTo/Product']").First);
            await scene.TypeAsync(Fld(scene, "DisplayName"), "Brand", delay: 80);
            await ReplaceAsync(scene, Fld(scene, "Name"), "Brand", 80);
            await scene.ClickAsync(scene.Page.Locator("input[name='FieldTypeName'][value='TextField']"), pauseAfter: 1200);
            await scene.ClickAsync(scene.Page.Locator("button[name='submit.Save']").First, pauseAfter: 500);
            await WaitAsync(scene, 4000);
        });

        await scene.StepAsync("fld-2", async () =>
        {
            await scene.NavigateAsync("/Admin/ContentParts/Product/Fields/Brand/Edit");
            var filterable = Fld(scene, AttributePrefix + "Filterable");
            await filterable.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(filterable.Locator("xpath=ancestor::div[contains(@class,'card') or contains(@class,'ocat') or self::fieldset or self::section][last()]"), "Product listing");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();

            foreach (var setting in new[] { "Filterable", "Sortable", "Searchable", "ShowOnCard", "ShowInSpecifications" })
            {
                await scene.ClickAsync(Fld(scene, AttributePrefix + setting), pauseAfter: 900);
            }

            await scene.TypeAsync(Fld(scene, AttributePrefix + "Label"), "Brand", delay: 80);
        });

        await scene.StepAsync("fld-3", async () =>
        {
            var swatches = Fld(scene, AttributePrefix + "SwatchColors");
            await swatches.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(swatches.Locator("xpath=ancestor::div[1]"), "Swatch colors");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button[name='submit.Save']").First, pauseAfter: 500);
            await WaitAsync(scene, 4000);
        });
    }

    // Gives the other products a brand, off camera.
    private static async Task BrandsAsync(IBrowser browser)
    {
        var page = await Seed.NewPageAsync(browser);
        await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems/Product", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var hrefs = await page.Locator("a").EvaluateAllAsync<string[]>("els => [...new Set(els.map(e => e.getAttribute('href')).filter(h => /ContentItems\\/[a-z0-9]{20,}\\/Edit/.test(h || '')))]");
        var brands = new[] { "Contoso Outdoors", "Harbor & Co", "Northwind" };
        var index = 0;

        foreach (var href in hrefs)
        {
            if (href.Contains("4swccm1c6r9fa4v1hjqgpfsn4h"))
            {
                continue;
            }

            await Seed.SubmitAsync(page, href.Split('?')[0], new() { ["Product.Brand.Text"] = brands[index++ % brands.Length] });
        }

        await page.Context.CloseAsync();
    }

    private static async Task ValueAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await BrandsAsync(browser);

        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync(BackpackPath);
        await scene.StartCaptureAsync();

        await scene.StepAsync("val-1", async () =>
        {
            var brand = Fld(scene, "Product.Brand.Text");
            await brand.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(brand.Locator("xpath=ancestor::div[contains(@class,'ocat') or contains(@class,'mb-3')][1]"), "The Brand field");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(brand, "Contoso Outdoors", delay: 60);
            await scene.ClickAsync(Publish(scene), pauseAfter: 500);
            await WaitAsync(scene, 5000);
        });
    }

    private static async Task ListingAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync(ShopPath);
        await scene.StartCaptureAsync();

        await scene.StepAsync("lst-1", async () =>
        {
            var products = scene.Page.Locator("input[name='ProductListingPart.ListEveryType']").First;
            await products.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(products.Locator("xpath=ancestor::*[contains(@class,'ocat') or contains(@class,'mb-3')][1]"), "Products");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            var filters = scene.Page.GetByText("Offer a filter for every filterable field").First;
            await filters.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(filters.Locator("xpath=ancestor::div[2]"), "Filters");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("lst-2", async () =>
        {
            var sorting = Fld(scene, "ProductListingPart.DefaultSort");
            await sorting.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(sorting.Locator("xpath=ancestor::*[contains(@class,'ocat') or contains(@class,'mb-3')][1]"), "Default sort");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("lst-3", async () =>
        {
            var paging = Fld(scene, "ProductListingPart.Paging");
            await paging.ScrollIntoViewIfNeededAsync();
            await PickAsync(scene, paging, "Load more", 1200);
            await ReplaceAsync(scene, Fld(scene, "ProductListingPart.PageSizes"), "4, 8, 24", 90);
            await ReplaceAsync(scene, Fld(scene, "ProductListingPart.DefaultPageSize"), "4", 120);
            await scene.HighlightAsync(paging.Locator("xpath=ancestor::*[contains(@class,'ocat') or contains(@class,'mb-3')][1]"), "Paging");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Publish(scene), pauseAfter: 500);
            await WaitAsync(scene, 5000);
        });
    }

    private static async Task VisitorsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/shop");
        await scene.StartCaptureAsync();

        await scene.StepAsync("vis-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Brand").First.Locator("xpath=ancestor::div[2]"), "Brand: a filter, with counts");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".card").First, "A chip on the card");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("vis-2", async () =>
        {
            var sort = scene.Page.Locator("select").First;
            await PickAsync(scene, sort, "Brand", 1800);
            await scene.ClickAsync(scene.Page.Locator("a.btn").Filter(new() { HasTextString = "List" }).First, pauseAfter: 2500);
            await ScrollAsync(scene, 0, 900);
            await scene.HighlightAsync(scene.Page.Locator("button, a").Filter(new() { HasTextString = "Load more" }).First, "Load more");
            await scene.PauseAsync(3000);
        });

        await scene.ClearHighlightsAsync();
    }
}
