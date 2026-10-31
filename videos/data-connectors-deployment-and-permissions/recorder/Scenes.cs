using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the "Deployment and permissions of Data Connectors" video, one method per "clip" of storyboard.json.
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
        ("02-roles", RolesAsync),
        ("03-plan", PlanAsync),
        ("05-check", CheckAsync),
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

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    /// <summary>
    /// Creates what the clips start from (outside the capture), on the site of the data sets video: the live data sets
    /// Orders and Customers, the imported data set Imported orders (waiting for its first import), and the role Analyst.
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = 1536, Height = 864 } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        async Task AddDataSetAsync(string connection, string name, string resource, bool imported, string identifier, string dateField, bool api)
        {
            await page.GotoAsync(BaseUrl + "/Admin/data-connectors/data-sets");
            await page.GetByRole(AriaRole.Button, new() { Name = "Add Data Set" }).First.ClickAsync();
            await page.Locator(".modal.show input[type=search]").FillAsync(connection);
            var card = page.Locator(".modal.show a[href*='/data-sets/Create/']:visible").First;
            await card.WaitForAsync();
            await page.RunAndWaitForNavigationAsync(() => card.ClickAsync());
            await page.Locator("#ConnectedDataSet_DisplayText").FillAsync(name);
            await page.Locator("#ConnectedDataSet_Resource").FillAsync(resource);
            await page.Locator(imported ? "#ConnectedDataSet_AccessMode_Section_Imported" : "#ConnectedDataSet_AccessMode_Section_Live").CheckAsync();

            if (api)
            {
                await page.Locator("#ConnectedDataSet_RecordsPath").FillAsync("data.items");
                await page.Locator("#ConnectedDataSet_PaginationType").SelectOptionAsync("PageNumber");
                await page.Locator("#ConnectedDataSet_PageSize").FillAsync("25");
            }

            if (imported)
            {
                await page.Locator("#ConnectedDataSet_RefreshIntervalMinutes").FillAsync("60");
            }

            await page.RunAndWaitForNavigationAsync(() => page.Locator("button[name='submit.DiscoverFields']").ClickAsync());
            await page.Locator($"table tbody tr:has(input[name$='.SourceName'][value='{identifier}']) input[type=checkbox][name$='.IsIdentifier']").CheckAsync();
            await page.Locator("#ConnectedDataSet_DefaultDateField").SelectOptionAsync(dateField);
            await page.RunAndWaitForNavigationAsync(() => page.Locator("button[name='submit.Save']").ClickAsync());
        }

        await AddDataSetAsync("Sales", "Orders", "/orders", false, "orderId", "orderedOn", true);
        await AddDataSetAsync("Demo", "Customers", "demo-data/customers.csv", false, "CustomerId", "Since", false);
        await AddDataSetAsync("Sales", "Imported orders", "/orders", true, "orderId", "orderedOn", true);

        await page.GotoAsync(BaseUrl + "/Admin/Roles/Create");
        await page.Locator("input[type=text]:visible").First.FillAsync("Analyst");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Create" }).Last.ClickAsync());
        Console.WriteLine("Prepared");
        await context.CloseAsync();
    }
}
