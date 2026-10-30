using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    // ----- Helpers -----

    private static ILocator Menu(Scene scene, string href) => scene.Page.Locator($"#left-nav a[href$='{href}']").First;

    private static ILocator Row(Scene scene, string text) => scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = text }).First;

    private static Task ClearAsync(Scene scene) => scene.ClearHighlightsAsync();

    private const string OpenShift = "/Admin/point-of-sale/Shift/4pgrefqatz9pk288m4kj5jhwn1";

    private const string ClosedShift = "/Admin/point-of-sale/Shift/4vc0tjx73h2ez7tjw15tn8jwmn";

    // ----- Clips -----

    private static async Task OpenShiftAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/point-of-sale/registers");
        await scene.StartCaptureAsync();

        await scene.StepAsync("open-1", async () =>
        {
            await scene.HighlightAsync(Menu(scene, "/Admin/point-of-sale/registers"), "Commerce > Registers");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
            await scene.HighlightAsync(Row(scene, "Downtown till 1"), "A register, its store address, and its shifts");
        });

        await ClearAsync(scene);

        await scene.StepAsync("open-2", async () =>
        {
            await scene.NavigateAsync("/Admin/point-of-sale/shifts");
            await scene.HighlightAsync(Row(scene, "Open"), "The shift open at the till right now");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await scene.NavigateAsync(OpenShift);
            await scene.HighlightAsync(scene.Page.GetByText("Opening float").First, "The float the shift opened with");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("Expected").First, "The cash that should be in the drawer");
            await scene.PauseAsync(3000);
        });
    }

    private static async Task ClosedShiftAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync(ClosedShift);
        await scene.StartCaptureAsync();

        await scene.StepAsync("closed-1", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Cash kept").First, "Cash kept");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("Card", new() { Exact = true }).First, "Card");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("Refunded", new() { Exact = true }).First, "Refunded");
            await scene.PauseAsync(2500);
            await ClearAsync(scene);
        });

        await scene.StepAsync("closed-2", async () =>
        {
            await scene.HighlightAsync(scene.Page.GetByText("Put in, less taken out").First, "Cash in and out");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("Counted").First, "What staff counted");
            await scene.PauseAsync(3000);
            await ClearAsync(scene);
            await scene.HighlightAsync(scene.Page.GetByText("Difference").First, "The difference");
            await scene.PauseAsync(3500);
            await ClearAsync(scene);
        });
    }

    private static async Task SalesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/orders");
        await scene.StartCaptureAsync();

        await scene.StepAsync("sales-1", async () =>
        {
            await scene.HighlightAsync(Row(scene, "Downtown till 1"), "A register sale is a paid order, marked with its register");
            await scene.PauseAsync(5000);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Downtown till 1").Locator("a[href*='/Admin/orders/']").First);
            await scene.PauseAsync(3500);
        });

        await scene.StepAsync("sales-2", async () =>
        {
            await scene.NavigateAsync("/Admin/orders");
            await scene.HighlightAsync(Row(scene, "Refunded"), "A return at the till shows as refunded");
            await scene.PauseAsync(4000);
            await ClearAsync(scene);
            await scene.ClickAndWaitForNavigationAsync(Row(scene, "Refunded").Locator("a[href*='/Admin/orders/']").First);
            await scene.PauseAsync(3500);
        });
    }
}
