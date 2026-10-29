using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the MCP Resources video, one method per "clip" of storyboard.json.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // Recorded in this order: each clip adds a resource to the list.
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-types", TypesAsync),
        ("03-file", FileAsync),
        ("04-media", MediaAsync),
        ("05-remote", RemoteAsync),
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
    /// Enables the features of the video (not recorded).
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
            "CrestApps.OrchardCore.AI.Mcp.Server",
            "CrestApps.OrchardCore.AI.Mcp.Resources.Ftp",
            "CrestApps.OrchardCore.AI.Mcp.Resources.Sftp",
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

        await context.CloseAsync();
    }

    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static ILocator TypeCard(Scene scene, string type) => scene.Page.Locator($"#modalAddResource a[href$='/create/{type}']");

    private static ILocator CardOf(Scene scene, string type) => TypeCard(scene, type).Locator("xpath=ancestor::*[contains(concat(' ',normalize-space(@class),' '),' card ')][1]");

    private static async Task SetNameAsync(Scene scene, string name)
    {
        // The Name is filled in from the Display Text: replace it.
        await scene.Page.Locator("#McpResource_Name").FillAsync(string.Empty);
        await scene.TypeAsync(scene.Page.Locator("#McpResource_Name"), name, delay: 15);
    }

    // ----- Clips -----

    private static async Task TypesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/mcp/resources");
        await scene.StartCaptureAsync();

        await scene.StepAsync("typ-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/ai/mcp/resources"), "Artificial Intelligence > Model Context Protocol > Resources");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button.create"), pauseAfter: 900);
        });

        await scene.StepAsync("typ-2", async () =>
        {
            await scene.HighlightAsync(CardOf(scene, "file"), "File");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(CardOf(scene, "media"), "Media");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("typ-3", async () =>
        {
            await scene.HighlightAsync(CardOf(scene, "ftp"), "FTP/FTPS");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(CardOf(scene, "sftp"), "SFTP");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("typ-4", async () =>
        {
            await scene.HighlightAsync(CardOf(scene, "recipe"), "Recipe");
            await scene.PauseAsync(3000);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task FileAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/mcp/resources");
        await scene.Page.Locator("button.create").ClickAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("file-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(TypeCard(scene, "file"));
            await scene.TypeAsync(scene.Page.Locator("#McpResource_DisplayText"), "Return policy", delay: 25);
            await SetNameAsync(scene, "return-policy");
        });

        await scene.StepAsync("file-2", async () =>
        {
            await scene.Page.Locator("#McpResource_Description").FillAsync("The Contoso return policy, in plain text.");
            await scene.HighlightAsync(Section(scene.Page.Locator("#McpResource_Description")), "Description");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("file-3", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#McpResource_MimeType")), "MIME Type");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("file-4", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#McpResource_Path"), "policies/{fileName}", delay: 40);
            await scene.HighlightAsync(Section(scene.Page.Locator("#McpResource_Path")), "Path");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("file-5", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1500);
        });
    }

    private static async Task MediaAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/mcp/resources");
        await scene.StartCaptureAsync();

        await scene.StepAsync("med-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.create"), pauseAfter: 1500);
            await scene.ClickAndWaitForNavigationAsync(TypeCard(scene, "media"));
        });

        await scene.StepAsync("med-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#McpResource_DisplayText"), "Product brochures", delay: 40);
            await SetNameAsync(scene, "product-brochures");
            await scene.Page.Locator("#McpResource_Description").FillAsync("The product brochures of the media library.");
            await scene.TypeAsync(scene.Page.Locator("#McpResource_Path"), "{path}", delay: 60);
            await scene.HighlightAsync(Section(scene.Page.Locator("#McpResource_Path")), "Path");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("med-3", async () =>
        {
            await scene.PauseAsync(1500);
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1500);
        });
    }

    private static async Task RemoteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ai/mcp/resources");
        await scene.StartCaptureAsync();

        await scene.StepAsync("rem-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button.create"), pauseAfter: 1500);
            await scene.ClickAndWaitForNavigationAsync(TypeCard(scene, "sftp"));
            await scene.Page.Locator("#McpResource_DisplayText").FillAsync("Partner price list");
            await scene.PauseAsync(700);
            await scene.Page.Locator("#McpResource_Name").FillAsync("partner-price-list");
            await scene.PauseAsync(700);
            await scene.Page.Locator("#McpResource_Description").FillAsync("The price list of a partner, read over SFTP.");
            await scene.PauseAsync(700);
        });

        await scene.StepAsync("rem-2", async () =>
        {
            await scene.ScrollAsync(0, 420);
            await scene.Page.Locator("#McpResource_Host").FillAsync("files.partner.example");
            await scene.PauseAsync(600);
            await scene.Page.Locator("#McpResource_Username").FillAsync("contoso");
            await scene.HighlightAsync(Section(scene.Page.Locator("#McpResource_Port")), "Port");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rem-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#McpResource_Password"), "demo-password", delay: 30);
            await scene.ScrollAsync(0, 250);
            await scene.HighlightAsync(Section(scene.Page.Locator("#McpResource_PrivateKey")), "Private Key");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rem-4", async () =>
        {
            await scene.ScrollAsync(0, 500);
            await scene.HighlightAsync(scene.Page.Locator("legend").Filter(new() { HasTextString = "Proxy Settings" }).First.Locator("xpath=.."), "Proxy Settings");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("rem-5", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.ClickAsync(scene.Page.Locator("button.create"), pauseAfter: 1500);
            await scene.ClickAndWaitForNavigationAsync(TypeCard(scene, "ftp"));
            await scene.TypeAsync(scene.Page.Locator("#McpResource_DisplayText"), "Legacy archive", delay: 35);
            await SetNameAsync(scene, "legacy-archive");
            await scene.Page.Locator("#McpResource_Host").FillAsync("ftp.contoso.example");
            await scene.Page.Locator("#McpResource_Username").FillAsync("archive");
            await scene.Page.Locator("#McpResource_Password").FillAsync("demo-password");
            await scene.ScrollAsync(0, 700);
            await scene.HighlightAsync(Section(scene.Page.Locator("#McpResource_EncryptionMode")), "Encryption Mode");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rem-6", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1500);
        });
    }
}
