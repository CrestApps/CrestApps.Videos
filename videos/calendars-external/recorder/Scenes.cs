using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-subscribe", SubscribeAsync),
        ("03-result", ResultAsync),
    ];

    // A small made-up calendar file kept in this repository, which the video subscribes to (see the README).
    private const string PartnerFeed = "https://raw.githubusercontent.com/CrestApps/CrestApps.Videos/videos/calendars-external/videos/calendars-external/feed/partner-events.ics";

    private static async Task AddSubscribedCalendarAsync(Scene scene, string name, string address, bool pace)
    {
        await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Add calendar" }), pauseAfter: 800);
        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".dropdown-menu.show a").Filter(new() { HasTextString = "General" }).First);
        await scene.TypeAsync(scene.Page.Locator("#Calendar_Name"), name);
        await scene.Page.Locator("#Calendar_SubscriptionEnabled").ScrollIntoViewIfNeededAsync();
        await scene.PauseAsync(700);
        await scene.ClickAsync(scene.Page.Locator("#Calendar_SubscriptionEnabled"), pauseAfter: 600);

        if (pace)
        {
            await scene.HighlightAsync(scene.Page.Locator("#Calendar_SubscriptionProvider"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
        }

        await scene.TypeAsync(scene.Page.Locator("#Calendar_SubscriptionUrl"), address, 12);
        await scene.Page.Locator("#Calendar_SubscriptionInterval").FillAsync("5");
    }

    private static async Task SaveCalendarAsync(Scene scene)
    {
        await scene.Page.Locator("button:has-text('Save')").Last.ScrollIntoViewIfNeededAsync();
        await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }));
    }

    private static async Task SubscribeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/calendars");
        await scene.StartCaptureAsync();

        await scene.StepAsync("sub-1", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Add calendar" }), pauseAfter: 800);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".dropdown-menu.show a").Filter(new() { HasTextString = "General" }).First);
            await scene.TypeAsync(scene.Page.Locator("#Calendar_Name"), "Partner events");
            await scene.Page.Locator("#Calendar_SubscriptionEnabled").ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(700);
            await scene.ClickAsync(scene.Page.Locator("#Calendar_SubscriptionEnabled"), pauseAfter: 600);
        });

        await scene.StepAsync("sub-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#Calendar_SubscriptionProvider"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(scene.Page.Locator("#Calendar_SubscriptionUrl"), PartnerFeed, 8);
        });

        await scene.StepAsync("sub-3", async () =>
        {
            await scene.Page.Locator("#Calendar_SubscriptionInterval").FillAsync(string.Empty);
            await scene.TypeAsync(scene.Page.Locator("#Calendar_SubscriptionInterval"), "5");
            await scene.HighlightAsync(scene.Page.Locator("#Calendar_SubscriptionInterval"));
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await SaveCalendarAsync(scene);
        });

        await scene.StepAsync("sub-4", async () =>
        {
            await AddSubscribedCalendarAsync(scene, "Broken feed", BaseUrl + "/broken-feed.ics", pace: false);
            await SaveCalendarAsync(scene);
        });
    }

    private static async Task<string> CalendarIdAsync(IPage page, string name)
    {
        await page.GotoAsync(BaseUrl + "/Admin/calendars");
        var href = await page.Locator("li, tr, .list-group-item").Filter(new() { HasTextString = name }).First.Locator("a[href*='/Admin/calendars/edit/']").First.GetAttributeAsync("href");

        return href[(href.LastIndexOf('/') + 1)..];
    }

    private static async Task ResultAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/calendars");
        string partner = null, broken = null;

        await scene.StartCaptureAsync();

        await scene.StepAsync("res-1", async () =>
        {
            // The site reads the subscriptions that are due once every five minutes: wait, off camera, for both reads.
            await scene.OffCameraAsync(async () =>
            {
                var poll = await scene.Context.NewPageAsync();
                partner = await CalendarIdAsync(poll, "Partner events");
                broken = await CalendarIdAsync(poll, "Broken feed");

                for (var i = 0; i < 60; i++)
                {
                    var done = 0;

                    foreach (var id in new[] { partner, broken })
                    {
                        await poll.GotoAsync($"{BaseUrl}/Admin/calendars/edit/{id}");
                        done += await poll.GetByText("Last synchronized").CountAsync() > 0 ? 1 : 0;
                    }

                    if (done == 2)
                    {
                        break;
                    }

                    await poll.WaitForTimeoutAsync(15000);
                }

                await poll.CloseAsync();
            });

            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("li, tr, .list-group-item").Filter(new() { HasTextString = "Partner events" }).First.GetByRole(AriaRole.Link, new() { Name = "Edit", Exact = true }));
            await scene.Page.GetByText("Last synchronized").ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.GetByText("Last synchronized").Locator("xpath=ancestor::div[contains(@class,'ocat-wrapper')][1]"));
        });

        await scene.StepAsync("res-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync($"/Admin/calendar?calendars={partner}");
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Month", Exact = true }), pauseAfter: 600);
            await scene.ClickAsync(scene.Page.Locator(".fc-next-button").First, pauseAfter: 1500);
        });

        await scene.StepAsync("res-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".fc-event").Filter(new() { HasTextString = "Partner webinar" }).First, pauseAfter: 2500);
        });

        await scene.StepAsync("res-4", async () =>
        {
            await scene.NavigateAsync($"/Admin/calendars/edit/{broken}");
            await scene.Page.GetByText("Last synchronized").ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.GetByText("Last synchronized").Locator("xpath=ancestor::div[contains(@class,'ocat-wrapper')][1]"));
        });

        await scene.StepAsync("res-5", async () =>
        {
            await scene.PauseAsync(3000);
        });
    }
}
