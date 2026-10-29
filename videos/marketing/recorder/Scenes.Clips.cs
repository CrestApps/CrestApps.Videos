using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // Where the demo site's email lands: a local sink that writes each message to a file (nothing leaves the machine).
    private const string MailFolder = "C:/cavideo/commerce-2/mail";

    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Row(Scene scene, string text) => scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = text }).First;

    private static ILocator LinkNamed(Scene scene, string name) => scene.Page.GetByRole(AriaRole.Link, new() { Name = name, Exact = true }).First;

    private static ILocator Field(Scene scene, string id) => scene.Page.Locator("#" + id);

    private static Task ClearAsync(Scene scene) => scene.ClearHighlightsAsync();

    private static string State(string user) => Path.Combine(Root, $"state-{user}.json");

    // ----- Clips -----

    private static async Task SegmentsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/marketing/segments");
        await scene.StartCaptureAsync();

        await scene.StepAsync("seg-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/marketing/segments"), "Commerce > Segments");
            await scene.PauseAsync(2400);
            await ClearAsync(scene);
            await scene.HighlightAsync(Row(scene, "New customers"), "Its rules, and who meets them");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Add segment"));
        });

        await scene.StepAsync("seg-2", async () =>
        {
            await scene.TypeAsync(Field(scene, "Name"), "Repeat buyers", delay: 60);
            await scene.TypeAsync(Field(scene, "Description"), "Customers who bought more than once", delay: 30);
        });

        await scene.StepAsync("seg-3", async () =>
        {
            await scene.HighlightAsync(Field(scene, "LastOrderAtLeastDaysAgo"), "Haven't ordered in (days)");
            await scene.PauseAsync(4000);
            await ClearAsync(scene);
            await scene.HighlightAsync(Field(scene, "LastOrderWithinDays"), "Ordered in the last (days)");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.HighlightAsync(Field(scene, "FirstOrderWithinDays"), "First ordered in the last (days)");
        });

        await ClearAsync(scene);

        await scene.StepAsync("seg-4", async () =>
        {
            await scene.TypeAsync(Field(scene, "MinOrders"), "2", delay: 90);
            await scene.HighlightAsync(Field(scene, "MinOrders"), "At least (orders)");
            await scene.PauseAsync(2200);
            await ClearAsync(scene);
            await scene.HighlightAsync(Field(scene, "MinSpent"), "Spent at least");
            await scene.PauseAsync(2400);
            await ClearAsync(scene);
            await scene.HighlightAsync(Field(scene, "Country"), "Country");
        });

        await ClearAsync(scene);

        await scene.StepAsync("seg-5", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last);
            await scene.HighlightAsync(Row(scene, "Repeat buyers"), "How many customers, and how many accept marketing");
        });
    }

    private static async Task CampaignsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/marketing/campaigns");
        await scene.StartCaptureAsync();

        await scene.StepAsync("camp-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/marketing/campaigns"), "Commerce > Campaigns");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Add campaign"));

            // The field starts with a sample email: empty it, so the campaign is written from nothing.
            await Field(scene, "Body").FillAsync(string.Empty);
        });

        await scene.StepAsync("camp-2", async () =>
        {
            await scene.TypeAsync(Field(scene, "Name"), "Autumn welcome", delay: 60);
            await scene.SelectAsync(Field(scene, "SegmentId"), "New customers", pauseAfter: 700);
            await scene.HighlightAsync(Field(scene, "SegmentId"), "Segment");
        });

        await ClearAsync(scene);

        await scene.StepAsync("camp-3", async () =>
        {
            await scene.TypeAsync(Field(scene, "Subject"), "Welcome to Contoso", delay: 50);
            await scene.TypeAsync(Field(scene, "Body"), "Thank you for your first order with us. Our autumn range is in, and we would love to hear what you think.", delay: 15);
            await scene.HighlightAsync(scene.Page.GetByText("An unsubscribe link is added at the end").First, "What you can put in");
        });

        await ClearAsync(scene);

        await scene.StepAsync("camp-4", async () =>
        {
            await scene.TypeAsync(Field(scene, "TestEmail"), "admin@example.com", delay: 40);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Send test" }));
        });

        await scene.StepAsync("camp-5", async () =>
        {
            await scene.HighlightAsync(Field(scene, "ScheduledLocal"), "Send at");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Save draft" }), "Save draft");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Send", Exact = true }));
        });

        await scene.StepAsync("camp-6", async () =>
        {
            await scene.OffCameraAsync(async () =>
            {
                for (var i = 0; i < 40; i++)
                {
                    await Task.Delay(5000);
                    await scene.Page.ReloadAsync();

                    if (await scene.Page.GetByText("Sent", new() { Exact = true }).CountAsync() > 0)
                    {
                        break;
                    }
                }
            });

            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.Locator(".alert").First, "Sent to, skipped and failed");
        });
    }

    private static async Task ConsentAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        // The unsubscribe link of the test email the campaign clip sent.
        var mail = Directory.GetFiles(MailFolder, "*.eml").OrderBy(file => file).Last();
        var link = Regex.Match(File.ReadAllText(mail), @"http://localhost:5375/[^\s""<>]*nsubscribe[^\s""<>]*").Value.TrimEnd('.', ')', '=');
        var path = new Uri(link).PathAndQuery;

        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/customers");
        await scene.StartCaptureAsync();

        await scene.StepAsync("cons-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Jamie Chen"));
            await Field(scene, "AcceptsMarketing").ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(Field(scene, "AcceptsMarketing"), "Accepts marketing");
        });

        await ClearAsync(scene);

        await scene.StepAsync("cons-2", async () =>
        {
            await scene.NavigateAsync(path);
            await scene.PauseAsync(800);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Heading).First, "The unsubscribe page");
        });

        await ClearAsync(scene);

        await scene.StepAsync("cons-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button).Filter(new() { HasTextString = "nsubscribe" }).Last);
            await scene.PauseAsync(1500);
        });
    }

    private static async Task AutomationsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Recipes");
        await scene.StartCaptureAsync();

        await scene.StepAsync("auto-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Automation: thank-you email").First, "Automation: thank-you email");
        });

        await ClearAsync(scene);

        await scene.StepAsync("auto-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Automation: canceled order alert").First, "Automation: canceled order alert");
            await scene.PauseAsync(4500);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("Automation: low stock alert").First, "Automation: low stock alert");
        });

        await ClearAsync(scene);

        await scene.StepAsync("auto-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#btn-run-CommerceThankYouEmail"), pauseAfter: 1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalOkButton"));
            await scene.PauseAsync(1500);
            await scene.NavigateAsync("/Admin/Workflows/Types");
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = "hank" }).First, "The workflow, to change it");
        });
    }
}
