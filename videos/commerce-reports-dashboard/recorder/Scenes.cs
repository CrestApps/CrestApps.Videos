using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Commerce reports and dashboard video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-run", RunAsync),
        ("04-tour", TourAsync),
        ("05-dashboard", DashboardAsync),
        ("06-adapt", AdaptAsync),
    ];

    /// <summary>
    /// Turns on Reports (with Excel and PDF exports), the Report Builder and the Admin Dashboard (not recorded). The site is the one the
    /// Orders video prepared: a shop with fake orders paid today.
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var page = await Seed.NewPageAsync(browser);

        await Seed.EnableAsync(page, "CrestApps.OrchardCore.Reports", "CrestApps.OrchardCore.Reports.OpenXml", "CrestApps.OrchardCore.Reports.Pdf", "OrchardCore.AdminDashboard", "CrestApps.OrchardCore.Reports.Builder");
        await page.Context.CloseAsync();
    }

    private static async Task WaitAsync(Scene scene, int millis = 4000)
        => await scene.OffCameraAsync(() => scene.Page.WaitForTimeoutAsync(millis));

    private static async Task RunAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Reports/view/order-sales");
        await scene.StartCaptureAsync();

        await scene.StepAsync("run-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("h1:visible").First, "Reports > Commerce > Sales");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            var picker = scene.Page.Locator("button.form-select").Filter(new() { HasTextString = "From" }).First;
            await scene.ClickAsync(picker, pauseAfter: 1500);
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show").First, "Presets, or your own dates");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("run-2", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".dropdown-menu.show a, .dropdown-menu.show button, .dropdown-menu.show label, .dropdown-menu.show li").Filter(new() { HasTextString = "Last 30 Days" }).First, pauseAfter: 800);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Show" }).First);
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.GetByText("Summary").First.Locator("xpath=ancestor::*[self::section or self::div][2]"), "Orders, average order value, discounts, shipping, tax and refunds");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("run-3", async () =>
        {
            await scene.ScrollAsync(0, 700);
            await scene.PauseAsync(3500);
            await scene.ScrollAsync(0, -1400);
            await scene.PauseAsync(800);
        });

        await scene.StepAsync("run-4", async () =>
        {
            var export = scene.Page.Locator("button").Filter(new() { HasTextString = "Export" }).First;
            await scene.ClickAsync(export, pauseAfter: 1200);
            await scene.HighlightAsync(scene.Page.Locator(".dropdown-menu.show").Last, "CSV, Excel and PDF");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();

            try
            {
                await scene.Page.RunAndWaitForDownloadAsync(
                    () => scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Export CSV" }).First, pauseAfter: 500),
                    new() { Timeout = 20000 });
            }
            catch (TimeoutException)
            {
            }

            await scene.PauseAsync(2500);
        });
    }

    private static async Task VisitAsync(Scene scene, string report, string caption, int hold = 4200)
    {
        await scene.NavigateAsync($"/Admin/Reports/view/{report}");
        await scene.PauseAsync(1200);
        await scene.HighlightAsync(scene.Page.Locator("h1:visible").First, caption);
        await scene.PauseAsync(hold);
        await scene.ClearHighlightsAsync();
    }

    private static async Task TourAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Reports/view/order-product-sales");
        await scene.StartCaptureAsync();

        await scene.StepAsync("tour-1", async () =>
        {
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.Locator("h1:visible").First, "Sales by product");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await VisitAsync(scene, "order-category-sales", "Sales by category");
        });

        await scene.StepAsync("tour-2", async () =>
        {
            await VisitAsync(scene, "order-customers", "Customers");
            await VisitAsync(scene, "order-cohorts", "Cohorts and retention");
            await VisitAsync(scene, "checkout-conversion", "Checkouts");
        });

        await scene.StepAsync("tour-3", async () =>
        {
            await VisitAsync(scene, "order-country-sales", "Sales by country");
            await VisitAsync(scene, "order-tax", "Tax on orders");
            await VisitAsync(scene, "order-vat-oss", "VAT one-stop shop (OSS)");
        });

        await scene.StepAsync("tour-4", async () =>
        {
            await VisitAsync(scene, "order-discounts", "Discounts on orders");
            await VisitAsync(scene, "order-refunds", "Refunds");
            await VisitAsync(scene, "order-disputes", "Disputes");
        });

        await scene.StepAsync("tour-5", async () =>
        {
            await VisitAsync(scene, "unfulfilled-orders", "Orders to ship", 5000);
        });

        await scene.StepAsync("tour-6", async () =>
        {
            await VisitAsync(scene, "stock-on-hand", "Stock on hand");
            await VisitAsync(scene, "stock-movements", "Stock movements");
        });

        await scene.StepAsync("tour-7", async () =>
        {
            await VisitAsync(scene, "shipping-performance", "Shipping times", 5000);
        });
    }

    private static async Task AddWidgetAsync(Scene scene, string widget, string title, Func<Task> more = null)
    {
        await scene.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await scene.PauseAsync(800);
        await scene.Page.EvaluateAsync("() => document.querySelectorAll('.message, .toast, .alert, [id^=notifications], .notification').forEach(e => { if (/published|saved/i.test(e.textContent)) { e.remove(); } })");
        var add = scene.Page.Locator("button, a").Filter(new() { HasTextString = "Add Widget" }).First;
        await scene.ClickAsync(add, pauseAfter: 1000);

        if (await scene.Page.Locator(".dropdown-menu.show").CountAsync() == 0)
        {
            await scene.ClickAsync(add, pauseAfter: 1000);
        }

        var item = scene.Page.Locator(".dropdown-menu.show a.dropdown-item").Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex("^\\s*" + System.Text.RegularExpressions.Regex.Escape(widget) + "\\s*$") }).First;
        try { await item.WaitForAsync(new() { Timeout = 8000 }); } catch (TimeoutException) { await scene.Page.ScreenshotAsync(new() { Path = "C:/cavideo/commerce-1/dbg.png" }); throw; }
        await item.ScrollIntoViewIfNeededAsync();
        await scene.ClickAndWaitForNavigationAsync(item);
        await scene.Page.GetByLabel("Width", new() { Exact = true }).First.FillAsync("4");
        await scene.TypeAsync(scene.Page.GetByLabel("Title", new() { Exact = true }).First, title, delay: 50);

        if (more is not null)
        {
            await more();
        }

        await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button.publish").First);
        await scene.PauseAsync(800);
    }

    private static async Task DashboardAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/dashboard/manage");
        await scene.StartCaptureAsync();

        await scene.StepAsync("dash-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("button, a").Filter(new() { HasTextString = "Add Widget" }).First, pauseAfter: 1200);
            var menu = scene.Page.Locator(".dropdown-menu.show").Last;
            await scene.HighlightAsync(menu, "A widget for each report");
            await scene.PauseAsync(1500);
            await menu.EvaluateAsync("el => el.scrollTo({ top: el.scrollHeight, behavior: 'smooth' })");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.Page.Keyboard.PressAsync("Escape");
        });

        await scene.StepAsync("dash-2", async () =>
        {
            await AddWidgetAsync(scene, "Sales Overview", "Sales, last 30 days", async () =>
            {
                await scene.HighlightAsync(scene.Page.GetByLabel("Period", new() { Exact = true }).First, "The period moves with the clock");
                await scene.PauseAsync(2500);
                await scene.ClearHighlightsAsync();
                await scene.HighlightAsync(scene.Page.GetByLabel("Keep for (minutes)").First, "How long it keeps what it shows");
                await scene.PauseAsync(2500);
                await scene.ClearHighlightsAsync();
            });
        });

        await scene.StepAsync("dash-3", async () =>
        {
            await AddWidgetAsync(scene, "Top Selling Products", "Best sellers");
        });

        await scene.StepAsync("dash-4", async () =>
        {
            await AddWidgetAsync(scene, "Report", "Orders to ship", async () =>
            {
                var report = scene.Page.GetByLabel("Report", new() { Exact = true }).First;
                await scene.Page.EvaluateAsync("() => { const s = [...document.querySelectorAll('select')].find(x => x.name.toLowerCase().includes('report')); if (s) { const o = [...s.options].find(o => /Orders to ship/.test(o.text)); if (o) { s.value = o.value; s.dispatchEvent(new Event('change', { bubbles: true })); } } }");
                await scene.HighlightAsync(report, "Any report, with its period and what to show");
                await scene.PauseAsync(3000);
                await scene.ClearHighlightsAsync();
            });
        });

        await scene.StepAsync("dash-5", async () =>
        {
            await scene.NavigateAsync("/Admin");
            await WaitAsync(scene, 3000);
            await scene.PauseAsync(2500);
            await scene.ScrollAsync(0, 500);
            await scene.PauseAsync(3500);
        });
    }

    private static async Task AdaptAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Recipes");
        await scene.StartCaptureAsync();

        await scene.StepAsync("ada-1", async () =>
        {
            var run = scene.Page.Locator("#btn-run-CommerceReportBuilderReports");
            await run.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(600);
            await scene.HighlightAsync(run.Locator("xpath=ancestor::tr[1] | ancestor::li[1]").First, "Reports: store reports to adapt");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(run, pauseAfter: 1000);

            var confirm = scene.Page.Locator(".modal.show .btn-danger, .modal.show .btn-primary").First;

            try
            {
                await confirm.WaitForAsync(new() { Timeout = 4000 });
                await scene.ClickAsync(confirm, pauseAfter: 500);
            }
            catch (TimeoutException)
            {
            }

            await WaitAsync(scene, 8000);
        });

        await scene.StepAsync("ada-2", async () =>
        {
            await scene.NavigateAsync("/Admin/reports/designs");
            await scene.PauseAsync(2000);
            await scene.HighlightAsync(scene.Page.Locator("h1:visible").First, "Report Builder: the store reports");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("ada-3", async () =>
        {
            var run = Row(scene, "Sales by day").Locator("a, button").Filter(new() { HasTextString = "Run" }).First;
            await scene.HighlightAsync(run, "Run it, or Edit it to change it");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(run);
            await WaitAsync(scene, 4000);
            await scene.PauseAsync(3500);
        });
    }
}
