using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Product import and feeds video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-export", ExportAsync),
        ("04-import", ImportAsync),
        ("05-feeds", FeedsAsync),
    ];

    /// <summary>
    /// Turns on Content Transfer and Product Feeds (not recorded). The site is the one the Shipping video prepared (the shop with nine
    /// products). <c>new-products.csv</c> is the file the import clip uploads.
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var page = await Seed.NewPageAsync(browser);

        await Seed.EnableAsync(page, "CrestApps.OrchardCore.ContentTransfer", "CrestApps.OrchardCore.Products.Feeds");
        await page.Context.CloseAsync();
    }

    private const string BackpackPath = "/Admin/Contents/ContentItems/4swccm1c6r9fa4v1hjqgpfsn4h/Edit";

    private static ILocator Fld(Scene scene, string name) => scene.Page.Locator($"[name='{name}']").First;

    private static async Task PickAsync(Scene scene, ILocator select, string contains, int pauseAfter = 700)
    {
        await scene.ClickAsync(select, pauseAfter: 300);
        await select.EvaluateAsync("(select, text) => { const option = [...select.options].find(o => o.text.includes(text)); select.value = option.value; select.dispatchEvent(new Event('change', { bubbles: true })); }", contains);
        await scene.PauseAsync(pauseAfter);
    }

    private static async Task WaitAsync(Scene scene, int millis = 5000)
        => await scene.OffCameraAsync(() => scene.Page.WaitForTimeoutAsync(millis));

    private static async Task ExportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/export/contents");
        await scene.StartCaptureAsync();

        await scene.StepAsync("exp-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/export/contents"), "Content > Bulk Export", 2000);
            var type = Fld(scene, "contentTypeId");
            await type.EvaluateAsync("select => { const option = [...select.options].find(o => o.text.trim() === 'Product'); select.value = option.value; select.dispatchEvent(new Event('change', { bubbles: true })); if (window.jQuery && jQuery(select).selectpicker) { jQuery(select).selectpicker('refresh'); } }");
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(type.Locator("xpath=ancestor::div[contains(@class,'mb-3') or contains(@class,'ocat') or contains(@class,'row')][1]"), "The Product type, in CSV");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();

            try
            {
                await scene.Page.RunAndWaitForDownloadAsync(
                    () => scene.ClickAsync(scene.Page.Locator("button[name='export_contents_button']").First, pauseAfter: 500),
                    new() { Timeout = 20000 });
            }
            catch (TimeoutException)
            {
            }

            await scene.PauseAsync(2500);
        });

        await scene.StepAsync("exp-2", async () =>
        {
            await scene.PauseAsync(6000);
        });
    }

    private static async Task ImportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/content-transfer-entries");
        await scene.StartCaptureAsync();

        await scene.StepAsync("imp-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/content-transfer-entries"), "Content > Bulk Import", 2000);
            await scene.ClickAsync(scene.Page.Locator("#new-dropdown, button[id='new-dropdown']").First, pauseAfter: 1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".dropdown-menu.show a[href*='/Admin/import/contents/Product?']").First);
            await scene.HighlightAsync(scene.Page.Locator("a").Filter(new() { HasTextString = "Download template" }).First, "Download template (CSV)");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("imp-2", async () =>
        {
            await scene.Page.Locator("[name='ImportContent.File']").SetInputFilesAsync(Path.Combine(Root, "new-products.csv"));
            await scene.PauseAsync(1500);
            await scene.ClickAsync(scene.Page.Locator("[name='ImportContent.PublishImportedContent']").First, pauseAfter: 1200);
            await scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Upload" }).First, pauseAfter: 500);
            await WaitAsync(scene, 8000);
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("imp-3", async () =>
        {
            await scene.NavigateAsync("/Admin/content-transfer-entries");
            await scene.PauseAsync(2000);
            var entry = scene.Page.Locator("li, tr").Filter(new() { HasTextString = "Product" }).Last;
            await scene.HighlightAsync(entry, "The import and its status");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(entry.Locator("button.dropdown-toggle, button[data-bs-toggle='dropdown']").Last, pauseAfter: 1500);
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show").Last, "Download errors: the skipped row");
            await scene.PauseAsync(4000);
        });
    }

    private static async Task FeedsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/product-feeds");
        await scene.StartCaptureAsync();

        await scene.StepAsync("fed-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/Settings/product-feeds"), "Settings > Commerce > Product feeds", 2000);
            await scene.HighlightAsync(scene.Page.GetByText("items are in the feed").First.Locator("xpath=ancestor::div[1]"), "The feed's address and what is in it");
            await scene.PauseAsync(4500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("fed-2", async () =>
        {
            await scene.TypeAsync(Fld(scene, "ISite.ProductFeedSettings.DefaultBrand"), "Contoso", delay: 80);
            await PickAsync(scene, Fld(scene, "ISite.ProductFeedSettings.DefaultCondition"), "New", 1000);
            await scene.HighlightAsync(Fld(scene, "ISite.ProductFeedSettings.IncludeOutOfStock").Locator("xpath=ancestor::div[contains(@class,'ocat') or contains(@class,'form-check')][1]"), "Products that are out of stock");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(SaveButton(scene), pauseAfter: 500);
            await WaitAsync(scene, 4000);
        });

        await scene.StepAsync("fed-3", async () =>
        {
            await scene.NavigateAsync("/Admin/ContentTypes/AddPartsTo/Product");
            var row = scene.Page.Locator("div.form-check").Filter(new() { HasTextString = "GTIN" }).First;
            await scene.ClickAsync(row.Locator("input[type=checkbox]"), pauseAfter: 1200);
            await scene.HighlightAsync(row, "Product Feed");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button, input[type=submit]").Filter(new() { HasTextString = "Add" }).Last, pauseAfter: 500);
            await WaitAsync(scene, 3500);
            await scene.NavigateAsync(BackpackPath);
            var gtin = scene.Page.GetByText("GTIN").First;
            await gtin.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(gtin.Locator("xpath=ancestor::div[contains(@class,'ocat') or contains(@class,'card') or contains(@class,'mb-3')][last()]"), "Brand, GTIN, MPN, condition and category");
            await scene.PauseAsync(4000);
        });

        await scene.ClearHighlightsAsync();
    }
}
