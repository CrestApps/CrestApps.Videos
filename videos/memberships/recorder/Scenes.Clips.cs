using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Field(Scene scene, string id) => scene.Page.Locator("#" + id);

    private static ILocator Row(Scene scene, string text) => scene.Page.Locator("li.list-group-item.item, li.list-group-item").Filter(new() { HasTextString = text }).First;

    private static Task ClearAsync(Scene scene) => scene.ClearHighlightsAsync();

    private const string AnnualEdit = "/Admin/Contents/ContentItems/4mmbrplan0annual000000000a/Edit";

    private const string GuideEdit = "/Admin/Contents/ContentItems/4mmbrartcl0trailguides000a/Edit";

    // ----- Clips -----

    private static async Task SellAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Recipes");
        await scene.StartCaptureAsync();

        await scene.StepAsync("sell-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Memberships sample").First, "Memberships sample");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await scene.ClickAsync(scene.Page.Locator("#btn-run-MembershipsSample"), pauseAfter: 1500);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#modalOkButton"));
            await scene.PauseAsync(2500);
        });

        await scene.StepAsync("sell-2", async () =>
        {
            await scene.NavigateAsync("/Admin/Roles");
            await scene.HighlightAsync(Row(scene, "Member"), "The Member role");
        });

        await ClearAsync(scene);

        await scene.StepAsync("sell-3", async () =>
        {
            await scene.NavigateAsync(AnnualEdit);
            await scene.Page.GetByText("Days a membership lasts").First.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(800);
            await scene.HighlightAsync(scene.Page.GetByLabel("Member", new() { Exact = true }).First, "The roles members are in");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByLabel("Days a membership lasts"), "Days a membership lasts");
            await scene.PauseAsync(2000);
        });
    }

    private static async Task MembersOnlyAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync(GuideEdit);
        await scene.StartCaptureAsync();

        await scene.StepAsync("mem-1", async () =>
        {
            await scene.Page.GetByText("Who can see it").First.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(800);
            await scene.HighlightAsync(scene.Page.GetByText("Who can see it").First, "Who can see it");
            await scene.PauseAsync(5000);
            await ClearAsync(scene);
            await scene.PauseAsync(2000);
        });
    }

    private static async Task GuestAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, State("guest"));

        await scene.OpenAsync("/members/trail-guide");
        await scene.StartCaptureAsync();

        await scene.StepAsync("mem-2", async () =>
        {
            await scene.PauseAsync(3500);
            await scene.HighlightAsync(scene.Page.Locator("main, body").First, "Asked to sign in");
            await scene.PauseAsync(3000);
        });
    }

    private static async Task BuyAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, State("jamie"));

        await scene.OpenAsync("/membership/annual-membership");
        await scene.StartCaptureAsync();

        await scene.StepAsync("buy-1", async () =>
        {
            await scene.PauseAsync(800);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Add to cart" }).First, "Add to cart");
            await scene.PauseAsync(1500);
            await ClearAsync(scene);
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Add to cart" }).First, pauseAfter: 1500);
            await scene.NavigateAsync("/cart");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Checkout" }).First);
        });

        await scene.StepAsync("buy-2", async () =>
        {
            await scene.Page.Locator("#Contact_Name").FillAsync("Jamie Chen");
            await scene.Page.Locator("#Contact_Email").FillAsync("jamie.chen@example.com");
            await scene.Page.Locator("#Contact_BillingAddress_AddressLine1").FillAsync("1 Pioneer Square");
            await scene.Page.Locator("#Contact_BillingAddress_City").FillAsync("Seattle");
            await scene.Page.Locator("#Contact_BillingAddress_Region").SelectOptionAsync("WA");
            await scene.Page.Locator("#Contact_BillingAddress_PostalCode").FillAsync("98104");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("#checkout-submit"));
            await scene.ClickAsync(scene.Page.GetByText("Bank transfer", new() { Exact = true }).First, pauseAfter: 1200);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Pay now" }));
            await scene.PauseAsync(1500);
        });
    }

    private static async Task MemberAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        // The money for Jamie's transfer has arrived, so the order is paid and the membership starts.
        var admin = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var adminPage = await admin.NewPageAsync();
        adminPage.SetDefaultTimeout(60_000);
        await MarkDueOrderPaidAsync(adminPage);
        await admin.CloseAsync();

        // Jamie signs in again, so the new role applies to the session.
        await SaveSignInAsync(browser, "jamie");

        await using var scene = await NewSceneAsync(browser, directory, durations, State("jamie"));

        await scene.OpenAsync("/account");
        await scene.StartCaptureAsync();

        await scene.StepAsync("member-1", async () =>
        {
            Console.WriteLine("ACCOUNT LINKS: " + string.Join(" ; ", await scene.Page.Locator("a[href^='/account']").EvaluateAllAsync<string[]>("els => els.map(e => e.textContent.trim() + ' ' + e.getAttribute('href'))")));
            await scene.HighlightAsync(scene.Page.Locator("a[href*='memberships']").First, "Memberships");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='memberships']").First);
            await scene.PauseAsync(3000);
        });

        await scene.StepAsync("member-2", async () =>
        {
            await scene.NavigateAsync("/members/trail-guide");
            await scene.PauseAsync(3500);
        });
    }

    private static async Task ManageMembershipsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/memberships");
        await scene.StartCaptureAsync();

        await scene.StepAsync("manage-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/memberships"), "Commerce > Memberships");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.HighlightAsync(Row(scene, "Annual Membership"), "Who, which membership, and when it ends");
            await scene.PauseAsync(4000);
            await ClearAsync(scene);
        });

        await scene.StepAsync("manage-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = "Annual Membership" }).First.GetByRole(AriaRole.Link, new() { Name = "End now" }), "End now");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = "Annual Membership" }).First.GetByRole(AriaRole.Link, new() { Name = "End now" }));
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = "Annual Membership" }).First, "Ended early");
            await scene.PauseAsync(2500);
        });
    }

    private static async Task AfterAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await SaveSignInAsync(browser, "jamie");

        await using var scene = await NewSceneAsync(browser, directory, durations, State("jamie"));

        await scene.OpenAsync("/account/memberships");
        await scene.StartCaptureAsync();

        await scene.StepAsync("after-1", async () =>
        {
            await scene.PauseAsync(2500);
            await scene.HighlightAsync(scene.Page.GetByText("Ended early").First, "Ended early");
            await scene.PauseAsync(3000);
        });
    }
}
