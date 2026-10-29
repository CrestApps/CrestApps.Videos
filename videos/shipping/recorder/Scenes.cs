using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Shipping video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-zones", ZonesAsync),
        ("04-methods", MethodsAsync),
        ("05-settings", SettingsAsync),
        ("06-pickup", PickupAsync),
        ("07-try", TryAsync),
        ("08-ship", ShipAsync),
    ];

    /// <summary>
    /// Turns on the EasyPost feature (not recorded). The site is the one the Orders video prepared: the shop, four fake orders,
    /// and a shipping zone Everywhere with a Standard shipping and an Express shipping flat rate.
    /// </summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var page = await Seed.NewPageAsync(browser);

        await Seed.EnableAsync(page, "CrestApps.OrchardCore.Shipping.EasyPost");
        await page.Context.CloseAsync();
    }

    private static ILocator Fld(Scene scene, string name) => scene.Page.Locator($"[name='{name}']").First;

    private static async Task PickAsync(Scene scene, ILocator select, string contains, int pauseAfter = 700)
    {
        await scene.ClickAsync(select, pauseAfter: 300);
        await select.EvaluateAsync("(select, text) => { const option = [...select.options].find(o => o.text.includes(text)); select.value = option.value; select.dispatchEvent(new Event('change', { bubbles: true })); }", contains);
        await scene.PauseAsync(pauseAfter);
    }

    private static async Task ReplaceAsync(Scene scene, ILocator field, string text, int delay = 60)
    {
        await scene.ClickAsync(field, pauseAfter: 200);
        await scene.Page.Keyboard.PressAsync("Control+A");
        await field.PressSequentiallyAsync(text, new() { Delay = delay });
        await scene.PauseAsync(300);
    }

    private static ILocator Tab(Scene scene, string name) => scene.Page.Locator(".nav-tabs a, .nav-tabs button").Filter(new() { HasTextString = name }).First;

    private static async Task ZonesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/shipping/zones");
        await scene.StartCaptureAsync();

        await scene.StepAsync("zone-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/shipping/zones"), "Shipping", 2200);
            await scene.HighlightAsync(Row(scene, "Everywhere"), "A zone, and its methods");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Link(scene, "Add Shipping Zone"));
        });

        await scene.StepAsync("zone-2", async () =>
        {
            await scene.TypeAsync(Fld(scene, "ShippingZone.Name"), "United States", delay: 55);
            await scene.Page.EvaluateAsync("() => { const s = document.querySelector('[name=\"ShippingZone.Countries\"]'); for (const o of s.options) { o.selected = o.text.trim() === 'United States'; } s.dispatchEvent(new Event('change', { bubbles: true })); }");
            await scene.HighlightAsync(Field(scene, "Countries"), "Countries");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "Rest of the world"), "Rest of the world");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "Position"), "Position");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("zone-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
            await scene.HighlightAsync(Link(scene, "Add method"), "Add method");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task MethodsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/shipping/zones");
        await scene.Page.RunAndWaitForNavigationAsync(() => Row(scene, "United States").Locator("a:has-text('Edit')").First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("meth-1", async () =>
        {
            await scene.ClickAsync(Link(scene, "Add method"), pauseAfter: 3500);
            await scene.HighlightAsync(scene.Page.Locator("a[href$='/methods/create/Flat']").First.Locator("xpath=ancestor::*[contains(@class,'modal') or contains(@class,'dropdown-menu') or contains(@class,'list-group')][1]"), "Method kinds");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/methods/create/Flat']").First);
        });

        await scene.StepAsync("meth-2", async () =>
        {
            await ReplaceAsync(scene, Fld(scene, "ShippingMethod.Name"), "Standard shipping", 45);
            await ReplaceAsync(scene, Fld(scene, "ShippingMethod.Amount"), "5", 90);
            await scene.HighlightAsync(Field(scene, "Amount per item"), "Amount per item");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "On"), "On");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });

        await scene.StepAsync("meth-3", async () =>
        {
            await scene.ClickAsync(Link(scene, "Add method"), pauseAfter: 1200);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/methods/create/Free']").First);
            await ReplaceAsync(scene, Fld(scene, "ShippingMethod.Name"), "Free shipping", 45);
            await ReplaceAsync(scene, Fld(scene, "ShippingMethod.MinimumSubtotal"), "100", 90);
            await scene.HighlightAsync(Field(scene, "Free from a subtotal of"), "Free from a subtotal of");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(SaveButton(scene));
        });
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/Product");
        await scene.Page.RunAndWaitForNavigationAsync(() => Row(scene, "Desk Lamp").Locator("a:has-text('Edit')").First.ClickAsync());
        await ScrollAsync(scene, 0, 3500);
        await scene.StartCaptureAsync();

        await scene.StepAsync("set-1", async () =>
        {
            await scene.HighlightAsync(Field(scene, "Weight (kg)"), "Weight (kg)");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("set-2", async () =>
        {
            await scene.NavigateAsync("/Admin/Settings/shipping");
            await scene.ClickAsync(Tab(scene, "General"), pauseAfter: 900);
            await ShowAsync(scene, Field(scene, "Weight unit"), "Weight unit", 1800);
            await scene.HighlightAsync(Field(scene, "Shipping classes"), "Shipping classes");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("set-3", async () =>
        {
            await ShowAsync(scene, Field(scene, "Tracking links"), "Tracking links", 2400);
            await scene.HighlightAsync(Field(scene, "Email the buyer when a shipment leaves"), "Email the buyer");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PickupAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/shipping");
        await scene.StartCaptureAsync();

        await scene.StepAsync("pick-1", async () =>
        {
            await scene.ClickAsync(Tab(scene, "Store pickup"), pauseAfter: 1000);
            await scene.HighlightAsync(Fld(scene, "ISite.ShippingSettings.PickupLocations[0].Name"), "Name, Fee, address");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("pick-2", async () =>
        {
            await ScrollAsync(scene, 0, -2000);
            await scene.ClickAsync(Tab(scene, "Sending parcels"), pauseAfter: 1000);
            await ShowAsync(scene, Fld(scene, "ISite.ShippingSettings.ShipFromName"), "Where parcels leave from", 2800);
            await ScrollAsync(scene, 0, 600);
            await scene.HighlightAsync(Fld(scene, "ISite.ShippingSettings.PackagingWeight"), "Default box and Box weight");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("pick-3", async () =>
        {
            await ScrollAsync(scene, 0, -2000);
            await scene.ClickAsync(Tab(scene, "EasyPost"), pauseAfter: 1000);
            await scene.HighlightAsync(Field(scene, "Enable EasyPost provider"), "Enable EasyPost provider");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Fld(scene, "ISite.EasyPostSettings.ApiKey"), "API key");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(Tab(scene, "Providers"), pauseAfter: 900);
            await scene.HighlightAsync(Field(scene, "Default carrier provider"), "Default carrier provider");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task TryAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/shipping/rates");
        await scene.StartCaptureAsync();

        await scene.StepAsync("try-1", async () =>
        {
            await PickAsync(scene, Fld(scene, "Country"), "United States");
            await scene.TypeAsync(Fld(scene, "Region"), "NY", delay: 90);
            await PickAsync(scene, Fld(scene, "Currency"), "USD");
            await ReplaceAsync(scene, Fld(scene, "Subtotal"), "120", 90);
            await ReplaceAsync(scene, Fld(scene, "Weight"), "2", 90);
            await ReplaceAsync(scene, Fld(scene, "Items"), "2", 90);
        });

        await scene.StepAsync("try-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Show rates" }).First);
            await scene.PauseAsync(1500);
            await ScrollAsync(scene, 0, 500);
        });
    }

    private static async Task ShipAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/orders");
        await scene.Page.RunAndWaitForNavigationAsync(() => OrderLink(scene, "Marcus Lee").ClickAsync());
        await ScrollAsync(scene, 0, 450);
        await scene.StartCaptureAsync();

        await scene.StepAsync("ship-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("div").Filter(new() { HasTextString = "New shipment" }).Filter(new() { HasTextString = "Left to ship" }).Filter(new() { HasNotTextString = "Refund" }).Last, "New shipment");
        });

        await scene.ClearHighlightsAsync();

        await scene.StepAsync("ship-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("[name='Shipment.Carrier']").First, "UPS", delay: 80);
            await scene.TypeAsync(scene.Page.Locator("[name='Shipment.TrackingNumber']").First, "1Z999AA10123456784", delay: 30);
            await scene.TypeAsync(scene.Page.Locator("[name='Shipment.TrackingUrl']").First, "https://example.com/track/1Z999AA10123456784", delay: 20);
        });

        await scene.StepAsync("ship-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("[name='Shipment.Ship']").First.Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"), "Mark it shipped now");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Create shipment" }).First);
        });
    }

    // The link of a buyer's order in the list.
    private static ILocator OrderLink(Scene scene, string buyer)
        => scene.Page.Locator($"xpath=((//li[contains(., '{buyer}')][.//a[contains(@href,'/Admin/orders/')]])[last()]//a[contains(@href,'/Admin/orders/')])[1]");
}
