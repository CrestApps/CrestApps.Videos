using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Company Approvals and Pay on Account video, one method per "clip" of storyboard.json.
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
        ("03-terms", TermsAsync),
        ("04-buyer", BuyerAsync),
        ("05-approve", ApproveAsync),
        ("06-pay", PayAsync),
        ("07-owed", OwedAsync),
        ("08-quote", QuoteAsync),
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

    private static ILocator Save(IPage page) => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    public static async Task PrepareAsync(IBrowser browser)
    {
        var admin = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await admin.NewPageAsync();
        page.SetDefaultTimeout(120_000);

        await EnableFeaturesAsync(page, "CrestApps.OrchardCore.Companies", "OrchardCore.Email.Smtp");

        // The site's base URL, and an email that is written to a folder instead of being sent.
        await page.GotoAsync(BaseUrl + "/Admin/Settings/general");
        await page.Locator("#ISite_BaseUrl").FillAsync(BaseUrl);
        await page.RunAndWaitForNavigationAsync(() => Save(page).ClickAsync());

        await page.GotoAsync(BaseUrl + "/Admin/Settings/email");
        await page.GetByText("SMTP", new() { Exact = true }).First.ClickAsync();
        await page.Locator("#ISite_SmtpSettings_IsEnabled").SetCheckedAsync(true);
        await page.Locator("#ISite_SmtpSettings_DefaultSender").FillAsync("store@contoso.example");
        await page.Locator("#ISite_SmtpSettings_Host").FillAsync("127.0.0.1");
        await page.Locator("#ISite_SmtpSettings_Port").FillAsync("2525");
        await page.Locator("#ISite_SmtpSettings_AutoSelectEncryption").SetCheckedAsync(false);
        await page.Locator("#ISite_SmtpSettings_EncryptionMethod").SelectOptionAsync("None");
        await page.Locator("#ISite_SmtpSettings_RequireCredentials").SetCheckedAsync(false);
        await page.RunAndWaitForNavigationAsync(() => Save(page).ClickAsync());

        // The deposit is paid by bank transfer (staff record it as received), and buyers may ask for quotes.
        await page.GotoAsync(BaseUrl + "/Admin/Settings/paylater");
        await page.Locator("#ISite_BankTransferSettings_Enabled").SetCheckedAsync(true);
        await page.Locator("#ISite_BankTransferSettings_Instructions").FillAsync("Contoso, account 000123456, Springfield Credit Union. Use the order number as the reference.");
        await page.RunAndWaitForNavigationAsync(() => Save(page).ClickAsync());

        await page.GotoAsync(BaseUrl + "/Admin/Settings/orders");
        await page.Locator("#ISite_OrderSettings_AllowQuoteRequests").SetCheckedAsync(true);
        await page.RunAndWaitForNavigationAsync(() => Save(page).ClickAsync());

        await admin.CloseAsync();

        foreach (var user in new[] { "riley", "morgan" })
        {
            await SaveSignInAsync(browser, user);
        }
    }
}
