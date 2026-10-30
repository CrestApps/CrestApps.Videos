using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Product search indexes video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-feature", FeatureAsync),
        ("04-index", IndexAsync),
        ("05-listing", ListingAsync),
        ("06-visitors", VisitorsAsync),
    ];

    /// <summary>
    /// Turns the product search feature off, so the video turns it on (not recorded). Run it with VIDEO_STAGE=on to turn the feature
    /// on again. The site is the one the Shipping video prepared (the shop with nine products and its Shop listing).
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var page = await Seed.NewPageAsync(browser);

        if (Environment.GetEnvironmentVariable("VIDEO_STAGE") == "on")
        {
            await Seed.EnableAsync(page, "CrestApps.OrchardCore.Products.Lucene");
        }
        else
        {
            // An index from an earlier video: the video makes its own.
            await page.GotoAsync(BaseUrl + "/Admin/indexing", new() { WaitUntil = WaitUntilState.NetworkIdle });

            while (await page.Locator("a.btn-danger:has-text('Delete'), button.btn-danger:has-text('Delete')").CountAsync() > 0)
            {
                await page.Locator("a.btn-danger:has-text('Delete'), button.btn-danger:has-text('Delete')").First.ClickAsync();
                await page.Locator(".modal.show .btn-danger, .modal.show .btn-primary").First.ClickAsync();
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                await page.GotoAsync(BaseUrl + "/Admin/indexing", new() { WaitUntil = WaitUntilState.NetworkIdle });
            }

            await Seed.DisableAsync(page, "CrestApps.OrchardCore.Products.Lucene");
        }

        await page.Context.CloseAsync();
    }

    private const string ShopPath = "/Admin/Contents/ContentItems/4jzxbym2j0f3psgb8s1jh63xcs/Edit";

    private static ILocator Fld(Scene scene, string name) => scene.Page.Locator($"[name='{name}']").First;

    private static async Task WaitAsync(Scene scene, int millis = 5000)
        => await scene.OffCameraAsync(() => scene.Page.WaitForTimeoutAsync(millis));

    private static async Task FeatureAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();

        await scene.StepAsync("fea-1", async () =>
        {
            var search = scene.Page.GetByPlaceholder("Search").First;
            await scene.TypeAsync(search, "Product Search", delay: 70);
            await scene.PauseAsync(1800);
            var enable = scene.Page.Locator("a[href$='/CrestApps.OrchardCore.Products.Lucene/Enable']").First;
            await scene.HighlightAsync(enable.Locator("xpath=ancestor::*[self::li or self::tr or contains(@class,'list-group-item')][1]"), "Product Search (Lucene)");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(enable, pauseAfter: 800);

            var confirm = scene.Page.Locator(".modal.show .btn-primary, .modal.show .btn-danger").First;

            try
            {
                await confirm.WaitForAsync(new() { Timeout = 2500 });
                await scene.ClickAsync(confirm, pauseAfter: 500);
            }
            catch (TimeoutException)
            {
            }

            await WaitAsync(scene, 12000);
            await scene.PauseAsync(1500);
        });
    }

    private static async Task IndexAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Index");
        await scene.NavigateAsync("/Admin/indexing");
        await scene.StartCaptureAsync();

        await scene.StepAsync("idx-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("a, button").Filter(new() { HasTextString = "Add index" }).First, pauseAfter: 1500);
            await scene.HighlightAsync(scene.Page.Locator("a[href$='create/Lucene/ProductSearch']").First.Locator("xpath=ancestor::*[self::li or self::tr or contains(@class,'list-group-item') or contains(@class,'card') or contains(@class,'modal-body')][1]"), "Products (Lucene)");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='create/Lucene/ProductSearch']").First);
        });

        await scene.StepAsync("idx-2", async () =>
        {
            await scene.TypeAsync(Fld(scene, "IndexProfile.IndexName"), "shop", delay: 90);
            await scene.TypeAsync(Fld(scene, "IndexProfile.Name"), "Shop products", delay: 60);
            await scene.ClickAsync(scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last, pauseAfter: 500);
            await WaitAsync(scene, 8000);
            await scene.PauseAsync(1500);
        });
    }

    private static async Task ListingAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync(ShopPath);
        await scene.StartCaptureAsync();

        await scene.StepAsync("lst-1", async () =>
        {
            var index = Fld(scene, "ProductListingPart.IndexProfileName");
            await index.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(index.Locator("xpath=ancestor::*[contains(@class,'ocat') or contains(@class,'mb-3')][1]"), "Search index");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(index, pauseAfter: 300);
            await index.EvaluateAsync("select => { const option = [...select.options].find(o => /Shop/.test(o.text)) || select.options[1]; select.value = option.value; select.dispatchEvent(new Event('change', { bubbles: true })); }");
            await scene.PauseAsync(2000);
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
            var search = scene.Page.GetByPlaceholder("Search").Nth(1);
            await scene.TypeAsync(search, "wo", delay: 150);
            await scene.Page.Keyboard.PressAsync("Enter");
            await scene.OffCameraAsync(() => scene.Page.WaitForLoadStateAsync(LoadState.NetworkIdle));
            await scene.HighlightAsync(scene.Page.Locator(".card").First, "The match, from the index");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("vis-2", async () =>
        {
            await scene.PauseAsync(5000);
        });
    }
}
