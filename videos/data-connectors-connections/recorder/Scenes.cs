using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the "Data connections" video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-open", OpenAsync),
        ("03-files", FilesAsync),
        ("04-http", HttpAsync),
        ("05-fix", FixAsync),
        ("06-manage", ManageAsync),
        ("07-kinds", KindsAsync),
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
    /// Creates the "Orders" data set of the Sales API connection (outside the capture), so the manage clip has a data set
    /// that blocks deleting the connection. Run after 05-fix.
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = 1536, Height = 864 } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        await page.GotoAsync(BaseUrl + "/Admin/data-connectors/data-sets");
        await page.GetByRole(AriaRole.Button, new() { Name = "Add Data Set" }).First.ClickAsync();
        await page.Locator(".modal.show input[type=search]").FillAsync("Sales");
        var card = page.Locator(".modal.show a[href*='/data-sets/Create/']:visible").First;
        await card.WaitForAsync();
        await page.RunAndWaitForNavigationAsync(() => card.ClickAsync());
        await page.Locator("#ConnectedDataSet_DisplayText").FillAsync("Orders");
        await page.Locator("#ConnectedDataSet_Resource").FillAsync("/orders");
        await page.Locator("#ConnectedDataSet_RecordsPath").FillAsync("data.items");
        await page.Locator("#ConnectedDataSet_PaginationType").SelectOptionAsync("PageNumber");
        await page.Locator("#ConnectedDataSet_SizeParameter").FillAsync("size");
        await page.Locator("#ConnectedDataSet_PageSize").FillAsync("25");
        await page.RunAndWaitForNavigationAsync(() => page.Locator("button[name='submit.Save']").ClickAsync());
        Console.WriteLine("Created the Orders data set");
        await context.CloseAsync();
    }

    // ----- Helpers -----

    private static ILocator Nav(Scene scene, string text)
        => scene.Page.Locator("#left-nav a:visible").Filter(new() { HasTextRegex = new Regex($"^\\s*{Regex.Escape(text)}\\s*$") }).First;

    private static ILocator AddConnection(Scene scene) => scene.Page.GetByRole(AriaRole.Button, new() { Name = "Add Connection" }).First;

    private static ILocator CardAdd(Scene scene, string kind) => scene.Page.Locator($".modal.show a[href$='/Create/{kind}']");

    private static ILocator Submit(Scene scene, string name) => scene.Page.Locator($"button[name='submit.{name}']");

    private static ILocator Row(Scene scene, string name)
        => scene.Page.Locator("li.item").Filter(new() { HasText = name }).First;

    private static async Task OpenActionsAsync(Scene scene, string rowName)
        => await scene.ClickAsync(Row(scene, rowName).Locator("button.actions"));

    // ----- Clips -----

    private static async Task OpenAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/data-connectors/connections");
        await scene.StartCaptureAsync();

        await scene.StepAsync("open-1", async () =>
        {
            await scene.HighlightAsync(Nav(scene, "Connections"), "Tools, Data Connectors, Connections");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".alert").First);
        });

        await scene.StepAsync("open-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(AddConnection(scene));
            await scene.Page.Locator(".modal.show").WaitForAsync();
            await scene.PauseAsync(1500);
            await scene.TypeAsync(scene.Page.Locator(".modal.show input[type=search]"), "sql", delay: 90);
            await scene.PauseAsync(1200);
        });

        await scene.HoldAsync(1);
    }

    private static async Task FilesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/Media");
        await scene.StartCaptureAsync();

        await scene.StepAsync("files-1", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByText("demo-data", new() { Exact = true }).First);
            await scene.Page.GetByText("customers.csv").First.WaitForAsync();
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.GetByText("customers.csv").First);
        });

        await scene.StepAsync("files-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/data-connectors/connections");
            await scene.ClickAsync(AddConnection(scene));
            await scene.Page.Locator(".modal.show").WaitForAsync();
            await scene.PauseAsync(500);
            await scene.ClickAndWaitForNavigationAsync(CardAdd(scene, "MediaFiles"));
            await scene.TypeAsync(scene.Page.Locator("#DataConnection_DisplayText"), "Demo files", delay: 55);
            await scene.TypeAsync(scene.Page.Locator("#DataConnection_Folder"), "demo-data", delay: 55);
        });

        await scene.StepAsync("files-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Submit(scene, "Test"));
            await scene.PauseAsync(600);
        });

        await scene.ClearHighlightsAsync();
        await scene.HoldAsync(1);
    }

    private static async Task HttpAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/data-connectors/connections/Create/HttpApi");
        await scene.StartCaptureAsync();

        await scene.StepAsync("http-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#DataConnection_DisplayText"), "Sales API", delay: 55);
            await scene.TypeAsync(scene.Page.Locator("#DataConnection_BaseUrl"), "http://localhost:5555/", delay: 40);
        });

        await scene.StepAsync("http-2", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#DataConnection_AuthenticationType"), "ApiKey");
            await scene.PauseAsync(500);
            await scene.HighlightAsync(scene.Page.Locator("#DataConnection_ApiKeyName"));
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#DataConnection_ApiKey"), "demo-key-not-real", delay: 55);
        });

        await scene.StepAsync("http-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Submit(scene, "Test"));
            await scene.PauseAsync(600);
        });

        await scene.ClearHighlightsAsync();
        await scene.HoldAsync(1);
    }

    private static async Task FixAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/data-connectors/connections");
        await scene.StartCaptureAsync();

        await scene.StepAsync("fix-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Sales API").GetByRole(AriaRole.Link, new() { Name = "Edit" }));
            await scene.ScrollAsync(0, 300);
            await scene.HighlightAsync(scene.Page.Locator("#DataConnection_ApiKey"));
        });

        await scene.StepAsync("fix-2", async () =>
        {
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 1200);
            await scene.MoveToAsync(Submit(scene, "Test"));
        });

        await scene.StepAsync("fix-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Submit(scene, "Test"));
            await scene.PauseAsync(600);
        });

        await scene.ClearHighlightsAsync();
        await scene.HoldAsync(1);
    }

    private static async Task ManageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/data-connectors/connections");
        await scene.StartCaptureAsync();

        await scene.StepAsync("manage-1", async () =>
        {
            await scene.HighlightAsync(Row(scene, "Sales API"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await OpenActionsAsync(scene, "Sales API");
            await scene.PauseAsync(1200);
            await scene.ClickAsync(Row(scene, "Sales API").Locator("a[href*='/Test/']"));
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("manage-2", async () =>
        {
            await OpenActionsAsync(scene, "Sales API");
            await scene.HighlightAsync(Row(scene, "Sales API").Locator("a[href*='/data-sets/Create/']"));
        });

        await scene.StepAsync("manage-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Row(scene, "Sales API").Locator("a.text-danger"));
            await scene.Page.Locator(".modal.show").WaitForAsync();
            await scene.PauseAsync(800);
            await scene.ClickAsync(scene.Page.Locator(".modal.show .btn-danger, .modal.show .btn-primary").First);
            await scene.PauseAsync(1500);
        });

        await scene.HoldAsync(1);
    }

    private static async Task KindsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/data-connectors/connections/Create/Elasticsearch");
        await scene.StartCaptureAsync();

        await scene.StepAsync("kinds-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#DataConnection_BaseUrl"));
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#DataConnection_AuthenticationType"));
        });

        await scene.StepAsync("kinds-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/data-connectors/connections/Create/SqlServer");
            await scene.HighlightAsync(scene.Page.Locator("#DataConnection_Server"));
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#DataConnection_Authentication"));
        });

        await scene.StepAsync("kinds-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 400);
            await scene.HighlightAsync(scene.Page.Locator("#DataConnection_Encrypt"));
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#DataConnection_ReadOnlyIntent"));
        });

        await scene.StepAsync("kinds-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#DataConnection_UserName"));
        });

        await scene.ClearHighlightsAsync();
        await scene.HoldAsync(1);
    }
}
