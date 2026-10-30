using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Product bundles and recommendations video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-bundle", BundleAsync),
        ("04-grouped", GroupedAsync),
        ("05-recs", RecsAsync),
        ("06-compare", CompareAsync),
    ];

    /// <summary>
    /// Turns on Bundles, Grouped products, Recommendations and Comparison, counts the Product type and gives the products their
    /// counts (not recorded). The site is the one the Inventory video prepared (the shop with nine products).
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var page = await Seed.NewPageAsync(browser);

        await Seed.EnableAsync(page, "CrestApps.OrchardCore.Products.Comparison", "CrestApps.OrchardCore.Products.Bundles", "CrestApps.OrchardCore.Products.Grouped", "CrestApps.OrchardCore.Products.Recommendations");

        await page.GotoAsync(BaseUrl + "/Admin/ContentTypes/AddPartsTo/Product", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var row = page.Locator("div.form-check").Filter(new() { HasTextString = "Counts how many units" }).First;

        if (await row.CountAsync() > 0)
        {
            await row.Locator("input[type=checkbox]").CheckAsync();
            await page.RunAndWaitForNavigationAsync(() => page.Locator("button, input[type=submit]").Filter(new() { HasTextString = "Add" }).Last.ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        }

        await page.GotoAsync(BaseUrl + "/Admin/inventory", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var find = page.Locator("a, button").Filter(new() { HasTextString = "Find products to count" }).First;

        if (await find.CountAsync() > 0)
        {
            await page.RunAndWaitForNavigationAsync(() => find.ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        }

        foreach (var (product, count) in new[] { ("Ceramic Mug", 4), ("Wool Beanie", 6), ("Canvas Backpack", 8), ("Trail Runner Shoe", 25), ("Wireless Earbuds", 18), ("Leather Wallet", 12), ("Hardcover Notebook", 30), ("Desk Lamp", 9), ("Insulated Water Bottle", 22) })
        {
            await CountToAsync(page, product, count);
        }

        await page.Context.CloseAsync();
    }

    private static async Task CountToAsync(IPage page, string product, int count)
    {
        await page.GotoAsync(BaseUrl + "/Admin/inventory", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var change = page.Locator("li, tr").Filter(new() { HasTextString = product }).Last.Locator("a:has-text('Change')").First;

        if (await change.CountAsync() == 0)
        {
            return;
        }

        await page.RunAndWaitForNavigationAsync(() => change.ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("[name='Count.Count']").FillAsync(count.ToString());
        await page.RunAndWaitForNavigationAsync(() => page.Locator("button").Filter(new() { HasTextString = "Record count" }).First.ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
    }

    private const string MugPath = "/Admin/Contents/ContentItems/4rz0evgwyc90rraecmkrmxg6xd/Edit";

    private static ILocator Fld(Scene scene, string name) => scene.Page.Locator($"[name='{name}']").First;

    private static ILocator Heading(Scene scene, string text)
        => scene.Page.Locator("h1:visible, h2:visible, h3:visible, h4:visible, h5:visible").Filter(new() { HasTextRegex = new Regex("^\\s*" + Regex.Escape(text) + "\\s*$") }).First;

    private static async Task WaitAsync(Scene scene, int millis = 4000)
        => await scene.OffCameraAsync(() => scene.Page.WaitForTimeoutAsync(millis));

    /// <summary>Opens a live-search product picker, searches for the product and ticks it.</summary>
    private static async Task ChooseAsync(Scene scene, string selectId, string product, bool keepOpen = false)
    {
        var wrapper = scene.Page.Locator($"div.bootstrap-select:has(button[data-id='{selectId}'])").First;
        var button = wrapper.Locator("button.dropdown-toggle").First;

        if (await button.GetAttributeAsync("aria-expanded") != "true")
        {
            await button.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(600);
            await scene.ClickAsync(button, pauseAfter: 600);
        }

        var search = wrapper.Locator(".bs-searchbox input").First;
        await search.FillAsync(string.Empty);
        await scene.TypeAsync(search, product, delay: 60);
        var option = wrapper.Locator(".dropdown-menu li a").Filter(new() { HasTextString = product }).First;
        await scene.OffCameraAsync(() => option.WaitForAsync(new() { Timeout = 15000 }));
        await scene.ClickAsync(option, pauseAfter: 700);

        if (!keepOpen)
        {
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.PauseAsync(300);
        }
    }

    private static async Task BundleAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentTypes/Product/Create");
        await scene.StartCaptureAsync();

        await scene.StepAsync("bun-1", async () =>
        {
            var made = Fld(scene, "ProductPart.Items[0].ContentItemId");
            await made.EvaluateAsync("el => el.closest('.bootstrap-select')?.scrollIntoView({ block: 'center' })");
            await scene.PauseAsync(800);
            await scene.HighlightAsync(Heading(scene, "Bundle"), "Bundle: what the product is made of");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("bun-2", async () =>
        {
            await ChooseAsync(scene, "ProductPart_Items_0__ContentItemId", "Ceramic Mug");
            await scene.TypeAsync(Fld(scene, "ProductPart.Items[0].Quantity"), "2", delay: 80);
            await ChooseAsync(scene, "ProductPart_Items_1__ContentItemId", "Hardcover Notebook");
            await scene.TypeAsync(Fld(scene, "ProductPart.Items[1].Quantity"), "1", delay: 80);
            await scene.HighlightAsync(scene.Page.Locator("[name='ProductPart.Items[0].Quantity']").First.Locator("xpath=ancestor::div[contains(@class,'row')][last()]"), "Two mugs and a notebook in each kit");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("bun-3", async () =>
        {
            await scene.TypeAsync(Fld(scene, "TitlePart.Title"), "Coffee break kit", delay: 60);
            var price = Fld(scene, "ProductPart.Price");
            await scene.ClickAsync(price, pauseAfter: 200);
            await price.FillAsync("24");
            await scene.HighlightAsync(price, "The kit's own price");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("bun-3b", async () =>
        {
            var box = scene.Page.GetByLabel("Count stock").First;
            await box.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(600);
            await scene.ClickAsync(box, pauseAfter: 900);
            await scene.HighlightAsync(box.Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"), "Do not count the kit's own stock");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Publish(scene), pauseAfter: 500);
            await WaitAsync(scene, 4000);
        });

        await scene.StepAsync("bun-4", async () =>
        {
            await scene.NavigateAsync("/products/coffee-break-kit");
            await scene.PauseAsync(2000);
            await scene.HighlightAsync(scene.Page.GetByText("Only 2 left").First, "Four mugs make two kits");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            var parts = scene.Page.GetByText("In this bundle").First;
            await parts.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(parts.Locator("xpath=ancestor::*[self::section or self::div][1]"), "The page lists what is in the kit");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("bun-5", async () =>
        {
            await scene.NavigateAsync("/Admin/inventory");
            await scene.HighlightAsync(Row(scene, "Ceramic Mug"), "Ceramic Mug: 4 in stock, 2 in each kit");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task GroupedAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentTypes/Product/Create");
        await scene.StartCaptureAsync();

        await scene.StepAsync("grp-1", async () =>
        {
            var group = Fld(scene, "ProductPart.ProductIds");
            await group.EvaluateAsync("el => el.closest('.bootstrap-select')?.scrollIntoView({ block: 'center' })");
            await scene.PauseAsync(800);
            await scene.HighlightAsync(Heading(scene, "Grouped products"), "Grouped products");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("grp-2", async () =>
        {
            await ChooseAsync(scene, "ProductPart_ProductIds", "Desk Lamp", keepOpen: true);
            await ChooseAsync(scene, "ProductPart_ProductIds", "Hardcover Notebook", keepOpen: true);
            await ChooseAsync(scene, "ProductPart_ProductIds", "Insulated Water Bottle");
            await scene.HighlightAsync(scene.Page.Locator("button[data-id='ProductPart_ProductIds']").First, "A lamp, a notebook and a bottle");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("grp-3", async () =>
        {
            await scene.TypeAsync(Fld(scene, "TitlePart.Title"), "Desk set", delay: 60);
            var box = scene.Page.GetByLabel("Count stock").First;
            await box.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(600);
            await scene.ClickAsync(box, pauseAfter: 900);
            await scene.HighlightAsync(box.Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"), "No price and no stock of its own");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Publish(scene), pauseAfter: 500);
            await WaitAsync(scene, 4000);
        });

        await scene.StepAsync("grp-4", async () =>
        {
            await scene.NavigateAsync("/products/desk-set");
            await scene.PauseAsync(2500);
            var quantities = scene.Page.Locator("input[type=number]");
            await scene.ClickAsync(quantities.Nth(0), pauseAfter: 200);
            await quantities.Nth(0).FillAsync("1");
            await scene.ClickAsync(quantities.Nth(1), pauseAfter: 200);
            await quantities.Nth(1).FillAsync("2");
            await scene.HighlightAsync(quantities.Nth(0).Locator("xpath=ancestor::div[3]"), "Each product with its own quantity");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Add to cart" }).First, "One Add to cart for the chosen products");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task RecsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync(MugPath);
        await scene.StartCaptureAsync();

        await scene.StepAsync("rec-1", async () =>
        {
            var upsells = Fld(scene, "ProductPart.UpsellIds");
            await upsells.EvaluateAsync("el => el.closest('.bootstrap-select')?.scrollIntoView({ block: 'center' })");
            await scene.PauseAsync(800);
            await scene.HighlightAsync(Heading(scene, "Recommendations"), "Upsells and cross-sells");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("rec-2", async () =>
        {
            await ChooseAsync(scene, "ProductPart_UpsellIds", "Insulated Water Bottle");
            await ChooseAsync(scene, "ProductPart_CrossSellIds", "Hardcover Notebook", keepOpen: true);
            await ChooseAsync(scene, "ProductPart_CrossSellIds", "Desk Lamp");
            await scene.HighlightAsync(scene.Page.Locator("button[data-id='ProductPart_CrossSellIds']").First, "A notebook and a lamp go with the mug");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Publish(scene), pauseAfter: 500);
            await WaitAsync(scene, 4000);
        });

        await scene.StepAsync("rec-3", async () =>
        {
            await scene.NavigateAsync("/products/hardcover-notebook");
            await scene.PauseAsync(1800);
            await scene.NavigateAsync("/products/ceramic-mug");
            await scene.PauseAsync(2000);
            var upsell = scene.Page.GetByText("You may also like").First;
            await upsell.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(upsell.Locator("xpath=ancestor::section[1] | ancestor::div[2]").First, "You may also like: the upsell");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("rec-4", async () =>
        {
            var related = scene.Page.GetByText("Related products").First;

            if (await related.CountAsync() > 0)
            {
                await related.ScrollIntoViewIfNeededAsync();
                await scene.HighlightAsync(related, "Related products, from the same categories");
                await scene.PauseAsync(2500);
                await scene.ClearHighlightsAsync();
            }

            var viewed = scene.Page.GetByText("Recently viewed").First;

            if (await viewed.CountAsync() > 0)
            {
                await viewed.ScrollIntoViewIfNeededAsync();
                await scene.HighlightAsync(viewed, "Recently viewed, kept in the browser");
                await scene.PauseAsync(2500);
                await scene.ClearHighlightsAsync();
            }
        });

        await scene.StepAsync("rec-5", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Add to cart" }).First, pauseAfter: 1500);
            await scene.NavigateAsync("/cart");
            await scene.PauseAsync(2000);
            var together = scene.Page.GetByText("Frequently bought together").First;
            await together.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(together, "Frequently bought together: the cross-sells");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task CompareAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/shop");
        await scene.StartCaptureAsync();

        await scene.StepAsync("cmp-1", async () =>
        {
            await scene.PauseAsync(1500);
            var first = scene.Page.GetByText("Compare").First;
            await scene.HighlightAsync(first, "A Compare tick box on each card");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("cmp-2", async () =>
        {
            foreach (var product in new[] { "Wireless Earbuds", "Canvas Backpack", "Trail Runner Shoe" })
            {
                var card = scene.Page.Locator(".card").Filter(new() { HasTextString = product }).First;
                await scene.ClickAsync(card.GetByText("Compare").First, pauseAfter: 900);
            }

            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("cmp-3", async () =>
        {
            var bar = scene.Page.Locator("a, button").Filter(new() { HasTextString = "Compare" }).Last;
            await scene.HighlightAsync(bar, "The compare bar");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(bar);
            await scene.PauseAsync(2500);
            await scene.ScrollAsync(0, 650);
            await scene.PauseAsync(3500);
            await scene.ScrollAsync(0, 500);
            await scene.PauseAsync(2500);
        });
    }
}
