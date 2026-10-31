using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the video, recorded as maria.lopez (a member of the Sales Team) on the demo site that the Calendars getting
/// started video seeds, and as admin for the shared category. The clips run in order: they build on each other's categories.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5396";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    // The sign-in of maria.lopez (Recorder setup with VIDEO_USER_NAME=maria.lopez), and of admin (state-admin.json).
    public static string StatePath => Path.Combine(Root, "state.json");

    public static Task RecordAsync(IBrowser browser, string outputRoot, IReadOnlyDictionary<string, double> durations, string[] names)
        => RecordClipsAsync(browser, outputRoot, durations, names);

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-make", MakeAsync),
        ("03-shared", SharedAsync),
        ("04-use", UseAsync),
        ("05-filter", FilterAsync),
        ("06-delete", DeleteAsync),
    ];

    private static async Task RecordClipsAsync(IBrowser browser, string outputRoot, IReadOnlyDictionary<string, double> durations, string[] names)
    {
        var selected = names is ["all"] ? s_clips : s_clips.Where(clip => names.Contains(clip.Name)).ToArray();

        foreach (var (name, record) in selected)
        {
            Console.WriteLine($"Recording {name}");
            await record(browser, Path.Combine(outputRoot, name), durations);
        }
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string statePath = null)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, statePath ?? StatePath, Path.GetDirectoryName(directory), durations);

    private static async Task AddCategoryAsync(Scene scene, string name, string color, bool shared = false)
    {
        var page = scene.Page;
        await scene.ClickAndWaitForNavigationAsync(page.Locator("a").Filter(new() { HasTextString = "Add category" }).First);
        await Task.Delay(700);
        await scene.TypeAsync(page.Locator("#Name"), name, delay: 60);
        await scene.ClickAsync(page.Locator("#Color"), pauseAfter: 300);
        await page.Locator("#Color").FillAsync(color);
        await Task.Delay(500);

        if (shared)
        {
            await scene.ClickAsync(page.Locator("label[for='Shared']"), pauseAfter: 900);
            await scene.HighlightAsync(page.Locator("#Shared"));
            await Task.Delay(1800);
            await scene.ClearHighlightsAsync();
        }

        await scene.ClickAndWaitForNavigationAsync(page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last);
        await Task.Delay(900);
    }

    private static async Task MakeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await Cal.OpenCalendarAsync(scene);
        await scene.StartCaptureAsync();

        var page = scene.Page;

        await scene.StepAsync("make-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(page.Locator("#left-nav a[href='/Admin/calendar-categories']").First);
            await Task.Delay(2500);
            await scene.HighlightAsync(page.Locator(".badge, .list-group-item span").Filter(new() { HasTextString = "Everyone" }).First, "Shared");
            await Task.Delay(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(page.Locator("a").Filter(new() { HasTextString = "Add category" }).First);
        });

        await scene.StepAsync("make-2", async () =>
        {
            await scene.TypeAsync(page.Locator("#Name"), "Sales calls", delay: 60);
            await scene.ClickAsync(page.Locator("#Color"), pauseAfter: 300);
            await page.Locator("#Color").FillAsync("#f76707");
            await Task.Delay(700);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last);
            await Task.Delay(600);
            await AddCategoryAsync(scene, "Admin work", "#868e96");
            await Task.Delay(2500);
        });
    }

    private static async Task SharedAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, Path.Combine(Root, "state-admin.json"));
        await scene.OpenAsync("/Admin/calendar-categories");
        await scene.StartCaptureAsync();

        await scene.StepAsync("shared-1", async () =>
        {
            await AddCategoryAsync(scene, "Company event", "#1971c2", shared: true);
        });
    }

    private static async Task UseAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await Cal.OpenCalendarAsync(scene, weeksAhead: 1);
        await scene.ScrollAsync(0, 450);
        await scene.StartCaptureAsync();

        var modal = Cal.Modal(scene);

        await scene.StepAsync("use-1", async () =>
        {
            await scene.ClickAsync(Cal.Event(scene, "Customer lunch"), pauseAfter: 1300);
            await scene.ClickAsync(modal.Locator("[data-event-edit]"), pauseAfter: 1500);
            await scene.HighlightAsync(modal.Locator("#CalendarEvent_CategoryId"));
            await scene.SelectAsync(modal.Locator("#CalendarEvent_CategoryId"), "Sales calls", pauseAfter: 1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Cal.ModalButton(scene, "Save"), pauseAfter: 1200);
            await modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
            await Task.Delay(900);
        });

        await scene.StepAsync("use-2", async () =>
        {
            await scene.ClickAsync(Cal.Event(scene, "Pricing workshop"), pauseAfter: 1200);
            await scene.ClickAsync(modal.Locator("[data-event-edit]"), pauseAfter: 1500);
            await scene.SelectAsync(modal.Locator("#CalendarEvent_CategoryId"), "Sales calls", pauseAfter: 600);
            await scene.MoveToAsync(800, 520);
            await scene.ScrollAsync(0, 700);
            await scene.ClickAsync(modal.Locator("label[for='CalendarEvent_UseColor']"), pauseAfter: 600);
            await modal.Locator("#CalendarEvent_Color").FillAsync("#2f9e44");
            await Task.Delay(700);
            await scene.HighlightAsync(modal.Locator("#CalendarEvent_Color"));
            await Task.Delay(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Cal.ModalButton(scene, "Save"), pauseAfter: 1200);
            await modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
            await Task.Delay(900);
        });

        await scene.StepAsync("use-3", async () =>
        {
            await scene.HighlightAsync(Cal.Event(scene, "Pipeline review"), "No category, no own color");
            await Task.Delay(3000);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task FilterAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await Cal.OpenCalendarAsync(scene, weeksAhead: 1);
        await scene.ScrollAsync(0, 300);
        await Task.Delay(300);
        await scene.StartCaptureAsync();

        var page = scene.Page;

        await scene.StepAsync("filt-1", async () =>
        {
            await scene.ScrollAsync(0, -300);
            await scene.HighlightAsync(page.Locator("input[data-category-toggle]").First.Locator("xpath=ancestor::ul"));
            await Task.Delay(1500);
            await scene.ClearHighlightsAsync();
            var sales = page.Locator("label[for^='category-']").Filter(new() { HasTextString = "Sales calls" }).First;
            await scene.ClickAsync(sales, pauseAfter: 2800);
            await scene.ClickAsync(sales, pauseAfter: 1500);
        });

        await scene.StepAsync("filt-2", async () =>
        {
            var none = page.Locator("label[for='category-none']");
            await scene.ClickAsync(none, pauseAfter: 3500);
            await scene.ClickAsync(none, pauseAfter: 1000);
        });
    }

    private static async Task DeleteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/calendar-categories");
        await scene.StartCaptureAsync();

        var page = scene.Page;

        await scene.StepAsync("del-1", async () =>
        {
            var row = page.Locator(".list-group-item").Filter(new() { HasTextString = "Sales calls" }).First;
            await scene.ClickAsync(row.Locator("button.dropdown-toggle"), pauseAfter: 900);
            await scene.ClickAsync(page.Locator(".dropdown-menu.show a").Filter(new() { HasTextString = "Delete" }).First, pauseAfter: 1500);
            await scene.ClickAndWaitForNavigationAsync(page.Locator(".modal.show .btn-danger, .modal.show .btn-primary").Last);
            await Task.Delay(1500);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("#left-nav a[href='/Admin/calendar']").First);
            await Task.Delay(800);
            await scene.ClickAsync(page.Locator(".fc-next-button"), pauseAfter: 1500);
            await scene.MoveToAsync(1000, 600);
            await scene.ScrollAsync(0, 450);
        });
    }
}
