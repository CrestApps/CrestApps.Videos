using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-mail", MailAsync),
        ("03-invite", InviteAsync),
        ("04-answer", AnswerAsync),
        ("05-change", ChangeAsync),
        ("05-mails", MailsAsync),
    ];

    // The messages the site wrote to its pickup folder, shown as a web page (see the README).
    private const string MailViews = "file:///C:/cavideo/cal2/mailview/";

    private static ILocator FirstEvent(Scene scene, string title) => scene.Page.Locator(".fc-event").Filter(new() { HasTextString = title }).First;

    private static async Task MailAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/calendars");
        await scene.StartCaptureAsync();

        await scene.StepAsync("mail-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("nav").GetByText("Settings", new() { Exact = true }).First, pauseAfter: 1000);
            await scene.ClickAsync(scene.Page.Locator("nav").GetByText("Communication", new() { Exact = true }).First, pauseAfter: 800);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("nav a[href='/Admin/Settings/email']").First);
            await scene.ClickAsync(scene.Page.GetByText("SMTP", new() { Exact = true }).First, pauseAfter: 600);
        });

        await scene.StepAsync("mail-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#ISite_SmtpSettings_IsEnabled"), pauseAfter: 600);
            await scene.TypeAsync(scene.Page.Locator("#ISite_SmtpSettings_DefaultSender"), "calendar@contoso-studio.example", 30);
            await scene.SelectAsync(scene.Page.Locator("#ISite_SmtpSettings_DeliveryMethod"), "Specified pickup directory");
            await scene.Page.Locator("#ISite_SmtpSettings_PickupDirectoryLocation").FillAsync(string.Empty);
            await scene.TypeAsync(scene.Page.Locator("#ISite_SmtpSettings_PickupDirectoryLocation"), "/");
        });

        await scene.StepAsync("mail-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }));
            await scene.NavigateAsync("/Admin/Settings/general");
            await scene.TypeAsync(scene.Page.Locator("#ISite_BaseUrl"), BaseUrl, 30);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }));
        });
    }

    private static async Task InviteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenCalendarAsync(scene, "Week", next: 1);
        await scene.StartCaptureAsync();

        await scene.StepAsync("invite-1", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByText("New event").First, pauseAfter: 1200);
            await ChooseKindAsync(scene, "Meeting", nth: 1);
            await scene.TypeAsync(Modal(scene).Locator("#CalendarEvent_Title"), "Launch review");
            await FillTimesAsync(scene, "2026-11-04", "14:00", "15:00");
        });

        await scene.StepAsync("invite-2", async () =>
        {
            await ModalScrollAsync(scene, 5000);
            await InviteUserAsync(scene, "sam");
        });

        await scene.StepAsync("invite-3", async () =>
        {
            await scene.TypeAsync(Modal(scene).Locator("#CalendarEvent_Guests"), "Dana Wright <dana.wright@example.com>", 35);
        });

        await scene.StepAsync("invite-4", async () =>
        {
            await scene.ClickAsync(Modal(scene).GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }), pauseAfter: 2500);
        });
    }

    private static async Task AnswerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/calendars");
        await scene.Page.GotoAsync(MailViews + "invite.html");
        await scene.StartCaptureAsync();

        await scene.StepAsync("answer-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".body p").Last);
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("answer-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Accept" }));
            await scene.PauseAsync(1200);
        });

        await scene.StepAsync("answer-3", async () =>
        {
            // The event's own page: its id is in the answer links of the message.
            var id = System.Text.RegularExpressions.Regex.Match(File.ReadAllText("C:/cavideo/cal2/mailview/invite.html"), "respond/([^/]+)/").Groups[1].Value;
            await scene.NavigateAsync($"/Admin/calendar/events/edit/{id}");
            await scene.Page.Locator("text=Answers").First.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.Locator("text=Answers").First.Locator("xpath=ancestor::div[contains(@class,'ocat-wrapper')][1]"));
        });
    }

    private static async Task ChangeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.Dialog += (_, dialog) => _ = dialog.AcceptAsync();
        await OpenCalendarAsync(scene, "Week", next: 1);
        await scene.StartCaptureAsync();

        await scene.StepAsync("change-1", async () =>
        {
            await scene.ClickAsync(FirstEvent(scene, "Launch review"), pauseAfter: 800);
            await scene.ClickAsync(scene.Page.Locator("#calendar-event-modal .modal-footer a, #calendar-event-modal .modal-footer button").First, pauseAfter: 1800);
            await FillTimesAsync(scene, "2026-11-04", "15:00", "16:00");
            await scene.PauseAsync(800);
            await ModalScrollAsync(scene, 5000);
            await scene.ClickAsync(Modal(scene).GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }), pauseAfter: 2500);
        });

        await scene.StepAsync("change-2", async () =>
        {
            await scene.ClickAsync(FirstEvent(scene, "Launch review"), pauseAfter: 800);
            await scene.ClickAsync(scene.Page.Locator("#calendar-event-modal .modal-footer a, #calendar-event-modal .modal-footer button").First, pauseAfter: 1800);
            await ModalScrollAsync(scene, 5000);
            await scene.ClickAsync(Modal(scene).GetByRole(AriaRole.Button, new() { Name = "Cancel event", Exact = true }), pauseAfter: 2500);
        });
    }

    private static async Task MailsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/calendars");
        await scene.Page.GotoAsync(MailViews + "update.html");
        await scene.StartCaptureAsync();

        await scene.StepAsync("mails-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("h1"));
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("mails-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.WaitForNavigationAsync(() => scene.Page.GotoAsync(MailViews + "cancel.html"));
            await scene.HighlightAsync(scene.Page.Locator(".body p").Last);
        });
    }
}
