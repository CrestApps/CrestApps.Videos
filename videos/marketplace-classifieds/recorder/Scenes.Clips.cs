using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Row(Scene scene, string text) => scene.Page.Locator("li.list-group-item.item").Filter(new() { HasTextString = text }).First;

    private static ILocator Labeled(Scene scene, string label) => scene.Page.GetByLabel(label).First;

    private static ILocator LinkNamed(Scene scene, string name) => scene.Page.GetByRole(AriaRole.Link, new() { Name = name, Exact = true }).First;

    private static Task ClearAsync(Scene scene) => scene.ClearHighlightsAsync();

    private static string State(string user) => Path.Combine(Root, $"state-{user}.json");

    // ----- Clips -----

    private static async Task MarketSettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/marketplace");
        await scene.StartCaptureAsync();

        await scene.StepAsync("mset-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/marketplace"), "Settings > Commerce > Marketplace");
            await scene.PauseAsync(2600);
            await ClearAsync(scene);
            await scene.HighlightAsync(Labeled(scene, "Commission (%)"), "Commission (%)");
        });

        await ClearAsync(scene);

        await scene.StepAsync("mset-2", async () =>
        {
            await scene.HighlightAsync(Labeled(scene, "Days held"), "Days held");
        });

        await ClearAsync(scene);

        await scene.StepAsync("mset-3", async () =>
        {
            await scene.HighlightAsync(Labeled(scene, "Hold a sale of goods until its order is fulfilled"), "Hold a sale of goods until its order is fulfilled");
        });

        await ClearAsync(scene);

        await scene.StepAsync("mset-4", async () =>
        {
            await scene.HighlightAsync(Labeled(scene, "Pay vendors out by themselves"), "Pay vendors out by themselves");
            await scene.PauseAsync(4500);
            await ClearAsync(scene);
            await scene.HighlightAsync(Labeled(scene, "Vendors' country"), "Vendors' country");
        });
    }

    private static async Task VendorAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/vendors");
        await scene.StartCaptureAsync();

        await scene.StepAsync("vend-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/vendors"), "Commerce > Vendors");
            await scene.PauseAsync(2400);
            await ClearAsync(scene);
            await scene.HighlightAsync(Row(scene, "Clayworks Studio"), "A vendor, and whether it can be paid");
            await scene.PauseAsync(2800);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Add vendor"));
        });

        await scene.StepAsync("vend-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#Name"), "Maple & Thread", delay: 50);
            await scene.TypeAsync(scene.Page.Locator("#Email"), "hello@maplethread.example", delay: 35);
            await scene.HighlightAsync(scene.Page.Locator("#Email"), "Email");
        });

        await ClearAsync(scene);

        await scene.StepAsync("vend-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#UserNames"), "dana", delay: 70);
            await scene.HighlightAsync(scene.Page.Locator("#UserNames"), "People who run it");
        });

        await ClearAsync(scene);

        await scene.StepAsync("vend-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#CommissionPercent"), "Commission (%)");
            await scene.PauseAsync(2800);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator("#Status"), "Status");
        });

        await ClearAsync(scene);

        await scene.StepAsync("vend-5", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("button[type=submit]").Filter(new() { HasTextString = "Save" }).Last);
            await scene.NavigateAsync("/Admin/Contents/ContentItems/VendorProduct");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("li.list-group-item-action").Filter(new() { HasTextString = "Handmade Ceramic Mug" }).GetByRole(AriaRole.Link, new() { Name = "Edit" }).First);
            await scene.Page.Locator("#VendorPart_VendorId").ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(scene.Page.Locator("#VendorPart_VendorId"), "Vendor");
        });
    }

    private static async Task DetailAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/vendors");
        await scene.StartCaptureAsync();

        await scene.StepAsync("det-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Clayworks Studio").GetByRole(AriaRole.Link, new() { Name = "View" }));
            await scene.HighlightAsync(scene.Page.GetByText("Held (USD)").First, "Held");
            await scene.PauseAsync(2600);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("Ready to pay out (USD)").First, "Ready to pay out");
            await scene.PauseAsync(2600);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("Paid out (USD)").First, "Paid out");
        });

        await ClearAsync(scene);

        await scene.StepAsync("det-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("table").First, "Each sale: paid, commission, earned, status");
        });

        await ClearAsync(scene);

        await scene.StepAsync("det-3", async () =>
        {
            await scene.Page.GetByText("Release now", new() { Exact = true }).First.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Release order" }).First, "Release now");
            await scene.PauseAsync(2800);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Release order" }).First);
        });

        await scene.StepAsync("det-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("It hasn't set up payouts yet").First, "No payout account yet");
        });

        await ClearAsync(scene);

        await scene.StepAsync("det-5", async () =>
        {
            await scene.Page.GetByText("Payouts", new() { Exact = true }).First.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(scene.Page.GetByText("Payouts", new() { Exact = true }).First, "Payouts");
        });
    }

    private static async Task PortalAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, State("morgan"));

        await scene.OpenAsync("/account/vendor");
        await scene.StartCaptureAsync();

        await scene.StepAsync("port-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Held (USD)").First, "Held");
            await scene.PauseAsync(2600);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("Ready to pay out (USD)").First, "Ready to pay out");
            await scene.PauseAsync(2600);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("Paid out (USD)").First, "Paid out");
        });

        await ClearAsync(scene);

        await scene.StepAsync("port-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Set up payouts").Last, "Set up payouts");
        });

        await ClearAsync(scene);

        await scene.StepAsync("port-3", async () =>
        {
            await scene.ScrollAsync(0, 300);
            await scene.HighlightAsync(scene.Page.Locator("table").First, "Sales");
        });
    }

    private static async Task ClassifiedTypeAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/ContentTypes/List");
        await scene.StartCaptureAsync();

        await scene.StepAsync("type-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Classified Ad" }).First);
            var part = scene.Page.Locator("a[href$='/ContentParts/ClassifiedPart/Edit']").First;
            await part.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(part, "The Classified part");
        });

        await ClearAsync(scene);

        await scene.StepAsync("type-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/ContentParts/ClassifiedPart/Edit']").First);
            await scene.HighlightAsync(scene.Page.Locator("[id$='TermDays']"), "Days a listing runs");
        });

        await ClearAsync(scene);

        await scene.StepAsync("type-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("[id$='RequireApproval']"), "A moderator approves each listing");
        });
    }

    private static async Task PostAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, State("jamie"));

        await scene.OpenAsync("/account/listings");
        await scene.StartCaptureAsync();

        await scene.StepAsync("post-1", async () =>
        {
            await scene.HighlightAsync(LinkNamed(scene, "My listings"), "Your account > My listings");
            await scene.PauseAsync(2200);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Post a Classified Ad"));
        });

        await scene.StepAsync("post-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#TitlePart_Title"), "Used acoustic guitar", delay: 50);
            await scene.ClickAsync(scene.Page.Locator(".CodeMirror").First, pauseAfter: 300);
            await scene.Page.Keyboard.TypeAsync("Spruce top, with a soft case.", new() { Delay = 10 });
            await scene.PauseAsync(500);
            await scene.HighlightAsync(scene.Page.Locator(".CodeMirror").First, "Description");
        });

        await ClearAsync(scene);

        await scene.StepAsync("post-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#ClassifiedPart_Place"), "Springfield", delay: 60);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Use where I am" }), "Use where I am");
        });

        await ClearAsync(scene);

        await scene.StepAsync("post-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Post it" }));
            await scene.PauseAsync(1200);
        });
    }

    private static async Task ApproveAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/classifieds");
        await scene.StartCaptureAsync();

        await scene.StepAsync("moderate-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/classifieds"), "Content > Posted listings");
            await scene.PauseAsync(2400);
            await ClearAsync(scene);
            await scene.ClickAsync(scene.Page.Locator(".bootstrap-select button.dropdown-toggle").First, pauseAfter: 3200);
            await scene.Page.Keyboard.PressAsync("Escape");
        });

        await scene.StepAsync("moderate-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Used acoustic guitar").GetByRole(AriaRole.Button, new() { Name = "Approve" }));
        });

        await scene.StepAsync("moderate-3", async () =>
        {
            await scene.TypeAsync(Row(scene, "Garden tools").Locator("input[name=reason]"), "Free items are not listed here", delay: 40);
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Garden tools").GetByRole(AriaRole.Button, new() { Name = "Reject" }));
        });

        await scene.StepAsync("moderate-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".bootstrap-select button.dropdown-toggle").First, "Filter by status");
        });
    }

    private static async Task OwnAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, State("riley"));

        await scene.OpenAsync("/account/listings");
        await scene.StartCaptureAsync();

        await scene.StepAsync("own-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Piano lessons for beginners").First, "Status, and when it is up until");
        });

        await ClearAsync(scene);

        await scene.StepAsync("own-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Renew" }).First, "Renew");
        });

        await ClearAsync(scene);

        await scene.StepAsync("own-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Feature it for 7 days" }).First, "An upgrade: Feature");
        });

        await ClearAsync(scene);

        await scene.StepAsync("own-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Feature it for 7 days" }).First);
            await scene.PauseAsync(1500);
        });
    }

    private static async Task SearchAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, State("jamie"));

        await scene.OpenAsync("/listings");
        await scene.StartCaptureAsync();

        await scene.StepAsync("search-1", async () =>
        {
            await scene.TypeAsync(Labeled(scene, "Words"), "bike", delay: 90);
            await scene.WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"));
        });

        await scene.StepAsync("search-2", async () =>
        {
            await scene.HighlightAsync(Labeled(scene, "Within (km)"), "Within (km)");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Near me" }), "Near me");
        });
    }
}
