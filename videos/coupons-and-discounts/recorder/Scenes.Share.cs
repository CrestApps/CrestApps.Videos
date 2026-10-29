using Microsoft.Playwright;

namespace Recorder;

public static partial class Scenes
{
    private static async Task ShareAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        await scene.OpenAsync("/Admin/coupons");
        await scene.Page.RunAndWaitForNavigationAsync(() => scene.Page.Locator("a").Filter(new() { HasTextString = "WELCOME10" }).First.ClickAsync());
        await scene.StartCaptureAsync();

        await scene.StepAsync("shr-1", async () =>
        {
            await scene.PauseAsync(1500);
            await ScrollAsync(scene, 0, 1500);
            await scene.HighlightAsync(scene.Page.Locator("#discount-link").Locator("xpath=ancestor::div[contains(@class,'ocat-wrapper') or contains(@class,'mb-3') or contains(@class,'card')][1]"), "Discount link");
            await scene.PauseAsync(3500);
            await scene.ClearHighlightsAsync();
        });

        await scene.StepAsync("shr-2", async () =>
        {
            await ReplaceAsync(scene, Fld(scene, "count"), "20", 120);
            await scene.TypeAsync(Fld(scene, "prefix"), "VIP-", delay: 90);
            await ReplaceAsync(scene, Fld(scene, "usesPerCode"), "1", 120);
            await scene.HighlightAsync(Fld(scene, "count").Locator("xpath=ancestor::*[self::form or contains(@class,'card')][1]"), "Share and generate codes");
            await scene.PauseAsync(2200);
            await scene.ClearHighlightsAsync();
            await scene.ClickAsync(scene.Page.Locator("button").Filter(new() { HasTextString = "Generate codes" }).First, pauseAfter: 500);
            await scene.OffCameraAsync(() => scene.Page.WaitForTimeoutAsync(5000));
        });

        await scene.StepAsync("shr-3", async () =>
        {
            var download = scene.Page.Locator("a").Filter(new() { HasTextString = "Download them" }).First;
            await download.ScrollIntoViewIfNeededAsync();
            await scene.HighlightAsync(download.Locator("xpath=ancestor::*[contains(@class,'card') or self::form or self::div][1]"), "Download them (CSV)");
            await scene.PauseAsync(3500);
        });

        await scene.ClearHighlightsAsync();
    }
}
