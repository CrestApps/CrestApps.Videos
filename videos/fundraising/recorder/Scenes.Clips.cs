using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Labeled(Scene scene, string label) => scene.Page.GetByLabel(label).First;

    private static ILocator LinkNamed(Scene scene, string name) => scene.Page.GetByRole(AriaRole.Link, new() { Name = name, Exact = true }).First;

    private static Task ClearAsync(Scene scene) => scene.ClearHighlightsAsync();

    private static string State(string user) => Path.Combine(Root, $"state-{user}.json");

    // ----- Clips -----

    private static async Task ReceiptsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/fundraising");
        await scene.StartCaptureAsync();

        await scene.StepAsync("rec-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/fundraising"), "Settings > Commerce > Fundraising");
            await scene.PauseAsync(2600);
            await ClearAsync(scene);
            await scene.TypeAsync(Labeled(scene, "Organization"), "Springfield Community Library", delay: 40);
            await scene.HighlightAsync(Labeled(scene, "Organization"), "Organization");
        });

        await ClearAsync(scene);

        await scene.StepAsync("rec-2", async () =>
        {
            await scene.TypeAsync(Labeled(scene, "Tax ID"), "EIN 99-0000000", delay: 60);
            await scene.HighlightAsync(Labeled(scene, "Tax ID"), "Tax ID");
        });

        await ClearAsync(scene);

        await scene.StepAsync("rec-3", async () =>
        {
            await Labeled(scene, "Receipt statement").FillAsync(string.Empty);
            await scene.TypeAsync(Labeled(scene, "Receipt statement"), "Thank you for your gift. No goods or services were provided in exchange for it.", delay: 25);
            await scene.HighlightAsync(Labeled(scene, "Receipt statement"), "Receipt statement");
        });

        await ClearAsync(scene);

        await scene.StepAsync("rec-4", async () =>
        {
            await scene.HighlightAsync(Labeled(scene, "Email donors their receipts"), "Email donors their receipts");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("form button[type=submit]").Filter(new() { HasTextString = "Save" }).Last);
        });
    }

    private static async Task CampaignAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/FundraisingCampaign");
        await scene.StartCaptureAsync();

        await scene.StepAsync("camp-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item-action").First, "The campaign");
            await scene.PauseAsync(2400);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Books for the community library"));
        });

        await scene.StepAsync("camp-2", async () =>
        {
            await Labeled(scene, "Goal").ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(Labeled(scene, "Goal"), "Goal");
        });

        await ClearAsync(scene);

        await scene.StepAsync("camp-3", async () =>
        {
            await scene.HighlightAsync(Labeled(scene, "Suggested amounts"), "Suggested amounts");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await scene.HighlightAsync(Labeled(scene, "Least donation"), "Least donation");
        });

        await ClearAsync(scene);

        await scene.StepAsync("camp-4", async () =>
        {
            await scene.HighlightAsync(Labeled(scene, "Ends (UTC)"), "Ends");
        });

        await ClearAsync(scene);

        await scene.StepAsync("camp-5", async () =>
        {
            await scene.HighlightAsync(Labeled(scene, "Donations get a tax receipt"), "Donations get a tax receipt");
        });
    }

    private static async Task GiveAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, State("jamie"));

        await scene.OpenAsync("/give/community-library");
        await scene.StartCaptureAsync();

        await scene.StepAsync("give-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("raised of").First, "Progress toward the goal");
        });

        await ClearAsync(scene);

        await scene.StepAsync("give-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".fundraising-donate").First, "Your donation");
            await scene.PauseAsync(2800);
            await ClearAsync(scene);
            await scene.ClickAsync(scene.Page.Locator("[data-donation-amount='100']").First, pauseAfter: 900);
        });

        await scene.StepAsync("give-3", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("input[id$='-tribute']").First, "Eleanor Quinn", delay: 60);
            await scene.HighlightAsync(scene.Page.Locator("input[id$='-tribute']").First, "In honor or memory of");
        });

        await ClearAsync(scene);

        await scene.StepAsync("give-4", async () =>
        {
            await scene.ClickAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Add to cart" }).First, pauseAfter: 2500);
            await scene.NavigateAsync("/cart");
        });
    }

    private static async Task OrdersAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/orders");
        await scene.StartCaptureAsync();

        await scene.StepAsync("don-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/orders"), "Commerce > Orders");
            await scene.PauseAsync(2400);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator("li.list-group-item.item").Filter(new() { HasTextString = "Jamie Chen" }).First, "A donation");
        });

        await ClearAsync(scene);

        await scene.StepAsync("don-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("li.list-group-item.item").Filter(new() { HasTextString = "Jamie Chen" }).First.GetByRole(AriaRole.Link, new() { Name = "View" }));
            await scene.HighlightAsync(scene.Page.GetByText("Donation: $50").First, "Donation");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("In honor of:").First, "In honor of");
        });

        await ClearAsync(scene);

        await scene.StepAsync("don-3", async () =>
        {
            await scene.Page.GetByText("Refund", new() { Exact = true }).First.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(scene.Page.GetByText("Refund", new() { Exact = true }).First, "Refund");
        });
    }

    private static async Task AccountAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, State("jamie"));

        await scene.OpenAsync("/account/orders");
        await scene.StartCaptureAsync();

        await scene.StepAsync("acct-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Donations"));
            await scene.HighlightAsync(scene.Page.GetByText("Margaret Hale").First, "Who it honors");
        });

        await ClearAsync(scene);

        await scene.StepAsync("acct-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href$='/receipt']").First);
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.GetByText("Springfield Community Library").First, "Organization");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("EIN 99-0000000").First, "Tax ID");
        });

        await ClearAsync(scene);

        await scene.StepAsync("acct-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("No goods or services").First, "Receipt statement");
        });
    }
}
