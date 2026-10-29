using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Numbers Not In Service video, one method per "clip" of storyboard.json.
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
        ("03-disposition", DispositionAsync),
        ("05-list", ListAsync),
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

    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator Card(Scene scene, string title)
        => scene.Page.Locator(".card").Filter(new() { Has = scene.Page.Locator(".card-header", new() { HasTextString = title }) }).First;

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static ILocator Feature(Scene scene, string id)
        => scene.Page.Locator($"#btn-enable-{id.Replace('.', '_')}, #btn-disable-{id.Replace('.', '_')}").First.Locator("xpath=ancestor::li[contains(@class,'list-group-item')][1]");

    private static async Task ScrollToAsync(Scene scene, ILocator target, double margin = 150)
    {
        var box = await target.BoundingBoxAsync();

        if (box is not null)
        {
            await scene.ScrollAsync(0, box.Y - margin);
        }
    }

    private static async Task PickAsync(Scene scene, string selectId, string text)
    {
        await scene.ClickAsync(scene.Page.Locator($"button[data-id='{selectId}']"), pauseAfter: 900);
        await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = text }).First, pauseAfter: 600);
    }

    // ----- Clips -----
    private static async Task MarkAsync(Scene scene, string number)
    {
        await scene.ClickAsync(scene.Page.Locator("button.create", new() { HasTextString = "Mark as not in service" }), pauseAfter: 350);
        await scene.Page.Locator("#mark-phone-number").FillAsync(number);
        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalMarkNotInService button[type=submit]"));
    }

    private static async Task QuickMarkAsync(Scene scene, string number)
    {
        await scene.Page.Locator("button.create", new() { HasTextString = "Mark as not in service" }).ClickAsync();
        await scene.PauseAsync(350);
        await scene.Page.Locator("#mark-phone-number").FillAsync(number);
        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalMarkNotInService button[type=submit]"));
    }

    private static async Task DispositionAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/omnichannel/dispositions");
        await scene.StartCaptureAsync();

        await scene.StepAsync("dis-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/omnichannel/dispositions"), "Interaction Center > Management > Dispositions");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.GetByText("Number not in service", new() { Exact = true }).First, "Outcome: Number not in service");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("dis-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create"));
            await scene.PauseAsync(500);
            await scene.SelectAsync(scene.Page.Locator("#OmnichannelDisposition_Outcome"), "NotInService", pauseAfter: 900);
            await scene.HighlightAsync(Section(scene.Page.Locator("#OmnichannelDisposition_Outcome")), "Outcome");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/omnichannel/numbers-not-in-service");
        await scene.StartCaptureAsync();

        await scene.StepAsync("list-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/omnichannel/numbers-not-in-service"), "Interaction Center > Management > Numbers Not In Service");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("p.text-muted").First, "Skipped by loads, the dialer and automated calls");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("list-2", async () =>
        {
            await MarkAsync(scene, "+17025550123");
        });

        await scene.StepAsync("list-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("li, .list-group-item, .card-body").Filter(new() { HasTextString = "555-0123" }).Last, "Found by, and when");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await QuickMarkAsync(scene, "+17025550188");
            await QuickMarkAsync(scene, "+17025550134");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("list-4", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "0188", delay: 120);
            await scene.WaitForNavigationAsync(() => scene.Page.Locator("#search-box").PressAsync("Enter"));
            await scene.PauseAsync(600);
        });

        await scene.StepAsync("list-5", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("a.btn", new() { HasTextString = "Allow dialing" }).First, pauseAfter: 1200);
            await scene.PauseAsync(2500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".modal.show .btn-primary, .modal.show .btn-danger, .modal.show .btn-success").Last);
            await scene.PauseAsync(800);
        });

        await scene.StepAsync("list-6", async () =>
        {
            await scene.NavigateAsync("/Admin/omnichannel/numbers-not-in-service");
            await scene.ClickAsync(scene.Page.Locator("input[type=checkbox]").First, pauseAfter: 800);
            await scene.ClickAsync(scene.Page.Locator("button.dropdown-toggle.btn-light", new() { HasTextString = "Actions" }).First, pauseAfter: 900);
        });
    }
}
