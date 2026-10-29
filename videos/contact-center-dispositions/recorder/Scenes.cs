using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Dispositions video, one method per "clip" of storyboard.json.
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
        ("02-create", CreateAsync),
        ("03-edit", EditAsync),
        ("04b-outcomes", OutcomesAsync),
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
    private static ILocator F(Scene scene, string name) => scene.Page.Locator($"#OmnichannelDisposition_{name}");

    private static async Task AddAsync(Scene scene, string name, string description, string outcome = null)
    {
        await scene.Page.Locator("#OmnichannelDisposition_Name").FillAsync(name);
        await scene.Page.Locator("#OmnichannelDisposition_Description").FillAsync(description);

        if (outcome is not null)
        {
            await scene.SelectAsync(F(scene, "Outcome"), outcome, pauseAfter: 700);
        }

        await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/omnichannel/dispositions");
        await scene.StartCaptureAsync();

        await scene.StepAsync("disp-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/omnichannel/dispositions"), "Interaction Center > Management > Dispositions");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create"));
        });

        await scene.StepAsync("disp-2", async () =>
        {
            await scene.TypeAsync(F(scene, "Name"), "Test Drive Booked", delay: 50);
            await scene.TypeAsync(F(scene, "Description"), "The customer booked a test drive", delay: 25);
        });

        await scene.StepAsync("disp-3", async () =>
        {
            await scene.HighlightAsync(Section(F(scene, "Outcome")), "Outcome");
            await scene.PauseAsync(5000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(F(scene, "SetLeadStatusId")), "Set lead status");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("disp-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1200);
        });

        await scene.StepAsync("disp-5", async () =>
        {
            await scene.NavigateAsync("/Admin/omnichannel/dispositions/Create");
            await AddAsync(scene, "Call Back Later", "The customer asked to be called again");
            await scene.NavigateAsync("/Admin/omnichannel/dispositions/Create");
            await AddAsync(scene, "Not Interested", "The customer does not want the offer");
            await scene.PauseAsync(800);
        });
    }

    private static async Task EditAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/omnichannel/dispositions");
        await scene.StartCaptureAsync();

        await scene.StepAsync("edit-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn", new() { HasTextString = "Edit" }).First);
            await scene.PauseAsync(600);
            await scene.HighlightAsync(Section(F(scene, "Name")), "The name cannot be changed");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(F(scene, "Description")), "The description can");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("edit-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.btn", new() { HasTextString = "Cancel" }).First);
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.Locator("a.btn-danger, button.btn-danger", new() { HasTextString = "Delete" }).First, "Delete");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task OutcomesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/omnichannel/dispositions");
        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create"));
        await scene.StartCaptureAsync();

        await scene.StepAsync("oc-1", async () =>
        {
            await scene.TypeAsync(F(scene, "Name"), "Number Not In Service", delay: 40);
            await scene.Page.Locator("#OmnichannelDisposition_Description").FillAsync("The number is disconnected or invalid");
            await scene.SelectAsync(F(scene, "Outcome"), "NotInService", pauseAfter: 900);
            await scene.HighlightAsync(Section(F(scene, "Outcome")), "Outcome: Number not in service");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("oc-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create"));
            await AddAsync(scene, "No Answer", "Nobody answered the call", "NoAnswer");
            await scene.PauseAsync(1200);
            await scene.HighlightAsync(scene.Page.GetByText("No answer", new() { Exact = true }).First, "Outcome badge");
        });

        await scene.ClearHighlightsAsync();
    }
}
