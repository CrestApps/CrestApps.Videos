using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the Set up your store video, one method per "clip" of storyboard.json.
/// </summary>
public static partial class Scenes
{
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("03-guide", GuideAsync),
        ("04-details", DetailsAsync),
        ("05-payments", PaymentsAsync),
        ("06-ship", ShipAsync),
        ("07-first", FirstAsync),
        ("08-roles", RolesAsync),
        ("09-theme", ThemeAsync),
    ];

    /// <summary>
    /// Nothing to prepare: the site is a brand new one made with the Online store recipe (site name Contoso, SQLite, user admin),
    /// and every step of the guide is made on camera.
    /// </summary>
    public static Task PrepareAsync(IBrowser browser) => Task.CompletedTask;

    private const string SetupPath = "/Admin/commerce/setup";

    private static ILocator Fld(Scene scene, string name) => scene.Page.Locator($"[name='{name}']").First;

    private static ILocator Step(Scene scene, string title) => scene.Page.Locator(".accordion-item").Filter(new() { HasTextString = title }).First;

    // Opens a step of the guide, unless it is open already.
    private static async Task OpenStepAsync(Scene scene, string title)
    {
        var header = Step(scene, title).Locator(".accordion-button").First;

        if (await header.GetAttributeAsync("aria-expanded") != "true")
        {
            await scene.ClickAsync(header, pauseAfter: 1200);
        }
    }

    private static async Task ReplaceAsync(Scene scene, ILocator field, string text, int delay = 60)
    {
        await scene.ClickAsync(field, pauseAfter: 200);
        await scene.Page.Keyboard.PressAsync("Control+A");
        await field.PressSequentiallyAsync(text, new() { Delay = delay });
        await scene.PauseAsync(300);
    }

    private static async Task PickAsync(Scene scene, ILocator select, string contains, int pauseAfter = 700)
    {
        await scene.ClickAsync(select, pauseAfter: 300);
        await select.EvaluateAsync("(select, text) => { const option = [...select.options].find(o => o.text.includes(text)); select.value = option.value; select.dispatchEvent(new Event('change', { bubbles: true })); }", contains);
        await scene.PauseAsync(pauseAfter);
    }

    private static async Task SaveAsync(Scene scene, ILocator button)
    {
        await scene.ClickAsync(button, pauseAfter: 500);
        await scene.OffCameraAsync(() => scene.Page.WaitForLoadStateAsync(LoadState.NetworkIdle));
        await scene.OffCameraAsync(() => scene.Page.WaitForTimeoutAsync(1500));
    }

    private static async Task GuideAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin");
        await scene.StartCaptureAsync();

        await scene.StepAsync("gui-1", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator("#left-nav figcaption#commerce"), pauseAfter: 1000);
            await ShowAsync(scene, Menu(scene, SetupPath), "Commerce > Set up your store", 2200);
            await scene.NavigateAsync(SetupPath);
            await scene.HighlightAsync(scene.Page.GetByText("steps done").First, "How many steps are done");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("gui-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".accordion").First, "Each step: a form or a link");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Step(scene, "Take bank transfers").Locator(".accordion-button").First, "Optional steps say so");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task DetailsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync(SetupPath);
        await scene.StartCaptureAsync();

        await scene.StepAsync("det-1", async () =>
        {
            await scene.TypeAsync(Fld(scene, "setup.name"), "Contoso", delay: 70);
            await scene.TypeAsync(Fld(scene, "setup.email"), "hello@example.com", delay: 40);
            await scene.TypeAsync(Fld(scene, "setup.phone"), "555-0100", delay: 70);
            await scene.TypeAsync(Fld(scene, "setup.address"), "12 Harbor Road, Portland", delay: 35);
            await SaveAsync(scene, scene.Page.Locator("button").Filter(new() { HasTextString = "Save the details" }).First);
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("det-2", async () =>
        {
            await OpenStepAsync(scene, "Your currency");
            await PickAsync(scene, Fld(scene, "setup.currency"), "US Dollar", 900);
            await SaveAsync(scene, scene.Page.Locator("button").Filter(new() { HasTextString = "Use this currency" }).First);
            await scene.PauseAsync(2000);
        });
    }

    private static async Task PaymentsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync(SetupPath);
        await scene.StartCaptureAsync();

        await scene.StepAsync("pay-1", async () =>
        {
            await OpenStepAsync(scene, "Take card payments");
            await scene.HighlightAsync(Step(scene, "Take card payments"), "Take card payments");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("pay-2", async () =>
        {
            await OpenStepAsync(scene, "Take bank transfers");
            await scene.TypeAsync(Fld(scene, "setup.instructions"), "Transfer to Contoso Bank, account 000-0000, and put your order number in the reference.", delay: 25);
            await scene.HighlightAsync(Step(scene, "Take bank transfers"), "Take bank transfers");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene, scene.Page.Locator("button").Filter(new() { HasTextString = "Take bank transfers" }).Last);
            await scene.PauseAsync(2000);
        });
    }

    private static async Task ShipAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync(SetupPath);
        await scene.StartCaptureAsync();

        await scene.StepAsync("shp-1", async () =>
        {
            await OpenStepAsync(scene, "Ship your products");
            await ReplaceAsync(scene, Fld(scene, "setup.amount"), "5", 120);
            await scene.HighlightAsync(Step(scene, "Ship your products"), "One rate for every order");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene, scene.Page.Locator("button").Filter(new() { HasTextString = "Ship everywhere at this rate" }).First);
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("shp-2", async () =>
        {
            await OpenStepAsync(scene, "Charge tax");
            await scene.HighlightAsync(Step(scene, "Charge tax"), "Charge tax");
            await scene.PauseAsync(2800);
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene, Step(scene, "Charge tax").Locator("button").Filter(new() { HasTextString = "Skip this step" }).First);
            await OpenStepAsync(scene, "Charge tax");
            await scene.HighlightAsync(Step(scene, "Charge tax").Locator("a, button").Filter(new() { HasTextString = "Do this step after all" }).First, "Do this step after all");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });
    }

    private static async Task FirstAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync(SetupPath);
        await scene.StartCaptureAsync();

        await scene.StepAsync("fst-1", async () =>
        {
            await OpenStepAsync(scene, "Add your first product");
            await scene.HighlightAsync(Step(scene, "Add your first product"), "Add your first product");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/Contents/ContentTypes/Product/Create");
            await scene.TypeAsync(Fld(scene, "TitlePart.Title"), "Ceramic Mug", delay: 60);
            await ReplaceAsync(scene, Fld(scene, "ProductPart.Price"), "18", 120);
            await scene.PauseAsync(800);
            await scene.ClickAsync(Publish(scene), pauseAfter: 500);
            await scene.OffCameraAsync(() => scene.Page.WaitForTimeoutAsync(6000));
        });

        await scene.StepAsync("fst-2", async () =>
        {
            await scene.NavigateAsync("/");
            await scene.HighlightAsync(scene.Page.Locator(".card").Filter(new() { HasTextString = "Ceramic Mug" }).First, "On sale once it is published");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(() => ToPaymentAsync(scene.Page, "Ceramic Mug"));
            await scene.Page.Mouse.MoveAsync(700, 500);
            await scene.ClickAsync(scene.Page.Locator("label, .form-check, div").Filter(new() { HasTextString = "Bank transfer" }).Last, pauseAfter: 1500);
            await scene.Page.RunAndWaitForNavigationAsync(() => scene.Page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
            await scene.Page.Mouse.MoveAsync(700, 500);
            await scene.HighlightAsync(scene.Page.Locator(".card").Filter(new() { HasTextString = "Order summary" }).First, "A test order");
            await scene.PauseAsync(3000);
        });

        await scene.ClearHighlightsAsync();
    }

    // Puts a product in the cart and goes through the checkout steps up to the payment step.
    private static async Task ToPaymentAsync(IPage page, string product)
    {
        await page.GotoAsync(BaseUrl + "/", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var card = page.Locator(".card").Filter(new() { HasTextString = product }).First;
        await card.Locator("button:has-text('Add to cart')").ClickAsync();
        await page.WaitForTimeoutAsync(1500);
        await page.GotoAsync(BaseUrl + "/cart", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.RunAndWaitForNavigationAsync(() => page.Locator(".card").Filter(new() { HasTextString = "Summary" }).Locator("a, button").Filter(new() { HasTextString = "Checkout" }).First.ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });

        for (var step = 0; step < 4; step++)
        {
            var submit = page.Locator("#checkout-submit");

            if (await submit.CountAsync() == 0 || (await submit.InnerTextAsync()).Contains("Place", StringComparison.OrdinalIgnoreCase) || (await submit.InnerTextAsync()).Contains("Pay", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (await page.GetByLabel("Full name").CountAsync() > 0 && await page.GetByLabel("Full name").First.IsVisibleAsync())
            {
                if (string.IsNullOrEmpty(await page.GetByLabel("Full name").First.InputValueAsync()))
                {
                    await page.GetByLabel("Full name").First.FillAsync("Jamie Chen");
                }

                if (await page.GetByLabel("Email").CountAsync() > 0 && string.IsNullOrEmpty(await page.GetByLabel("Email").First.InputValueAsync()))
                {
                    await page.GetByLabel("Email").First.FillAsync("customer@example.com");
                }
            }

            if (await page.Locator("#Shipping_Address_Country").CountAsync() > 0 && await page.Locator("#Shipping_Address_Country").IsVisibleAsync())
            {
                await page.Locator("#Shipping_Address_Country").SelectOptionAsync(new SelectOptionValue { Label = "United States" });
                await page.WaitForTimeoutAsync(1200);
                await page.Locator("#Shipping_Address_Name").FillAsync("Jamie Chen");
                await page.Locator("#Shipping_Address_AddressLine1").FillAsync("350 Fifth Avenue");
                await page.Locator("#Shipping_Address_City").FillAsync("New York");
                await page.Locator("#Shipping_Address_Region").SelectOptionAsync(new SelectOptionValue { Label = "New York" });
                await page.Locator("#Shipping_Address_PostalCode").FillAsync("10118");
            }

            if (await page.Locator("input[type=radio]").CountAsync() > 0 && await page.Locator("input[type=radio]").First.IsVisibleAsync() && await page.Locator("input[type=radio]:checked").CountAsync() == 0)
            {
                await page.Locator("input[type=radio]").First.CheckAsync();
            }

            await page.RunAndWaitForNavigationAsync(() => submit.ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        }
    }

    private static async Task RolesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync(SetupPath);
        await scene.StartCaptureAsync();

        await scene.StepAsync("rol-1", async () =>
        {
            await OpenStepAsync(scene, "Add roles for your staff");
            await scene.HighlightAsync(Step(scene, "Add roles for your staff"), "Add roles for your staff");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene, scene.Page.Locator("button").Filter(new() { HasTextString = "Add the staff roles" }).First);
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("rol-2", async () =>
        {
            await scene.NavigateAsync("/Admin/Roles/Index");
            await scene.HighlightAsync(scene.Page.Locator("li, tr").Filter(new() { HasTextString = "Store manager" }).Last, "Store manager");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("li, tr").Filter(new() { HasTextString = "Warehouse" }).Last, "Warehouse");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("li, tr").Filter(new() { HasTextString = "Marketing" }).Last, "Marketing and Customer support");
            await scene.PauseAsync(2500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ThemeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/");
        await scene.StartCaptureAsync();

        await scene.StepAsync("thm-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("nav.navbar, .navbar").First, "The header");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("thm-2", async () =>
        {
            await scene.ScrollAsync(0, 2500);
            await scene.HighlightAsync(scene.Page.Locator("footer").First, "The footer");
            await scene.PauseAsync(4500);
        });

        await scene.ClearHighlightsAsync();
    }
}
