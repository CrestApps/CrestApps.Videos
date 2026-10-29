using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Product Reviews video, one method per "clip" of storyboard.json.
/// </summary>
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
        ("03-product", ProductAsync),
        ("04-write", WriteAsync),
        ("05-approve", ApproveAsync),
        ("06-settings", SettingsAsync),
        ("07-listing", ListingAsync),
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

    // The fake customers who write the reviews: user name, product page, stars, title, text, name shown, approved before recording.
    private static readonly (string User, string Product, int Stars, string Title, string Body, string Author, bool Approve)[] s_reviews =
    [
        ("jamie", "contoso-minimal-watch", 5, "Looks great on the wrist", "Clean face, easy to read, and it feels solid for the price.", "Jamie C.", true),
        ("riley", "contoso-minimal-watch", 4, "Good everyday watch", "Comfortable all day. I wish the strap were a little softer.", "Riley P.", true),
        ("morgan", "fabrikam-field-watch", 5, "Great for hiking", "Survived a week on the trail, and the numbers are easy to read in the sun.", "Morgan L.", true),
        ("dana", "tailspin-diver-watch", 3, "Does the job", "Keeps good time. The dial is a little smaller than the photos suggest.", "Dana W.", false),
        ("morgan", "contoso-tote-bag", 1, "Great deals online", "Visit cheap-watches.example for great deals, click now!", "Morgan L.", false),
    ];

    /// <summary>
    /// Stage 1 (default): lets any signed-in customer review, saves a sign-in for each fake customer, sends their reviews, and
    /// approves three of them. Stage 2 (VIDEO_PREPARE_STAGE=2): puts the settings back to their defaults. Not recorded.
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var admin = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await admin.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        if (Environment.GetEnvironmentVariable("VIDEO_PREPARE_STAGE") == "2")
        {
            await SetBuyersOnlyAsync(page, true);
            await admin.CloseAsync();
            return;
        }

        await SetBuyersOnlyAsync(page, false);

        foreach (var user in s_reviews.Select(review => review.User).Append("dana").Distinct())
        {
            var buyer = await browser.NewContextAsync(new BrowserNewContextOptions { ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
            var buyerPage = await buyer.NewPageAsync();
            buyerPage.SetDefaultTimeout(60_000);
            await buyerPage.GotoAsync(BaseUrl + "/Login");
            await buyerPage.Locator("#LoginForm_UserName").FillAsync(user);
            await buyerPage.Locator("#LoginForm_Password").FillAsync(Password);
            await buyerPage.Locator("#LoginForm_Password").PressAsync("Enter");
            await buyerPage.WaitForURLAsync(url => !url.Contains("/Login", StringComparison.OrdinalIgnoreCase));
            await buyer.StorageStateAsync(new() { Path = Path.Combine(Root, $"state-{user}.json") });

            foreach (var review in s_reviews.Where(review => review.User == user))
            {
                await buyerPage.GotoAsync(BaseUrl + "/shop/" + review.Product);
                await buyerPage.Locator("#review-rating").SelectOptionAsync(review.Stars.ToString());
                await buyerPage.Locator("#review-title").FillAsync(review.Title);
                await buyerPage.Locator("#review-body").FillAsync(review.Body);
                await buyerPage.Locator("#review-author").FillAsync(review.Author);
                await buyerPage.RunAndWaitForNavigationAsync(() => buyerPage.GetByRole(AriaRole.Button, new() { Name = "Send review" }).ClickAsync());
                Console.WriteLine($"{user} reviewed {review.Product}");
            }

            await buyer.CloseAsync();
        }

        foreach (var review in s_reviews.Where(review => review.Approve))
        {
            await page.GotoAsync(BaseUrl + "/Admin/reviews");
            var row = page.Locator("li.list-group-item").Filter(new() { HasTextString = review.Title }).First;
            await page.RunAndWaitForNavigationAsync(() => row.GetByRole(AriaRole.Link, new() { Name = "Approve" }).ClickAsync());
            Console.WriteLine($"Approved {review.Title}");
        }

        await admin.CloseAsync();
    }

    private static async Task SetBuyersOnlyAsync(IPage page, bool on)
    {
        await page.GotoAsync(BaseUrl + "/Admin/Settings/reviews");
        var box = page.Locator("#ISite_ReviewSettings_BuyersOnly");

        if (await box.IsCheckedAsync() != on)
        {
            await box.SetCheckedAsync(on);
            await page.RunAndWaitForNavigationAsync(() => page.Locator("button.save, form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
        }

        Console.WriteLine($"Only buyers can review: {on}");
    }
}
