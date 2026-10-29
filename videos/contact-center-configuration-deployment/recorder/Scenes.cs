using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Contact Center Configuration and Deployment video, one method per "clip" of storyboard.json.
/// The site holds queues in two queue groups and five fake agents (see README.md); `Recorder prepare` adds the voice media clip
/// "Main hold music" from the audio file named by VIDEO_AUDIO. Record the clips in order: 04 downloads the plan, 05 imports it, 06 uploads the audio again.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // The fake hold music: a short tone made with ffmpeg, kept outside the repository.
    private static string AudioPath => Environment.GetEnvironmentVariable("VIDEO_AUDIO") ?? throw new InvalidOperationException("Set VIDEO_AUDIO to a WAV or MP3 file.");

    // The plan the export clip downloads and the replay clip imports; outside the repository.
    private static string PlanPath => Path.Combine(Path.GetTempPath(), "cc-plan-demo.zip");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-features", FeaturesAsync),
        ("03-recipe", RecipeAsync),
        ("04-export", ExportAsync),
        ("05-replay", ReplayAsync),
        ("06-voice", VoiceAsync),
    ];

    // Off camera: adds the voice media clip the plan will carry.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = 1536, Height = 864 } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(120_000);
        await page.GotoAsync(BaseUrl + "/Admin/contact-center/voice-media/create");
        await page.Locator("#VoiceMediaItem_Name").FillAsync("Main hold music");
        await page.Locator("#VoiceMediaItem_Description").FillAsync("Played to callers while they wait in a queue.");
        await page.Locator("#VoiceMediaItem_Audio").SetInputFilesAsync(AudioPath);
        await page.RunAndWaitForNavigationAsync(() => page.GetByRole(AriaRole.Button, new() { Name = "Save" }).Last.ClickAsync());
        Console.WriteLine("Added the voice media clip Main hold music");
        await context.CloseAsync();
    }

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

    private static ILocator Button(Scene scene, string name) => scene.Page.GetByRole(AriaRole.Button, new() { Name = name }).First;

    private static ILocator AddStep(Scene scene) => scene.Page.GetByText("Add Step", new() { Exact = true }).First;

    private static ILocator Link(Scene scene, string text) => scene.Page.Locator("a", new() { HasTextString = text }).First;

    private static async Task FeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();

        await scene.StepAsync("feat-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "Contact Center", delay: 70);
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item").Filter(new() { HasText = "Contact Center Work Distribution" }).First, "Queues");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item").Filter(new() { HasText = "Contact Center Outbound Dialer" }).First, "The dialer");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("feat-2", async () =>
        {
            var row = scene.Page.Locator("li.list-group-item").Filter(new() { HasText = "Contact Center Inbound Voice" }).First;
            await scene.HighlightAsync(row.Locator(".direct-dependencies"), "Direct dependencies");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(row.GetByText("Show indirect dependencies").First, pauseAfter: 1500);
        });

        await scene.StepAsync("feat-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item").Filter(new() { HasText = "Contact Center Outbound Dialer" }).First, "The dialer's own step needs this feature");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task RecipeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync("/Admin/Recipes");
        await scene.StartCaptureAsync();

        await scene.StepAsync("rec-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("input[type=search], #search-box").First, "Asterisk", delay: 80);
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.GetByText("Asterisk (GA-Core)").First, "One recipe turns on the whole set");
            await scene.PauseAsync(3000);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rec-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Enables the supported GA-Core Contact Center feature set").First, "Features only: no queues, no entry points");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ExportAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        await scene.OpenAsync("/Admin/DeploymentPlan/Index");
        await scene.StartCaptureAsync();

        await scene.StepAsync("exp-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Link(scene, "Add Deployment Plan"));
            await scene.TypeAsync(scene.Page.Locator("input[name='Name']"), "Contact Center setup");
            await scene.ClickAndWaitForNavigationAsync(Button(scene, "Create"));
        });

        await scene.StepAsync("exp-2", async () =>
        {
            await scene.ClickAsync(AddStep(scene), pauseAfter: 1500);
            await scene.TypeAsync(scene.Page.Locator(".modal.show input[type=text], .modal.show input[type=search]").First, "Contact Center", delay: 60);
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("exp-3", async () =>
        {
            var planId = scene.Page.Url.TrimEnd('/').Split('/').Last();

            // The first step is added on camera from the open list; the other two are added without filming the same clicks again.
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='type=ContactCenterQueueGroupDeploymentStep']").First);
            await scene.ClickAndWaitForNavigationAsync(Button(scene, "Create"));

            await scene.OffCameraAsync(async () =>
            {
                foreach (var step in new[] { "ContactCenterQueueDeploymentStep", "ContactCenterVoiceMediaDeploymentStep" })
                {
                    await scene.NavigateAsync($"/Admin/DeploymentPlan/{planId}/Step/Create?type={step}");
                    await scene.WaitForNavigationAsync(() => Button(scene, "Create").ClickAsync());
                }
            });

            await scene.ClickAsync(Button(scene, "Execute"), pauseAfter: 1200);
            var download = await scene.Page.RunAndWaitForDownloadAsync(() => scene.Page.Locator("a[href*='ExportFile/Execute']").First.ClickAsync());
            await download.SaveAsAsync(PlanPath);
        });
    }

    private static async Task ReplayAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(90_000);

        // Off camera: the voice media clip is deleted, so the import brings it back as an entry with no audio.
        await scene.OpenAsync("/Admin/contact-center/voice-media");
        await scene.Page.Locator("a, button", new() { HasTextString = "Delete" }).First.ClickAsync();
        await scene.WaitForNavigationAsync(() => scene.Page.Locator(".modal.show .btn-danger, .modal.show a.btn-primary").First.ClickAsync());
        await scene.OpenAsync("/Admin/contact-center/queues");
        await scene.StartCaptureAsync();

        await scene.StepAsync("rep-1", async () =>
        {
            var billing = scene.Page.Locator("li.list-group-item, tr").Filter(new() { HasText = "Billing" }).First;
            await scene.ClickAndWaitForNavigationAsync(billing.Locator("a", new() { HasTextString = "Edit" }).First);
            await scene.Page.Locator("#ActivityQueue_Name").FillAsync(string.Empty);
            await scene.TypeAsync(scene.Page.Locator("#ActivityQueue_Name"), "Billing test");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).Last);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Billing test", Exact = true }).First, "Renamed");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("rep-2", async () =>
        {
            await scene.NavigateAsync("/Admin/DeploymentPlan/Import/Index");
            await scene.Page.Locator("input[name='importedPackage']").SetInputFilesAsync(PlanPath);
            await scene.HighlightAsync(scene.Page.Locator("input[name='importedPackage']"), "The downloaded plan");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.WaitForNavigationAsync(() => scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Import", Exact = true }).First, pauseAfter: 500));
        });

        await scene.StepAsync("rep-3", async () =>
        {
            await scene.NavigateAsync("/Admin/contact-center/queues");
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Billing", Exact = true }).First, "Billing again");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task VoiceAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(120_000);

        await scene.OpenAsync("/Admin/contact-center/voice-media");
        await scene.StartCaptureAsync();

        await scene.StepAsync("vm-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("a", new() { HasTextString = "Main hold music" }).First, "Imported entry: the audio is not in the plan");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("vm-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a", new() { HasTextString = "Main hold music" }).First);
            await scene.Page.Locator("#VoiceMediaItem_Audio").SetInputFilesAsync(AudioPath);
            await scene.HighlightAsync(scene.Page.Locator("#VoiceMediaItem_Audio"), "Choose the audio file again");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("vm-3", async () =>
        {
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).Last.ClickAsync()));
            await scene.PauseAsync(1000);
        });
    }
}
