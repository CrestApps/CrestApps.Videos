using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the video. <c>01-create</c> is recorded as admin and creates the Project Falcon calendar; run
/// <c>Recorder prepare</c> after it to add the calendar's events; <c>02-jamie</c> and <c>03-alex</c> are recorded as those
/// people; <c>04-permissions</c> is recorded as admin. The site is the one the Calendars getting started video seeds.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5396";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    // The sign-in of admin (Recorder setup), and of jamie.chen and alex.kim (state-jamie.json, state-alex.json).
    public static string StatePath => Path.Combine(Root, "state.json");

    private static string StateOf(string person) => Path.Combine(Root, $"state-{person}.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("01-create", CreateAsync),
        ("02-jamie", JamieAsync),
        ("03-alex", AlexAsync),
        ("04-permissions", PermissionsAsync),
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

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string statePath = null)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, statePath ?? StatePath, Path.GetDirectoryName(directory), durations);

    /// <summary>Adds the events of the Project Falcon calendar (made by the 01-create clip), outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = 1536, Height = 864 } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        await page.GotoAsync(BaseUrl + "/Admin/calendars");
        var id = await page.EvaluateAsync<string>("() => { const row = [...document.querySelectorAll('.list-group-item, tr')].find(r => /Project Falcon/.test(r.innerText)); const link = [...row.querySelectorAll('a')].map(a => a.getAttribute('href')).find(h => /[0-9a-z]{26}/.test(h)); return link.match(/[0-9a-z]{26}/)[0]; }");

        foreach (var (title, start, end, description) in new[]
        {
            ("Kickoff", "2026-11-02T16:00:00Z", "2026-11-02T17:00:00Z", "Goals and owners"),
            ("Design review", "2026-11-04T19:00:00Z", "2026-11-04T20:00:00Z", "Review the mockups"),
            ("Sprint demo", "2026-11-06T15:00:00Z", "2026-11-06T16:00:00Z", "Show the first build"),
        })
        {
            await page.GotoAsync($"{BaseUrl}/Admin/calendar/events/create?calendarId={id}&eventType=Meeting&start={start}&end={end}");
            await page.Locator("#CalendarEvent_Title").FillAsync(title);
            await page.Locator("#CalendarEvent_Description").FillAsync(description);
            await page.RunAndWaitForNavigationAsync(() => page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).First.ClickAsync());
            Console.WriteLine("Event " + title);
        }

        await context.CloseAsync();
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        var page = scene.Page;

        await scene.StepAsync("create-1", async () =>
        {
            await scene.ClickAsync(page.Locator("#left-nav button").Filter(new() { HasTextString = "Calendars" }).First, pauseAfter: 700);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("#left-nav a[href='/Admin/calendars']").First);
            await Task.Delay(1500);
            await scene.ClickAsync(page.Locator("a, button").Filter(new() { HasTextString = "Add calendar" }).First, pauseAfter: 1200);
            await scene.ClickAndWaitForNavigationAsync(page.Locator(".dropdown-menu.show a[href='/Admin/calendars/create/General']"));
            await Task.Delay(800);
        });

        await scene.StepAsync("create-2", async () =>
        {
            await scene.TypeAsync(page.Locator("#Calendar_Name"), "Project Falcon", delay: 60);
            await scene.TypeAsync(page.Locator("#Calendar_Description"), "Kickoff, design reviews and demos of the Falcon project", delay: 30);
            await scene.ClickAsync(page.Locator("#Calendar_Color"), pauseAfter: 400);
            await page.Locator("#Calendar_Color").FillAsync("#7048e8");
            await Task.Delay(500);
            await scene.HighlightAsync(page.Locator("#Calendar_TimeZoneId"));
            await Task.Delay(1500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("create-3", async () =>
        {
            await scene.MoveToAsync(1000, 600);
            await scene.ScrollAsync(0, 300);
            await scene.HighlightAsync(page.Locator("#Calendar_Visibility"));
            await scene.SelectAsync(page.Locator("#Calendar_Visibility"), "Shared", pauseAfter: 1500);
            await scene.SelectAsync(page.Locator("#Calendar_Visibility"), "Public", pauseAfter: 1500);
            await scene.SelectAsync(page.Locator("#Calendar_Visibility"), "Private", pauseAfter: 800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("#Calendar_Enabled"));
        });

        await scene.StepAsync("share-1", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.MoveToAsync(1000, 600);
            await scene.ScrollAsync(0, 500);
            await scene.ClickAsync(page.Locator("select[name='Calendar.FreeBusyRoles'] option[value='Support Team']"), pauseAfter: 1200);
            await scene.HighlightAsync(page.Locator("select[name='Calendar.FreeBusyRoles']"));
        });

        await scene.StepAsync("share-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(page.Locator("button[data-id='share-users-Read']"), pauseAfter: 800);
            await scene.TypeAsync(page.Locator(".dropdown-menu.show .bs-searchbox input"), "jamie", delay: 80);
            await Task.Delay(1300);
            await scene.ClickAsync(page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = "jamie" }).First, pauseAfter: 500);
            await page.Keyboard.PressAsync("Escape");
            await Task.Delay(500);
            await scene.ClickAsync(page.Locator("button[data-id='share-users-Write']"), pauseAfter: 800);
            await scene.TypeAsync(page.Locator(".dropdown-menu.show .bs-searchbox input"), "maria", delay: 80);
            await Task.Delay(1300);
            await scene.ClickAsync(page.Locator(".dropdown-menu.show .dropdown-item").Filter(new() { HasTextString = "maria" }).First, pauseAfter: 500);
            await page.Keyboard.PressAsync("Escape");
        });

        await scene.StepAsync("share-3", async () =>
        {
            await scene.MoveToAsync(1000, 600);
            await scene.ScrollAsync(0, 700);
            await scene.ClickAsync(page.Locator("label[for='Calendar_LimitOverlaps']"), pauseAfter: 1800);
            await scene.ClickAsync(page.Locator("label[for='Calendar_LimitOverlaps']"), pauseAfter: 800);
            await scene.ClickAndWaitForNavigationAsync(page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last);
            await Task.Delay(1200);
        });
    }

    private static async Task SeeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string person, string step, bool openDetails)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, StateOf(person));
        await Cal.OpenCalendarAsync(scene, weeksAhead: 1);
        await scene.StartCaptureAsync();

        var page = scene.Page;

        await scene.StepAsync(step, async () =>
        {
            await scene.ClickAsync(page.Locator("label[for^='calendar-']").Filter(new() { HasTextString = "Project Falcon" }).First, pauseAfter: 1500);
            await scene.MoveToAsync(1000, 600);
            await scene.ScrollAsync(0, 450);
            await Task.Delay(1000);
            await scene.ClickAsync(page.Locator(".fc-event").Filter(new() { HasTextString = openDetails ? "Design review" : "Busy" }).First, pauseAfter: 1500);
            await scene.HighlightAsync(Cal.Modal(scene));
            await Task.Delay(1500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static Task JamieAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => SeeAsync(browser, directory, durations, "jamie", "jam-1", openDetails: true);

    private static Task AlexAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => SeeAsync(browser, directory, durations, "alex", "alex-1", openDetails: false);

    private static async Task PermissionsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/Roles/Index");
        await scene.StartCaptureAsync();

        var page = scene.Page;
        ILocator Row(string text) => page.Locator("tr").Filter(new() { HasTextString = text }).First;

        await scene.StepAsync("perm-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(page.Locator("a[href='/Admin/Roles/Edit/Editor']").First);
            await Task.Delay(1200);
            await page.EvaluateAsync("() => { const h = [...document.querySelectorAll('td')].find(e => e.textContent.trim() === 'Create calendars'); window.scrollTo(0, h.getBoundingClientRect().top + scrollY - 330); }");
            await Task.Delay(1200);
            await scene.HighlightAsync(Row("Use calendars"));
            await Task.Delay(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row("Create calendars"));
        });

        await scene.StepAsync("perm-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row("View all calendars"));
            await Task.Delay(2200);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row("Manage all calendars"));
        });

        await scene.StepAsync("perm-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row("List anyone's calendar events"));
            await Task.Delay(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row("View anyone's calendar events"));
            await Task.Delay(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Row("own calendar events"));
        });

        await scene.ClearHighlightsAsync();
    }
}
