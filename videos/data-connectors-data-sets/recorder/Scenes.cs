using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the "Data sets" video, one method per "clip" of storyboard.json.
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
        ("02-file", FileAsync),
        ("03-preview", PreviewAsync),
        ("04-api", ApiAsync),
        ("05-others", OthersAsync),
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
    /// Creates the four connections the clips start from (outside the capture): Demo files, Sales API, Warehouse (SQL Server)
    /// and Search cluster (Elasticsearch). The SQL Server and Elasticsearch ones are only saved, never reached.
    /// The site must allow its own network (the fake API listens on localhost).
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = 1536, Height = 864 } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        async Task SaveAsync(string kind, params (string Id, string Value)[] fields)
        {
            await page.GotoAsync(BaseUrl + "/Admin/data-connectors/connections/Create/" + kind);

            foreach (var (id, value) in fields)
            {
                var element = page.Locator("#" + id);

                if (await element.EvaluateAsync<string>("e => e.tagName") == "SELECT")
                {
                    await element.SelectOptionAsync(value);
                }
                else
                {
                    await element.FillAsync(value);
                }
            }

            await page.RunAndWaitForNavigationAsync(() => page.Locator("button[name='submit.Save']").ClickAsync());
        }

        await SaveAsync("MediaFiles", ("DataConnection_DisplayText", "Demo files"), ("DataConnection_Folder", "demo-data"));
        await SaveAsync("HttpApi", ("DataConnection_DisplayText", "Sales API"), ("DataConnection_BaseUrl", "http://localhost:5555/"));
        await SaveAsync("SqlServer", ("DataConnection_DisplayText", "Warehouse"), ("DataConnection_Server", "sql.example.com,1433"), ("DataConnection_Database", "Warehouse"), ("DataConnection_UserName", "reports_reader"), ("DataConnection_Password", "demo-not-a-secret"));
        await SaveAsync("Elasticsearch", ("DataConnection_DisplayText", "Search cluster"), ("DataConnection_BaseUrl", "https://search.example.com:9200"));
        Console.WriteLine("Created the connections");
        await context.CloseAsync();
    }

    // ----- Helpers -----

    private static ILocator AddDataSet(Scene scene) => scene.Page.GetByRole(AriaRole.Button, new() { Name = "Add Data Set" }).First;

    private static ILocator Submit(Scene scene, string name) => scene.Page.Locator($"button[name='submit.{name}']");

    private static ILocator Row(Scene scene, string name) => scene.Page.Locator("li.item").Filter(new() { HasText = name }).First;

    private static ILocator Card(Scene scene, string connection)
        => scene.Page.Locator(".modal.show .card").Filter(new() { HasText = connection }).Locator("a[href*='/data-sets/Create/']").First;

    private static async Task OpenActionsAsync(Scene scene, string rowName)
        => await scene.ClickAsync(Row(scene, rowName).Locator("button.actions"));

    private static ILocator FieldRow(Scene scene, string source)
        => scene.Page.Locator("table tbody tr").Filter(new() { Has = scene.Page.Locator($"input[name$='.SourceName'][value='{source}']") }).First;
}
