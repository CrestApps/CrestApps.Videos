using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static async Task ColumnsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenReportAsync(scene, "Activities by channel");
        await scene.StartCaptureAsync();

        await scene.StepAsync("agg-1", async () =>
        {
            await scene.DragToAsync(Field(scene, "Activities.Attempts"), Shelf(scene, "columns"), 330);
            await scene.PauseAsync(900);
            await scene.HighlightAsync(Property(scene, "Aggregate"));
            await scene.SelectAsync(Property(scene, "Aggregate").Locator("select"), "Average");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("agg-2", async () =>
        {
            await scene.Page.Locator(".report-designer-data input").First.FillAsync("created");
            await scene.PauseAsync(500);
            await scene.DragToAsync(Field(scene, "Activities.CreatedUtc"), Shelf(scene, "columns"), -260);
            await scene.PauseAsync(800);
            await scene.HighlightAsync(Property(scene, "Transform"));
            await scene.SelectAsync(Property(scene, "Transform").Locator("select"), "Month");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("agg-3", async () =>
        {
            await scene.ClickAsync(Pill(scene, "Attempts").Locator("button").First, pauseAfter: 600);
            var header = Property(scene, "Header").Locator("input");
            await header.FillAsync(string.Empty);
            await scene.TypeAsync(header, "Avg. attempts", delay: 60);
            await scene.Page.Keyboard.PressAsync("Tab");
            await scene.PauseAsync(900);
            await scene.ClickAsync(Property(scene, "Format").Locator("button[aria-label='Common formats']"), pauseAfter: 900);
            await scene.HighlightAsync(scene.Page.Locator(".rd-format-help").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator(".rd-format-option").Filter(new() { HasTextString = "One decimal" }).First, pauseAfter: 800);
            await scene.HighlightAsync(scene.Page.Locator(".report-designer-properties .form-check").Filter(new() { HasTextString = "Hide from tables" }).First);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("agg-4", async () =>
        {
            await scene.Page.Locator(".report-designer-data input").First.FillAsync(string.Empty);
            var add = scene.Page.Locator(".report-designer-data button, .report-designer-data a").Filter(new() { HasTextString = "New" }).First;
            await scene.ClickAsync(add, pauseAfter: 800);
            var modal = scene.Page.Locator(".modal.show");
            await scene.TypeAsync(modal.Locator("input").First, "Completion rate", delay: 50);
            await scene.TypeAsync(modal.Locator("textarea").First, "SUM(IF([Activities.Status] = 'Completed', 1, 0)) / COUNT()", delay: 28);
            await scene.ClickAsync(modal.Locator("button").Filter(new() { HasTextString = "Check" }), pauseAfter: 1200);
            await scene.ClickAsync(modal.Locator("button").Filter(new() { HasTextString = "Apply" }), pauseAfter: 800);
        });

        await scene.StepAsync("agg-5", async () =>
        {
            var calculated = scene.Page.Locator(".report-designer-field").Filter(new() { HasTextString = "Completion rate" }).First;
            await scene.DragToAsync(calculated, Shelf(scene, "columns"), 150);
            await scene.PauseAsync(600);
            await scene.ClickAsync(Property(scene, "Format").Locator("button[aria-label='Common formats']"), pauseAfter: 700);
            await scene.ClickAsync(scene.Page.Locator(".rd-format-option").Filter(new() { HasTextString = "Percentage" }).First, pauseAfter: 800);
        });
    }

    private static async Task FiltersAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenReportAsync(scene, "Activities by channel");
        await scene.StartCaptureAsync();

        await scene.StepAsync("filter-1", async () =>
        {
            await scene.DragToAsync(Field(scene, "Activities.Channel"), Shelf(scene, "filters"), 120);
            await scene.PauseAsync(800);
            await scene.HighlightAsync(Property(scene, "Applies to"));
            await scene.SelectAsync(Property(scene, "Applies to").Locator("select"), "Result", pauseAfter: 2200);
            await scene.SelectAsync(Property(scene, "Applies to").Locator("select"), "Rows");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("filter-2", async () =>
        {
            await scene.ClickAsync(Check(scene, ".report-designer-properties", "Let viewers change this filter"), pauseAfter: 800);
            await scene.HighlightAsync(Property(scene, "Control"));
            await scene.SelectAsync(Property(scene, "Control").Locator("select"), "MultiSelect");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("filter-3", async () =>
        {
            await scene.PauseAsync(1200);
            var picker = scene.Page.Locator(".report-designer-preview .bootstrap-select").Last;
            await scene.ClickAsync(picker.Locator(".dropdown-toggle"), pauseAfter: 700);
            await scene.ClickAsync(picker.Locator(".dropdown-menu .dropdown-item").Filter(new() { HasTextString = "Phone" }).First, pauseAfter: 500);
            await scene.Page.Keyboard.PressAsync("Escape");
            await scene.ClickAsync(scene.Page.Locator(".report-designer-preview button").Filter(new() { HasTextString = "Apply" }).First, pauseAfter: 900);
        });
    }

    private static async Task VisualsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenReportAsync(scene, "Activities by channel");

        // Give the preview the room: collapse the data pane and the columns and filters.
        await scene.Page.Locator("button[aria-label='Collapse the data pane']").First.ClickAsync();
        await scene.Page.Locator("button[aria-label='Collapse the columns and filters']").First.ClickAsync();
        await scene.PauseAsync(800);
        await scene.StartCaptureAsync();
        var visuals = scene.Page.Locator(".report-designer-visuals");
        var preview = scene.Page.Locator(".report-designer-preview").First;

        ILocator Select(string label)
            => visuals.Locator($"xpath=.//*[normalize-space(text())='{label}']/following::select[1]").First;

        ILocator Option(string well, string text)
            => visuals.Locator($"xpath=.//*[normalize-space(text())='{well}']/following::*[contains(@class,'form-check')][.//label[normalize-space()='{text}']][1]//input").First;

        async Task MoveUpAsync(string name, int times)
        {
            for (var i = 0; i < times; i++)
            {
                var row = visuals.Locator(".list-group-item").Filter(new() { HasTextString = name }).First;
                await scene.ClickAsync(row.Locator("button[aria-label='Move up']"), pauseAfter: 700);
            }
        }

        await scene.StepAsync("vis-1", async () =>
        {
            await scene.HighlightAsync(visuals.Locator(".list-group").First);
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(visuals.Locator(".list-group-item button").First, pauseAfter: 600);
            await scene.ClickAsync(Check(scene, ".report-designer-visuals", "Show totals"), pauseAfter: 600);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("vis-2", async () =>
        {
            await scene.ClickAsync(visuals.Locator(".card-body button").Filter(new() { HasTextString = "Chart" }).First, pauseAfter: 800);
            await scene.SelectAsync(Select("Chart type"), "Bar");
            await scene.SelectAsync(Select("Categories"), "Created (Month)");
            await scene.ClickAsync(Option("Values", "Avg. attempts"), pauseAfter: 300);
            await scene.ClickAsync(Option("Values", "Completion rate"), pauseAfter: 300);
            await scene.SelectAsync(Select("Split into series by"), "Status");
            await scene.ClickAsync(Check(scene, ".report-designer-visuals", "Stack series"), pauseAfter: 300);
            await MoveUpAsync("Chart", 1);
            await scene.MoveToAsync(preview);
        });

        await scene.StepAsync("vis-3", async () =>
        {
            await scene.ClickAsync(visuals.Locator(".card-body button").Filter(new() { HasTextString = "Metrics" }).First, pauseAfter: 600);
            await MoveUpAsync("Metrics", 2);
        });

        await scene.StepAsync("vis-4", async () =>
        {
            await scene.ClickAsync(visuals.Locator(".card-body button").Filter(new() { HasTextString = "Pivot table" }).First, pauseAfter: 600);
            await scene.ClickAsync(Option("Rows", "Created (Month)"), pauseAfter: 300);
            await scene.SelectAsync(Select("Columns across"), "Status");
            await scene.MoveToAsync(preview);
            await scene.PauseAsync(1500);
            await scene.ScrollAsync(0, 1600);
        });

        await scene.StepAsync("vis-5", async () =>
        {
            await scene.SelectAsync(Select("Width"), "6", pauseAfter: 500);
            await scene.ClickAsync(visuals.Locator(".list-group-item button").Filter(new() { HasTextString = "Table" }).First, pauseAfter: 500);
            await scene.SelectAsync(Select("Width"), "6", pauseAfter: 900);
            await scene.MoveToAsync(preview);
            await scene.PauseAsync(1500);
            await scene.ScrollAsync(0, 500);
        });
    }

    private static async Task JoinsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        // A small report of its own, so the effect of each join shows in the preview: activities counted.
        await scene.OpenAsync("/Admin/reports/builder/create");
        await CompactMenuAsync(scene);
        await scene.PauseAsync(1500);
        await Tab(scene, "Settings").ClickAsync();
        await scene.Page.Locator(".report-designer-settings input[type=text]").First.FillAsync("Activities per agent");
        await scene.PauseAsync(800);
        await Tab(scene, "Design").ClickAsync();
        await scene.PauseAsync(1500);
        await AddDataSetAsync(scene, "Omnichannel", "Activities");
        await scene.DragToAsync(Field(scene, "$count"), Shelf(scene, "columns"));
        await scene.PauseAsync(1500);
        await scene.StartCaptureAsync();

        var preview = scene.Page.Locator(".report-designer-preview table").First;

        await scene.StepAsync("join-4", async () =>
        {
            await AddDataSetAsync(scene, "Users", "Users");
        });

        await scene.StepAsync("join-5", async () =>
        {
            await scene.Page.Locator(".report-designer-data input").First.FillAsync("user name");
            await scene.PauseAsync(600);
            await scene.DragToAsync(Field(scene, "Users.UserName"), Shelf(scene, "columns"), -200);
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(preview, "Assigned activities only");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("join-6", async () =>
        {
            await scene.ClickAsync(Tab(scene, "Data model"), pauseAfter: 1200);
            await scene.HighlightAsync(scene.Page.Locator(".rd-model-main .badge").Filter(new() { HasTextString = "Base" }).First, "The base");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".rd-model-badge").First, "The join type");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("join-7", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".rd-model-badge").First, pauseAfter: 800);
            var keep = scene.Page.Locator(".rd-model-side select").First;
            await scene.HighlightAsync(keep);
            await scene.SelectAsync(keep, "Left", pauseAfter: 900);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Tab(scene, "Design"), pauseAfter: 1500);
            await scene.HighlightAsync(preview, "With and without an agent");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("join-8", async () =>
        {
            await scene.ClickAsync(Tab(scene, "Data model"), pauseAfter: 1000);
            await scene.ClickAsync(scene.Page.Locator(".rd-model-main button").Filter(new() { HasTextString = "Add data set" }).First, pauseAfter: 700);
            var modal = scene.Page.Locator(".modal.show");
            await scene.ClickAsync(modal.Locator(".nav-link").Filter(new() { HasTextString = "Omnichannel" }), pauseAfter: 500);
            var card = modal.Locator(".card").Filter(new() { Has = scene.Page.Locator(".card-title", new() { HasTextRegex = new Regex(@"^\s*Campaigns\s*$") }) }).First;
            await scene.ClickAsync(card.Locator(".card-footer button").First, pauseAfter: 1500);
            await scene.DragToAsync(scene.Page.Locator(".rd-model-field[data-field-key='Activities.CampaignId']"), scene.Page.Locator(".rd-model-field[data-field-key='Campaigns.ItemId']"));
            await scene.PauseAsync(1200);
            await scene.HighlightAsync(scene.Page.Locator(".rd-model-field[data-field-key='Campaigns.ItemId']"), "Key column");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("join-9", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".rd-model-badge").Last, pauseAfter: 900);
            var side = scene.Page.Locator(".rd-model-side");
            await scene.HighlightAsync(side.Locator("button").Filter(new() { HasTextString = "Add matching columns" }).First, "Another pair");
        });

        await scene.ClearHighlightsAsync();
    }
    private static async Task KindsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var campaigns = await RunUrlAsync(scene, "Campaign activity summary");
        var outcomes = await RunUrlAsync(scene, "Activity outcomes by agent");
        var roles = await RunUrlAsync(scene, "Activities by role and agent");
        var dispositions = await RunUrlAsync(scene, "Dispositions by channel");
        await scene.OpenAsync(campaigns);
        await CompactMenuAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("kinds-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("table").First);
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("table tr.fw-semibold, table tfoot tr").Last, "Totals");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("kinds-2", async () =>
        {
            await scene.NavigateAsync(outcomes);
            await scene.PauseAsync(1500);
            await scene.ScrollAsync(0, 300);
        });

        await scene.StepAsync("kinds-3", async () =>
        {
            await scene.NavigateAsync(roles);
            await scene.PauseAsync(1200);
            await scene.HighlightAsync(scene.Page.Locator("tr.table-info").First, "Subtotal per agent");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ScrollAsync(0, 400);
        });

        await scene.StepAsync("kinds-4", async () =>
        {
            await scene.NavigateAsync(dispositions);
            await scene.PauseAsync(1500);
        });
    }

    private static async Task SourcesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/reports/builder/create");
        await CompactMenuAsync(scene);
        await scene.Page.Locator("button").Filter(new() { HasTextString = "Add data set" }).First.ClickAsync();
        await scene.PauseAsync(900);
        await scene.StartCaptureAsync();
        var modal = scene.Page.Locator(".modal.show");

        async Task ShowAsync(string source)
        {
            await scene.ClickAsync(modal.Locator(".nav-link").Filter(new() { HasTextString = source }).First, pauseAfter: 800);
        }

        await scene.StepAsync("sources-1", async () =>
        {
            await ShowAsync("Content items");
            await scene.HighlightAsync(modal.Locator(".card").Filter(new() { HasTextString = "Customer" }).First);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("sources-2", async () => await ShowAsync("Users"));

        await scene.StepAsync("sources-3", async () =>
        {
            await ShowAsync("Contact Center");
            await scene.MoveToAsync(modal.Locator(".card").Nth(4));
            await scene.ScrollAsync(0, 400);
        });

        await scene.StepAsync("sources-4", async () =>
        {
            await ShowAsync("Omnichannel");
            await scene.PauseAsync(2500);
            await ShowAsync("AI chat");
            await scene.PauseAsync(2500);
            await ShowAsync("Messaging");
        });

        await scene.StepAsync("sources-5", async () => await ShowAsync("Report views"));
    }

    private static async Task PublishAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenReportAsync(scene, "Activities by channel");
        var url = scene.Page.Url.Replace(BaseUrl, string.Empty);
        await scene.StartCaptureAsync();

        await scene.StepAsync("pub-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("[data-report-designer] div").Filter(new() { HasTextRegex = new Regex(@"^\s*This report is saved as a draft") }).Last, "Saved as you work");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync(url);
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("pub-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("[data-report-designer] button").Filter(new() { HasTextString = "Publish" }).First, pauseAfter: 2000);
        });

        await scene.StepAsync("pub-3", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("[data-report-designer] button").Filter(new() { HasTextString = "Versions" }).First, pauseAfter: 1200);
            await scene.HighlightAsync(scene.Page.Locator(".modal.show .list-group-item").First);
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator(".modal.show .btn-close").First, pauseAfter: 600);
        });
    }

    private static async Task ViewsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/reports/views");
        await CompactMenuAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("view-1", async () =>
        {
            await scene.PauseAsync(3500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a").Filter(new() { HasTextString = "Add View" }).First);
            await scene.PauseAsync(800);
        });

        await scene.StepAsync("view-2", async () =>
        {
            await AddDataSetAsync(scene, "Omnichannel", "Activities");
            await AddDataSetAsync(scene, "Users", "Users");
            await scene.ClickAsync(Tab(scene, "Data model"), pauseAfter: 900);
            await scene.ClickAsync(scene.Page.Locator(".rd-model-badge").First, pauseAfter: 700);
            await scene.SelectAsync(scene.Page.Locator(".rd-model-side select").First, "Left", pauseAfter: 600);
        });

        await scene.StepAsync("view-3", async () =>
        {
            await scene.ClickAsync(Tab(scene, "Design"), pauseAfter: 500);
            await scene.Page.Locator(".report-designer-data input").First.FillAsync(string.Empty);
            var add = scene.Page.Locator(".report-designer-data button, .report-designer-data a").Filter(new() { HasTextString = "New" }).First;
            await scene.ClickAsync(add, pauseAfter: 700);
            var modal = scene.Page.Locator(".modal.show");
            await scene.TypeAsync(modal.Locator("input").First, "Agent", delay: 60);
            await scene.TypeAsync(modal.Locator("textarea").First, "IFNULL([Users.UserName], 'Unassigned')", delay: 30);
            await scene.ClickAsync(modal.Locator("button").Filter(new() { HasTextString = "Check" }), pauseAfter: 1000);
            await scene.ClickAsync(modal.Locator("button").Filter(new() { HasTextString = "Apply" }), pauseAfter: 700);
        });

        await scene.StepAsync("view-4", async () =>
        {
            await scene.DragToAsync(Field(scene, "Activities.ItemId"), Shelf(scene, "columns"));
            await scene.PauseAsync(500);
            var agent = scene.Page.Locator(".report-designer-field").Filter(new() { Has = scene.Page.Locator("span.flex-grow-1", new() { HasTextRegex = new Regex(@"^\s*Agent\s*$") }) }).First;
            await scene.DragToAsync(agent, Shelf(scene, "columns"), 120);
            await scene.PauseAsync(500);
            await scene.DragToAsync(Field(scene, "Activities.Channel"), Shelf(scene, "columns"), 240);
            await scene.PauseAsync(500);
            await scene.ClickAsync(Field(scene, "Activities.Status").Locator("button[aria-label^='Add to columns']"));
            await scene.PauseAsync(900);
        });

        await scene.StepAsync("view-5", async () =>
        {
            await scene.ClickAsync(Tab(scene, "Settings"), pauseAfter: 500);
            var settings = scene.Page.Locator(".report-designer-settings");
            await scene.TypeAsync(settings.Locator("input[type=text]").First, "Activities with agents", delay: 50);
            var refresh = settings.GetByLabel("Refresh", new() { Exact = true });
            await scene.HighlightAsync(refresh);
            await scene.PauseAsync(1500);
            await scene.SelectAsync(refresh, "60", pauseAfter: 900);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("[data-report-designer] button").Filter(new() { HasTextString = "Save" }).First, pauseAfter: 1500);
        });

        await scene.StepAsync("view-6", async () =>
        {
            await scene.NavigateAsync("/Admin/reports/builder/create");
            await CompactMenuAsync(scene);
            await AddDataSetAsync(scene, "Report views", "Activities with agents");
        });

        await scene.StepAsync("view-7", async () =>
        {
            var agent = scene.Page.Locator(".report-designer-field").Filter(new() { Has = scene.Page.Locator("span.flex-grow-1", new() { HasTextRegex = new Regex(@"^\s*Agent\s*$") }) }).First;
            await scene.DragToAsync(agent, Shelf(scene, "columns"));
            await scene.PauseAsync(800);
            await scene.DragToAsync(Field(scene, "$count"), Shelf(scene, "columns"), 150);
            await scene.PauseAsync(1200);
            await scene.HighlightAsync(scene.Page.Locator(".report-designer-preview table").First);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("view-8", async () =>
        {
            var visuals = scene.Page.Locator(".report-designer-visuals");
            await scene.ClickAsync(visuals.Locator(".card-body button").Filter(new() { HasTextString = "Chart" }).First, pauseAfter: 900);
            await scene.SelectAsync(visuals.Locator("xpath=.//*[normalize-space(text())='Chart type']/following::select[1]").First, "HorizontalBar", pauseAfter: 900);
            var chart = visuals.Locator(".list-group-item").Filter(new() { HasTextString = "Chart" }).First;
            await scene.ClickAsync(chart.Locator("button[aria-label='Move up']"), pauseAfter: 900);
            await scene.MoveToAsync(scene.Page.Locator(".report-designer-preview").First);
            await scene.PauseAsync(1500);
        });
    }
    private static async Task ShareAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenReportAsync(scene, "Activities by channel");

        // A new link shows its token once: never record it.
        await scene.Page.AddStyleTagAsync(new() { Content = ".report-designer-sharing .alert input { filter: blur(6px); }" });
        await scene.StartCaptureAsync();
        var sharing = scene.Page.Locator(".report-designer-sharing");
        var people = sharing.Locator("input[type=search]").First;
        var recipients = sharing.Locator("input[type=search]").Nth(1);

        await scene.StepAsync("share-1", async () =>
        {
            await scene.ClickAsync(Tab(scene, "Sharing"), pauseAfter: 600);
            await scene.TypeAsync(people, "jamie", delay: 90);
            await scene.PauseAsync(1200);
            await scene.ClickAsync(sharing.Locator(".report-designer-user-results button").First, pauseAfter: 1200);
            var authenticated = sharing.Locator(".form-check").Filter(new() { HasTextString = "Authenticated" }).First.Locator("input");
            await scene.ClickAsync(authenticated, pauseAfter: 600);

            if (!await authenticated.IsCheckedAsync())
            {
                await scene.ClickAsync(authenticated, pauseAfter: 600);
            }

            var anonymous = sharing.Locator(".form-check").Filter(new() { HasTextString = "Anonymous" }).First;
            await scene.MoveToAsync(anonymous);
            await scene.HighlightAsync(anonymous, "Public");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("share-2", async () =>
        {
            await scene.ScrollAsync(0, 380);
            await scene.TypeAsync(recipients, "rpark", delay: 90);
            await scene.PauseAsync(1200);
            await scene.ClickAsync(sharing.Locator(".report-designer-user-results button").Last, pauseAfter: 600);
            var expires = sharing.GetByLabel("Expires after (days)");
            await expires.FillAsync(string.Empty);
            await scene.TypeAsync(expires, "30", delay: 90);
            var uses = sharing.GetByLabel("Most uses");
            await uses.FillAsync(string.Empty);
            await scene.TypeAsync(uses, "10", delay: 90);
        });

        await scene.StepAsync("share-3", async () =>
        {
            await scene.TypeAsync(sharing.GetByLabel("Note"), "Weekly operations review", delay: 40);
            var email = sharing.GetByLabel("Email the link to the recipients");
            await scene.HighlightAsync(email.Locator("xpath=ancestor::*[contains(@class,'form-check')][1]"), "Or let the site email it");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(email, pauseAfter: 400);
            await scene.ClickAsync(sharing.Locator("button").Filter(new() { HasTextString = "Create link" }).First, pauseAfter: 1500);
            await scene.HighlightAsync(sharing.Locator("table").First);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("share-4", async () =>
        {
            await scene.ScrollAsync(0, -1200);
            await scene.HighlightAsync(sharing.Locator("p.text-muted").First);
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("share-5", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("[data-report-designer] button").Filter(new() { HasTextString = "Publish" }).First, pauseAfter: 1500);
            var run = scene.Page.Locator("[data-report-designer] a").Filter(new() { HasTextString = "Run report" }).First;
            await scene.ClickAndWaitForNavigationAsync(run);
            await scene.PauseAsync(1000);
            var export = scene.Page.Locator("button.dropdown-toggle").Filter(new() { HasTextString = "Export" }).First;
            await scene.MoveToAsync(export);
            await scene.HighlightAsync(export, "CSV or Excel");
        });

        await scene.ClearHighlightsAsync();
    }
}
