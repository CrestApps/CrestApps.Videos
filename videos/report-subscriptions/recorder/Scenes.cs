using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the "Report subscriptions" video, one method per "clip" of storyboard.json.
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
        ("02-features", FeaturesAsync),
        ("02b-roles", RolesAsync),
        ("03-email", EmailAsync),
        ("04-add", AddAsync),
        ("05-send", SendAsync),
        ("06-manage", ManageAsync),
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
    /// Nothing to prepare: the clips start from the demo site with the Reports features on.
    /// </summary>
    public static Task PrepareAsync(IBrowser browser) => Task.CompletedTask;

    // ----- Helpers -----

    private static ILocator Nav(Scene scene, string text)
        => scene.Page.Locator("#left-nav button:visible, #left-nav a:visible").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex($"^\\s*{System.Text.RegularExpressions.Regex.Escape(text)}\\s*$") }).First;

    private static ILocator SaveButton(Scene scene) => scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last;

    private static ILocator SaveSubscription(Scene scene) => scene.Page.Locator("button[type=submit][value=save]");

    private static ILocator FeatureRow(Scene scene, string id)
        => scene.Page.Locator("li.list-group-item").Filter(new() { Has = scene.Page.Locator($"#btn-enable-{id.Replace('.', '_')}, #btn-disable-{id.Replace('.', '_')}") }).First;

    // ----- Clips -----

    private static async Task FeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();

        await scene.StepAsync("feat-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "subscriptions", delay: 80);
            await scene.PauseAsync(600);
            var row = FeatureRow(scene, "CrestApps.OrchardCore.Reports.Subscriptions");
            await scene.HighlightAsync(row);
            await scene.PauseAsync(900);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#btn-enable-CrestApps_OrchardCore_Reports_Subscriptions"));
        });

        await scene.StepAsync("feat-2", async () =>
        {
            await scene.ClickAsync(Nav(scene, "Reports"));
            await scene.PauseAsync(400);
            var item = scene.Page.Locator("#left-nav a:visible").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("^\\s*Subscriptions\\s*$") }).First;
            await scene.HighlightAsync(item, "Reports, Subscriptions");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task RolesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Roles/Edit/Report%20viewer");
        await scene.StartCaptureAsync();

        await scene.StepAsync("perm-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "subscriptions", delay: 80);
            await scene.PauseAsync(700);
            var row = scene.Page.Locator("tr").Filter(new() { Has = scene.Page.Locator("[id='Checkbox.ManageReportSubscriptions']") }).First;
            await scene.HighlightAsync(row);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task EmailAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/Settings/email");
        await scene.StartCaptureAsync();

        await scene.StepAsync("email-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".alert").First);
        });

        await scene.StepAsync("email-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/Features");
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "smtp", delay: 80);
            await scene.PauseAsync(600);
            await scene.HighlightAsync(FeatureRow(scene, "OrchardCore.Email.Smtp"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#btn-enable-OrchardCore_Email_Smtp"));
        });

        await scene.StepAsync("email-3", async () =>
        {
            await scene.NavigateAsync("/Admin/Settings/email");
            await scene.ClickAsync(scene.Page.Locator("a[href='#tab-s-m-t-p']"));
            await scene.ClickAsync(scene.Page.Locator("#ISite_SmtpSettings_IsEnabled"));
            await scene.TypeAsync(scene.Page.Locator("#ISite_SmtpSettings_DefaultSender"), "reports@contoso.example", delay: 35);
            await scene.TypeAsync(scene.Page.Locator("#ISite_SmtpSettings_Host"), "mail.contoso.example", delay: 35);
            await scene.Page.Locator("#ISite_SmtpSettings_Port").FillAsync("25");
            await scene.HighlightAsync(scene.Page.Locator("#ISite_SmtpSettings_Host"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.HoldAsync(1);
    }

    private static async Task AddAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/Reports/subscriptions");
        await scene.StartCaptureAsync();

        await scene.StepAsync("add-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".alert, .admin-list").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Add subscription" }).First);
        });

        await scene.StepAsync("add-2", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#ReportName"), "contact-center-queue-service-level");
            await scene.TypeAsync(scene.Page.Locator("#Name"), "Weekly service review", delay: 45);
        });

        await scene.StepAsync("add-3", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#Frequency"), "Monthly");
            await scene.HighlightAsync(scene.Page.Locator("#DayOfMonth"));
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.SelectAsync(scene.Page.Locator("#Frequency"), "Weekly");
            await scene.SelectAsync(scene.Page.Locator("#DayOfWeek"), "Monday");
            await scene.HighlightAsync(scene.Page.Locator("#Hour"));
        });

        await scene.StepAsync("add-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#Recipients"), "supervisors@contoso.example", delay: 35);
            await scene.SelectAsync(scene.Page.Locator("#AttachmentFormat"), "pdf");
        });

        await scene.StepAsync("add-5", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#IsEnabled"));
            await scene.PauseAsync(800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveSubscription(scene));
            await scene.HighlightAsync(scene.Page.Locator(".alert").First);
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("ol.breadcrumb a, .breadcrumb a").Filter(new() { HasTextString = "Report subscriptions" }).First);
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item").Filter(new() { HasTextString = "Weekly service review" }).First);
        });

        await scene.HoldAsync(2);
    }

    private static async Task SendAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(120_000);

        await scene.OpenAsync("/Admin/Reports/subscriptions/Create");
        await scene.Page.Locator("#ReportName").SelectOptionAsync(new SelectOptionValue { Value = "contact-center-call-insights" });
        await scene.Page.Locator("#Name").FillAsync("Daily call pulse");
        await scene.Page.Locator("#Frequency").SelectOptionAsync(new SelectOptionValue { Value = "Daily" });
        await scene.Page.Locator("#Hour").FillAsync("8");
        await scene.Page.Locator("#Recipients").FillAsync("floor.manager@contoso.example");
        await scene.StartCaptureAsync();

        await scene.StepAsync("send-1", async () =>
        {
            var send = scene.Page.Locator("button[type=submit][value=send]");
            await scene.MoveToAsync(send);
            await scene.HighlightAsync(send);
        });

        await scene.StepAsync("send-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var send = scene.Page.Locator("button[type=submit][value=send]");
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => send.ClickAsync()));
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.Locator(".alert").First);
        });

        await scene.StepAsync("send-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".breadcrumb a").Filter(new() { HasTextString = "Report subscriptions" }).First);
            await scene.HighlightAsync(scene.Page.Locator(".text-danger").First);
        });

        await scene.HoldAsync(1.5);
    }

    private static async Task ManageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultTimeout(60_000);

        await scene.OpenAsync("/Admin/Reports/subscriptions");
        await scene.StartCaptureAsync();

        await scene.StepAsync("man-1", async () =>
        {
            var row = scene.Page.Locator(".list-group-item").Filter(new() { HasTextString = "Weekly service review" }).First;
            await scene.ClickAndWaitForNavigationAsync(row.GetByRole(AriaRole.Link, new() { Name = "Edit" }).First);
            await scene.ClickAsync(scene.Page.Locator("#IsEnabled"));
            await scene.HighlightAsync(scene.Page.Locator("#IsEnabled"));
            await scene.PauseAsync(1200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button[type=submit][value=save]"));
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".breadcrumb a").Filter(new() { HasTextString = "Report subscriptions" }).First);
            await scene.HighlightAsync(scene.Page.Locator(".list-group-item .badge").First);
        });

        await scene.StepAsync("man-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "pulse", delay: 80);
            await scene.WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"));
            var delete = scene.Page.GetByRole(AriaRole.Link, new() { Name = "Delete" }).First;
            await scene.HighlightAsync(delete);
        });

        await scene.ClearHighlightsAsync();
    }
}
