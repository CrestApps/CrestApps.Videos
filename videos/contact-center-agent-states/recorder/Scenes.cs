using Microsoft.Playwright;

namespace Recorder;

public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-create", CreateAsync),
        ("04-menu", MenuAsync),
        ("05-disable", DisableAsync),
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

    private static ILocator Section(ILocator field) => field.Locator("xpath=ancestor::*[contains(@class,'ocat-wrapper')][1]");

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static ILocator Field(Scene scene, string id) => scene.Page.Locator("#" + id);

    private static async Task SetAsync(Scene scene, ILocator field, string text, int delay = 40)
    {
        await scene.ClickAsync(field, pauseAfter: 150);
        await field.FillAsync(string.Empty);
        await field.PressSequentiallyAsync(text, new() { Delay = delay });
        await scene.PauseAsync(250);
    }

    private static ILocator MenuItem(Scene scene, string text) => scene.Page.Locator(".cc-menu.is-open button").Filter(new() { HasTextString = text }).First;

    // ----- Clips -----

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/agent-states");
        await scene.StartCaptureAsync();

        await scene.StepAsync("cre-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("ul.admin-list").First, "Reason codes and their states");
            await scene.PauseAsync(4500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a").Filter(new() { HasTextString = "Add reason code" }).First);
        });

        await scene.StepAsync("cre-2", async () =>
        {
            await scene.TypeAsync(Field(scene, "AgentStateReasonCode_Name"), "Customer visit", delay: 50);
            await scene.TypeAsync(Field(scene, "AgentStateReasonCode_Description"), "Meeting a customer away from the desk", delay: 20);
        });

        await scene.StepAsync("cre-3", async () =>
        {
            await scene.SelectAsync(Field(scene, "AgentStateReasonCode_AppliesTo"), "Meeting", pauseAfter: 600);
            await scene.HighlightAsync(Section(Field(scene, "AgentStateReasonCode_AppliesTo")), "Applies to state");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("cre-4", async () =>
        {
            await SetAsync(scene, Field(scene, "AgentStateReasonCode_SortOrder"), "5");
            await scene.HighlightAsync(Section(Field(scene, "AgentStateReasonCode_SortOrder")), "Sort order");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }

    private static async Task MenuAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/workspace");
        await scene.StartCaptureAsync();

        await scene.StepAsync("men-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".cc-presence__button").First, "Presence");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator(".cc-presence__button").First, pauseAfter: 600);
        });

        await scene.StepAsync("men-2", async () =>
        {
            await scene.HighlightAsync(MenuItem(scene, "Available"), "Always listed");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(MenuItem(scene, "Customer visit"), "Sorted by Sort order");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("men-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".cc-menu.is-open").Locator("xpath=.//*[normalize-space()='Break' or normalize-space()='BREAK']").First, "Break group");
            await scene.PauseAsync(2500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task DisableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/contact-center/agent-states");
        await scene.StartCaptureAsync();

        await scene.StepAsync("dis-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("ul.admin-list a").Filter(new() { HasTextString = "Customer visit" }).First);
            await scene.ClickAsync(Field(scene, "AgentStateReasonCode_Enabled"), pauseAfter: 700);
            await scene.HighlightAsync(Section(Field(scene, "AgentStateReasonCode_Enabled")), "Enabled");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.StepAsync("dis-2", async () =>
        {
            await scene.NavigateAsync("/Admin/contact-center/workspace");
            await scene.ClickAsync(scene.Page.Locator(".cc-presence__button").First, pauseAfter: 800);
            await scene.HighlightAsync(scene.Page.Locator(".cc-menu.is-open").First, "Customer visit is gone");
        });

        await scene.ClearHighlightsAsync();
    }
}
