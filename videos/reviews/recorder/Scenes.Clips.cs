using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Row(Scene scene, string text) => scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = text }).First;

    private static Task ClearAsync(Scene scene) => scene.ClearHighlightsAsync();

    private static ILocator Labeled(Scene scene, string label) => scene.Page.GetByLabel(label).First;

    // ----- Clips -----

    private static async Task ProductAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, Path.Combine(Root, "state-dana.json"));

        await scene.OpenAsync("/shop/contoso-minimal-watch");
        await scene.StartCaptureAsync();

        await scene.StepAsync("prod-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator(".product-rating"), "Stars, average and number of reviews");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await scene.ClickAsync(scene.Page.Locator(".product-rating a"), pauseAfter: 600);
        });

        await scene.StepAsync("prod-2", async () =>
        {
            await scene.ScrollAsync(0, -150);
            await scene.HighlightAsync(scene.Page.Locator("#reviews h2"), "Customer reviews");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.Locator("#reviews article").First, "A review");
        });

        await ClearAsync(scene);

        await scene.StepAsync("prod-3", async () =>
        {
            await scene.ScrollAsync(0, 500);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Heading, new() { Name = "Write a review" }), "Write a review");
        });
    }

    private static async Task WriteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, Path.Combine(Root, "state-jamie.json"));

        await scene.OpenAsync("/shop/northwind-wool-beanie");
        await scene.Page.Locator("#review-rating").ScrollIntoViewIfNeededAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("write-1", async () =>
        {
            await scene.SelectAsync(scene.Page.Locator("#review-rating"), "5 stars", pauseAfter: 600);
            await scene.HighlightAsync(scene.Page.Locator("#review-rating"), "Your rating");
        });

        await ClearAsync(scene);

        await scene.StepAsync("write-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#review-title"), "Warm and soft", delay: 60);
            await scene.TypeAsync(scene.Page.Locator("#review-body"), "Keeps my ears warm on cold mornings, and it still looks good after a month of wear.", delay: 12);
        });

        await scene.StepAsync("write-3", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#review-author"), "Name shown with your review");
        });

        await ClearAsync(scene);

        await scene.StepAsync("write-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Send review" }));
            await scene.PauseAsync(1200);
        });
    }

    private static async Task ApproveAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/reviews");
        await scene.StartCaptureAsync();

        await scene.StepAsync("appr-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/reviews"), "Commerce > Reviews");
            await scene.PauseAsync(2800);
            await ClearAsync(scene);
            await scene.HighlightAsync(Row(scene, "Does the job"), "Product, author, rating and text");
        });

        await ClearAsync(scene);

        await scene.StepAsync("appr-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Warm and soft").GetByRole(AriaRole.Link, new() { Name = "Approve" }));
        });

        await scene.StepAsync("appr-3", async () =>
        {
            await scene.HighlightAsync(Row(scene, "Great deals online").GetByRole(AriaRole.Link, new() { Name = "Reject" }), "Reject");
            await scene.PauseAsync(900);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Great deals online").GetByRole(AriaRole.Link, new() { Name = "Reject" }));
        });

        await scene.StepAsync("appr-4", async () =>
        {
            await scene.ClickAsync(scene.Page.Locator(".bootstrap-select button.dropdown-toggle").First, pauseAfter: 2200);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator(".bootstrap-select .dropdown-menu.show").GetByText("Any status"));
        });

        await scene.StepAsync("appr-5", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Delete" }).First, "Delete");
        });
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/reviews");
        await scene.StartCaptureAsync();

        await scene.StepAsync("set-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/reviews"), "Settings > Commerce > Reviews");
            await scene.PauseAsync(2600);
            await ClearAsync(scene);
            await scene.HighlightAsync(Labeled(scene, "Approve reviews before they show"), "Approve reviews before they show");
        });

        await ClearAsync(scene);

        await scene.StepAsync("set-2", async () =>
        {
            await scene.HighlightAsync(Labeled(scene, "Only buyers can review"), "Only buyers can review");
        });

        await ClearAsync(scene);

        await scene.StepAsync("set-3", async () =>
        {
            await scene.ClickAsync(Labeled(scene, "Ask buyers for reviews"), pauseAfter: 700);
            await Labeled(scene, "Days after paying").FillAsync(string.Empty);
            await scene.TypeAsync(Labeled(scene, "Days after paying"), "14", delay: 90);
            await scene.HighlightAsync(Labeled(scene, "Days after paying"), "Days after paying");
        });

        await ClearAsync(scene);

        await scene.StepAsync("set-4", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Emails buyers who accept marketing").First);
        });
    }

    private static async Task ListingAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/shop");
        await scene.StartCaptureAsync();

        await scene.StepAsync("list-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("select").Filter(new() { HasText = "Customer rating: highest first" }).First, "Sort by");
            await scene.PauseAsync(1000);
            await ClearAsync(scene);
            await scene.WaitForNavigationAsync(() => scene.SelectAsync(scene.Page.Locator("select").Filter(new() { HasText = "Customer rating: highest first" }).First, "Customer rating: highest first"));
        });

        await scene.StepAsync("list-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.Locator("#storefront-main").GetByText("Customer rating").First, "Customer rating");
        });

        await ClearAsync(scene);

        await scene.StepAsync("list-3", async () =>
        {
            await scene.NavigateAsync("/Admin/Contents/ContentItems?q=type:ProductListing");
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("a[href*='/ContentItems/'][href*='/Edit']").First);
            await scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Choose filters" }).First, pauseAfter: 1000);
            await scene.HighlightAsync(scene.Page.GetByText("Customer rating", new() { Exact = true }).First, "Customer rating");
        });
    }
}
