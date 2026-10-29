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
        ("03-settings", MarketSettingsAsync),
        ("04-vendor", VendorAsync),
        ("05-detail", DetailAsync),
        ("06-portal", PortalAsync),
        ("07-type", ClassifiedTypeAsync),
        ("08-post", PostAsync),
        ("09-approve", ApproveAsync),
        ("10-own", OwnAsync),
        ("11-search", SearchAsync),
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

    // The fake listings: owner, title, description, place, approved before recording.
    private static readonly (string User, string Title, string Body, string Place, bool Approve)[] s_listings =
    [
        ("jamie", "Trek mountain bike, 21 speed", "Lightly used, serviced last spring. Includes a helmet and a lock.", "Springfield", true),
        ("riley", "Piano lessons for beginners", "Friendly weekly lessons for children and adults, at your home or at mine.", "Springfield", true),
        ("morgan", "Garden tools, free to a good home", "A spade, a rake and two pairs of gloves. Pick up this weekend.", "Eugene", false),
        ("jamie", "Oak bookshelf", "Solid oak, five shelves, 180 cm tall. Buyer collects.", "Springfield", false),
    ];

    // Stage "seed": makes the vendor and its sale (the vendor, then seed.py), and posts the listings.
    private static async Task SeedAsync(IBrowser browser, IPage page)
    {
        await page.GotoAsync(BaseUrl + "/Admin/vendors");

        if (await page.GetByText("Clayworks Studio").CountAsync() == 0)
        {
            await page.GotoAsync(BaseUrl + "/Admin/vendors/create");
            await page.Locator("#Name").FillAsync("Clayworks Studio");
            await page.Locator("#Email").FillAsync("hello@clayworks.example");
            await page.Locator("#UserNames").FillAsync("morgan");
            await page.Locator("#Country").FillAsync("US");
            await page.RunAndWaitForNavigationAsync(() => page.Locator("button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
            Console.WriteLine("Added Clayworks Studio");

            await page.GotoAsync(BaseUrl + "/Admin/Contents/ContentItems/4mrktprod0ceramicmug00000a/Edit");
            await page.Locator("#VendorPart_VendorId").SelectOptionAsync(new SelectOptionValue { Label = "Clayworks Studio" });
            await page.RunAndWaitForNavigationAsync(() => page.GetByRole(AriaRole.Button, new() { Name = "Publish", Exact = true }).First.ClickAsync());
            Console.WriteLine("Chose the vendor on the mug");
        }

        foreach (var listing in Environment.GetEnvironmentVariable("VIDEO_PREPARE_STAGE") == "seed" ? s_listings : [])
        {
            var owner = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = Path.Combine(Root, $"state-{listing.User}.json"), ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
            var ownerPage = await owner.NewPageAsync();
            ownerPage.SetDefaultTimeout(60_000);
            await ownerPage.GotoAsync(BaseUrl + "/listings/post/ClassifiedAd");
            await ownerPage.Locator("#TitlePart_Title").FillAsync(listing.Title);
            await ownerPage.EvaluateAsync("body => { const e = document.getElementById('HtmlBodyPart_Html'); e.value = body; e.dispatchEvent(new Event('input', { bubbles: true })); }", "<p>" + listing.Body + "</p>");
            await ownerPage.Locator("#ClassifiedPart_Place").FillAsync(listing.Place);
            await ownerPage.RunAndWaitForNavigationAsync(() => ownerPage.GetByRole(AriaRole.Button, new() { Name = "Post it" }).ClickAsync());
            Console.WriteLine($"{listing.User} posted {listing.Title}");
            await owner.CloseAsync();
        }

        foreach (var listing in s_listings.Where(listing => listing.Approve))
        {
            // The piano lessons are approved for five days only, so that their owner is offered Renew.
            var shortTerm = listing.Title.StartsWith("Piano", StringComparison.Ordinal);

            if (shortTerm)
            {
                await SetTermDaysAsync(page, 5);
            }

            await page.GotoAsync(BaseUrl + "/Admin/classifieds");
            var row = page.Locator("li.list-group-item.item").Filter(new() { HasTextString = listing.Title }).First;
            await page.RunAndWaitForNavigationAsync(() => row.GetByRole(AriaRole.Button, new() { Name = "Approve" }).ClickAsync());
            Console.WriteLine($"Approved {listing.Title}");

            if (shortTerm)
            {
                await SetTermDaysAsync(page, 30);
            }
        }
    }

    private static async Task SetTermDaysAsync(IPage page, int days)
    {
        await page.GotoAsync(BaseUrl + "/Admin/ContentTypes/ClassifiedAd/ContentParts/ClassifiedPart/Edit");
        await page.Locator("#ClassifiedAd_ClassifiedPart_ClassifiedPartSettingsDisplayDriver_TermDays").FillAsync(days.ToString());
        await page.RunAndWaitForNavigationAsync(() => page.Locator("button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
        Console.WriteLine($"A listing runs {days} days");
    }

    public static async Task PrepareAsync(IBrowser browser)
    {
        var admin = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await admin.NewPageAsync();
        page.SetDefaultTimeout(120_000);

        if (Environment.GetEnvironmentVariable("VIDEO_PREPARE_STAGE") is "seed" or "approve")
        {
            await SeedAsync(browser, page);
            await admin.CloseAsync();
            return;
        }

        if (Environment.GetEnvironmentVariable("VIDEO_PREPARE_STAGE") != "signin")
        {
        await EnableFeaturesAsync(page, "CrestApps.OrchardCore.Customers.Api", "CrestApps.OrchardCore.Orders.Api");
        await RunRecipeAsync(page, "Areas/CrestApps.OrchardCore.Marketplace/Recipes", "marketplace-sample.recipe.json");
        await RunRecipeAsync(page, "Areas/CrestApps.OrchardCore.Classifieds/Recipes", "classifieds-sample.recipe.json");
        }

        await admin.CloseAsync();

        foreach (var user in new[] { "jamie", "riley", "morgan" })
        {
            await SaveSignInAsync(browser, user);
        }
    }
}
