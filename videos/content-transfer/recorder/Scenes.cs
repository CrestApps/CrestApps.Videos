using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Content Transfer video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    // The customers file made by make-data.py.
    public static string DataFolder { get; } = Environment.GetEnvironmentVariable("VIDEO_DATA") ?? Path.Combine(Root, "data");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-find", FindAsync),
        ("03-types", TypesAsync),
        ("04-file", FileAsync),
        ("05-import", ImportAsync),
        ("06-follow", FollowAsync),
        ("07-export", ExportAsync),
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
    /// Enables the provider features (not recorded).
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = StatePath,
            ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height },
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        foreach (var feature in new[]
        {
            "CrestApps.OrchardCore.ContentTransfer",
            "CrestApps.OrchardCore.ContentTransfer.OpenXml",
        })
        {
            await page.GotoAsync(BaseUrl + "/Admin/Features");
            var enable = page.Locator($"a[href$=\"/{feature}/Enable\"]");

            if (await enable.CountAsync() > 0)
            {
                await page.RunAndWaitForNavigationAsync(() => enable.First.ClickAsync());
                Console.WriteLine($"Enabled {feature}");
            }
        }

        await ImportRecipeAsync(page, Path.Combine(Root, "seed", "customer-type.recipe.json"));

        await context.CloseAsync();
    }

    // ----- Helpers -----

    // Runs a recipe through Configuration > Import/Export > JSON Import (not recorded).
    private static async Task ImportRecipeAsync(IPage page, string path)
    {
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        var status = await page.EvaluateAsync<int>(
            @"async (json) => {
                const form = document.querySelector('form[action*=""Import/Json""], form:has(textarea#Json)');
                const data = new URLSearchParams(new FormData(form));
                data.set('Json', json);
                const response = await fetch(form.action, { method: 'POST', body: data, redirect: 'manual' });
                return response.status;
            }",
            await File.ReadAllTextAsync(path));
        Console.WriteLine($"Imported {Path.GetFileName(path)} ({status})");
    }

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator Limited(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-limited-wrapper')][1]");

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static async Task TickAsync(Scene scene, string id, bool on)
    {
        var box = scene.Page.Locator("#" + id);

        if (await box.IsCheckedAsync() != on)
        {
            await scene.ClickAsync(box, pauseAfter: 400);
        }
    }

    private static ILocator FileRequirement(Scene scene, int index)
        => scene.Page.Locator("div.card").Filter(new() { HasText = "File Requirements" }).Last.Locator("ul > li").Nth(index);

    // ----- Clips -----

    private static async Task FindAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await scene.StartCaptureAsync();

        await scene.StepAsync("find-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/content-transfer-entries"), "Content > Import");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/content-transfer-entries"));
            await scene.HighlightAsync(scene.Page.Locator("h1").First, "Bulk Import");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("find-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/export/contents"));
            await scene.HighlightAsync(scene.Page.Locator(".action-bar button.btn-secondary").Last, "Starts a new export");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task TypesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.StartCaptureAsync();

        await scene.StepAsync("types-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ContentTypes/List"), "Design > Content Definition > Content Types");
            await scene.PauseAsync(2800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/Admin/ContentTypes/Edit/Customer']").Last);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("types-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#ContentTypeDefinition_AllowBulkImport").Locator("xpath=.."), "Allow bulk import");
            await scene.PauseAsync(2600);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#ContentTypeDefinition_AllowBulkExport").Locator("xpath=.."), "Allow bulk export");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task FileAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/content-transfer-entries");
        await scene.StartCaptureAsync();

        await scene.StepAsync("file-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#new-dropdown"), pauseAfter: 1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".dropdown-menu a.dropdown-item").Filter(new() { HasTextString = "Customer" }).First);
        });

        await scene.StepAsync("file-2", async () =>
        {
            await scene.ScrollAsync(0, 380);
            await scene.HighlightAsync(FileRequirement(scene, 0), "Enabled formats");
            await scene.PauseAsync(3200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(FileRequirement(scene, 1), "The first row holds the column names");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("file-3", async () =>
        {
            await scene.HighlightAsync(FileRequirement(scene, 2), "Order does not matter");
            await scene.PauseAsync(2800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(FileRequirement(scene, 3), "Download a template");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("file-4", async () =>
        {
            await scene.ScrollAsync(0, 380);
            await scene.HighlightAsync(scene.Page.Locator("table.table").Last, "Columns Requirements");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("file-5", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("table.table tbody tr").Filter(new() { HasText = "ContentItemId" }).First, "Updates an existing item");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ImportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        // Reached from the list, so the menu shows Import as the current item.
        await scene.OpenAsync("/Admin/content-transfer-entries");
        await scene.Page.Locator("#new-dropdown").ClickAsync();
        await scene.Page.RunAndWaitForNavigationAsync(() => scene.Page.Locator(".dropdown-menu a.dropdown-item").Filter(new() { HasTextString = "Customer" }).First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("import-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("input[type=file]"), pauseAfter: 900);
            await scene.Page.Locator("input[type=file]").SetInputFilesAsync(Path.Combine(DataFolder, "customers.csv"));
            await scene.PauseAsync(900);
            await scene.HighlightAsync(scene.Page.Locator("input[type=file]"), "customers.csv");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("import-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("input[name$='PublishImportedContent']").First.Locator("xpath=.."), "Publish imported content");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("import-3", async () =>
        {
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator("#upload-actions button"), pauseAfter: 100));
            await scene.PauseAsync(1200);
        });
    }

    private static async Task FollowAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        // The list does not refresh itself: open it again until the import has finished.
        await scene.OpenAsync("/Admin/content-transfer-entries");

        for (var attempt = 0; attempt < 30 && !(await scene.Page.Locator("text=Completed").CountAsync() > 0); attempt++)
        {
            await Task.Delay(2000);
            await scene.OpenAsync("/Admin/content-transfer-entries");
        }

        await scene.StartCaptureAsync();

        var row = scene.Page.Locator(".list-group-item").Filter(new() { HasText = "customers.csv" }).First;

        await scene.StepAsync("follow-1", async () =>
        {
            await scene.HighlightAsync(row, "customers.csv: type, status, person, time");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("follow-2", async () =>
        {
            await scene.HighlightAsync(row.Locator("text=Completed with errors").First, "Status");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("follow-3", async () =>
        {
            await scene.HighlightAsync(row.Locator("text=594").First, "Imported");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(row.Locator("text=Did not pass validation").First, "Rejected rows, with the reason");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("follow-4", async () =>
        {
            await scene.ClickAsync(row.Locator("button.dropdown-toggle:visible").Filter(new() { HasTextString = "Actions" }), pauseAfter: 800);
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show a").Filter(new() { HasTextString = "Download errors" }).First, "Only the rejected rows");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("follow-5", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show a").Filter(new() { HasTextString = "Delete" }).First, "Never deletes the content items");
        });

        await scene.ClearHighlightsAsync();
        await scene.Page.Keyboard.PressAsync("Escape");

        await scene.StepAsync("follow-6", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button[title='Show']").First, pauseAfter: 2500);
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.ClickAsync(scene.Page.Locator("button[title='Sort']").First, pauseAfter: 2200);
        });

        await scene.StepAsync("follow-7", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/Contents/ContentItems/"));
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Nth(1), "Customer content items");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ExportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/export/contents");
        await scene.StartCaptureAsync();

        await scene.StepAsync("export-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".action-bar button.btn-secondary").Last, pauseAfter: 900);
        });

        await scene.StepAsync("export-2", async () =>
        {
            await scene.HighlightAsync(Limited(scene.Page.Locator("#Extension")), "File type");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#export_contents_button"), "Disabled until a type is chosen");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(scene.Page.Locator("#contentTypeId"), "Customer", pauseAfter: 700);
            await scene.HighlightAsync(Limited(scene.Page.Locator("#contentTypeId")), "Content type");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("export-3", async () =>
        {
            await scene.HighlightAsync(Limited(scene.Page.Locator("#exportAll")), "Export scope");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#exportPartial"), pauseAfter: 600);
        });

        await scene.StepAsync("export-4", async () =>
        {
            await scene.ScrollAsync(0, 260);
            await scene.HighlightAsync(Limited(scene.Page.Locator("#createdFrom")), "Created from, Created to");
            await scene.PauseAsync(2800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Limited(scene.Page.Locator("#owners")), "Owners");
            await scene.PauseAsync(2800);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 260);
            await scene.HighlightAsync(Limited(scene.Page.Locator("#publishedOnly")), "Published only");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Limited(scene.Page.Locator("#latestOnly")), "Latest only");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Limited(scene.Page.Locator("#allVersions")), "All versions");
        });

        await scene.ClearHighlightsAsync();
        await scene.ScrollAsync(0, -520);
        await scene.ClickAsync(scene.Page.Locator("#exportAll"), pauseAfter: 400);

        await scene.StepAsync("export-5", async () =>
        {
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.Locator("#export_contents_button"), pauseAfter: 100));
            await scene.PauseAsync(800);
            await scene.HighlightAsync(scene.Page.Locator(".toast, .alert, [role=alert]").Filter(new() { HasTextString = "queued" }).First, "Queued for background processing");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("export-6", async () =>
        {
            await scene.OffCameraAsync(() => Task.Delay(5000));
            await scene.NavigateAsync("/Admin/export/contents");
            var row = scene.Page.Locator(".list-group-item").Filter(new() { HasText = "Customer_Export" }).First;
            await scene.HighlightAsync(row.Locator("text=Completed").First, "Completed");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(row.Locator("a").Filter(new() { HasTextString = "Download" }).First, "Download");
        });

        await scene.StepAsync("export-7", async () =>
        {
            await scene.PauseAsync(1500);
        });

        await scene.ClearHighlightsAsync();
    }
}
