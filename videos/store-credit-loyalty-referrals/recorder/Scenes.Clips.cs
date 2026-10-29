using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static ILocator Fld(Scene scene, string name) => scene.Page.Locator($"[name='{name}']").First;

    private static ILocator Summary(Scene scene) => scene.Page.Locator(".card").Filter(new() { HasTextString = "Order summary" }).First;

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
        await scene.OffCameraAsync(() => scene.Page.WaitForTimeoutAsync(5000));
    }

    // The form of the customer's page that holds a field, as a block to highlight.
    private static ILocator FormOf(Scene scene, string fieldName)
        => scene.Page.Locator("form").Filter(new() { Has = scene.Page.Locator($"[name='{fieldName}']") }).First;

    // The content of an account page: the block that holds its heading.
    private static ILocator Panel(Scene scene, string heading)
        => scene.Page.Locator("h1, h2").Filter(new() { HasTextString = heading }).First.Locator("xpath=..");

    // Opens Ana Rivera's customer page from the list.
    private static async Task OpenAnaAsync(Scene scene)
    {
        await scene.NavigateAsync("/Admin/customers");
        await scene.ClickAndWaitForNavigationAsync(Row(scene, "Ana Rivera").Locator("a").First);
    }

    // Signs the demo customer in on the scene's page, off camera.
    private static async Task SignedInAsync(Scene scene)
    {
        await scene.OffCameraAsync(() => SignInAsync(scene.Page));
    }

    private static async Task CreditAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/customers");
        await scene.StartCaptureAsync();

        await scene.StepAsync("crd-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/customers"), "Commerce > Customers", 2200);
            await scene.HighlightAsync(Row(scene, "Ana Torres"), "A guest: no store credit");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Ana Rivera").Locator("a").First);
        });

        await scene.StepAsync("crd-2", async () =>
        {
            var form = FormOf(scene, "amount");
            await form.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(form, "Store credit");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await ReplaceAsync(scene, Fld(scene, "amount"), "25", 120);
            await scene.HighlightAsync(Fld(scene, "currency"), "Currency");
            await scene.PauseAsync(1500);
            await scene.ClearHighlightsAsync();
            await scene.TypeAsync(form.Locator("[name='reason']"), "Late delivery", delay: 55);
            await SaveAsync(scene, form.Locator("button[type=submit]"));
        });

        await scene.StepAsync("crd-3", async () =>
        {
            var form = FormOf(scene, "amount");
            await form.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(form, "Store credit");
            await scene.PauseAsync(3500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task RefundAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/orders");
        await scene.Page.RunAndWaitForNavigationAsync(() => Row(scene, "ana.rivera@example.com").Locator("a").First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("ref-1", async () =>
        {
            var amount = scene.Page.Locator("#refund-amount");
            await amount.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(amount.Locator("xpath=ancestor::form[1]"), "Refund");
            await scene.PauseAsync(2800);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("ref-2", async () =>
        {
            await ReplaceAsync(scene, scene.Page.Locator("#refund-amount"), "20", 110);
            await scene.TypeAsync(scene.Page.Locator("#refund-reason"), "Goodwill", delay: 60);
            await scene.ClickAsync(scene.Page.Locator("#refund-store-credit"), pauseAfter: 900);
            await scene.HighlightAsync(scene.Page.Locator("#refund-store-credit").Locator("xpath=ancestor::div[contains(@class,'form-check')][1]"), "Give it as store credit");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("#refund-amount").Locator("xpath=ancestor::form[1]").Locator("button[type=submit]").Last, pauseAfter: 1200);
            var confirm = scene.Page.Locator(".modal.show .btn-danger, .modal.show .btn-primary").First;

            try
            {
                await confirm.WaitForAsync(new() { Timeout = 2500 });
                await scene.ClickAsync(confirm, pauseAfter: 500);
            }
            catch (TimeoutException)
            {
            }

            await scene.OffCameraAsync(() => scene.Page.WaitForTimeoutAsync(6000));
            await scene.PauseAsync(2000);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SpendAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        await SignedInAsync(scene);
        await ToPaymentAsync(scene.Page, "Hardcover Notebook", "1");
        await scene.Page.Mouse.MoveAsync(700, 500);

        await scene.StartCaptureAsync();

        var box = scene.Page.Locator(".card").Filter(new() { HasTextString = "store credit" }).Last;

        await scene.StepAsync("spd-1", async () =>
        {
            await scene.HighlightAsync(box, "Store credit");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(box.Locator("button").Filter(new() { HasTextString = "Use my store credit" }).First, pauseAfter: 2500);
        });

        await scene.StepAsync("spd-2", async () =>
        {
            await scene.HighlightAsync(Summary(scene), "Store credit under the total");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
            await scene.Page.RunAndWaitForNavigationAsync(() => scene.Page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
            await scene.NavigateAsync("/account/store-credit");
            await scene.HighlightAsync(Panel(scene, "Store credit"), "My account: Store credit");
            await scene.PauseAsync(3000);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PointsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/loyalty");
        await scene.StartCaptureAsync();

        await scene.StepAsync("pts-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/Settings/loyalty"), "Settings > Commerce > Rewards", 2200);
            await scene.TypeAsync(Fld(scene, "ISite.LoyaltySettings.ProgramName"), "Contoso Rewards", delay: 55);
            await scene.HighlightAsync(Field(scene, "Program name"), "Program name");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "Points per unit spent"), "Points per unit spent");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("pts-2", async () =>
        {
            await scene.HighlightAsync(Field(scene, "What a point is worth"), "What a point is worth");
            await scene.PauseAsync(2800);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Field(scene, "Fewest points to spend"), "Fewest points to spend");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await SaveAsync(scene, SaveButton(scene));
            await scene.PauseAsync(1500);
        });

        await scene.StepAsync("pts-3", async () =>
        {
            await OpenAnaAsync(scene);
            var form = FormOf(scene, "points");
            await form.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(form, "Contoso Rewards");
            await scene.PauseAsync(1800);
            await scene.ClearHighlightsAsync();
            await ReplaceAsync(scene, Fld(scene, "points"), "1500", 100);
            await scene.TypeAsync(form.Locator("[name='reason']"), "Welcome bonus", delay: 55);
            await SaveAsync(scene, form.Locator("button[type=submit]"));
            await form.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(2500);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task PointsSpendAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        await SignedInAsync(scene);
        await scene.OffCameraAsync(() => scene.Page.GotoAsync(BaseUrl + "/account/rewards", new() { WaitUntil = WaitUntilState.NetworkIdle }));
        await scene.StartCaptureAsync();

        await scene.StepAsync("pnt-1", async () =>
        {
            await scene.HighlightAsync(Panel(scene, "Rewards"), "My account: Rewards");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("pnt-2", async () =>
        {
            await scene.OffCameraAsync(() => ToPaymentAsync(scene.Page, "Desk Lamp", "1"));
            await scene.Page.Mouse.MoveAsync(700, 500);
            var box = scene.Page.Locator(".card").Filter(new() { HasTextString = "Contoso Rewards" }).Last;
            await scene.HighlightAsync(box, "Contoso Rewards");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(box.Locator("button").Filter(new() { HasTextString = "Use my points" }).First, pauseAfter: 2500);
            await scene.HighlightAsync(Summary(scene), "Paid with points");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.Page.RunAndWaitForNavigationAsync(() => scene.Page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task SetupAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/referrals");
        await scene.StartCaptureAsync();

        await scene.StepAsync("set-1", async () =>
        {
            await ShowAsync(scene, Menu(scene, "/Admin/Settings/referrals"), "Settings > Commerce > Referrals", 2200);
            await PickAsync(scene, Fld(scene, "ISite.ReferralSettings.FriendCouponId"), "FRIEND15", 900);
            await scene.HighlightAsync(Field(scene, "What a friend gets"), "What a friend gets");
            await scene.PauseAsync(2500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("set-2", async () =>
        {
            await PickAsync(scene, Fld(scene, "ISite.ReferralSettings.RewardKind"), "Store credit", 900);
            await scene.HighlightAsync(Field(scene, "What the customer gets"), "What the customer gets");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await ReplaceAsync(scene, Fld(scene, "ISite.ReferralSettings.MinimumOrderAmount"), "20", 100);
            await SaveAsync(scene, SaveButton(scene));
            await scene.PauseAsync(1500);
        });
    }

    // The path of Ana's referral link, read off her referral page, off camera.
    private static async Task<string> ReferralPathAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new() { ViewportSize = new() { Width = 1920, Height = 1080 } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(90_000);
        await SignInAsync(page);
        await page.GotoAsync(BaseUrl + "/account/refer", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var text = await page.Locator("input[value*='/refer/'], [data-referral-link], code, a[href*='/refer/']").First.EvaluateAsync<string>("e => e.value || e.href || e.textContent");
        await context.CloseAsync();
        var index = text.IndexOf("/refer/", StringComparison.Ordinal);

        return text[index..].Trim();
    }

    private static async Task ReferAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        await SignedInAsync(scene);
        await scene.OffCameraAsync(() => scene.Page.GotoAsync(BaseUrl + "/account/refer", new() { WaitUntil = WaitUntilState.NetworkIdle }));
        await scene.StartCaptureAsync();

        await scene.StepAsync("rfr-1", async () =>
        {
            await scene.HighlightAsync(Panel(scene, "Refer a friend"), "My account: Refer a friend");
            await scene.PauseAsync(5000);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task FriendAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var path = await ReferralPathAsync(browser);

        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        await scene.OffCameraAsync(() => scene.Page.GotoAsync(BaseUrl + "/shop", new() { WaitUntil = WaitUntilState.NetworkIdle }));
        await scene.StartCaptureAsync();

        await scene.StepAsync("rfr-2", async () =>
        {
            await scene.NavigateAsync(path);
            await scene.PauseAsync(3500);
            await scene.OffCameraAsync(() => ToPaymentAsync(scene.Page, "Wireless Earbuds", "1", "Ben Carter", "ben.carter@example.com"));
            await scene.Page.Mouse.MoveAsync(700, 500);
            await scene.HighlightAsync(scene.Page.Locator(".card").Filter(new() { HasTextString = "Promotion code" }).First, "The friend's discount code");
            await scene.PauseAsync(3000);
            await scene.ClearHighlightsAsync();
            await scene.Page.RunAndWaitForNavigationAsync(() => scene.Page.Locator("#checkout-submit").ClickAsync(), new() { WaitUntil = WaitUntilState.NetworkIdle });
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task RewardAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewGuestSceneAsync(browser, directory, durations);

        await SignedInAsync(scene);
        await scene.OffCameraAsync(() => scene.Page.GotoAsync(BaseUrl + "/account/refer", new() { WaitUntil = WaitUntilState.NetworkIdle }));
        await scene.StartCaptureAsync();

        await scene.StepAsync("rfr-3", async () =>
        {
            await scene.HighlightAsync(Panel(scene, "Refer a friend"), "Friends who bought, and the reward");
            await scene.PauseAsync(4000);
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/account/store-credit");
            await scene.HighlightAsync(Panel(scene, "Store credit"), "The reward as store credit");
            await scene.PauseAsync(4000);
        });

        await scene.ClearHighlightsAsync();
    }
}
