using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Marketing video, one method per "clip" of storyboard.json.
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
        ("03-segments", SegmentsAsync),
        ("04-campaigns", CampaignsAsync),
        ("05-consent", ConsentAsync),
        ("06-automations", AutomationsAsync),
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

    // Everyone who has bought and agreed to news and offers (the customers of the demo site's orders).
    private static async Task AcceptMarketingAsync(IPage page)
    {
        await page.GotoAsync(BaseUrl + "/Admin/customers?Options.Filter=All");
        var customers = await page.Locator("li.list-group-item.item a:not(.btn)").EvaluateAllAsync<string[]>("links => links.map(a => a.getAttribute('href')).filter(h => h && h.includes('/customers/'))");

        foreach (var customer in customers.Distinct())
        {
            await page.GotoAsync(BaseUrl + customer);
            var box = page.Locator("#AcceptsMarketing");

            if (await box.CountAsync() == 0 || await box.IsDisabledAsync())
            {
                continue;
            }

            await box.SetCheckedAsync(true);
            await page.RunAndWaitForNavigationAsync(() => page.Locator("button.save").First.ClickAsync());
        }

        Console.WriteLine($"{customers.Distinct().Count()} customers accept marketing");
    }

    public static async Task PrepareAsync(IBrowser browser)
    {
        var admin = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await admin.NewPageAsync();
        page.SetDefaultTimeout(120_000);

        await EnableFeaturesAsync(page, "CrestApps.OrchardCore.Marketing", "OrchardCore.Email.Smtp");

        // The site's base URL (for the unsubscribe links), and an email that is written to a folder instead of being sent.
        
        await page.GotoAsync(BaseUrl + "/Admin/Settings/general");
        await page.Locator("#ISite_BaseUrl").FillAsync(BaseUrl);
        await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());

        await page.GotoAsync(BaseUrl + "/Admin/Settings/email");
        await page.GetByText("SMTP", new() { Exact = true }).First.ClickAsync();
        await page.Locator("#ISite_SmtpSettings_IsEnabled").SetCheckedAsync(true);
        await page.Locator("#ISite_SmtpSettings_DefaultSender").FillAsync("news@contoso.example");
        await page.Locator("#ISite_SmtpSettings_Host").FillAsync("127.0.0.1");
        await page.Locator("#ISite_SmtpSettings_Port").FillAsync("2525");
        await page.Locator("#ISite_SmtpSettings_AutoSelectEncryption").SetCheckedAsync(false);
        await page.Locator("#ISite_SmtpSettings_EncryptionMethod").SelectOptionAsync("None");
        await page.Locator("#ISite_SmtpSettings_RequireCredentials").SetCheckedAsync(false);
        await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
        Console.WriteLine("EMAIL SAVE: " + string.Join(" / ", await page.Locator(".field-validation-error, .text-danger, .alert, .toast-body, .message-success, .message-error").AllInnerTextsAsync()));

        await AcceptMarketingAsync(page);

        // A segment and a draft campaign, so the lists are not empty.
        await page.GotoAsync(BaseUrl + "/Admin/marketing/segments");

        if (await page.GetByText("Lapsed buyers", new() { Exact = true }).CountAsync() == 0)
        {
            await page.GotoAsync(BaseUrl + "/Admin/marketing/segments/create");
            await page.Locator("#Name").FillAsync("Lapsed buyers");
            await page.Locator("#Description").FillAsync("Bought before, but not in the last 90 days");
            await page.Locator("#LastOrderAtLeastDaysAgo").FillAsync("90");
            await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
            Console.WriteLine("Added the Lapsed buyers segment");

            await page.GotoAsync(BaseUrl + "/Admin/marketing/segments/create");
            await page.Locator("#Name").FillAsync("New customers");
            await page.Locator("#Description").FillAsync("First ordered in the last 30 days");
            await page.Locator("#FirstOrderWithinDays").FillAsync("30");
            await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
            Console.WriteLine("Added the New customers segment");
        }

        await admin.CloseAsync();
    }
}
