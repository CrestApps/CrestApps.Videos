using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Campaigns video, one method per "clip" of storyboard.json.
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
        ("02-groups", GroupsAsync),
        ("03-campaigns", CampaignsAsync),
        ("04-agents", AgentsAsync),
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

    private static async Task GroupsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/omnichannel/campaign-groups");
        await scene.StartCaptureAsync();

        await scene.StepAsync("grp-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/omnichannel/campaign-groups"), "Interaction Center > Management > Campaign Groups");
            await scene.PauseAsync(4500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create"));
        });

        await scene.StepAsync("grp-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#OmnichannelCampaignGroup_DisplayText"), "Spring season", delay: 40);
            await scene.Page.Locator("#OmnichannelCampaignGroup_Description").FillAsync("Every campaign of the spring season");
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(800);
        });
    }

    private static async Task CampaignsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/omnichannel/campaign-groups");
        await scene.StartCaptureAsync();

        await scene.StepAsync("camp-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/omnichannel/campaigns"));
            await scene.PauseAsync(600);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create"));
        });

        await scene.StepAsync("camp-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#OmnichannelCampaign_DisplayText"), "Spring lead drive", delay: 55);
            await scene.SelectAsync(scene.Page.Locator("#OmnichannelCampaign_CampaignGroupId"), "Spring season", pauseAfter: 900);
            await scene.HighlightAsync(Section(scene.Page.Locator("#OmnichannelCampaign_CampaignGroupId")), "Campaign group");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#OmnichannelCampaign_Description"), "Calls to the leads of the spring season", delay: 25);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("camp-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("camp-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create"));
            await scene.Page.Locator("#OmnichannelCampaign_DisplayText").FillAsync("Renewal reminders");
            await scene.Page.Locator("#OmnichannelCampaign_Description").FillAsync("Reminders to renew");
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(800);
        });

        await scene.StepAsync("camp-5", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("a.btn", new() { HasTextString = "Edit" }).First, "Edit only: no Delete");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task AgentsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/omnichannel/campaigns");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ent-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Menu(scene, "/Admin/contact-center/agent-entitlements"));
            await scene.PauseAsync(500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a.create"));
            await PickAsync(scene, "agent-entitlement-user-selector", "jamie.chen");
        });

        await scene.StepAsync("ent-2", async () =>
        {
            await scene.HighlightAsync(Section(scene.Page.Locator("#AllowedCampaignIds")), "Allowed campaigns");
            await scene.ClickAsync(scene.Page.Locator("button[data-id='AllowedCampaignIds']"), pauseAfter: 900);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = "Spring lead drive" }).First, pauseAfter: 700);
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.PauseAsync(600);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ent-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.PauseAsync(1500);
        });
    }
}
