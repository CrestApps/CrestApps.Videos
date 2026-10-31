using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static async Task PickConnectionAsync(Scene scene, string filter)
    {
        await scene.ClickAsync(AddDataSet(scene));
        await scene.Page.Locator(".modal.show").WaitForAsync();
        await scene.PauseAsync(800);
        await scene.TypeAsync(scene.Page.Locator(".modal.show input[type=search]"), filter, delay: 80);
        await scene.PauseAsync(600);
        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".modal.show a[href*='/data-sets/Create/']:visible").First);
    }

    private static async Task FileAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/data-connectors/data-sets");
        await scene.StartCaptureAsync();

        await scene.StepAsync("file-1", async () =>
        {
            await scene.ClickAsync(AddDataSet(scene));
            await scene.Page.Locator(".modal.show").WaitForAsync();
            await scene.PauseAsync(1500);
            await scene.ClickAsync(scene.Page.Locator(".modal.show").GetByText("Media library files").First);
            await scene.PauseAsync(1200);
            await scene.TypeAsync(scene.Page.Locator(".modal.show input[type=search]"), "demo", delay: 90);
            await scene.PauseAsync(800);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".modal.show a[href*='/data-sets/Create/']:visible").First);
        });

        await scene.StepAsync("file-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#ConnectedDataSet_DisplayText"), "Customers", delay: 60);
            await scene.TypeAsync(scene.Page.Locator("#ConnectedDataSet_Resource"), "demo-data/customers.csv", delay: 35);
        });

        await scene.StepAsync("file-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#ConnectedDataSet_AccessMode_Section_Live"));
            await scene.PauseAsync(1200);
            await scene.Page.Locator("#ConnectedDataSet_Delimiter").ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.Locator("#ConnectedDataSet_Delimiter"));
            await scene.PauseAsync(1600);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#ConnectedDataSet_HasHeaderRow"));
            await scene.PauseAsync(1600);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#ConnectedDataSet_SheetName"));
        });

        await scene.StepAsync("file-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.Page.Locator("button[name='submit.DiscoverFields']").ScrollIntoViewIfNeededAsync();
            await scene.ClickAndWaitForNavigationAsync(Submit(scene, "DiscoverFields"));
            await scene.PauseAsync(800);
            await FieldRow(scene, "CustomerId").ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(600);
            await scene.HighlightAsync(FieldRow(scene, "Since").Locator("select[name$='.Type']"));
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(FieldRow(scene, "PostalCode").Locator("select[name$='.Type']"));
        });

        await scene.StepAsync("file-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(FieldRow(scene, "Email").Locator("input[type=checkbox][name$='.IsIncluded']"));
            await scene.HighlightAsync(FieldRow(scene, "Email"));
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(FieldRow(scene, "CustomerId").Locator("input[type=text][name$='.DisplayName']"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(FieldRow(scene, "CustomerId").Locator("input[type=checkbox][name$='.IsIdentifier']"));
            await scene.HighlightAsync(FieldRow(scene, "CustomerId"));
        });

        await scene.StepAsync("file-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.Page.Locator("#ConnectedDataSet_DefaultDateField").ScrollIntoViewIfNeededAsync();
            await scene.SelectAsync(scene.Page.Locator("#ConnectedDataSet_DefaultDateField"), "Since");
            await scene.PauseAsync(800);
            await scene.Page.Locator("button[name='submit.Save']").ScrollIntoViewIfNeededAsync();
            await scene.ClickAndWaitForNavigationAsync(Submit(scene, "Save"));
        });

        await scene.HoldAsync(1);
    }

    private static async Task PreviewAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/data-connectors/data-sets");
        await scene.StartCaptureAsync();

        await scene.StepAsync("prev-1", async () =>
        {
            await scene.HighlightAsync(Row(scene, "Customers"));
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await OpenActionsAsync(scene, "Customers");
            await scene.PauseAsync(800);
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Customers").Locator("a[href*='/Preview/']"));
        });

        await scene.StepAsync("prev-2", async () =>
        {
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.Locator("table").First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("prev-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Edit" }).Last);
            await scene.HighlightAsync(scene.Page.Locator("code").First);
        });

        await scene.ClearHighlightsAsync();
        await scene.HoldAsync(1);
    }

    private static async Task ApiAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/data-connectors/data-sets");
        await scene.StartCaptureAsync();

        await scene.StepAsync("api-1", async () =>
        {
            await PickConnectionAsync(scene, "Sales");
            await scene.TypeAsync(scene.Page.Locator("#ConnectedDataSet_DisplayText"), "Orders", delay: 60);
            await scene.TypeAsync(scene.Page.Locator("#ConnectedDataSet_Resource"), "/orders", delay: 60);
            await scene.ClickAsync(scene.Page.Locator("#ConnectedDataSet_AccessMode_Section_Live"));
        });

        await scene.StepAsync("api-2", async () =>
        {
            await scene.Page.Locator("#ConnectedDataSet_Method").ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(scene.Page.Locator("#ConnectedDataSet_Method"));
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#ConnectedDataSet_Format"));
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#ConnectedDataSet_RecordsPath"), "data.items", delay: 70);
        });

        await scene.StepAsync("api-3", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#ConnectedDataSet_PaginationType"), "PageNumber");
            await scene.PauseAsync(1200);
            await scene.Page.Locator("#ConnectedDataSet_PageSize").FillAsync(string.Empty);
            await scene.TypeAsync(scene.Page.Locator("#ConnectedDataSet_PageSize"), "25", delay: 70);
            await scene.HighlightAsync(scene.Page.Locator("#ConnectedDataSet_MaxPages"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("api-4", async () =>
        {
            await scene.Page.Locator("button[name='submit.DiscoverFields']").ScrollIntoViewIfNeededAsync();
            await scene.ClickAndWaitForNavigationAsync(Submit(scene, "DiscoverFields"));
            await scene.PauseAsync(800);
            await FieldRow(scene, "orderId").ScrollIntoViewIfNeededAsync();
            await scene.ClickAsync(FieldRow(scene, "orderId").Locator("input[type=checkbox][name$='.IsIdentifier']"));
            await scene.Page.Locator("#ConnectedDataSet_DefaultDateField").ScrollIntoViewIfNeededAsync();
            await scene.SelectAsync(scene.Page.Locator("#ConnectedDataSet_DefaultDateField"), "orderedOn");
            await scene.Page.Locator("button[name='submit.Save']").ScrollIntoViewIfNeededAsync();
            await scene.ClickAndWaitForNavigationAsync(Submit(scene, "Save"));
        });

        await scene.StepAsync("api-5", async () =>
        {
            await OpenActionsAsync(scene, "Orders");
            await scene.PauseAsync(600);
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Orders").Locator("a[href*='/Preview/']"));
            await scene.PauseAsync(1500);
            await scene.ScrollAsync(0, 500);
        });

        await scene.HoldAsync(1);
    }

    private static async Task OthersAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        // The create pages of the SQL Server and Elasticsearch connections, found before the capture starts.
        await scene.OpenAsync("/Admin/data-connectors/connections");
        async Task<string> CreateHrefAsync(string name)
            => await Row(scene, name).Locator("a[href*='/data-sets/Create/']").First.GetAttributeAsync("href");

        var sql = await CreateHrefAsync("Warehouse");
        var search = await CreateHrefAsync("Search cluster");

        await scene.OpenAsync(sql);
        await scene.StartCaptureAsync();

        await scene.StepAsync("oth-1", async () =>
        {
            await scene.Page.Locator("#ConnectedDataSet_Query").ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(scene.Page.Locator("#ConnectedDataSet_Query"));
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#ConnectedDataSet_CommandTimeoutSeconds"));
        });

        await scene.StepAsync("oth-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync(search);
            await scene.Page.Locator("#ConnectedDataSet_Query").ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(scene.Page.Locator("#ConnectedDataSet_Query"));
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#ConnectedDataSet_ScrollMinutes"));
        });

        await scene.StepAsync("oth-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.Page.Locator("#ConnectedDataSet_AccessMode_Section_Imported").ScrollIntoViewIfNeededAsync();
            await scene.ClickAsync(scene.Page.Locator("#ConnectedDataSet_AccessMode_Section_Imported"));
            await scene.PauseAsync(1000);
            await scene.HighlightAsync(scene.Page.Locator("#ConnectedDataSet_RefreshIntervalMinutes"));
        });

        await scene.ClearHighlightsAsync();
        await scene.HoldAsync(1);
    }
}
