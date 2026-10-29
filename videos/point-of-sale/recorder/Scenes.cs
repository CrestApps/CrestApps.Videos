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
        ("04-registers", RegistersAsync),
        ("06-shifts", ShiftsAsync),
        ("07-orders", OrdersAsync),
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

    // The sign-in server and the till app's client, as a recipe: the till app signs staff in with the authorization code flow.
    private const string TillAppRecipe = "{\"steps\":[{\"name\":\"OpenIdServerSettings\",\"EnableTokenEndpoint\":true,\"EnableAuthorizationEndpoint\":true,\"EnableUserInfoEndpoint\":true,\"EnableLogoutEndpoint\":true,\"AllowAuthorizationCodeFlow\":true,\"AllowRefreshTokenFlow\":true,\"RequireProofKeyForCodeExchange\":true},{\"name\":\"OpenIdValidationSettings\",\"Tenant\":\"Default\"},{\"name\":\"OpenIdApplication\",\"ClientId\":\"till-app\",\"DisplayName\":\"Till app\",\"Type\":\"public\",\"ConsentType\":\"implicit\",\"AllowAuthorizationCodeFlow\":true,\"AllowRefreshTokenFlow\":true,\"RequireProofKeyForCodeExchange\":true,\"RedirectUris\":\"http://localhost:9/callback\",\"ScopeEntries\":[{\"Name\":\"profile\"},{\"Name\":\"roles\"}]}]}";

    /// <summary>
    /// Enables the features, sets up the sign-in of a till app (so seed.py can ring up sales as staff would), adds the stores, and
    /// adds the first register (not recorded). Run seed.py afterwards: it opens the shifts and rings up the sales.
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var admin = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await admin.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        await EnableFeaturesAsync(page, "OrchardCore.Deployment", "CrestApps.OrchardCore.PointOfSale", "CrestApps.OrchardCore.Transactions.Receipts", "OrchardCore.OpenId", "OrchardCore.OpenId.Management", "OrchardCore.OpenId.Server", "OrchardCore.OpenId.Validation");

        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        var imported = await page.EvaluateAsync<int>(
            "async json => { const token = document.querySelector('input[name=__RequestVerificationToken]').value; const body = new URLSearchParams(); body.set('Json', json); body.set('__RequestVerificationToken', token); const r = await fetch('/Admin/DeploymentPlan/Import/Json', { method: 'POST', credentials: 'include', headers: { 'Content-Type': 'application/x-www-form-urlencoded' }, body }); return r.status; }",
            TillAppRecipe);
        Console.WriteLine($"Till app sign-in: {imported}");

        // The stores: a register sells from one of them.
        await page.GotoAsync(BaseUrl + "/Admin/Settings/inventory");

        if (await page.Locator("#ISite_InventorySettings_Locations_1__Name").InputValueAsync() == string.Empty)
        {
            await page.Locator("#ISite_InventorySettings_Locations_1__Name").FillAsync("Downtown store");
            await page.Locator("#ISite_InventorySettings_Locations_1__Address").FillAsync("100 Main Street, Springfield");
            await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());

            await page.GotoAsync(BaseUrl + "/Admin/Settings/inventory");
            await page.Locator("#ISite_InventorySettings_Locations_2__Name").FillAsync("Uptown store");
            await page.Locator("#ISite_InventorySettings_Locations_2__Address").FillAsync("45 Hill Road, Springfield");
            await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
            Console.WriteLine("Added the stores");
        }

        await page.GotoAsync(BaseUrl + "/Admin/point-of-sale/registers");

        if (await page.GetByText("Downtown till 1").CountAsync() == 0)
        {
            await page.GotoAsync(BaseUrl + "/Admin/point-of-sale/create");
            await page.Locator("#Name").FillAsync("Downtown till 1");
            await page.Locator("#LocationId").SelectOptionAsync(new SelectOptionValue { Label = "Downtown store" });
            await page.Locator("#AddressLine1").FillAsync("100 Main Street");
            await page.Locator("#City").FillAsync("Springfield");
            await page.Locator("#Region").FillAsync("OR");
            await page.Locator("#PostalCode").FillAsync("97477");
            await page.Locator("#Country").FillAsync("US");
            await page.Locator("#Currency").FillAsync("USD");
            await page.RunAndWaitForNavigationAsync(() => page.Locator("button.save, form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last.ClickAsync());
            Console.WriteLine("Added Downtown till 1");
        }

        await admin.CloseAsync();
    }
}
