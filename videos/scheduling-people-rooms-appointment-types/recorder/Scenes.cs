using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Scheduling "people, rooms and appointment types" video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5330";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-resources", ResourcesAsync),
        ("03-types", TypesAsync),
        ("04-calendar", CalendarAsync),
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

    private static async Task CompactMenuAsync(Scene scene)
    {
        await scene.Page.EvaluateAsync("() => { if (!document.body.classList.contains('left-sidebar-compact')) document.querySelector('.leftbar-compactor')?.click(); }");
        await Task.Delay(400);
    }

    private static ILocator Save(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static ILocator Add(Scene scene, string text) => scene.Page.Locator("a.btn").Filter(new() { HasTextString = text }).First;

    private static async Task ResourcesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/scheduling/resources");
        await CompactMenuAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("res-1", async () =>
        {
            await scene.PauseAsync(2500);
            await scene.ClickAndWaitForNavigationAsync(Add(scene, "Add resource"));
        });

        await scene.StepAsync("res-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#Name"), "Dr. Maya Chen", delay: 60);
            await scene.HighlightAsync(scene.Page.Locator("#Kind"));
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("res-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button[data-id='UserId']"), pauseAfter: 900);
            await scene.Page.Keyboard.TypeAsync("maya", new() { Delay = 80 });
            await scene.PauseAsync(1800);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = "maya.chen" }).First, pauseAfter: 900);
        });

        await scene.StepAsync("res-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("button[data-id='CalendarId']"));
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("button[data-id='TimeZoneId']"));
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".form-check").Filter(new() { HasTextString = "Takes appointments" }).First);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("res-5", async () =>
        {
            await scene.ScrollAsync(0, 600);
            await scene.PauseAsync(600);
            var hours = scene.Page.Locator("#WorkingHours");
            await scene.ClickAsync(hours, pauseAfter: 400);
            await scene.Page.EvaluateAsync("() => { const t = document.querySelector('#WorkingHours'); t.value = t.value.replace(/(Monday|Tuesday|Wednesday|Thursday|Friday) 09:00-17:00/g, '$1 09:00-12:00, 13:00-17:00'); }");
            await scene.PauseAsync(600);
            await scene.HighlightAsync(hours);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("res-6", async () =>
        {
            var off = scene.Page.Locator("#DateOverrides");
            await scene.ClickAsync(off, pauseAfter: 300);
            await scene.Page.Keyboard.TypeAsync("2026-12-24 closed\n2026-12-31 09:00-12:00", new() { Delay = 40 });
            await scene.HighlightAsync(off);
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#AvailabilityMode"));
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 600);
            await scene.ClickAndWaitForNavigationAsync(Save(scene));
        });

        await scene.StepAsync("res-7", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Add(scene, "Add resource"));
            await scene.TypeAsync(scene.Page.Locator("#Name"), "Treatment room 1", delay: 55);
            await scene.SelectAsync(scene.Page.Locator("#Kind"), "Room");
            await scene.ScrollAsync(0, 600);
            await scene.PauseAsync(1200);
            await scene.ScrollAsync(0, 600);
            await scene.ClickAndWaitForNavigationAsync(Save(scene));
        });

        await scene.StepAsync("res-8", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").First);
            await scene.PauseAsync(1500);
        });
    }

    private static async Task TypesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/scheduling/types");
        await CompactMenuAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("type-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Add(scene, "Add appointment type"));
            await scene.TypeAsync(scene.Page.Locator("#Name"), "Initial consultation", delay: 50);
            await scene.TypeAsync(scene.Page.Locator("#Description"), "A first visit with the doctor.", delay: 35);
            await scene.TypeAsync(scene.Page.Locator("#Location"), "Contoso Clinic, treatment room 1", delay: 30);
        });

        await scene.StepAsync("type-2", async () =>
        {
            await scene.ScrollAsync(0, 560);
            await scene.HighlightAsync(scene.Page.Locator("#DurationMinutes"));
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            var after = scene.Page.Locator("#BufferAfterMinutes");
            await scene.ClickAsync(after, pauseAfter: 200);
            await after.FillAsync("10");
            await scene.HighlightAsync(after);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("type-3", async () =>
        {
            await scene.ScrollAsync(0, 560);
            await scene.ClickAsync(scene.Page.Locator("button[data-id='ResourceIds']"), pauseAfter: 600);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = "Dr. Maya Chen" }).First, pauseAfter: 500);
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = "Treatment room 1" }).First, pauseAfter: 500);
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.PauseAsync(500);
            await scene.HighlightAsync(scene.Page.Locator("#Assignment"));
            await scene.SelectAsync(scene.Page.Locator("#Assignment"), "Share out (round robin)");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("type-4", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#Assignment"), "All resources together");
            await scene.HighlightAsync(scene.Page.Locator("#Assignment"));
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#Capacity"));
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("type-5", async () =>
        {
            await scene.ScrollAsync(0, 560);
            await scene.HighlightAsync(scene.Page.Locator("#MinimumNoticeMinutes"));
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#HorizonDays"));
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#MaxPerDay"));
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("#HoldMinutes"));
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("type-6", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".form-check").Filter(new() { HasTextString = "Bookings wait for someone" }).First);
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#AllowSelfService"), pauseAfter: 400);
            await scene.TypeAsync(scene.Page.Locator("#Slug"), "consultation", delay: 60);
            await scene.ScrollAsync(0, 600);
            await scene.ClickAndWaitForNavigationAsync(Save(scene));
        });

        await scene.StepAsync("type-7", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").First);
            await scene.PauseAsync(1500);
        });
    }

    private static async Task CalendarAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/calendar");
        await CompactMenuAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("cal-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("label, .form-check").Filter(new() { HasTextString = "Dr. Maya Chen" }).First);
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("label, .form-check").Filter(new() { HasTextString = "Treatment room 1" }).First);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("cal-2", async () =>
        {
            await scene.PauseAsync(3000);
        });
    }
}
