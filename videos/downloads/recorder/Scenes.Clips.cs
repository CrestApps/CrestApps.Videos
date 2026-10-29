using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Labeled(Scene scene, string label) => scene.Page.GetByLabel(label).First;

    private static ILocator LinkNamed(Scene scene, string name) => scene.Page.GetByRole(AriaRole.Link, new() { Name = name, Exact = true }).First;

    private static Task ClearAsync(Scene scene) => scene.ClearHighlightsAsync();

    // ----- Clips -----

    private static async Task ProductAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Contents/ContentItems/DigitalProduct,Product,StoreProduct");
        await scene.StartCaptureAsync();

        await scene.StepAsync("prod-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Contents/ContentItems/DigitalProduct,Product,StoreProduct"), "Commerce > Products");
            await scene.PauseAsync(1600);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Trail Field Guide (PDF)"));
        });

        await scene.StepAsync("prod-2", async () =>
        {
            var files = scene.Page.GetByText("Buyers download these after paying");
            await files.ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 150);
            await scene.HighlightAsync(scene.Page.GetByText("trail-field-guide.pdf").First, "Files");
            await scene.PauseAsync(4000);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("Remove", new() { Exact = true }).First, "Remove");
        });

        await ClearAsync(scene);

        await scene.StepAsync("prod-3", async () =>
        {
            await scene.HighlightAsync(Labeled(scene, "Downloads per buyer"), "Downloads per buyer");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.HighlightAsync(Labeled(scene, "Days the links work"), "Days the links work");
        });

        await ClearAsync(scene);

        await scene.StepAsync("prod-4", async () =>
        {
            await scene.ScrollAsync(0, 3000);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Publish" }).First, "Publish");
        });
    }

    private static async Task SettingsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/Settings/downloads");
        await scene.StartCaptureAsync();

        await scene.StepAsync("set-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/Settings/downloads"), "Settings > Commerce > Downloads");
            await scene.PauseAsync(2600);
            await ClearAsync(scene);
            await scene.HighlightAsync(Labeled(scene, "Downloads per buyer"), "Downloads per buyer");
        });

        await ClearAsync(scene);

        await scene.StepAsync("set-2", async () =>
        {
            await scene.HighlightAsync(Labeled(scene, "Days the links work"), "Days the links work");
        });
    }

    private static async Task BuyerAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations, Path.Combine(Root, "state-jamie.json"));

        await scene.OpenAsync("/account/orders");
        await scene.StartCaptureAsync();

        await scene.StepAsync("buyer-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Order 261029-", new() { Exact = false }).First, "A paid order");
        });

        await ClearAsync(scene);

        await scene.StepAsync("buyer-2", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Downloads"));
            await scene.HighlightAsync(scene.Page.GetByText("3 downloads left").First, "Downloads left, and until when");
        });

        await ClearAsync(scene);

        await scene.StepAsync("buyer-3", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(LinkNamed(scene, "Orders"));
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Order 261029-" }).First);
            await scene.Page.GetByText("Your downloads").First.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(500);
            await scene.HighlightAsync(scene.Page.GetByText("Your downloads").First, "Your downloads");
        });

        await ClearAsync(scene);

        await scene.StepAsync("buyer-4", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Download", Exact = true }).First);
            await scene.PauseAsync(1500);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Download" }).First, "Download");
        });
    }

    private static async Task ManageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/orders");
        await scene.StartCaptureAsync();

        string orderPath = null;
        string link = null;

        await scene.StepAsync("man-1", async () =>
        {
            await scene.ClickAndWaitForNavigationAsync(scene.Page.Locator("li.list-group-item.item").Filter(new() { HasTextString = "Jamie Chen" }).First.GetByRole(AriaRole.Link, new() { Name = "View" }));
            orderPath = new Uri(scene.Page.Url).AbsolutePath;
            await scene.Page.GetByText("Buyer's link").First.ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 120);
            await scene.HighlightAsync(scene.Page.GetByText("0 of 3 downloads").First, "Downloads, and until when");
        });

        await ClearAsync(scene);
        link = await scene.Page.EvaluateAsync<string>("() => (document.body.innerText.match(/Buyer's link: (\\S+)/) || [])[1]");

        await scene.StepAsync("man-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Take back" }).First, "Take back");
            await scene.PauseAsync(1200);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Take back" }).First);
            await scene.PauseAsync(800);
            await scene.NavigateAsync(new Uri(link).PathAndQuery);
        });

        await scene.StepAsync("man-3", async () =>
        {
            await scene.NavigateAsync(orderPath);
            await scene.Page.GetByText("Buyer's link").First.ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 120);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Give downloads back" }).First, "Give downloads back");
            await scene.PauseAsync(1500);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(scene.Page.GetByRole(AriaRole.Button, new() { Name = "Give downloads back" }).First);
        });

        await scene.StepAsync("man-4", async () =>
        {
            await scene.Page.GetByText("Buyer's link").First.ScrollIntoViewIfNeededAsync();
            await scene.ScrollAsync(0, 120);
            await scene.HighlightAsync(scene.Page.GetByText("Buyer's link").First, "Buyer's link");
        });

        await ClearAsync(scene);

        await scene.StepAsync("man-5", async () =>
        {
            await scene.Page.GetByText("Cancel the order").First.ScrollIntoViewIfNeededAsync();
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.GetByText("Cancel the order").First, "Cancel the order");
        });
    }
}
