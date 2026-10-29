using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Products video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // The made-up catalog of the Contoso demo store: title, SKU, price, compare-at price, weight in kg, picture, featured.
    private static readonly (string Title, string Sku, string Price, string CompareAt, string Weight, string Picture, bool Featured, string Description)[] s_catalog =
    [
        ("Trail Runner Shoe", "SHOE-TR-001", "129", "149", "0.8", "trail-runner", true, "<p>A light, grippy shoe for wet and dry trails.</p>"),
        ("Ceramic Mug", "MUG-CER-001", "18", "", "0.4", "ceramic-mug", false, "<p>A 12 oz stoneware mug, dishwasher safe.</p>"),
        ("Canvas Backpack", "BAG-CNV-001", "64", "", "0.9", "canvas-backpack", true, "<p>A 22 liter backpack in waxed canvas with a laptop sleeve.</p>"),
        ("Wool Beanie", "HAT-WOL-001", "24", "", "0.1", "wool-beanie", false, "<p>A soft merino wool beanie in one size.</p>"),
        ("Insulated Water Bottle", "BTL-INS-001", "32", "", "0.5", "water-bottle", false, "<p>Keeps drinks cold for a day and hot for twelve hours.</p>"),
        ("Desk Lamp", "LMP-DSK-001", "48", "", "1.2", "desk-lamp", false, "<p>An adjustable LED lamp with three brightness levels.</p>"),
        ("Hardcover Notebook", "NTB-HRD-001", "14", "", "0.3", "notebook", false, "<p>A hardcover notebook with 192 dotted pages.</p>"),
        ("Leather Wallet", "WLT-LTH-001", "39", "45", "0.1", "leather-wallet", false, "<p>A slim wallet in full-grain leather.</p>"),
        ("Wireless Earbuds", "EAR-WLS-001", "59", "", "0.1", "earbuds", true, "<p>Earbuds with a charging case and up to 24 hours of play time.</p>"),
    ];

    /// <summary>
    /// Turns the product features on and adds the demo store's catalog (not recorded).
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var page = await Seed.NewPageAsync(browser);

        // Stage two, before the "08-extras" clip: the features whose sections the product editor shows above its tabs.
        if (Environment.GetEnvironmentVariable("VIDEO_STAGE") == "extras")
        {
            await Seed.EnableAsync(page, "CrestApps.OrchardCore.Products.Bundles", "CrestApps.OrchardCore.Products.Grouped", "CrestApps.OrchardCore.Products.Recommendations");
            await page.Context.CloseAsync();

            return;
        }

        if (Environment.GetEnvironmentVariable("VIDEO_STAGE") == "role")
        {
            await Seed.SubmitAsync(page, "/Admin/Roles/Create", new() { ["RoleName"] = "Wholesale" }, "css:button.btn-primary[type=submit]");
            await page.Context.CloseAsync();

            return;
        }

        if (Environment.GetEnvironmentVariable("VIDEO_STAGE") == "reset")
        {
            await Seed.DisableAsync(page, "CrestApps.OrchardCore.Products.Bundles", "CrestApps.OrchardCore.Products.Grouped", "CrestApps.OrchardCore.Products.Recommendations");
            await page.Context.CloseAsync();

            return;
        }

        await Seed.EnableAsync(
            page,
            "CrestApps.OrchardCore.Products.Comparison",
            "CrestApps.OrchardCore.Products.Feeds",
            "CrestApps.OrchardCore.Products.Lucene");

        // The feeds need the site's address for their links and photos, and the listings a search index to offer.
        await Seed.SubmitAsync(page, "/Admin/Settings/general", new() { ["ISite.BaseUrl"] = BaseUrl }, "css:button.btn-primary[type=submit]");
        await Seed.SubmitAsync(page, "/Admin/indexing/create/Lucene/ProductSearch", new() { ["IndexProfile.IndexName"] = "productsearch", ["IndexProfile.Name"] = "Product search" }, "css:button.btn-primary[type=submit]");

        if (Environment.GetEnvironmentVariable("VIDEO_FEATURES_ONLY") == "1")
        {
            await page.Context.CloseAsync();

            return;
        }

        await Seed.SubmitAsync(page, "/Admin/Roles/Create", new() { ["RoleName"] = "Wholesale" }, "css:button.btn-primary[type=submit]");

        foreach (var product in s_catalog)
        {
            var values = new Dictionary<string, string>
            {
                ["TitlePart.Title"] = product.Title,
                ["ProductPart.Price"] = product.Price,
                ["ProductPart.CompareAtPrice"] = product.CompareAt,
                ["ProductPart.Sku"] = product.Sku,
                ["ProductPart.Featured"] = product.Featured ? "true" : "false",
                ["ShippingPart.Weight"] = product.Weight,
                ["HtmlBodyPart.Html"] = product.Description,
                ["Product.Images.Paths"] = Seed.Pictures($"products/{product.Picture}.jpg"),
            };

            await Seed.CreateAsync(page, "Product", values);
            Console.WriteLine($"Created {product.Title}");
        }

        await page.Context.CloseAsync();
    }
}
