using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Shipments and labels video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-create", CreateAsync),
        ("04-follow", FollowAsync),
        ("05-easypost", EasyPostAsync),
        ("06-track", TrackAsync),
    ];

    /// <summary>
    /// Nothing to prepare: the site is the one the Shipping video prepared, with the four fake orders of the Orders video
    /// (this video ships Ana Torres's order, 261029-RAZ0B9) and the EasyPost feature on. Nothing is sent to EasyPost.
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var page = await Seed.NewPageAsync(browser);

        await Seed.EnableAsync(page, "CrestApps.OrchardCore.Shipping.EasyPost");
        await page.Context.CloseAsync();
    }

    private static ILocator Fld(Scene scene, string name) => scene.Page.Locator($"[name='{name}']").First;

    private static ILocator Tab(Scene scene, string name) => scene.Page.Locator("a.nav-link, button.nav-link").Filter(new() { HasTextString = name }).First;

    private static async Task ReplaceAsync(Scene scene, ILocator field, string text, int delay = 60)
    {
        await scene.ClickAsync(field, pauseAfter: 200);
        await scene.Page.Keyboard.PressAsync("Control+A");
        await field.PressSequentiallyAsync(text, new() { Delay = delay });
        await scene.PauseAsync(300);
    }

    private static async Task SaveAsync(Scene scene, ILocator button)
    {
        await scene.ClickAsync(button, pauseAfter: 500);
        await scene.OffCameraAsync(() => scene.Page.WaitForLoadStateAsync(LoadState.NetworkIdle));
        await scene.OffCameraAsync(() => scene.Page.WaitForTimeoutAsync(2000));
    }

    // Opens Ana Torres's order from the orders list.
    private static async Task OpenOrderAsync(Scene scene)
    {
        await scene.Page.RunAndWaitForNavigationAsync(() => Row(scene, "ana.torres@example.com").Locator("a").First.ClickAsync());
    }

    private static ILocator Shipments(Scene scene) => scene.Page.Locator("div.card, section").Filter(new() { HasTextString = "Shipments" }).Filter(new() { HasTextString = "New shipment" }).Last;

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/orders");
        await OpenOrderAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("crt-1", async () =>
        {
            var lines = Fld(scene, "Shipment.Lines[1].Quantity");
            await lines.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(lines.Locator("xpath=ancestor::table[1]"), "New shipment: how many of each item");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await ReplaceAsync(scene, lines, "1", 120);
        });

        await scene.StepAsync("crt-2", async () =>
        {
            await scene.TypeAsync(Fld(scene, "Shipment.Carrier"), "UPS", delay: 90);
            await scene.TypeAsync(Fld(scene, "Shipment.TrackingNumber"), "1Z999AA10123456784", delay: 45);
            await scene.TypeAsync(Fld(scene, "Shipment.TrackingUrl"), "https://example.com/track/1Z999AA10123456784", delay: 25);
            await scene.TypeAsync(Fld(scene, "Shipment.Note"), "Packed by Sam", delay: 50);
            await scene.HighlightAsync(Fld(scene, "Shipment.Ship").Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"), "It has left: mark it shipped now");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene, scene.Page.Locator("button").Filter(new() { HasTextString = "Create shipment" }).First);
        });

        await scene.StepAsync("crt-3", async () =>
        {
            var section = scene.Page.Locator("h5, h6, h4, div").Filter(new() { HasTextString = "UPS" }).Last;
            await section.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(section, "The shipment, with its tracking link");
            await scene.PauseAsync(3500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task FollowAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/orders");
        await OpenOrderAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("fol-1", async () =>
        {
            var ship = Fld(scene, "Shipment.Ship");
            await ship.ScrollIntoViewIfNeededAsync();
            await scene.TypeAsync(Fld(scene, "Shipment.Carrier"), "UPS", delay: 90);
            await scene.ClickAsync(ship, pauseAfter: 900);
            await scene.HighlightAsync(ship.Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"), "Untick it: Being packed");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene, scene.Page.Locator("button").Filter(new() { HasTextString = "Create shipment" }).First);
            var packing = scene.Page.Locator("li, div, tr").Filter(new() { HasTextString = "Being packed" }).Last;
            await packing.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(packing, "Being packed, with a packing slip");
            await scene.PauseAsync(2800);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("fol-2", async () =>
        {
            var markShipped = scene.Page.Locator("button").Filter(new() { HasTextString = "Mark shipped" }).First;
            await scene.HighlightAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Remove" }).First, "Remove, while it is being packed");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene, markShipped);
        });

        await scene.StepAsync("fol-3", async () =>
        {
            var delivered = scene.Page.Locator("button").Filter(new() { HasTextString = "Delivered" }).First;
            await delivered.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Returned" }).First, "Returned: send it again");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene, delivered);
            await scene.PauseAsync(2000);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task EasyPostAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/shipping");
        await scene.StartCaptureAsync();

        await scene.StepAsync("eas-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/Settings/shipping"), "Settings > Commerce > Shipping", 2000);
            await scene.ClickAsync(Tab(scene, "EasyPost"), pauseAfter: 1200);
            await scene.HighlightAsync(scene.Page.Locator(".tab-pane.active, .tab-pane.show").First, "EasyPost");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Tab(scene, "Sending parcels"), pauseAfter: 1200);
            await scene.HighlightAsync(scene.Page.Locator(".tab-pane.active, .tab-pane.show").First, "Where you ship from, and the default box");
            await scene.PauseAsync(3500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task TrackAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        // The site's links carry the address it was set up with: follow them on the demo site.
        await scene.Page.Context.RouteAsync("**/orders/track", async route =>
        {
            if (route.Request.Method != "POST")
            {
                await route.ContinueAsync();

                return;
            }

            var response = await route.FetchAsync(new() { MaxRedirects = 0 });
            var headers = response.Headers.ToDictionary(pair => pair.Key, pair => pair.Value);

            if (headers.TryGetValue("location", out var location))
            {
                headers["location"] = location.Replace("localhost:5374", "localhost:5384");
            }

            await route.FulfillAsync(new() { Response = response, Headers = headers });
        });

        await scene.OpenAsync("/orders/track");
        await scene.StartCaptureAsync();

        await scene.StepAsync("trk-1", async () =>
        {
            await scene.TypeAsync(scene.Page.GetByLabel("Order number"), "261029-RAZ0B9", delay: 70);
            await scene.TypeAsync(scene.Page.GetByLabel("Email"), "ana.torres@example.com", delay: 45);
            await scene.HighlightAsync(scene.Page.Locator("form").Last, "Track your order");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button).Last);
        });

        await scene.StepAsync("trk-2", async () =>
        {
            var shipment = scene.Page.Locator("li, div, tr").Filter(new() { HasTextString = "UPS" }).Last;
            await shipment.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(shipment, "The shipment and its tracking link");
            await scene.PauseAsync(4000);
        });

        await scene.ClearHighlightsAsync();
    }
}
