using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Dialer Pacing video, one method per "clip" of storyboard.json.
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
        ("02-abandoned", AbandonedAsync),
        ("03-predictive", PredictiveAsync),
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

    private static ILocator Field(Scene scene, string name) => scene.Page.Locator($"#DialerProfile_{name}");

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

    private static async Task AbandonedAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/dialers");
        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("ul.list-group .list-group-item a").Filter(new() { HasTextString = "Lead drive - power" }).First);
        await scene.StartCaptureAsync();

        await scene.StepAsync("ab-1", async () =>
        {
            await ScrollToAsync(scene, Card(scene, "Abandoned calls"), margin: 100);
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.Locator(".ocat-wrapper").Filter(new() { HasTextString = "Measured abandonment" }).First, "Measured abandonment");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ab-2", async () =>
        {
            await scene.ClickAsync(Field(scene, "EnforceAbandonmentCap"), pauseAfter: 700);
            await scene.HighlightAsync(Section(Field(scene, "EnforceAbandonmentCap")), "Enforce an abandonment-rate cap");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ab-3", async () =>
        {
            await scene.HighlightAsync(Section(Field(scene, "MaxAbandonmentRatePercent")), "Maximum abandonment rate");
            await scene.PauseAsync(5000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Field(scene, "AbandonmentSampleFloor")), "Abandonment sample floor");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ab-4", async () =>
        {
            await scene.ScrollAsync(0, 200);
            await scene.ClickAsync(Field(scene, "SafeHarborEnabled"), pauseAfter: 700);
            await scene.HighlightAsync(Section(Field(scene, "SafeHarborEnabled")), "Play a message when a call is abandoned");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ab-5", async () =>
        {
            await scene.ScrollAsync(0, 150);
            await scene.TypeAsync(Field(scene, "SafeHarborMessage"), "Sorry we missed you. This call was from {company}. To be removed from our list or to reach us, please call {number}. Goodbye.", delay: 14);
            await scene.HighlightAsync(Section(Field(scene, "SafeHarborMessage")), "Abandoned call message");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }

    private static async Task PredictiveAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/dialers");
        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create"));
        await scene.Page.Locator("#DialerProfile_Name").FillAsync("Lead drive - predictive");
        await scene.Page.Locator("#DialerProfile_Description").FillAsync("Predictive pacing for the large list");
        await scene.StartCaptureAsync();

        await scene.StepAsync("pred-1", async () =>
        {
            await ScrollToAsync(scene, Card(scene, "Dialing"));
            await PickAsync(scene, "DialerProfile_Mode", "Predictive");
            await ScrollToAsync(scene, Card(scene, "Predictive pacing"), margin: 100);
            await scene.HighlightAsync(Section(Field(scene, "PredictivePacingModel")), "Pacing");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("pred-2", async () =>
        {
            await scene.SelectAsync(Field(scene, "PredictivePacingModel"), "Over-dial", pauseAfter: 900);
            await scene.HighlightAsync(Section(Field(scene, "TargetAbandonmentRatePercent")), "Target abandonment rate");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("pred-3", async () =>
        {
            await scene.HighlightAsync(Section(Field(scene, "MaxLinesPerAgent")), "Lines per agent");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Field(scene, "MaxCallsInFlight")), "Calls in flight");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("pred-4", async () =>
        {
            await scene.ScrollAsync(0, 200);
            await scene.HighlightAsync(Section(Field(scene, "AnswerRateSampleFloor")), "Answer rate sample floor");
            await scene.PauseAsync(4500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Field(scene, "AnswerRateWindowMinutes")), "Answer rate window (minutes)");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("pred-5", async () =>
        {
            await scene.ScrollAsync(0, 250);
            await scene.HighlightAsync(Section(Field(scene, "CreditAgentsFreeingUp")), "Count agents about to free up");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Section(Field(scene, "ConnectWaitMilliseconds")), "Connect wait (milliseconds)");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("pred-6", async () =>
        {
            await scene.ScrollAsync(0, 250);
            await scene.HighlightAsync(Section(Field(scene, "AbandonedRetryRequiresAgent")), "Retry abandoned calls only with an agent reserved");
            await scene.PauseAsync(5000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("pred-7", async () =>
        {
            await ScrollToAsync(scene, Card(scene, "Abandoned calls"), margin: 100);
            await scene.ClickAsync(Field(scene, "EnforceAbandonmentCap"), pauseAfter: 300);
            await scene.ClickAsync(Field(scene, "SafeHarborEnabled"), pauseAfter: 300);
            await scene.Page.Locator("#DialerProfile_SafeHarborMessage").FillAsync("Sorry we missed you. This call was from {company}. To be removed from our list or to reach us, please call {number}. Goodbye.");
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(800);
        });
    }
}
