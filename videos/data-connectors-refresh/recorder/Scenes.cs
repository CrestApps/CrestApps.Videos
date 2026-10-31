using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the "Refreshing imported data sets" video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // The fake sales API of seed/fake-api.py.
    private const string Api = "http://localhost:5555";

    private const string DataSets = "/Admin/data-connectors/data-sets";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-create", CreateAsync),
        ("03-now", NowAsync),
        ("04-fail", FailAsync),
        ("05-more", MoreAsync),
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
    /// Nothing to prepare: the clips start from the site of the data sets video (the four connections, no data sets) with
    /// the fake API running and the site allowed to reach its own network.
    /// </summary>
    public static Task PrepareAsync(IBrowser browser) => Task.CompletedTask;

    // ----- Helpers -----

    private static async Task SetApiAsync(string query)
    {
        using var http = new HttpClient();
        await http.GetStringAsync($"{Api}/control?{query}");
    }

    private static ILocator Submit(Scene scene, string name) => scene.Page.Locator($"button[name='submit.{name}']");

    private static ILocator Row(Scene scene, string name) => scene.Page.Locator("li.item").Filter(new() { HasText = name }).First;

    private static async Task OpenActionsAsync(Scene scene, string rowName)
        => await scene.ClickAsync(Row(scene, rowName).Locator("button.actions"));

    private static ILocator FieldRow(Scene scene, string source)
        => scene.Page.Locator("table tbody tr").Filter(new() { Has = scene.Page.Locator($"input[name$='.SourceName'][value='{source}']") }).First;

    // Reloads the data sets until the list shows the text: the refresh task runs once a minute. Used off camera.
    private static async Task WaitForListAsync(Scene scene, string text)
    {
        for (var attempt = 0; attempt < 60; attempt++)
        {
            await scene.Page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });

            if ((await scene.Page.InnerTextAsync("body")).Contains(text))
            {
                return;
            }

            await Task.Delay(5000);
        }

        throw new TimeoutException($"The list never showed '{text}'.");
    }

    private static async Task<string> CreateHrefAsync(Scene scene, string connection)
        => await scene.Page.Locator("li.item").Filter(new() { HasText = connection }).Locator("a[href*='/data-sets/Create/']").First.GetAttributeAsync("href");

    // ----- Clips -----

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await SetApiAsync("fail=0");
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync(DataSets);
        await scene.StartCaptureAsync();

        await scene.StepAsync("imp-1", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Add Data Set" }).First);
            await scene.Page.Locator(".modal.show").WaitForAsync();
            await scene.PauseAsync(300);
            await scene.TypeAsync(scene.Page.Locator(".modal.show input[type=search]"), "Sales", delay: 40);
            await scene.PauseAsync(300);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".modal.show a[href*='/data-sets/Create/']:visible").First);
            await scene.TypeAsync(scene.Page.Locator("#ConnectedDataSet_DisplayText"), "Imported orders", delay: 22);
            await scene.TypeAsync(scene.Page.Locator("#ConnectedDataSet_Resource"), "/orders", delay: 22);
            await scene.ClickAsync(scene.Page.Locator("#ConnectedDataSet_AccessMode_Section_Imported"));
        });

        await scene.StepAsync("imp-2", async () =>
        {
            await scene.Page.Locator("#ConnectedDataSet_RefreshIntervalMinutes").ScrollIntoViewIfNeededAsync();
            await scene.TypeAsync(scene.Page.Locator("#ConnectedDataSet_RefreshIntervalMinutes"), "60", delay: 80);
            await scene.HighlightAsync(scene.Page.Locator("#ConnectedDataSet_RefreshIntervalMinutes"));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#ConnectedDataSet_RefreshMode"));
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#ConnectedDataSet_SchemaChangePolicy"));
        });

        await scene.StepAsync("imp-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.Page.Locator("#ConnectedDataSet_RecordsPath").FillAsync("data.items");
            await scene.Page.Locator("#ConnectedDataSet_PaginationType").SelectOptionAsync("PageNumber");
            await scene.Page.Locator("#ConnectedDataSet_PageSize").FillAsync("25");
            await scene.Page.Locator("button[name='submit.DiscoverFields']").ScrollIntoViewIfNeededAsync();
            await scene.ClickAndWaitForNavigationAsync(Submit(scene, "DiscoverFields"));
            await FieldRow(scene, "orderId").ScrollIntoViewIfNeededAsync();
            await scene.ClickAsync(FieldRow(scene, "orderId").Locator("input[type=checkbox][name$='.IsIdentifier']"));
            await scene.Page.Locator("button[name='submit.Save']").ScrollIntoViewIfNeededAsync();
            await scene.ClickAndWaitForNavigationAsync(Submit(scene, "Save"));
            await scene.HighlightAsync(Row(scene, "Imported orders"));
        });

        await scene.StepAsync("imp-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(() => WaitForListAsync(scene, "Succeeded"));
            await scene.NavigateAsync(DataSets);
            await scene.HighlightAsync(Row(scene, "Imported orders"));
        });

        await scene.ClearHighlightsAsync();
        await scene.HoldAsync(1);
    }

    private static async Task NowAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await SetApiAsync("fail=0&extra=5");
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync(DataSets);
        await scene.StartCaptureAsync();

        await scene.StepAsync("now-1", async () =>
        {
            await scene.HighlightAsync(Row(scene, "Imported orders"));
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await OpenActionsAsync(scene, "Imported orders");
            await scene.PauseAsync(1000);
            await scene.HighlightAsync(Row(scene, "Imported orders").Locator("a[href*='/Refresh/']"));
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Imported orders").Locator("a[href*='/Refresh/']"));
            await scene.PauseAsync(2500);
        });

        await scene.StepAsync("now-2", async () =>
        {
            await scene.OffCameraAsync(() => WaitForListAsync(scene, "65 rows"));
            await scene.NavigateAsync(DataSets);
            await scene.HighlightAsync(Row(scene, "Imported orders"));
        });

        await scene.StepAsync("now-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await OpenActionsAsync(scene, "Imported orders");
            await scene.PauseAsync(800);
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Imported orders").Locator("a[href*='/History/']"));
            await scene.PauseAsync(1000);
            await scene.HighlightAsync(scene.Page.Locator("li.item, .list-group-item").Nth(1));
        });

        await scene.ClearHighlightsAsync();
        await scene.HoldAsync(1);
    }

    private static async Task FailAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await SetApiAsync("fail=1");
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync(DataSets);
        await scene.StartCaptureAsync();

        await scene.StepAsync("fail-1", async () =>
        {
            await OpenActionsAsync(scene, "Imported orders");
            await scene.PauseAsync(800);
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Imported orders").Locator("a[href*='/Refresh/']"));
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("fail-2", async () =>
        {
            await scene.OffCameraAsync(() => WaitForListAsync(scene, "Failed"));
            await scene.NavigateAsync(DataSets);
            await scene.HighlightAsync(Row(scene, "Imported orders"));
        });

        await scene.StepAsync("fail-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await OpenActionsAsync(scene, "Imported orders");
            await scene.PauseAsync(800);
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Imported orders").Locator("a[href*='/Preview/']"));
            await scene.PauseAsync(1500);
            await scene.ScrollAsync(0, 400);
        });

        await scene.StepAsync("fail-4", async () =>
        {
            await scene.NavigateAsync(DataSets);
            await OpenActionsAsync(scene, "Imported orders");
            await scene.PauseAsync(800);
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Imported orders").Locator("a[href*='/History/']"));
            await scene.PauseAsync(1000);
            await scene.HighlightAsync(scene.Page.Locator("li.item, .list-group-item").Nth(1));
        });

        await scene.ClearHighlightsAsync();
        await scene.HoldAsync(1);
        await SetApiAsync("fail=0");
    }

    private static async Task MoreAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        // The create page of the SQL Server connection, found before the capture starts.
        await scene.OpenAsync("/Admin/data-connectors/connections");
        var sql = await CreateHrefAsync(scene, "Warehouse");

        await scene.OpenAsync(sql);
        await scene.Page.Locator("#ConnectedDataSet_AccessMode_Section_Imported").CheckAsync();
        await scene.Page.Locator("#ConnectedDataSet_RefreshIntervalMinutes").ScrollIntoViewIfNeededAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("more-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#ConnectedDataSet_RefreshMode"));
            await scene.PauseAsync(1500);
            await scene.SelectAsync(scene.Page.Locator("#ConnectedDataSet_RefreshMode"), "Incremental");
        });

        await scene.StepAsync("more-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#ConnectedDataSet_IncrementalField"));
        });

        await scene.StepAsync("more-3", async () =>
        {
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("more-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#ConnectedDataSet_SchemaChangePolicy"));
        });

        await scene.ClearHighlightsAsync();
        await scene.HoldAsync(1);
    }
}
